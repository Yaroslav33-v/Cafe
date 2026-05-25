async function updateCartCounter() {
    try {
        const response = await fetch('/cart-count');
        const data = await response.json();

        const cartCounter = document.querySelector('.cart-count');
        if (cartCounter) {
            cartCounter.textContent = data.count;
        }
    } catch (error) {
        console.error('Ошибка обновления корзины: ', error);
    }
}

async function getUserData() {
    try {
        const response = await fetch('/me');
        const data = await response.json();

        return data;

    } catch (error){
        console.error('Ошибка получения данных пользователя: ', error);
    }
}

async function openFoodModal(food, element) {
    const modal = document.getElementById('food-modal');

    const name = modal.querySelector('#modal-food-name');
    const frontImg = modal.querySelector('#modal-food-img-front');
    const backImg = modal.querySelector('#modal-food-img-back');
    const price = modal.querySelector('#modal-food-price');
    const weight = modal.querySelector('#modal-food-weight');
    const desc = modal.querySelector('#modal-food-desc');
    const ingredients = modal.querySelector('#modal-food-ingredients');
    const calories = modal.querySelector('#modal-food-calories');

    name.textContent = food.Name;
    frontImg.src = food.FrontImageAddress;
    frontImg.alt = food.Name;
    backImg.src = food.BackImageAddress;
    backImg.alt = food.Name;
    price.textContent = food.Price;
    weight.textContent = food.Weight;
    desc.textContent = food.Description;
    ingredients.textContent = food.Ingredients;
    calories.textContent = food.Calories;

    const addToCartBtn = modal.querySelector('#modal-food-cart-btn');
    const updateFavBtn = modal.querySelector('.modal-food-fav-btn');
    updateFavBtn.classList.remove('liked');

    const card = element.parentElement;
    const isLiked = card.querySelector('.modal-food-fav-btn').classList.contains('liked');

    if (isLiked) {
        updateFavBtn.classList.toggle('liked')
    }

    let wasFavUpdated = false;

    modal.querySelector('#close-btn').onclick = function () {
        modal.close();
        if (wasFavUpdated) {
            document.location.reload();
        }
    }

    addToCartBtn.onclick = async function () {
        await addToCart(food.Id);
        modal.close();
    }

    updateFavBtn.onclick = async function () {
        await updateFavourite(food.Id, updateFavBtn);
        wasFavUpdated = !wasFavUpdated;
    }

    modal.showModal();
}

async function addToCart(id) {
    try {
        const response = await fetch(`/cafe/addtocart?foodId=${id}`);
        const data = await response.json();
        if (response.ok && data.success) {
            // Успешное добавление
            showNotification(data.message || 'Блюдо добавлено в корзину!', 'success');
            await updateCartCounter();
        } else if (response.status === 400) {
            // Ошибка валидации
            showNotification(data.message || 'Нельзя добавить это блюдо', 'warning');

        } else if (response.status === 404) {
            // Блюдо не найдено
            showNotification('Блюдо не найдено', 'error');

        } else {
            // Другие ошибки
            throw new Error(data.message || 'Неизвестная ошибка');
        }

    } catch (error) {
        // Показываем ошибку пользователю
        showNotification(
            error.message || 'Ошибка при добавлении в корзину. Попробуйте позже.',
            'error'
        );
    }
}

async function updateFavourite(foodId, buttonElement) {
    try {
        const response = await fetch(`/cafe/updatefavourite?foodId=${foodId}`);
        const data = await response.json();

        if (response.ok && data.success) {
            // Успешное добавление
            showNotification(data.message, 'success');
            if (buttonElement.classList.contains('liked')) {
                buttonElement.classList.toggle('liked');
            } else {
                buttonElement.classList.toggle('liked');
            }
            // Здесь надо как-то изменять блок избранных
            // Самый простой вариант: Подождать 2-3 секунды и обновить страницу, но можешь сделать че то другое
        } else if (response.status === 404) {
            // Блюдо не найдено
            showNotification('Блюдо не найдено', 'error');

        } else {
            // Другие ошибки
            throw new Error(data.message || 'Неизвестная ошибка');
        }

    } catch (error) {
        // Показываем ошибку пользователю
        showNotification(
            error.message || 'Ошибка при добавлении в избранное. Попробуйте позже.',
            'error'
        );
    }   
}

function sanitizeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// Над этим тоже рекомендую подумать, потому что без перезагрузки страницы, у него начинаются проблемы
document.addEventListener('DOMContentLoaded', async () => {
    // Отображаем кнопку для входа, либо имя пользователя
    let user = await getUserData();

    const profileActionsDiv = document.getElementById('profile-actions');

    if (user.name) {
        const safeName = sanitizeHtml(user.name);
        profileActionsDiv.innerHTML = `
            <a href="/user/me" class="user-icon" title="Профиль">
                <i class="fas fa-user-circle"></i>
                <span class="username">${safeName}</span>
            </a>`;
    }
    else {
        profileActionsDiv.innerHTML = `
            <button class="login-btn" onclick="goToPage('/user/signin')">
                Войти
            </button>`;
    }

    // Добавляем действие для тайной кнопки
    const adminTeleportation = document.getElementById('admin-relocation');
    adminTeleportation.addEventListener('click', async () => {
        let data = await getUserData();

        if (data.role === "admin") {
            document.location.href = "/admin/index";
        }
    });

    // Обновляем корзину
    await updateCartCounter();

    // Работаем с избранным
    const favoriteDiv = document.querySelector('.menu-category[data-category-name="Избранное"]');

    if (favoriteDiv) {
        const ids = Array.from(favoriteDiv.querySelectorAll('.card')).map(card => parseInt(card.dataset.cardId));
        const cards = document.querySelectorAll('.card');

        cards.forEach(card => {
            const foodId = parseInt(card.dataset.cardId);
            const button = card.querySelector('button[onclick*="updateFavourite"]');

            if (button && ids.includes(foodId)) {
                button.classList.toggle('liked')
            }
        });
    }
})

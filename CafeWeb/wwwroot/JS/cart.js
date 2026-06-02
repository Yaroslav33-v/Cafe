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

// Функция для получения данных пользователя
async function getUserData() {
    try {
        const response = await fetch('/me');
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Ошибка получения данных пользователя: ', error);
        return {};
    }
}

async function clearCart() {
    const isConfirmed = confirm('Вы уверены, что хотите очистить корзину?');
    if (!isConfirmed) return;

    try {
        const response = await fetch('/cafe/clearcart');
        const data = await response.json();

        if (data.success) {
            showNotification(data.message || 'Корзина очищена', 'success');
            location.reload();
        } else {
            showNotification(data.message || 'Ошибка при очистке', 'error');
        }
    } catch (error) {
        showNotification('Ошибка при очистке корзины', 'error');
    }
}

async function updateQuantity(id, value) {
    try {
        const response = await fetch(`/cafe/updatequantity?id=${id}&value=${value}`);
        const data = await response.json();

        if (data.success) {
            if (data.totalItems === 0) {
                location.reload();
            } else {
                location.reload(); // Просто перезагружаем страницу
            }
        } else {
            showNotification(data.message || 'Ошибка при изменении количества', 'error');
        }
    } catch (error) {
        showNotification('Ошибка при изменении количества блюд', 'error');
    }
}

async function updateOfferQuantity(id, value) {
    try {
        const response = await fetch(`/cafe/updateofferquantity?offerId=${id}&value=${value}`);
        const data = await response.json();

        if (data.success) {
            if (data.totalItems === 0) {
                location.reload();
            } else {
                location.reload();
            }
        } else {
            showNotification(data.message || 'Ошибка при изменении количества', 'error');
        }
    } catch (error) {
        showNotification('Ошибка при изменении количества', 'error');
    }
}
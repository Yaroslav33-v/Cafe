async function addToCart(id){
    try {
        const response = await fetch(`/cafe/addtocart?foodId=${id}`);
        const data = await response.json();
        if (response.ok && data.success) {
            // Успешное добавление
            showNotification(data.message || 'Блюдо добавлено в корзину!', 'success');
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

        let btnText = buttonElement.textContent;

        if (response.ok && data.success) {
            // Успешное добавление
            showNotification(data.message, 'success');
            if (btnText === 'Добавить в избранное') {
                buttonElement.textContent = 'Удалить из избранного';
            } else {
                buttonElement.textContent = 'Добавить в избранное';
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

// Над этим тоже рекомендую подумать, потому что без перезагрузки страницы, у него начинаются проблемы
document.addEventListener('DOMContentLoaded', () => {
    const favoriteDiv = document.querySelector('.menu-category[data-category-name="Избранное"]');
    if (favoriteDiv) {
        const ids = Array.from(favoriteDiv.querySelectorAll('.card')).map(card => parseInt(card.dataset.cardId));
        const cards = document.querySelectorAll('.card');

        cards.forEach(card => {
            const foodId = parseInt(card.dataset.cardId);
            const button = card.querySelector('button[onclick*="updateFavourite"]');

            if (button && ids.includes(foodId)) {
                button.textContent = 'В избранном';

                button.onmouseenter = () => {
                    button.textContent = 'Удалить из избранного';
                };

                button.onmouseleave = () => {
                    button.textContent = 'В избранном';
                };
            }
        });
    }
})

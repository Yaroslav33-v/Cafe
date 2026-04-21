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

async function updateFavourite(foodId) {
    try {
        const response = await fetch(`/cafe/updatefavourite?foodId=${foodId}`);
        const data = await response.json();

        if (response.ok && data.success) {
            // Успешное добавление
            showNotification(data.message, 'success');
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
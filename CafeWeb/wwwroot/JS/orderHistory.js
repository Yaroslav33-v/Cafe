// Обновление счетчика корзины
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

// Загрузка данных пользователя
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

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', async () => {
    // Обновляем счетчик корзины
    await updateCartCounter();
});
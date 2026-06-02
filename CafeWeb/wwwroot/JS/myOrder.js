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

// Экранирование HTML
function sanitizeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', async () => {
    // Обновляем счетчик корзины
    await updateCartCounter();

    const userId = document.getElementById('userId').value || '0';
    let connection;

    async function initializeSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/cafeHub")
            .build();

        connection.on("OrderUpdated", (update) => {
            let statusElement = document.querySelector(`[data-order-id="${update.orderId}"]`);
            if (statusElement) {
                statusElement.textContent = 'Готов';
                statusElement.className = `order-status-badge badge-ready`;
            }
        });

        try {
            await connection.start();
            if (userId && userId !== '0') {
                await connection.invoke("RegisterUser", parseInt(userId));
            }
        } catch (error) {
            console.error("SignalR error:", error);
        }
    }

    if (userId && userId !== '0') {
        initializeSignalR();
    }
});
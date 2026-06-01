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

// SignalR для обновления статуса заказа
const userIdElement = document.getElementById('userId');
const userId = userIdElement?.value || '0';
let connection;

async function initializeSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/cafeHub")
        .build();

    connection.on("OrderUpdated", (update) => {
        let statusElement = document.querySelector(`[data-order-id="${update.orderId}"]`);
        if (statusElement) {
            statusElement.textContent = update.status;
            statusElement.className = `order-status status-${update.status.toLowerCase().replace(" ", "-")}`;
            showNotification(`Статус заказа №${update.orderNumber} изменён на "${update.status}"`, 'success');
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

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', async () => {
    // Обновляем счетчик корзины
    await updateCartCounter();

    // Отображаем кнопку для входа, либо имя пользователя
    let user = await getUserData();
    const profileActionsDiv = document.getElementById('profile-actions');

    if (profileActionsDiv) {
        if (user.name) {
            const safeName = sanitizeHtml(user.name);
            profileActionsDiv.innerHTML = `
                <a href="/user/me" class="user-icon" title="Профиль">
                    <i class="fas fa-user-circle"></i>
                    <span class="username">${safeName}</span>
                </a>`;
        } else {
            profileActionsDiv.innerHTML = `
                <button class="login-btn" onclick="goToPage('/user/signin')">
                    Войти
                </button>`;
        }
    }

    // Запускаем SignalR для отслеживания статуса
    if (userId && userId !== '0') {
        initializeSignalR();
    }
});
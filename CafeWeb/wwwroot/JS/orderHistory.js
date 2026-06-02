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
});
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

    } catch (error) {
        console.error('Ошибка получения данных пользователя: ', error);
    }
}

// Функция для экранирования HTML
function sanitizeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

const profileActionsDiv = document.getElementById('profile-actions');

document.addEventListener('DOMContentLoaded', async () => {
    // Отображаем кнопку для входа, либо имя пользователя
    let user = await getUserData();

    await updateCartCounter();

    if (user.name) {
        const safeName = sanitizeHtml(user.name);
        profileActionsDiv.innerHTML = `
        <div class="user-menu-wrapper">
            <a href="/user/me" class="user-icon" title="Профиль">
                <i class="fas fa-user-circle"></i>
                <span class="username">${safeName}</span>
            </a>
            <button class="dropdown-arrow" id="dropdown-arrow" title="Меню">
                <i class="fas fa-chevron-down"></i>
            </button>
            <div class="dropdown-panel" id="dropdown-panel">
                <button class="logout-btn" onclick="goToPage('/signout')">
                    <i class="fa-solid fa-right-from-bracket"></i> Выйти
                </button>
            </div>
        </div>`;

        // Логика открытия/закрытия панельки
        const arrow = document.getElementById('dropdown-arrow');
        const panel = document.getElementById('dropdown-panel');

        arrow.addEventListener('click', (e) => {
            e.stopPropagation();
            panel.classList.toggle('active');
            arrow.classList.toggle('rotated');
        });

        // Закрытие при клике вне панели
        document.addEventListener('click', (e) => {
            if (!profileActionsDiv.contains(e.target)) {
                panel.classList.remove('active');
                arrow.classList.remove('rotated');
            }
        });

    } else {
        profileActionsDiv.innerHTML = `
        <button class="login-btn" onclick="goToPage('/user/signin')">
            Войти
        </button>`;
    }
});

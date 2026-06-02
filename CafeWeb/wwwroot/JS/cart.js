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

// Загружаем данные пользователя и отображаем профиль с выпадающим меню
document.addEventListener('DOMContentLoaded', async () => {
    const profileActionsDiv = document.getElementById('profile-actions');
    if (!profileActionsDiv) return;

    let user = await getUserData();

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
                        <i class="fas fa-sign-out-alt"></i> Выйти
                    </button>
                </div>
            </div>
        `;

        const arrow = document.getElementById('dropdown-arrow');
        const panel = document.getElementById('dropdown-panel');

        if (arrow && panel) {
            arrow.addEventListener('click', (e) => {
                e.stopPropagation();
                panel.classList.toggle('active');
                arrow.classList.toggle('rotated');
            });

            document.addEventListener('click', (e) => {
                if (!profileActionsDiv.contains(e.target)) {
                    panel.classList.remove('active');
                    arrow.classList.remove('rotated');
                }
            });
        }
    } else {
        profileActionsDiv.innerHTML = `
            <button class="login-btn" onclick="goToPage('/user/signin')">
                Войти
            </button>
        `;
    }
});

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
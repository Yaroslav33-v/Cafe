function debounce(func, delay) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, delay);
    };
}

// Проверка логина через API
async function checkUsername(login) {
    const statusEl = document.getElementById('status');

    if (login.length < 3) {
        statusEl.textContent = 'Минимум 3 символа';
        statusEl.className = 'checking';
        return;
    }

    statusEl.textContent = 'Проверяем...';
    statusEl.className = 'checking';

    try {
        // Отправляем GET запрос на бэкенд
        const response = await fetch(`/check-login/${encodeURIComponent(login)}`);
        const data = await response.json();

        if (data.available) {
            statusEl.textContent = '✅ Логин свободен';
            statusEl.className = 'available';
        } else {
            statusEl.textContent = '❌ Логин уже занят';
            statusEl.className = 'taken';
        }
    } catch (error) {
        statusEl.textContent = 'Ошибка проверки';
        statusEl.className = 'taken';
    }
}

// Вешаем обработчик с debounce (ждем 300мс после последнего нажатия)
const usernameInput = document.getElementById('username');
const debouncedCheck = debounce(() => checkUsername(usernameInput.value), 300);

usernameInput.addEventListener('input', debouncedCheck);

// Мгновенная проверка при потере фокуса (на случай если debounce не сработал)
usernameInput.addEventListener('blur', () => {
    if (usernameInput.value.length >= 3) {
        checkUsername(usernameInput.value);
    }
});

document.getElementById('rand-password').addEventListener('click', function () {
    document.getElementById('password-input').value = getRandomPassword();
})

function getRandomPassword() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let passwordLength = Math.floor(Math.random() * (20 - 8 + 1) + 8); // 8 - минимальная длина пароля, 20 - максимальная
    let randPassword = '';

    for (let i = 0; i < passwordLength; i++) {
        let randIndex = Math.floor(Math.random() * chars.length);
        randPassword += chars[randIndex];
    }

    return randPassword;
}
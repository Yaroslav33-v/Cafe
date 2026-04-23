const dialog = document.getElementById('password-dialog');
const pswdButton = document.getElementById('password-btn');
const form = document.getElementById('password-form');

pswdButton.onclick = () => dialog.showModal(); // Открыть диалог

document.getElementById('cancel-btn').onclick = () => dialog.close();

// Обработка отправки формы
form.addEventListener('submit', async (e) => {
    e.preventDefault();

    const formData = new FormData(form);
    try {
        const response = await fetch('/user/changepassword', {
            method: 'POST',
            body: formData
        });
        const data = await response.json();

        if (response.ok && data.success) {
            dialog.close();
            showNotification(data.message || 'Пароль успешно изменён', 'success');
        } else if (response.ok && !data.success) {
            showNotification(data.message || 'Неправильный текущий пароль', 'error');
        } else if (response.status === 400) {
            showNotification(data.message || 'Не удалось найти пользователя', 'error');
        } else {
            showNotification(data.message || 'Системная ршибка', 'error');
        }
    } catch (error) {
        showNotification(data.message || 'Системная ршибка', 'error');
    } finally {
        document.querySelectorAll('.passwordInput').forEach(input => input.value = '');
    }
});
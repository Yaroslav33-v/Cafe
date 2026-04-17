async function clearCart() {
    // Подтверждение действия
    const isConfirmed = confirm('Вы уверены, что хотите очистить корзину?');

    if (!isConfirmed) return;

    try {
        const response = await fetch('/cafe/clearcart');

        const data = await response.json();

        if (data.success) {
            // Обновляем UI
            location.reload()
        } else {
            showNotification(data.message || 'Ошибка при очистке', 'error');
        }

    } catch (error) {
        showNotification('Ошибка при очистке корзины', 'error');
    }
}
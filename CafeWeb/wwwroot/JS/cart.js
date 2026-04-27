async function clearCart() {
    // Подтверждение действия
    const isConfirmed = confirm('Вы уверены, что хотите очистить корзину?');

    if (!isConfirmed)
        return;

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

async function updateQuantity(id, value) {
    try {
        const response = await fetch(`/cafe/updatequantity?id=${id}&value=${value}`);

        const data = await response.json();
        if (data.success) {
            const foodBlock = document.querySelector(`[data-id="${id}"]`);
            const totalQuantitySpan = document.getElementById('totalQuantity');
            const totalPriceSpan = document.getElementById('totalPrice');

            // Получаем quantity span
            const quantitySpan = foodBlock.querySelector(`[data-quantity]`);

            // Получаем total span
            const totalSpan = foodBlock.querySelector(`[data-total]`);

            if (data.itemQuantity <= 0) {
                foodBlock.remove();
            }

            // Обновляем данные
            quantitySpan.textContent = data.itemQuantity;
            totalSpan.textContent = data.itemTotal.toFixed(2);
            totalQuantitySpan.textContent = data.totalItems;
            totalPriceSpan.textContent = data.totalAmount.toFixed(2);
        }
        else {
            showNotification(data.message || 'Ошибка при изменении количества блюд', 'error');
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
            const offerBlock = document.querySelector(`[data-offer-id="${id}"]`);
            const totalQuantitySpan = document.getElementById('totalQuantity');
            const totalPriceSpan = document.getElementById('totalPrice');

            // Получаем quantity span
            const quantitySpan = offerBlock.querySelector(`[data-offer-quantity]`);

            // Получаем total span
            const totalSpan = offerBlock.querySelector(`[data-offer-total]`);

            if (data.totalItems <= 0) {
                await clearCart()
            }
            if (data.itemQuantity <= 0) {
                offerBlock.remove();
            }

            // Обновляем данные
            quantitySpan.textContent = data.itemQuantity;
            totalSpan.textContent = data.itemTotal.toFixed(2);
            totalQuantitySpan.textContent = data.totalItems;
            totalPriceSpan.textContent = data.totalAmount.toFixed(2);
        }
        else {
            showNotification(data.message || 'Ошибка при изменении количества блюд', 'error');
        }

    } catch (error) {
        showNotification('Ошибка при изменении количества блюд', 'error');
    }
}
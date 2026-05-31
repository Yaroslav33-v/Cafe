﻿async function clearCart() {
    const isConfirmed = confirm('Вы уверены, что хотите очистить корзину?');
    if (!isConfirmed) return;

    try {
        const response = await fetch('/cafe/clearcart');
        const data = await response.json();

        if (data.success) {
            showNotification(data.message || 'Корзина очищена', 'success');
            setTimeout(() => location.reload(), 500);
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
            const foodBlock = document.querySelector(`[data-item-id="${id}"]`);
            
            if (data.itemQuantity <= 0) {
                if (foodBlock) foodBlock.remove();
                showNotification('Блюдо удалено из корзины', 'info');
            } else {
                const quantitySpan = foodBlock?.querySelector(`[data-quantity="${id}"]`);
                const totalSpan = foodBlock?.querySelector(`[data-total="${id}"]`);
                if (quantitySpan) quantitySpan.textContent = data.itemQuantity;
                if (totalSpan) totalSpan.textContent = data.itemTotal.toFixed(2) + ' ₽';
            }
            
            const totalQuantitySpan = document.getElementById('totalQuantity');
            const totalPriceSpan = document.getElementById('totalPrice');
            const cartCountSpan = document.querySelector('.cart-count');
            
            if (totalQuantitySpan) totalQuantitySpan.textContent = data.totalItems;
            if (totalPriceSpan) totalPriceSpan.textContent = data.totalAmount.toFixed(2) + ' ₽';
            if (cartCountSpan) cartCountSpan.textContent = data.totalItems;
            
            if (data.totalItems === 0) {
                location.reload();
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
            const offerBlock = document.querySelector(`[data-offer-id="${id}"]`);
            
            if (data.itemQuantity <= 0) {
                if (offerBlock) offerBlock.remove();
                showNotification('Акция удалена из корзины', 'info');
            } else {
                const quantitySpan = offerBlock?.querySelector(`[data-offer-quantity="${id}"]`);
                const totalSpan = offerBlock?.querySelector(`[data-offer-total="${id}"]`);
                if (quantitySpan) quantitySpan.textContent = data.itemQuantity;
                if (totalSpan) totalSpan.textContent = data.itemTotal.toFixed(2) + ' ₽';
            }
            
            const totalQuantitySpan = document.getElementById('totalQuantity');
            const totalPriceSpan = document.getElementById('totalPrice');
            const cartCountSpan = document.querySelector('.cart-count');
            
            if (totalQuantitySpan) totalQuantitySpan.textContent = data.totalItems;
            if (totalPriceSpan) totalPriceSpan.textContent = data.totalAmount.toFixed(2) + ' ₽';
            if (cartCountSpan) cartCountSpan.textContent = data.totalItems;
            
            if (data.totalItems === 0) {
                location.reload();
            }
        } else {
            showNotification(data.message || 'Ошибка при изменении количества', 'error');
        }
    } catch (error) {
        showNotification('Ошибка при изменении количества', 'error');
    }
}

async function getCart() {
    try {
        const response = await fetch('/cafe/getcart');
        return await response.json();
    } catch (error) {
        console.error('Ошибка получения корзины:', error);
        return null;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    // Отображаем кнопку для входа, либо имя пользователя
    let user = await getUserData();

    const profileActionsDiv = document.getElementById('profile-actions');

    if (user.name) {
        const safeName = sanitizeHtml(user.name);
        profileActionsDiv.innerHTML = `
            <a href="/user/me" class="user-icon" title="Профиль">
                <i class="fas fa-user-circle"></i>
                <span class="username">${safeName}</span>
            </a>`;
    }
    else {
        profileActionsDiv.innerHTML = `
            <button class="login-btn" onclick="goToPage('/user/signin')">
                Войти
            </button>`;
    }
});
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
function parsePrice(priceString) {
    // Удаляем все пробелы и заменяем запятую на точку
    const cleanString = priceString.toString().replace(/\s/g, '').replace(',', '.');
    return parseFloat(cleanString);
}

// Проверка промокода через API
async function checkPromocode(promo) {
    const responseEl = document.getElementById('fetch-response');
    const totalPriceEl = document.getElementById('total-price');
    const totalInput = document.getElementById('total-input');
    const originalTotal = parsePrice(totalPriceEl.dataset.originalTotal || totalPriceEl.textContent);

    // Сохраняем оригинальную сумму при первом вызове
    if (!totalPriceEl.dataset.originalTotal) {
        totalPriceEl.dataset.originalTotal = originalTotal;
    }

    if (promo.length < 3 && promo.length > 0) {
        responseEl.textContent = 'Минимум 3 символа';
        responseEl.className = 'checking';
        // Восстанавливаем исходную сумму
        totalPriceEl.textContent = originalTotal.toFixed(2);
        if (totalInput) totalInput.value = originalTotal.toFixed(2); 
        return;
    } else if (promo.length === 0) {
        // Если промокод пустой, очищаем сообщение и восстанавливаем сумму
        responseEl.textContent = '';
        responseEl.className = '';
        totalPriceEl.textContent = originalTotal.toFixed(2);
        if (totalInput) totalInput.value = originalTotal.toFixed(2);
        return;
    }

    responseEl.textContent = 'Проверяем промокод...';
    responseEl.className = 'checking';

    try {
        // Отправляем GET запрос на бэкенд
        const response = await fetch(`/is-valid-promo/${encodeURIComponent(promo)}`);
        const data = await response.json();

        if (data.available) {
            responseEl.textContent = `✅ ${data.message} (скидка ${data.discount} руб. от ${data.fromSum} руб.)`;
            responseEl.className = 'available';
            
            // Применяем скидку, если сумма заказа достаточна
            if (originalTotal >= data.fromSum) {
                const discountedTotal = originalTotal - data.discount;
                totalPriceEl.textContent = discountedTotal.toFixed(2);
                if (totalInput) totalInput.value = discountedTotal.toFixed(2);
            } else {
                totalPriceEl.textContent = originalTotal.toFixed(2);
                if (totalInput) totalInput.value = originalTotal.toFixed(2);
            }
        } else {
            responseEl.textContent = `❌ ${data.message}`;
            responseEl.className = 'no-available';
            // Восстанавливаем исходную сумму
            totalPriceEl.textContent = originalTotal.toFixed(2);
            if (totalInput) totalInput.value = originalTotal.toFixed(2); 
        }
    } catch (error) {
        responseEl.textContent = 'Ошибка проверки промокода';
        responseEl.className = 'no-available';
        // Восстанавливаем исходную сумму
        totalPriceEl.textContent = originalTotal.toFixed(2);
        if (totalInput) totalInput.value = originalTotal.toFixed(2); 
    }
}

// Вешаем обработчик с debounce (ждем 500мс после последнего ввода)
const promoInput = document.getElementById('promo');
const debouncedCheck = debounce(() => checkPromocode(promoInput.value), 500);

promoInput.addEventListener('input', debouncedCheck);

// Мгновенная проверка при потере фокуса
promoInput.addEventListener('blur', () => {
    if (promoInput.value.length >= 3) {
        checkPromocode(promoInput.value);
    }
});

// Очистка промокода и сброс суммы (если нужно)
promoInput.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        promoInput.value = '';
        checkPromocode('');
    }
});

document.getElementById('expiry').addEventListener('input', function (e) {
    let value = e.target.value.replace(/\D/g, ''); // только цифры
    if (value.length >= 2) {
        value = value.slice(0, 2) + '/' + value.slice(2, 4);
    }
    e.target.value = value;
});

document.getElementById('card-number').addEventListener('input', function (e) {
    let number = e.target.value.replace(/\D/g, ''); 

    if (number.length > 0) {
        let formatted = number.match(/.{1,4}/g)?.join(' ') || '';
        e.target.value = formatted;
    }

    if (number.length === 16) {
        if (luhnAlgorithm(number)) {
            e.target.style.borderColor = 'green';
        } else {
            e.target.style.borderColor = 'red';
        }
    } else {
        e.target.style.borderColor = '';
    }
});

// Алгоритм Луна
function luhnAlgorithm(cardNumber) {
    let sum = 0;
    let isEven = false;

    for (let i = cardNumber.length - 1; i >= 0; i--) {
        let digit = parseInt(cardNumber.charAt(i), 10);

        if (isEven) {
            digit *= 2;
            if (digit > 9) {
                digit -= 9;
            }
        }

        sum += digit;
        isEven = !isEven;
    }

    return sum % 10 === 0;
}


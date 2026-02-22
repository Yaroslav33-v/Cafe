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
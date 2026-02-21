document.getElementById('rand-promo').addEventListener('click', function () {
    document.getElementById('promo-input').value = getRandomPromocode();
})

function getRandomPromocode() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let promoLength = Math.floor(Math.random() * (20 - 5 + 1) + 5); // 5 - минимальная длина промокода, 20 - максимальная
    let randPromo = '';

    for (let i = 0; i < promoLength; i++) {
        let randIndex = Math.floor(Math.random() * chars.length);
        randPromo += chars[randIndex];
    }

    return randPromo;
}
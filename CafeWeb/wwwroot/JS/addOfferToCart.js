async function addToCart(offerName, offerId, foodIds, discount) {
    // Отправляем на сервер
    const data = {
        name: offerName,
        id: offerId,
        discount: discount,
        foodIds: foodIds
    };

    try {
        const response = await fetch('/cafe/addoffertocart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        const responseData = await response.json();

        if (response.ok && responseData.success) {
            showNotification(responseData.message || 'Акция добавлена в корзину', 'success')
        } else {
            showNotification(responseData.message || 'Ошибка при добавлении акции в корзину', 'error')
        }
    } catch (error) {
        showNotification(responseData.message || 'Ошибка при добавлении акции в корзину', 'error')
        console.log(error);
    }
}
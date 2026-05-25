document.getElementById('searchInput').addEventListener('input', function () {
    const query = this.value.toLowerCase();

    // Если поиск пустой - показываем всё
    if (query === '') {
        document.querySelectorAll('.card').forEach(card => {
            card.style.display = '';
        });
        document.querySelectorAll('.menu-category').forEach(category => {
            category.style.display = '';
        });
        return;
    }

    // Флаг для отслеживания, есть ли совпадения по категориям
    let categoryMatch = false;

    // Проверяем категории
    document.querySelectorAll('.menu-category').forEach(category => {
        const categoryName = category.querySelector('h1').textContent.toLowerCase();
        const cards = category.querySelectorAll('.card');

        // Если категория совпадает с поиском
        if (categoryName.includes(query)) {
            category.style.display = '';
            categoryMatch = true;
            // Показываем все карточки в этой категории
            cards.forEach(card => {
                card.style.display = '';
            });
        } else {
            // Если категория не совпала, проверяем карточки внутри
            let hasVisibleCards = false;

            cards.forEach(card => {
                const title = card.querySelector('.card-title').textContent.toLowerCase();

                if (title.includes(query)) {
                    card.style.display = '';
                    hasVisibleCards = true;
                } else {
                    card.style.display = 'none';
                }
            });

            // Показываем категорию только если в ней есть видимые карточки
            if (hasVisibleCards) {
                category.style.display = '';
            } else {
                category.style.display = 'none';
            }
        }
    });
});
document.getElementById('searchInput').addEventListener('input', function () {
    const query = this.value.toLowerCase();

    
    if (query === '') {
        document.querySelectorAll('.card').forEach(card => {
            card.style.display = '';
        });
        document.querySelectorAll('.menu-category').forEach(category => {
            category.style.display = '';
        });
        return;
    }

    
    let categoryMatch = false;

    
    document.querySelectorAll('.menu-category').forEach(category => {
        const categoryName = category.querySelector('h1').textContent.toLowerCase();
        const cards = category.querySelectorAll('.card');

        
        if (categoryName.includes(query)) {
            category.style.display = '';
            categoryMatch = true;
            
            cards.forEach(card => {
                card.style.display = '';
            });
        } else {
            
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

            
            if (hasVisibleCards) {
                category.style.display = '';
            } else {
                category.style.display = 'none';
            }
        }
    });
});

document.getElementById('searchInput').addEventListener('input', function () {
    const query = this.value.toLowerCase();
    let hasMatches = false;

    document.querySelectorAll('.menu-category').forEach(category => {
        let categoryHasMatch = false;
        const cards = category.querySelectorAll('.card');

        cards.forEach(card => {
            const title = card.querySelector('.card-title').textContent.toLowerCase();
            if (title.includes(query)) {
                card.style.display = '';
                categoryHasMatch = true;
                hasMatches = true;
            } else {
                card.style.display = 'none';
            }
        });

        category.style.display = categoryHasMatch || query === '' ? '' : 'none';
    });

    let notFoundMsg = document.getElementById('not-found-message');

    if (!hasMatches && query !== '') {
        if (!notFoundMsg) {
            notFoundMsg = document.createElement('div');
            notFoundMsg.id = 'not-found-message';
            notFoundMsg.innerHTML = '<p> Ничего не найдено</p>';
            document.querySelector('.container').appendChild(notFoundMsg);
        }
        notFoundMsg.style.display = 'block';
    } else {
        if (notFoundMsg) notFoundMsg.style.display = 'none';
    }
});
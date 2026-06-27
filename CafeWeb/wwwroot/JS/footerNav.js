document.addEventListener('DOMContentLoaded', function () {
    // Находим все ссылки в футере, которые ведут на about.html#section
    const footerLinks = document.querySelectorAll('.footer-nav a[data-section]');

    footerLinks.forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const sectionIndex = this.getAttribute('data-section');
            const sectionId = this.getAttribute('data-section-id');

            // Если мы уже на странице about.html
            if (window.location.pathname.includes('/about') || window.location.pathname.endsWith('/about')) {
                // Ищем карусель на странице
                const container = document.querySelector('.horizontal-container');
                if (container) {
                    // Переключаемся на нужный слайд
                    const sections = document.querySelectorAll('.horizontal-section');
                    const total = sections.length;
                    const index = parseInt(sectionIndex);

                    if (!isNaN(index) && index >= 0 && index < total) {
                        // Используем существующую функцию goTo() если она есть
                        if (typeof goTo === 'function') {
                            goTo(index);
                        } else {
                            // Если функции нет, делаем сами
                            container.style.transform = `translateX(-${index * 100}%)`;

                            // Обновляем точки
                            const dots = document.querySelectorAll('.dot');
                            dots.forEach((dot, i) => {
                                dot.classList.toggle('active', i === index);
                            });

                            // Обновляем навигацию сверху
                            const navLinks = document.querySelectorAll('.sticky-nav-link');
                            navLinks.forEach((link, i) => {
                                link.classList.toggle('active', i === index);
                            });

                            // Обновляем кнопки навигации
                            const prevBtn = document.getElementById('prevBtn');
                            const nextBtn = document.getElementById('nextBtn');
                            if (prevBtn) prevBtn.classList.toggle('disabled', index === 0);
                            if (nextBtn) nextBtn.classList.toggle('disabled', index === total - 1);
                        }
                    }
                }
            } else {
                // Если мы на другой странице - переходим на about.html с параметром
                const currentUrl = window.location.origin + '/about?section=' + sectionIndex;
                window.location.href = currentUrl;
            }
        });
    });

    // Обработка параметра ?section= при загрузке страницы
    const urlParams = new URLSearchParams(window.location.search);
    const sectionParam = urlParams.get('section');

    if (sectionParam !== null && window.location.pathname.includes('/about')) {
        const index = parseInt(sectionParam);
        if (!isNaN(index)) {
            // Ждём загрузки карусели
            const checkContainer = setInterval(function () {
                const container = document.querySelector('.horizontal-container');
                if (container) {
                    clearInterval(checkContainer);

                    // Если есть функция goTo - используем её
                    if (typeof goTo === 'function') {
                        goTo(index);
                    } else {
                        // Иначе делаем сами
                        const sections = document.querySelectorAll('.horizontal-section');
                        const total = sections.length;
                        if (index >= 0 && index < total) {
                            container.style.transform = `translateX(-${index * 100}%)`;

                            const dots = document.querySelectorAll('.dot');
                            dots.forEach((dot, i) => {
                                dot.classList.toggle('active', i === index);
                            });

                            const navLinks = document.querySelectorAll('.sticky-nav-link');
                            navLinks.forEach((link, i) => {
                                link.classList.toggle('active', i === index);
                            });

                            const prevBtn = document.getElementById('prevBtn');
                            const nextBtn = document.getElementById('nextBtn');
                            if (prevBtn) prevBtn.classList.toggle('disabled', index === 0);
                            if (nextBtn) nextBtn.classList.toggle('disabled', index === total - 1);

                            // Обновляем currentIndex
                            if (typeof currentIndex !== 'undefined') {
                                currentIndex = index;
                            }
                        }
                    }
                }
            }, 100);
        }
    }
});
document.addEventListener('DOMContentLoaded', function () {
    let currentIndex = 0;
    const container = document.querySelector('.horizontal-container');
    const sections = document.querySelectorAll('.horizontal-section');
    const total = sections.length;
    const dots = document.querySelectorAll('.dot');
    const navLinks = document.querySelectorAll('.sticky-nav-link');
    const footerLinks = document.querySelectorAll('.footer-nav a');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    let isAnimating = false;

    function goTo(index) {
        if (isAnimating) return;
        if (index < 0) index = 0;
        if (index >= total) index = total - 1;
        if (index === currentIndex) return;

        isAnimating = true;
        currentIndex = index;

        container.style.transform = `translateX(-${currentIndex * 100}%)`;

        // Точки
        dots.forEach((dot, i) => {
            dot.classList.toggle('active', i === currentIndex);
        });

        // Навигация сверху
        navLinks.forEach((link, i) => {
            link.classList.toggle('active', i === currentIndex);
        });

        if (prevBtn) {
            prevBtn.classList.toggle('disabled', currentIndex === 0);
        }
        if (nextBtn) {
            nextBtn.classList.toggle('disabled', currentIndex === total - 1);
        }

        setTimeout(() => {
            isAnimating = false;
        }, 800);
    }

    function next() {
        if (currentIndex < total - 1) goTo(currentIndex + 1);
    }

    function prev() {
        if (currentIndex > 0) goTo(currentIndex - 1);
    }

    // Клик по навигации сверху
    navLinks.forEach((link, index) => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            goTo(index);
        });
    });

    // Клик по навигации в футере
    footerLinks.forEach((link) => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const index = parseInt(link.getAttribute('data-index'));
            if (!isNaN(index)) {
                goTo(index);
            }
        });
    });

    // Кружки
    if (prevBtn) prevBtn.addEventListener('click', prev);
    if (nextBtn) nextBtn.addEventListener('click', next);

    // Точки
    dots.forEach((dot, i) => {
        dot.addEventListener('click', () => goTo(i));
    });

    // Клавиши
    document.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowRight' || e.key === 'ArrowDown') next();
        if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') prev();
    });

    // Свайп
    let touchStartX = 0;
    document.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    });

    document.addEventListener('touchend', (e) => {
        const diff = touchStartX - e.changedTouches[0].screenX;
        if (Math.abs(diff) > 50) {
            diff > 0 ? next() : prev();
        }
    });

    // Инициализация
    goTo(0);
});
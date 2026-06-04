
const links = document.querySelectorAll('aside nav ul a');
const container = document.querySelector('.container');
const sections = document.querySelectorAll('.menu-category');

function updateActive() {
    if (!container) return;

    const scrollPos = container.scrollTop;
    let currentId = '';

    sections.forEach(section => {
        const sectionTop = section.offsetTop;
        const sectionBottom = sectionTop + section.offsetHeight;

        if (scrollPos + 100 >= sectionTop && scrollPos + 100 < sectionBottom) {
            currentId = section.getAttribute('id');
        }
    });

    // Обновляем боковое меню
    links.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href').replace('#', '');
        if (href === currentId) {
            link.classList.add('active');
        }
    });

    // Обновляем sticky-nav
    stickyLinks.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href').replace('#', '');
        if (href === currentId) {
            link.classList.add('active');
        }
    });
}

// Клик по боковому меню
links.forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const id = link.getAttribute('href').replace('#', '');
        const target = document.getElementById(id);

        if (target && container) {
            container.scrollTo({
                top: target.offsetTop - 20,
                behavior: 'smooth'
            });
        }
    });
});



const stickyLinks = document.querySelectorAll('.sticky-nav-link');

stickyLinks.forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const id = link.getAttribute('href').replace('#', '');
        const target = document.getElementById(id);

        if (target) {
            // Вычисляем отступ с учётом высоты sticky-nav
            const stickyNav = document.getElementById('stickyNav');
            const offset = stickyNav ? stickyNav.offsetHeight + 10 : 20;

            window.scrollTo({
                top: target.offsetTop - offset,
                behavior: 'smooth'
            });
        }
    });
});


//ОТСЛЕЖИВАНИЕ СКРОЛЛА СТРАНИЦЫ 
window.addEventListener('scroll', () => {
    const scrollPos = window.scrollY;
    const stickyNav = document.getElementById('stickyNav');
    const navHeight = stickyNav ? stickyNav.offsetHeight : 0;

    let currentId = '';

    sections.forEach(section => {
        const sectionTop = section.offsetTop - navHeight - 20;
        const sectionBottom = sectionTop + section.offsetHeight;

        if (scrollPos >= sectionTop && scrollPos < sectionBottom) {
            currentId = section.getAttribute('id');
        }
    });

    stickyLinks.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href').replace('#', '');
        if (href === currentId) {
            link.classList.add('active');
        }
    });
});



container?.addEventListener('scroll', updateActive);
updateActive();
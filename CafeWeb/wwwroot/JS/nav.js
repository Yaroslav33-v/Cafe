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

    links.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href').replace('#', '');
        if (href === currentId) {
            link.classList.add('active');
        }
    });
}


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

container?.addEventListener('scroll', updateActive);
updateActive();
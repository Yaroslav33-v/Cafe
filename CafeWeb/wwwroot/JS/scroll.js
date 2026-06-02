const scrollBtn = document.getElementById('scrollToTop');


window.addEventListener('scroll', () => {
    if (window.scrollY > 400) {
        scrollBtn.classList.add('visible');
    } else {
        scrollBtn.classList.remove('visible');
    }
});


scrollBtn.addEventListener('click', () => {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
});
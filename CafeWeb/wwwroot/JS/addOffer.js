// Получаем элементы
const modal = document.getElementById('OfferModal');
const openBtn = document.getElementById('openOfferModal');
const closeBtn = document.querySelector('.offer-modal-close');
const form = document.getElementById('originalForm');
const formContainer = document.getElementById('formContainer');
const customSelect = document.querySelector('.custom-select')
const formInput = document.getElementById('form-input');

if (form && formContainer) {
    // Прячем оригинальную форму
    form.style.display = 'none';

    openBtn?.addEventListener('click', () => {
        // Клонируем форму в модальное окно
        const formClone = form.cloneNode(true);
        formClone.style.display = 'flex';
        formClone.style.flexDirection = 'column';
        formClone.style.gap = '12px';
        formContainer.innerHTML = '';
        formContainer.appendChild(formClone);
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
    });

    closeBtn?.addEventListener('click', () => {
        modal.classList.remove('show');
        document.body.style.overflow = '';
    });

    modal?.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.classList.remove('show');
            document.body.style.overflow = '';
        }
    });
}
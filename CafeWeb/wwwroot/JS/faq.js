const tabBtns = document.querySelectorAll('.tab-btn');
const tabContents = document.querySelectorAll('.tab-content');

tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        tabBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        const tabId = btn.dataset.tab;
        tabContents.forEach(content => {
            content.classList.remove('active');
            if (content.id === tabId) {
                content.classList.add('active');
            }
        });
    });
});

// Аккордеон внутри табов
const questions = document.querySelectorAll('.faq-question-modern');

questions.forEach(question => {
    question.addEventListener('click', () => {
        const answer = question.nextElementSibling;
        const isActive = question.classList.contains('active');

        // Закрываем все в текущем табе
        const currentTab = question.closest('.tab-content');
        currentTab.querySelectorAll('.faq-question-modern').forEach(q => {
            q.classList.remove('active');
            q.nextElementSibling.classList.remove('active');
        });

        // Открываем текущий
        if (!isActive) {
            question.classList.add('active');
            answer.classList.add('active');
        }
    });
});
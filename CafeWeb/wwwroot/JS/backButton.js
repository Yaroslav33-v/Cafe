/**
* Универсальная кнопка "Назад"
* Возвращает пользователя на предыдущую страницу
* Если истории нет - перенаправляет на главную
*/
function goBack() {
    // Проверяем, есть ли история переходов
    if (document.referrer && window.history.length > 1) {
        window.history.back();
    } else {
        // Если истории нет - идём на главную
        window.location.href = '/cafe/index';
    }
}

// Альтернативный вариант: сохранять предыдущую страницу в sessionStorage
function goBackSafe(defaultUrl) {
    defaultUrl = defaultUrl || '/cafe/index';

    // Пытаемся вернуться через history
    if (window.history.length > 1) {
        window.history.back();
        return;
    }

    // Если не получилось - проверяем sessionStorage
    const lastPage = sessionStorage.getItem('lastPage');
    if (lastPage) {
        window.location.href = lastPage;
        return;
    }

    // Иначе - на главную
    window.location.href = defaultUrl;
}

// Сохраняем текущую страницу перед уходом
document.addEventListener('DOMContentLoaded', function () {
    // Сохраняем текущий URL как "последнюю страницу"
    sessionStorage.setItem('lastPage', window.location.href);
});
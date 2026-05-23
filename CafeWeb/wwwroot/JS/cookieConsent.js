(function () {
    // Проверяем, есть ли уже сохраненное согласие
    const consentGiven = localStorage.getItem('cookieConsent');

    // Если согласие уже было дано - ничего не показываем
    if (consentGiven !== null) {
        return;
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', showCookieBanner);
    } else {
        showCookieBanner();
    }

    function showCookieBanner() {
        const bannerHTML = `
            <div id="cookie-consent-banner" style="
                position: fixed;
                bottom: 20px;
                left: 20px;
                right: auto;
                max-width: 450px;
                background: #2d2d2d;
                color: white;
                padding: 18px 20px;
                border-radius: 12px;
                box-shadow: 0 4px 15px rgba(0,0,0,0.3);
                z-index: 10000;
                font-family: Arial, sans-serif;
                font-size: 14px;
                line-height: 1.5;
            ">
                <div style="margin-bottom: 15px;">
                    🍪 Мы используем файлы cookie для улучшения работы сайта. 
                    Продолжая использование, вы соглашаетесь с нашей 
                    <a href="/privacy-policy" style="color: #ffa500; text-decoration: underline;">политикой конфиденциальности</a>.
                </div>
                <div style="display: flex; gap: 12px; justify-content: flex-end;">
                    <button id="cookie-reject" style="
                        background: #555;
                        border: none;
                        color: white;
                        padding: 8px 16px;
                        cursor: pointer;
                        border-radius: 6px;
                        font-size: 13px;
                    ">Принимаю только необходимые</button>
                    <button id="cookie-accept" style="
                        background: #4CAF50;
                        border: none;
                        color: white;
                        padding: 8px 20px;
                        cursor: pointer;
                        border-radius: 6px;
                        font-weight: bold;
                        font-size: 13px;
                    ">Принимаю все</button>
                </div>
            </div>
        `;

        // Добавляем баннер на страницу
        document.body.insertAdjacentHTML('beforeend', bannerHTML);

        // Обработчики кнопок
        const acceptBtn = document.getElementById('cookie-accept');
        const rejectBtn = document.getElementById('cookie-reject');
        const banner = document.getElementById('cookie-consent-banner');

        if (acceptBtn) {
            acceptBtn.addEventListener('click', function () {
                localStorage.setItem('cookieConsent', 'accepted');
                if (banner) banner.remove();
            });
        }

        if (rejectBtn) {
            rejectBtn.addEventListener('click', function () {
                localStorage.setItem('cookieConsent', 'rejected');
                if (banner) banner.remove();
            });
        }
    }
})();
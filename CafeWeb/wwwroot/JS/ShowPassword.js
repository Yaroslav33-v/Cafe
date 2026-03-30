document.getElementById('show-password').addEventListener('change', function () {
    const passwordInput = document.getElementById('password-input');
    passwordInput.type = this.checked ? 'text' : 'password';
});
window.submitLoginForm = (action, username, password, returnUrl) => {
    const form = document.createElement('form');
    form.method = 'post';
    form.action = action;

    const userIn = document.createElement('input');
    userIn.type = 'hidden';
    userIn.name = 'username';
    userIn.value = username;
    form.appendChild(userIn);

    const passIn = document.createElement('input');
    passIn.type = 'hidden';
    passIn.name = 'password';
    passIn.value = password;
    form.appendChild(passIn);

    if (returnUrl) {
        const returnUrlIn = document.createElement('input');
        returnUrlIn.type = 'hidden';
        returnUrlIn.name = 'returnUrl';
        returnUrlIn.value = returnUrl;
        form.appendChild(returnUrlIn);
    }

    document.body.appendChild(form);
    form.submit();
};

// Cierra la sesión con un POST que incluye el token antiforgery (el endpoint ya no acepta GET)
window.submitLogoutForm = (action, tokenName, token) => {
    const form = document.createElement('form');
    form.method = 'post';
    form.action = action;

    if (tokenName && token) {
        const tokenIn = document.createElement('input');
        tokenIn.type = 'hidden';
        tokenIn.name = tokenName;
        tokenIn.value = token;
        form.appendChild(tokenIn);
    }

    document.body.appendChild(form);
    form.submit();
};

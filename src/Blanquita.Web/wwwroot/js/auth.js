window.submitLoginForm = (action, username, password) => {
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

    document.body.appendChild(form);
    form.submit();
};

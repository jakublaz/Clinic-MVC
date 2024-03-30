function submitLoginForm() {
    var username = document.getElementById('username').value;
    var password = document.getElementById('password').value;

    // // Check if the username is not shorter than 3 characters
    // if (username.length < 3) {
    //     alert('Username must be at least 3 characters long.');
    //     return; // Stop form submission
    // }

    // // Check if the password meets the minimum length requirement
    // if (password.length < 8) {
    //     alert('Password must be at least 8 characters long.');
    //     return; // Stop form submission
    // }

    // Check if the password contains at least one uppercase letter
    if (!/[A-Z]/.test(password)) {
        alert('Password must contain at least one uppercase letter.');
        return; // Stop form submission
    }

    $.ajax({
        url: '/Account/Login', // Update the URL to match the new controller
        method: 'POST',
        contentType: 'application/x-www-form-urlencoded',
        data: {
            username: username,
            password: password,
            rememberMe: document.getElementById('RememberMe').checked
        },
        success: function (response) {
            if (response.success) {
                // Redirect to the new page upon successful login
                window.location.href = "/Dashboard";
            } else {
                alert('Invalid username or password.');
            }
        },
        error: function (xhr, status, error) {
            console.error('An error occurred during login:', error);
            alert('An error occurred during login:\n' + xhr.responseText);
            console.error(xhr, status, error);
        }
    });
}

function togglePasswordVisibility() {
    var passwordInput = document.getElementById('password');
    var showPasswordButton = document.getElementById('showPassword');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        showPasswordButton.textContent = 'Hide';
    } else {
        passwordInput.type = 'password';
        showPasswordButton.textContent = 'Show';
    }
}

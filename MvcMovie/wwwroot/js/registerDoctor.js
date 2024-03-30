
    function submitRegisterForm() {
        var name = document.getElementById('name').value;
        var surname = document.getElementById('surname').value;
        var specjalization = document.getElementById('specjalization').value;
        var username = document.getElementById('username').value;
        var password = document.getElementById('password').value;
        var confirmPassword = document.getElementById('confirmPassword').value;

        // Check if passwords match
        if (password !== confirmPassword) {
            alert('Passwords do not match.');
            return;
        }

        // Your AJAX call to submit registration data to the server
        $.ajax({
            url: '/Account/Register',
            method: 'POST',
            data: {
                Name: name,
                Surname: surname,
                Specjalization: specjalization,
                Username: username,
                Password: password,
                ConfirmPassword: confirmPassword,
                activated: true
            },
            success: function (response) {
                // Handle the response from the server
                if (response.success) {
                    // Registration successful, redirect to the home page or login page
                    window.location.href = '/Home/Index'; // Change the URL
                } else {
                    // Registration failed, display the error message
                    alert('Registration failed: ' + response.error);
                }
            },
            error: function (xhr, status, error) {
                // Display the detailed error message in the alert
                alert('An error occurred during registration:\n' + xhr.responseText);
            }
        });
    }



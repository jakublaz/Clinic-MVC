document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('click', function(event) {
        if (event.target.classList.contains('add-schedule-button')) {
            const userId = event.target.getAttribute('data-patient-id');
            activateAccount(userId);
        }
    });
});

function activateAccount(userId) {
    console.log('Activating account for user ID: ' + userId);
    $.ajax({
        url: '/Manager/ActivateAccount', // Endpoint URL
        method: 'POST',
        data: { userId: userId },
        success: function(response) {
            if(response.success){
                alert('Account activated successfully!');
            }else{
                alert('Error activating account. Please try again.');
            }
        },  
        error: function(xhr, status, error) {
            // Handle error response here
            console.error(error);
            alert('Error activating account. Please try again.');
        }
    });
}

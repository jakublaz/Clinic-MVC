document.addEventListener('DOMContentLoaded', function () {
    const buttons = document.querySelectorAll('.add-schedule-button');
    const form = document.getElementById('scheduleForm');
    const doctorIdInput = document.getElementById('doctorId');

    buttons.forEach(button => {
        button.addEventListener('click', function (event) {
            const doctorId = this.getAttribute('data-doctor-id');
            doctorIdInput.value = doctorId;
            form.style.display = 'block';
        });
    });
});

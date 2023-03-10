document.querySelector("form").addEventListener("submit", (e) => {
    e.preventDefault();
    fetch('/Home/AddUser', {
    method: 'POST',
    body: formData = new FormData(document.querySelector("form"))
})
    .then(response => response.json())
    .then(data => {
        if (data.message) {
            const errorMessage = document.createElement('div');
            errorMessage.textContent = data.message;
            document.body.appendChild(errorMessage);
        } else if (data.redirectToUrl) {
            window.location.href = data.redirectToUrl;
        }
    })
    .catch(error => {
        console.error('Wystąpił błąd:', error);
    });

    document.querySelector("form").reset();
});
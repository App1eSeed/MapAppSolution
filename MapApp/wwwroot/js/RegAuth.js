var regForm = document.getElementById('RegForm');
var logFrom = document.getElementById('LogForm');

window.onclick = function (event) {
    if (event.target == regForm) {
        regForm.style.display = "none";
    }
    if (event.target == logFrom) {
        logFrom.style.display = "none";
    }
}

function OpneLogForm() {
    document.getElementById('LogForm').style.display = 'block'
    document.getElementById('RegForm').style.display = 'none';
}

function OpneRegForm() {
    document.getElementById('RegForm').style.display = 'block'
    document.getElementById('LogForm').style.display = 'none';
}
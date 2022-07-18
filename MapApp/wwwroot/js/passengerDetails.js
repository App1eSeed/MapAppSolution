let user = {
    email: "",
    fname: "",
    lanme: "",
    phone: "",
    month: "",
    day: "",
    year: ""
}

function updateLocalStorage(e) {
    user[e.name] = e.value;
    localStorage.setItem("user", JSON.stringify(user));
}
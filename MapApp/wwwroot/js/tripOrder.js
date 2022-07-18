$('.dropdown-el').click(function (e) {
    e.preventDefault();
    e.stopPropagation();
    $(this).toggleClass('expanded');
    $('#' + $(e.target).attr('for')).prop('checked', true);
});
$(document).click(function () {
    $('.dropdown-el').removeClass('expanded');
});

var localStorageServices = [];

function updateLocalStorageServices(e,serviceId,serviceName,servicePrice) { 

    if (e.checked) {
        let service = {
            id: serviceId,
            name: serviceName,
            price: servicePrice
        }
        localStorageServices.push(service);
    }
    else {
        localStorageServices = localStorageServices.filter(s => s.id != serviceId);
       
    }
    
    localStorage.setItem("userServices", JSON.stringify(localStorageServices));
    updateCheck();
}

var services = []

function updateLocalStorageSeatNumber(number) {
    
    //console.log(element);
    localStorage.setItem("seatNumber", number);
}

function updateCheck() {
    let busket = document.getElementById("BusketItems");
    let totalPrice = document.getElementById("BusketTotal");

    busketHtml = [];
    busketHtml.push('<div class="BusketItem"><div class="BusketItemDescription" > Ticket(1 passanger)</div><div class="BusketItemPrice">' + parseFloat(selectedRoute["fullPrice"]) + ' hrn.' + '</div></div >');

    let sumPrice = 0;
    for (let service of JSON.parse(localStorage["userServices"])) {

        busketHtml.push('<div class="BusketItem"><div class="BusketItemDescription" > ' + service.name + '</div><div class="BusketItemPrice">' + service.price + ' hrn.' + '</div></div >');
        sumPrice += service.price;

    }

    busket.innerHTML = "";
    for (let html of busketHtml) {
        busket.innerHTML += html;
    }
    sumPrice = parseFloat(sumPrice) + parseFloat(parseFloat(selectedRoute["fullPrice"]));
    localStorage.setItem("sumPrice", sumPrice);
    totalPrice.innerHTML = '<div class="BusketTotalDescription">Total</div><div class="BusketTotalPrice" > ' + sumPrice + ' hrn.' + '</div > ';
    console.log("asd");
}
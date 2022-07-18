let outbandRouteDate = document.getElementById("RouteInfoMainDateOut");
let outbandRouteCityFrom = document.getElementById("RouteInfoMainContentStationOut1");
let outbandRouteCityTo = document.getElementById("RouteInfoMainContentStationOut2");
let outbandRouteTime = document.getElementById("RouteInfoTimeOut");
let selectedRoute = JSON.parse(localStorage.getItem("selectedRoute"));
let busket = document.getElementById("BusketItems");
let totalPrice = document.getElementById("BusketTotal");

outbandRouteDate.innerHTML = selectedRoute["fromCityDate"] + " " + selectedRoute["fromCityTime"] + " – " + selectedRoute["toCityDate"] + " " + selectedRoute["toCityTime"];
outbandRouteCityFrom.innerHTML = selectedRoute["fromCity"] + " Bus Station";
outbandRouteCityTo.innerHTML = selectedRoute["toCity"] + " Bus Station";
outbandRouteTime.innerHTML = "<div>" + selectedRoute["tripDuration"] + " • 0 transfers</div>";
busket.innerHTML = '<div class="BusketItem"><div class="BusketItemDescription" > Ticket(1 passanger)</div><div class="BusketItemPrice">' + parseFloat(selectedRoute["fullPrice"]) + ' hrn.' + '</div></div > '; 
totalPrice.innerHTML = '<div class="BusketTotalDescription">Total</div><div class="BusketTotalPrice" > ' + parseFloat(selectedRoute["fullPrice"]) + ' hrn.' + '</div>';


updateCheck();
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

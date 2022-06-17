
var busMarkers = [];
var buses = L.layerGroup(),
	cities = L.layerGroup();
const map = L.map('map', {
}).setView([49.047968403958926, 33.22724770404179], 7);


L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png', { attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' }) //https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J
	.addTo(map);




for (const routeBlock of routeBlocks) {
	routeBlock.addEventListener("click", drawWay);
}


// zoom the map to the polyline
map.fitBounds(polyBusWayLine.getBounds());




function drawWay() {
	//Получаем параметры с Get запроса
	var url_string = window.location;
	var url = new URL(url_string);
	var fromCity = url.searchParams.get("fromCity");
	var toCity = url.searchParams.get("toCIty");

	
	
	$.post("/Map/GetRouteFromCityToCity", { busId: this.id, fromCity: fromCity, toCity: toCity}, function (result) {

		map.removeLayer(polyBusWayLine);
		console.log(result.pathCoords);
		polyBusWayLine = L.polyline(result.pathCoords).addTo(map);
		//polyBusWayLine.setStyle({
		//	color: "#333",
		//	weigth: 2,
		//	offset: -1
		//});
		map.fitBounds(polyBusWayLine.getBounds());
	});
}



const map = L.map('map', {
}).setView([49.047968403958926, 33.22724770404179], 7);


L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png', { attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' }) //https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J
	.addTo(map);

var markers = [];
var routeBlocks = document.getElementsByClassName("Route");

localStorage.setItem("userServices", markers);

for (const routeBlock of routeBlocks) {
	routeBlock.addEventListener("click", routeClick);
}

var busWay = [];
var polyBusWayLine = L.polyline(busWay).addTo(map);

var beforeRoute;

function routeClick() {
	console.log(routes);
	collapseInfoPanel(this);
	fillLocalStorage(this);	
	drawWay(this);
}

function collapseInfoPanel(route) {
	if (beforeRoute != null) {
		beforeRoute.children[3].style.display = "none";
		beforeRoute.children[4].style.display = "none";
	} 
	console.log(beforeRoute);
	route.children[3].style.display = "flex";
	route.children[4].style.display = "block";

	beforeRoute = route;
}

function fillLocalStorage(route) {

	localStorage.setItem("selectedRoute", JSON.stringify(routes[route.id]));


	//listRoute = {
	//	transportationId: '@routes[0].TransportationId',
	//	fromCity: '@routes[0].CityFrom',
	//	fromCityTime: '@routes[0].DepartTime',
	//	fromCityDate: '@routes[0].DepartDate.ToString("dd MMM yyyy")',
	//	toCity: '@routes[routes.Count-1].CityTo',
	//	toCityTime: '@routes[routes.Count-1].ArrivalTime',
	//	toCityDate: '@routes[routes.Count-1].ArrivalDate.ToString("dd MMM yyyy")',
	//	tripDuration: '@transpDuration',
	//	fullPrice: '@routes.Sum(r => r.Price)',
	//	departDate: '@routes[0].DepartDate.ToString("dd MMM yyyy")',
	//	seatNumber: '',
	//	services: []
 //                   paymentType: ''
	//}
}

function drawWay(route) {
	//Получаем параметры с Get запроса
	var url_string = window.location;
	var url = new URL(url_string);
	var fromCity = url.searchParams.get("fromCity");
	var toCity = url.searchParams.get("toCity");

	
	
	$.post("/Map/GetRouteFromCityToCity", { transportationId: route.id, fromCity: fromCity, toCity: toCity}, function (result) {
		
		console.log(result);
		let busLine = [];
		let counter = 0;

		map.removeLayer(polyBusWayLine);

		//for (var i = 0; i < markers.length; i++) {
		//	markers[i].remove();
		//}
      
		for (let marker of markers) {
			marker.remove();

		}
		markers = [];
		
		for (let city of result) {
			busLine.push([city.latitude, city.longtitude]);						
			
		}

        for (var i = 0; i < result.length; i++) {
			markers[i] = (L.circleMarker([result[i].latitude, result[i].longtitude], {
				fillColor: "#9672D3",
				color: "#f2f3f0",
				weight: 4,
				radius: 7,
				fillOpacity: 1,
				opacity:1,
				smoothFactor: 0
			}).bindTooltip(result[i].city,
				{
					permanent: true,
					direction: 'right'
				}));
        }
		
		for (let marker of markers) {
			marker.addTo(map).bringToFront();
        }
		
		//L.circleMarker(busLine[0], {
		//	fill: "#9672D3",
		//	color:"#9672D3",
		//	weight: 2,
		//	radius: 6,
		//	fillOpacity:1,
		//}).addTo(map);
		
		
		
		polyBusWayLine = L.polyline(busLine).addTo(map).bringToBack();
		
		polyBusWayLine.setStyle({
			color: "#9672D3",
			weigth: 5,
		
			lineJoin: "round",
			lineCap: "round",
			
			opacity:1,
			smoothFactor: 0
		});
		map.fitBounds(polyBusWayLine.getBounds());
	});
}



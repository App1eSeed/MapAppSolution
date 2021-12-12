var busMarkers = [];
var buses = L.layerGroup(),
	cities = L.layerGroup();


const map = L.map('map', {
	layers: [cities, buses]
}).setView([49.047968403958926, 33.22724770404179], 7);


L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png', { attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' }) //https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J
	.addTo(map);


map.on('zoomstart', function (e) {
	updateMap(map.getBounds());
	//console.log(map.getBounds());
	//setMarkersByBounds(map.getBounds())
});

map.on('moveend', throttle(function (e) {
		setMarkersByBounds(map.getBounds());
},2000));

map.on('tileload', function (e) {
	console.log(e);
});
function updateMap(bounds) {

	if (true) {

		$('.my-custom-pin').fadeOut(0);
		map.removeLayer(polyBusWayLine);

		setTimeout(function () {
			$('.my-custom-pin').fadeIn(0);
			map.addLayer(polyBusWayLine);
		}, 1000);
		$.each(map._layers, function (i, marker) {

			//if (marker._latlng < bounds._southWest || marker._latlng > bounds._northEast) {
			//	marker.fadeOut(0);
			//}
//            else {
			//	marker.fadeIn(0);
//            }

				

				
			//marker.setLatLng(marker._latlng);
			//marker.addTo(map);
		});
			
			
	}
}

var tempArr = [];
var routeLines = [];
var icons = [];
var busWay = [];
var polyBusWayLine = L.polyline(busWay).addTo(map);
var cityMarkers = [];
var existingRoutes = [];



function setMarkersByBounds(bounds) {


	$.post("/Home/GetVisibleRoutes", {
		topLat: bounds._northEast.lat,
		topLong: bounds._northEast.lng,
		botLat: bounds._southWest.lat,
		botLong: bounds._southWest.lng,
		time: current.toLocaleTimeString(),
		existingRoutes: existingRoutes
	},
	function (result) {
		console.log("Result");
		console.log(result);


		//console.log("existingRoutes");
		//console.log(existingRoutes);
		if (result.length > 0) {

			$.each(result, function (i, route) {
				$.post("/Home/GetPath", { busId: route.busId, busDepartTime: route.departTime, sequence: route.sequence }, function (pathResult) {
					if (pathResult.pathCoords.length > 0) {

                    
						existingRoutes.push(route.busId);

						let routeLine = L.polyline(pathResult.pathCoords)// Test
						markerColour = colourForBus(route.country);
						
						let markerHtmlStyles = `
							background-color: ${markerColour};
							z-index: 49;
							width: 0.6rem;
							height: 0.6rem;
							display: block;
							position: relative;
							border-radius: 1rem 1rem 1rem 1rem ;
							transform: rotate(45deg);
							border: 1px solid #000000`;

						let icon = L.divIcon({
							className: "my-custom-pin",
							color: markerColour,
							html: `<span style="${markerHtmlStyles}" />`
						});	

						var marker = L.animatedMarker(routeLine.getLatLngs(), {
							
							customId: route.busId,
							sequenceForNext: route.sequence + 1,
							distance: pathResult.speed * scaleArr[scaleIndex],
							interval: 1000,
							currentSpeed: pathResult.speed,
							nextDepartTime: pathResult.nextDepartTime,

							icon: icon,
							autoStart: false,
							onEnd: function () {
								let busId = route.busId;

								$.post("/Home/GetPath", { busId: busId , sequence: this.options.sequenceForNext }, function (result)
								{
									console.log(result);
									getWayToNextCity(result, busId);
								});
							}
						}).on('click', markerOnClick);
						buses.addLayer(marker).addTo(map);
						busMarkers[route.busId] = marker;

						let depTimeInSec = getSecondsByTime(route.departTime);
						let currentTimeInSec = getSecondsByTime(current.toLocaleTimeString());

						setTimeout(function () {

							marker.start();
						
						}, (depTimeInSec - currentTimeInSec) * 1000);//Test
					}
				});
					
					
					
			});				
		}
		});
	console.log(map);
}




//$.ajax({
//	type: "Get",
//	url: "/Home/GetAllCities",
//	dataType: "json",
//	success: function (result) {

//		console.log(result);
//		L.geoJSON(result, {
//			pointToLayer: function (feature, latlng) {
//				let markerColour = colourForBus(null);
//				let markerHtmlStyles = `
//						background-color: ${markerColour};
//						z-index: 47;
//						width: 0.3rem;
//						height: 0.3rem;
//						display: block;
//						position: relative;
//						border-radius: 1rem 1rem 1rem 1rem ;
//						transform: rotate(45deg);
//						border: 1px solid #000000`;

//				return L.marker(latlng, {
//					icon: L.divIcon({
//						//cityId: city.Id,
//						className: "my-custom-pin",
//						color: "#E93724",
//						html: `<span style="${markerHtmlStyles}" />`
//					})
//				});
//			},
//			renderer: L.canvas()
//		});
//			//.addTo(map);

//		//$.each(result, function (i, city) {

//		//	//let myCustomColour = colourForBus(city.name);
//		//	let markerColour = colourForBus(null);
//		//	let markerHtmlStyles = `
//		//				background-color: ${markerColour};
//		//				z-index: 47;
//		//				width: 0.3rem;
//		//				height: 0.3rem;
//		//				display: block;
//		//				position: relative;
//		//				border-radius: 1rem 1rem 1rem 1rem ;
//		//				transform: rotate(45deg);
//		//				border: 1px solid #000000`;

//		//	let marker = new L.marker([city.latitude, city.longtitude], {
//		//		icon: L.divIcon({
//		//			cityId: city.Id,
//		//			className: "my-custom-pin",
//		//			color: "#E93724",
//		//			html: `<span style="${markerHtmlStyles}" />`
//		//		})
//		//	});
//		//	//marker.addTo(map);

//		//	//cities.addLayer(marker).addTo(cities);
//		//	cityMarkers[city.Id] = marker;

//		//});

		

//	},
//	error: function (error) {
//		alert("There was an error posting the data to the server: " + error.responseText);
//	}
//});


function markerOnClick(e) {
	var busId = this.options.customId;
	var color = this.options.icon.options.color;
	console.log(e);
	
	fillInfoPanel(busId,color);
	resizeMarker(e,color)		
	drawWay(color,busId);
}

function fillInfoPanel(busId,color) {
	let loader = document.getElementById("loader");
	let top = document.getElementById("Top");
	let bot = document.getElementById("Bottom")
	if (document.getElementById("InfoPanel").style.display != "block") {
		openInfoPanel();
	}


	loader.style.display = "block";
	top.style.display = "none";
	bot.style.display = "none";
	setTimeout(function () {
		loader.style.display = "none";
		top.style.display = "block";
		bot.style.display = "block";
		if (isShowLessMode) {
			showLessPanel();
		}
		else {
			showMorePanel();
		}
	}, 1000);

	$.post("/Home/FillInfoPanel", { busId: busId }, function (result) {
		setTimeout(function () {
			$("#InfoPanel").html(result);
			document.getElementById("InfoPanelMarker").style.backgroundColor = color;
		}, 200);
	});
}

var prevMarker;
var prevMarkerColor;

function resizeMarker(marker,color) {

    if (marker != prevMarker) {
		marker.sourceTarget._icon.children[0].style = `
				background-color: ${color};
				z-index:48;
				width: 1rem;
				height: 1rem;
				display: block;
				position: relative;
				border-radius: 1rem 1rem 1rem 1rem ;
				transform: rotate(45deg);
				border: 2px solid #000000;
			-webkit-transition: .1s ease-in-out,max-height .1s ease-in-out;
			-moz-transition:.1s ease-in-out,max-height .1s ease-in-out;
			-o-transition:  .1s ease-in-out,max-height .1s ease-in-out;
			transition:  .1s ease-in-out,max-height .1s ease-in-out;`;
    }

	if (prevMarker != null) {
		prevMarker.sourceTarget._icon.children[0].style = `
				background-color: ${prevMarkerColor};
				z-index:49;
				width: 0.6rem;
				height: 0.6rem;
				display: block;
				position: relative;
				border-radius: 1rem 1rem 1rem 1rem ;
				transform: rotate(45deg);
				border: 1px solid #000000;
			-webkit-transition: .1s ease-in-out,max-height .1s ease-in-out;
			-moz-transition:.1s ease-in-out,max-height .1s ease-in-out;
			-o-transition:  .1s ease-in-out,max-height .1s ease-in-out;
			transition:  .1s ease-in-out,max-height .1s ease-in-out;`;

		prevMarker = marker;
		prevMarkerColor = color;
	}
	else {
		prevMarker = marker;
		prevMarkerColor = color;
	}
		
	
}

function colourForBus(country){
	switch (country) {
		case "Ukraine":
			return "#189C6E";
		case "Russia":
			return "#F66F89";
		case "Poland":
			return "#FFC540";
		case "Belarus":
			return "#3a62eb";
		case "Romania":
			return "#e46cca";
		case "Bulgaria":
			return "#9f5e34";
		case "Germany":
			return "#5f228a";
		default:
			return "#E93724";
    }
		
}

function drawWay(color,busId) {
	$.post("/Home/GetFullRoute", { busId: busId }, function (result) {

		map.removeLayer(polyBusWayLine);
			
		polyBusWayLine = L.polyline(result.pathCoords).addTo(map);
		polyBusWayLine.setStyle({
			color: color,
			weigth: 2,
			offset: -1
		});
			
	});
}

function getWayToNextCity(pathResult, busId) {
	let marker = busMarkers[busId];
	console.log(pathResult);
	if (pathResult.pathCoords.length > 0) {
	


	var currentTimeInSec = getSecondsByTime(current.toLocaleTimeString());
	console.log("currentTimeInSec");
	console.log(currentTimeInSec);
	console.log(current.toLocaleTimeString());

	var departTimeInSec = getSecondsByTime(marker.options.nextDepartTime);
	console.log("nextDepartTime");
	console.log(marker.options.nextDepartTime);
	console.log("Compare");
	console.log(currentTimeInSec > departTimeInSec ? (currentTimeInSec - departTimeInSec > 75000)
			? (departTimeInSec + 86400 - currentTimeInSec) * 1000
			: (departTimeInSec + 300 - currentTimeInSec) * 1000
		: (departTimeInSec - currentTimeInSec) * 1000);

	
		marker.stop();
		marker.options.sequenceForNext = marker.options.sequenceForNext + 1;
		marker.options.nextDepartTime = pathResult.nextDepartTime;
		marker.initialize(L.polyline(pathResult.pathCoords).getLatLngs(), marker.options);

		setTimeout(function () {

			marker.start();
		}, currentTimeInSec > departTimeInSec ? (currentTimeInSec - departTimeInSec > 75000)
				? (departTimeInSec + 86400 - currentTimeInSec) * 1000
				: (departTimeInSec + 300 - currentTimeInSec) * 1000
			: (departTimeInSec - currentTimeInSec) * 1000); 

			
	}
	else {
		$(marker._shadow).fadeOut();
		$(marker._icon).fadeOut(3000, function () {
			buses.removeLayer(marker);
			delete busMarkers[busId];

			var index = existingRoutes.indexOf(busId);
			if (index !== -1) {
				existingRoutes.splice(index, 1);
			};
		});
		closeInfoPanel();
		
	}
}

function getSecondsByTime(time) {
	a = time.split(':');

	return (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]);
}

function throttle(callback, limit) {
	var waiting = false;
	return function () {
		if (!waiting) {
			callback.apply(this, arguments);
			waiting = true;
			setTimeout(function () {
				waiting = false;
			}, limit);
		}
	}
}

//});

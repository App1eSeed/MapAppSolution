//import 'ol/ol.css';
//import Feature from 'ol/Feature';
//import Map from 'ol/Map';
//import Point from 'ol/geom/Point';
//import Polyline from 'ol/format/Polyline';
//import VectorSource from 'ol/source/Vector';
//import View from 'ol/View';
//import XYZ from 'ol/source/XYZ';
//import {
//    Circle as CircleStyle,
//    Fill,
//    Icon,
//    Stroke,
//    Style,
//} from 'ol/style';
//import { Tile as TileLayer, Vector as VectorLayer } from 'ol/layer';
//import { getVectorContext } from 'ol/render';


//const key = 'Get your own API key at https://www.maptiler.com/cloud/';
const attributions =
    '<a href="https://www.maptiler.com/copyright/" target="_blank">&copy; MapTiler</a> ' +
    '<a href="https://www.openstreetmap.org/copyright" target="_blank">&copy; OpenStreetMap contributors</a>';
ol.proj.useGeographic();


const map = new ol.Map({
    target: 'map',
    view: new ol.View({
		center: [33.22724770404179, 49.047968403958926],
        zoom: 4,
        minZoom: 2,
        maxZoom: 19,
    }),
    layers: [
        new ol.layer.Tile({
            source: new ol.source.XYZ({
                attributions: attributions,
                url: "https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J",
                tileSize: 512,
            }),
        })  
    ],
});


map.on('moveend', function () {
	console.log(map.getView().getCenter());
	setMarkersByCenter(map.getView().getCenter(),11);

});


//'https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png'

var cities;
var citiesSource;

//$.ajax({
//	type: "Get",
//	url: "/Home/GetAllCities",
//	dataType: "json",
//	success: function (result) {
//		var points = [];
//		var points1 = [];
//		$.each(result, function (i, city) {
//			//points.push(new ol.Feature(new ol.geom.Point(city.geometry.coordinates)));
//			const startMarker = new ol.Feature({
//				type: 'icon',
//				geometry: new ol.geom.Point(city.geometry.coordinates),
//			});
//			const position = startMarker.getGeometry().clone();
//			points.push(new ol.Feature({
//				type: 'geoMarker',
//				geometry: position
//			}));
//			points1.push(new ol.Feature({
//				type: 'geoMarker',
//				geometry: position
//			}))
			
//		});
//		citiesSource = new ol.source.Vector({
//			features: points,
//		});

//		cities = new ol.layer.Vector({
//			source: citiesSource,
//			style:  new ol.style.Style({
//				image: new ol.style.Circle({
//					radius: 4,
//					fill: new ol.style.Fill({ color: 'black' }),
//					stroke: new ol.style.Stroke({
//						color: 'white',
//						width: 1,
//					}),
//				}),
//			}),
//		});

//		setInterval(function () {



//			points.forEach(function (city) {
//				city.getGeometry().setCoordinates([getRandomArbitrary(-85, 85), getRandomArbitrary(-180, 180)]);
        		
//			});
//			console.log(map.getView().zoom);
//			citiesSource = new ol.source.Vector({
//				features: points,
//			});

			

//		}, 5000);


		
//		map.addLayer(cities);

//	},
//	error: function (error) {
//		alert("There was an error posting the data to the server: " + error.responseText);
//	}
//});

console.log(map);
function getRandomArbitrary(min, max) {
	return Math.random() * (max - min) + min;
}


function moveFeature(event) {
	const speed = Number(speedInput.value);
	const time = event.frameState.time;
	const elapsedTime = time - lastTime;
	distance = (distance + (speed * elapsedTime) / 1e6) % 2;
	lastTime = time;

	const currentCoordinate = route.getCoordinateAt(
		distance > 1 ? 2 - distance : distance
	);
	position.setCoordinates(currentCoordinate);
	const vectorContext = getVectorContext(event);
	vectorContext.setStyle(styles.geoMarker);
	vectorContext.drawGeometry(position);
	// tell OpenLayers to continue the postrender animation
	map.render();
}


var existingRoutes = [];
console.log(map);
var busSource;
var layers = [];

var buses = new ol.layer.Vector({
	style: new ol.style.Style({
		image: new ol.style.Circle({
			radius: 4,
			fill: new ol.style.Fill({ color: 'black' }),
			stroke: new ol.style.Stroke({
				color: 'white',
				width: 1,
			}),
		}),
	}),
});

function setMarkersByCenter(center,zoom) {


	$.post("/Home/GetNewRoutesStep", {
		centerLat: center[1],
		centerLng: center[0],
		time: current.toLocaleTimeString(),
		existingRoutes: existingRoutes
	},
		function (result) {
			console.log("Result");
			console.log(result);


			result.forEach(route => function () {
				if (route.CurrentLatLng.length > 0) {

					if (existingRoutes[route.busId] == null) {
						//const startMarker = new ol.Feature({
						//	type: 'icon',
						//	geometry: new ol.geom.Point(route.latLng),
						//});
						//const position = startMarker.getGeometry().clone();

						var feature = new ol.Feature({
							type: 'geoMarker',
							geometry: route.latLng,
							id: route.busId
						});

						points.push(feature);


						existingRoutes[route.busId] = feature;

					}
					else {

						existingRoutes[route.busId].value.getGeometry().setCoordinates(route.latLng);
					}
				}
			});

			busSource = new ol.source.Vector({
				features: existingRoutes.values,
			});
			
			buses.setSource(busSource);



							
				//existingRoutes.push(route.busId);




				let routeLine = L.polyline(pathResult.pathCoords);
				markerColour = colourForBus(route.country);

							

				//setTimeout(function () {

				//	marker.start();

				//}, (depTimeInSec - currentTimeInSec) * 1000);//Test

	});
}




function startAnimation() {
	animating = true;
	lastTime = Date.now();
	startButton.textContent = 'Stop Animation';
	vectorLayer.on('postrender', moveFeature);
	// hide geoMarker and trigger map render through change event
	geoMarker.setGeometry(null);
}


function colourForBus(country) {
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
		case "Lithuania":
			return "#E93724";
		case "Moldova":
			return "#E93724";
		case "Czechia":
			return "#E93724";
		case "Slovakia":
			return "#E93724";
		case "Austria":
			return "#E93724";
		case "Hungary":
			return "#E93724";
		default:
			return "#E93724";
	}

}





//Add marker code
//var markers = new ol.layer.Vector({
//	source: new ol.source.Vector(),
//	style: new ol.style.Style({
//		image: new ol.style.Icon({
//			anchor: [0.5, 1],
//			src: 'marker.png'
//		})
//	})
//});
//map.addLayer(markers);

//var marker = new ol.Feature(new ol.geom.Point(ol.proj.fromLonLat([106.8478695, -6.1568562])));
//markers.getSource().addFeature(marker);
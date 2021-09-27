//const markers = [];

//$(document).ready(function () {
	const map = L.map('map').setView([49.047968403958926, 33.22724770404179], 7);
	
//const { ajax } = require("jquery");

	L.tileLayer('https://api.maptiler.com/maps/basic/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J', { attribution: 'OSM' }) //https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J
		.addTo(map);

	map.on('zoomstart', function (e) {
		updateMap(map.getBounds());
		//console.log(map.getBounds());
		//setMarkersByBounds(map.getBounds())
	});

	map.on('moveend', function (e) {
		//console.log(map.getBounds());
		setMarkersByBounds(map.getBounds());
		console.log(map);
	});


	console.log(map);

	function updateMap(bounds) {



		if (true) {
			$.each(map._layers, function (i, marker) {

				//if (marker._latlng < bounds._southWest || marker._latlng > bounds._northEast) {
				//	marker.fadeOut(0);
				//}
    //            else {
				//	marker.fadeIn(0);
    //            }

				$('.my-custom-pin').fadeOut(0);
				map.removeLayer(polyBusWayLine);

				setTimeout(function () {
					$('.my-custom-pin').fadeIn(0);
					map.addLayer(polyBusWayLine);
				}, 1000);

				
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
		dateTime: localTime.toLocaleTimeString(),
		existingRoutes: existingRoutes
	},
		function (result) {
			//console.log("Result");
			//console.log(result);


			//console.log("existingRoutes");
			//console.log(existingRoutes);
            if (result.length > 0) {
				$.each(result, function (i, route) {
					$.post("/Home/GetPath", { busId: route.busId }, function (pathResult) {
						existingRoutes.push(route.busId);

						let routeLine = L.polyline(pathResult)
						let myCustomColour = colourForBus(route.country);

						const markerHtmlStyles = `
						  background-color: ${myCustomColour};
							z-index:49;
						  width: 0.6rem;
						  height: 0.6rem;
						  display: block;
						  position: relative;
						  border-radius: 1rem 1rem 1rem 1rem ;
						  transform: rotate(45deg);
						  border: 1px solid #000000`;


						let icon = L.divIcon({
							className: "my-custom-pin",
							color: myCustomColour,
							html: `<span style="${markerHtmlStyles}" />`
						});




						var marker = L.animatedMarker(routeLine.getLatLngs(), {
							customId: route.busId,

							distance: 20,
							interval: 1000,

							icon: icon,
							autoStart: false,
							onEnd: function () {
								$(this._shadow).fadeOut();
								$(this._icon).fadeOut(3000, function () {
									map.removeLayer(this);
								});
							}
						}).on('click', markerOnClick);;

						map.addLayer(marker);
						//markers.push(marker);

						marker.start();
					});
				});				
            }
		});
	}
	///////////////////////////////////////////////////////////////////////////////////////////////
	/*$.ajax({
		type: "Get",//Post???
		url: "/Home/GetAllPaths",
		dataType: "json",
		success: function (result) {

			//console.log(result);
			for (var i = 0; i < result.length; i++) {
				tempArr.push([]);
            }

			$.each(result, function (i, routeLine) {
				$.each(result[i], function (j, routeLine) {
					tempArr[i].push([routeLine.longtitude, routeLine.latitude]);
				});

				

				routeLines.push(L.polyline(tempArr[i]));
				let myCustomColour = colourForBus(result[i][0].country);

				const markerHtmlStyles = `
				  background-color: ${myCustomColour};
					z-index:49;
				  width: 0.6rem;
				  height: 0.6rem;
				  display: block;
				  position: relative;
				  border-radius: 1rem 1rem 1rem 1rem ;
				  transform: rotate(45deg);
				  border: 1px solid #000000`;


				 icons.push(L.divIcon({
					 className: "my-custom-pin",
					 color: myCustomColour,
					 html: `<span style="${markerHtmlStyles}" />`
				}));
			});

			console.log("routeLines11111");
			console.log(routeLines);
		

			$.each(routeLines, function (i, routeLine) {
				var marker = L.animatedMarker(routeLine.getLatLngs(), {
					customId: result[i][0].busId,

					distance: 80,					
					interval: 1000,

					icon: icons[i],
					autoStart: false,
					onEnd: function () {
						$(this._shadow).fadeOut();
						$(this._icon).fadeOut(3000, function () {
							map.removeLayer(this);
						});
					}
				}).on('click', markerOnClick);;

				map.addLayer(marker);
				markers.push(marker);
			});


			$.each(markers, function (i, marker) {
				marker.start();
			
			});

		},
		error: function (error) {
			alert("There was an error posting the data to the server: " + error.responseText);
		}


	});*/

	

	$.ajax({
		type: "Get",
		url: "/Home/GetAllCities",
		dataType: "json",
		success: function (result) {

			$.each(result, function (i, city) {

				//let myCustomColour = colourForBus(city.name);

				const markerHtmlStyles = `
				  background-color: #E93724;
				  width: 0.3rem;
				  height: 0.3rem;
				  display: block;
				  position: relative;
				  border-radius: 1rem 1rem 1rem 1rem ;
				  transform: rotate(45deg);
				  border: 1px solid #000000`;

				let marker = new L.marker([city.latitude, city.longtitude], {
					icon: L.divIcon({
						cityId: city.Id,
						className: "my-custom-pin",
						color: "#E93724",
						html: `<span style="${markerHtmlStyles}" />`
					})
				}).addTo(map);

				cityMarkers.push(marker);
			});
		},
		error: function (error) {
			alert("There was an error posting the data to the server: " + error.responseText);
		}
	});

	

	function markerOnClick(e) {
		var busId = this.options.customId;
		var color = this.options.icon.options.color;

		$.post("/Home/GetBusInfo", { busId: busId }, function (result) {

			openInfoPanel(e, result);
		});
		
		resizeMarker(e,color)		
		drawWay(color,e);
	}

	var prevMarker;
	var prevMarkerColor;

	function resizeMarker(marker,color) {

		if (prevMarker != null) {
			prevMarker.style = `
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
			prevMarker = marker.sourceTarget._icon.children[0];
			prevMarkerColor = color;
		}
		else {
			prevMarker = marker.sourceTarget._icon.children[0];
			prevMarkerColor = color;
		}

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

	function colourForBus(country){
		switch (country) {
			case "Ukraine":
				return "#189C6E";
			case "Russia":
				return "#F66F89";
			case "Poland":
				return "#FFC540";
			default:
				return "#E93724";
        }
		
	}

	function drawWay(color,marker) {

		map.removeLayer(polyBusWayLine);
		polyBusWayLine = L.polyline(marker.sourceTarget._latlngs).addTo(map);
		polyBusWayLine.setStyle({
			color: color,
			weigth: 2
		});
		//$.post("/Home/GetWay", { busId: busId }, function (result) {

		//	map.removeLayer(polyBusWayLine);
		//	busWay.length = 0;
		//	$.each(result, function (i, coordinate) {
		//		busWay.push([coordinate.longtitude, coordinate.latitude]);
		//	});

			
		//});
	}

	

//});

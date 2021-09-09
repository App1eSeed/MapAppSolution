﻿$(document).ready(function () {
	var map = L.map('map').setView([49.047968403958926, 33.22724770404179], 7);

//const { ajax } = require("jquery");

	L.tileLayer('https://api.maptiler.com/maps/voyager/{z}/{x}/{y}.png?key=Wz0I5fVrU6CoXb3ZCY4J', { attribution: 'OSM' })
	.addTo(map);

////var line = L.polyline([[40.68510, -73.94136], [40.68576, -73.94149], [40.68649, -73.94165]]);
////map.addLayer(line);

////var animatedMarker = L.animatedMarker(line.getLatLngs());
////map.addLayer(animatedMarker)



//mapquest.js
//var map = L.mapquest.key = 'iVOoDHSx5Ykdj4sIKnWbkmO2SgjbCOBI';

////'map' refers to a <div> element with the ID map
//L.mapquest.map('map', {
//    center: [49.047968403958926, 33.22724770404179],
//    layers: L.mapquest.tileLayer('map'),
//    zoom: 12
//});
//var routeLines = [
//	L.polyline([[40.68510, -73.94136], [40.68576, -73.94149], [40.68649, -73.94165], [40.68722, -73.94178], [40.68795, -73.94193], [40.68869, -73.94207], [40.68942, -73.94223], [40.69016, -73.94236], [40.69089, -73.94251], [40.69162, -73.94266], [40.69234, -73.94281], [40.69309, -73.94295], [40.69337, -73.94301], [40.69382, -73.94310], [40.69455, -73.94324], [40.69527, -73.94339], [40.69603, -73.94353], [40.69822, -73.94394], [40.69897, -73.94409], [40.69968, -73.94424], [40.70042, -73.94438], [40.70053, -73.94440], [40.70109, -73.94501], [40.70165, -73.94564], [40.70221, -73.94627], [40.70277, -73.94690], [40.70335, -73.94753], [40.70388, -73.94814], [40.70407, -73.94779], [40.70436, -73.94781], [40.70544, -73.94798], [40.70685, -73.94819], [40.70759, -73.94830], [40.70830, -73.94842], [40.70901, -73.94854], [40.70879, -73.95076], [40.70914, -73.95083], [40.70971, -73.95236], [40.71026, -73.95385], [40.71059, -73.95473], [40.71055, -73.95509], [40.71058, -73.95551], [40.71065, -73.95625], [40.71065, -73.95647], [40.71051, -73.95709], [40.71044, -73.95736], [40.71035, -73.95833], [40.71032, -73.95875], [40.71078, -73.95994], [40.71103, -73.96058], [40.71047, -73.96094], [40.71041, -73.96113], [40.71061, -73.96176], [40.71115, -73.96354], [40.71162, -73.96508], [40.71217, -73.96703], [40.71215, -73.96730], [40.71549, -73.97831], [40.71544, -73.97834], [40.71757, -73.98535], [40.71770, -73.98579], [40.71783, -73.98572], [40.71908, -73.98507], [40.71933, -73.98591], [40.71958, -73.98675], [40.71983, -73.98754], [40.72007, -73.98835], [40.72030, -73.98911], [40.72046, -73.98962], [40.72052, -73.98985], [40.72076, -73.99063], [40.72102, -73.99150], [40.72115, -73.99195], [40.72124, -73.99224], [40.72139, -73.99273], [40.72161, -73.99346], [40.72234, -73.99320], [40.72238, -73.99332], [40.72272, -73.99416], [40.72303, -73.99490], [40.72336, -73.99570], [40.72368, -73.99647], [40.72388, -73.99695], [40.72423, -73.99779], [40.72462, -73.99858], [40.72500, -73.99934], [40.72538, -74.00010], [40.72576, -74.00089], [40.72611, -74.00159], [40.72649, -74.00236], [40.72687, -74.00312], [40.72690, -74.00321], [40.72694, -74.00327], [40.72695, -74.00337], [40.72695, -74.00344], [40.72766, -74.00316], [40.72831, -74.00308], [40.72838, -74.00309], [40.72871, -74.00330], [40.72934, -74.00365], [40.72987, -74.00397], [40.73044, -74.00430], [40.73071, -74.00446], [40.73100, -74.00462], [40.73154, -74.00493], [40.73135, -74.00553], [40.73162, -74.00570], [40.73158, -74.00578], [40.73163, -74.00632]]),
//	L.polyline([[40.73271, -73.99818], [40.73261, -73.99828], [40.73221, -73.99861], [40.73192, -73.99849], [40.73118, -73.99819], [40.73096, -73.99773], [40.73093, -73.99775], [40.73088, -73.99776], [40.73078, -73.99774], [40.73071, -73.99766], [40.73049, -73.99788], [40.73028, -73.99749], [40.72987, -73.99667], [40.72955, -73.99655], [40.72918, -73.99582], [40.72881, -73.99506], [40.72840, -73.99425], [40.72815, -73.99372], [40.72786, -73.99314], [40.72711, -73.99161], [40.72705, -73.99148], [40.72618, -73.98942], [40.72558, -73.98987], [40.72518, -73.99016], [40.72491, -73.99034], [40.72426, -73.99082], [40.72402, -73.99103], [40.72365, -73.99101], [40.72240, -73.99164], [40.72218, -73.99091], [40.72191, -73.99004], [40.72167, -73.98924], [40.72161, -73.98903], [40.72146, -73.98852], [40.72123, -73.98776], [40.72097, -73.98695], [40.72074, -73.98615], [40.72048, -73.98531], [40.71933, -73.98591], [40.71808, -73.98655], [40.71797, -73.98660], [40.71770, -73.98579], [40.71757, -73.98535], [40.71544, -73.97834], [40.71538, -73.97837], [40.71203, -73.96735], [40.71186, -73.96721], [40.71125, -73.96529], [40.71092, -73.96426], [40.71074, -73.96366], [40.71121, -73.96337], [40.71190, -73.96294], [40.71244, -73.96446], [40.71307, -73.96408], [40.71382, -73.96360], [40.71445, -73.96320], [40.71510, -73.96278], [40.71558, -73.96229], [40.71593, -73.96191], [40.71638, -73.96142], [40.71697, -73.96079], [40.71752, -73.96019], [40.71808, -73.95960], [40.71862, -73.95900], [40.71904, -73.95856]]),
//	L.polyline([[40.72022, -74.00005], [40.72043, -73.99986], [40.72142, -73.99904], [40.72265, -73.99798], [40.72388, -73.99695], [40.72466, -73.99631], [40.72520, -73.99584], [40.72508, -73.99533], [40.72585, -73.99471], [40.72593, -73.99464], [40.72601, -73.99458], [40.72626, -73.99440], [40.72664, -73.99412], [40.72728, -73.99364], [40.72786, -73.99314], [40.72988, -73.99141], [40.72979, -73.99074], [40.72974, -73.99036], [40.72955, -73.98991], [40.72988, -73.98966], [40.72990, -73.98891], [40.72999, -73.98736], [40.72977, -73.98682], [40.72878, -73.98443], [40.72937, -73.98400], [40.73001, -73.98353], [40.73064, -73.98306], [40.73135, -73.98255], [40.73202, -73.98206], [40.73221, -73.98192], [40.73265, -73.98160], [40.73325, -73.98115], [40.73382, -73.98073], [40.73406, -73.98056], [40.73442, -73.98030], [40.73498, -73.97990], [40.73558, -73.97943], [40.73619, -73.97899], [40.73687, -73.97851], [40.73755, -73.97805], [40.73816, -73.97760], [40.73879, -73.97715], [40.73941, -73.97670], [40.74002, -73.97625], [40.74013, -73.97616], [40.74064, -73.97581], [40.74127, -73.97534], [40.74145, -73.97521], [40.74217, -73.97467], [40.74309, -73.97402], [40.74378, -73.97351], [40.74445, -73.97303], [40.74506, -73.97257], [40.74568, -73.97212], [40.74629, -73.97167], [40.74692, -73.97122], [40.74751, -73.97073], [40.74783, -73.97049], [40.74865, -73.96990], [40.75200, -73.96746], [40.75283, -73.96690], [40.75312, -73.96669], [40.75324, -73.96661], [40.75334, -73.96654], [40.75387, -73.96615], [40.75450, -73.96569], [40.75513, -73.96524], [40.75575, -73.96478], [40.75638, -73.96432], [40.75700, -73.96387], [40.75763, -73.96341], [40.75831, -73.96292], [40.75898, -73.96243], [40.75961, -73.96197], [40.76023, -73.96152], [40.76023, -73.96163], [40.76071, -73.96278], [40.76068, -73.96280], [40.76061, -73.96276], [40.76017, -73.96168], [40.75956, -73.96033], [40.75797, -73.95673], [40.75521, -73.95040], [40.75353, -73.94654], [40.75148, -73.94183], [40.75105, -73.94082], [40.75106, -73.94068], [40.75108, -73.94075]])
	//];
	var tempArr = [];
	var routeLines = [];
	var markers = [];


    //$.post("/Home/GetAllPaths", function (result) {
    //    console.log("äsdsdfäsd");
    //    console.log(result);
    //});
	
	$.ajax({
		type: "Get",//Post???
		url: "/Home/GetAllPaths",
		dataType: "json",
		success: function (result) {
			for (var i = 0; i < result.length; i++) {
				tempArr.push([]);
            }

			$.each(result, function (i, routeLine) {
				$.each(result[i], function (j, routeLine) {
					tempArr[i].push([routeLine.longtitude, routeLine.latitude]);
				});
				
				routeLines.push(L.polyline(tempArr[i]));
			});
			console.log(routeLines);

			var myCustomColour = 'green';

			const markerHtmlStyles = `
			  background-color: ${myCustomColour};
			  width: 0.6rem;
			  height: 0.6rem;
			  display: block;
			  position: relative;
			  border-radius: 1rem 1rem 1rem 1rem ;
			  transform: rotate(45deg);
			  border: 1px solid #000000`;


			var icon = L.divIcon({
				customId: "",
				className: "my-custom-pin",
				html: `<span style="${markerHtmlStyles}" />`
			});

			$.each(routeLines, function (i, routeLine) {
				var marker = L.animatedMarker(routeLine.getLatLngs(), {
					customId: "Enter Id here",

					//distance: 80,					
					//interval: 1000,

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
				markers.push(marker);
			});

			$.each(markers, function (i, marker) {
				marker.start();
			});

		},
		error: function (error) {
			alert("There was an error posting the data to the server: " + error.responseText);
		}
	});


	function markerOnClick(e) {
		var customId = this.options.customId;
		console.log(customId);
	}



});


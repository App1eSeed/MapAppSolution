
const timeBlock = document.getElementById("currentTime");
const localTimeBlock = document.getElementById("currentLocalTime");
const scaleLable = document.getElementById("ScaleInfo");

    
var scaleArr = [0, 0.1, 0.2, 0.5, 1, 2, 3, 6, 12, 20, 30];
var scaleIndex = 4;

var infoPanelHeight = "17rem";

    
let current = new Date();
timeBlock.innerHTML = current.toLocaleTimeString();

var localTime = current;
localTimeBlock.innerHTML = "Local time: "+ current.toLocaleTimeString()

var normalClock = setInterval(()=>{
    current = new Date();
    timeBlock.innerHTML = current.toLocaleTimeString();                      
},1000);

var localClock = setInterval(() => {
    localTime.setSeconds(localTime.getSeconds() + 1);
    localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
}, 1000);
    

function makeControlChanges(action) {

    switch (action) {
        case 1:
            
            if (scaleIndex < scaleArr.length) {
                scaleIndex += 1;
                console.log("adsfsadf");
                console.log(scaleArr.length);
                break;
            }
            else {
                break;
            }
        case -1:
            if (scaleIndex == 0) {
                scaleIndex -= 1;
                break;
            }
            else {
                scaleIndex -= 1;
                break;
            }        
        case 0:
            scaleIndex = 4;
            break;
        default:
            break;
    }

    scaleLable.innerHTML = scaleArr[scaleIndex] + "x";
    setTimeout(clearInterval(localClock), 0);
    localClock = setInterval(() => {
        localTime.setSeconds(localTime.getSeconds() + 1);
        localTimeBlock.innerHTML = "Local time: " + localTime.toLocaleTimeString();
    }, 1000 / scaleArr[scaleIndex]);

    setTimeout(clearInterval(normalClock), 0);
    normalClock = setInterval(() => {
        current = new Date();
        timeBlock.innerHTML = current.toLocaleTimeString();
    }, 1000);

    $.each(buses._layers, function (i, marker) {
        marker.options.distance = marker.options.currentSpeed * scaleArr[scaleIndex];
    });
}

function Start(){
    document.getElementById("StartButton").style.display = "none";
    document.getElementById("PauseButton").style.display = "flex";
    scaleLable.innerHTML = scaleArr[scaleIndex] +"x";
    localClock = setInterval(() => {
            localTime.setSeconds(localTime.getSeconds() + 1);
            localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
    }, 1000 / scaleArr[scaleIndex]);

    setTimeout(clearInterval(normalClock),0);
    normalClock = setInterval(()=>{
        current = new Date();
        timeBlock.innerHTML = current.toLocaleTimeString();                      
    }, 1000);

    $.each(busMarkers, function (i, marker) {
        marker.value.start();
    });
}

function Pause(){
    document.getElementById("PauseButton").style.display = "none";
    document.getElementById("StartButton").style.display = "flex";
    scaleLable.innerHTML = "0x";
    setTimeout(clearInterval(localClock), 0);

    $.each(busMarkers, function (i, marker) {
        marker.value.stop();
    });
}

function openInfoPanel(marker, info) {  
    document.getElementById("InfoPanel").style.display = "block";

    setTimeout(function () {
        document.getElementById("InfoPanel").style.height = infoPanelHeight;
    }, 200);

}

function closeInfoPanel() {
    document.getElementById("InfoPanel").style.height = "0rem";
    resizeMarker(prevMarker, prevMarkerColor);

    map.removeLayer(polyBusWayLine);
    polyBusWayLine = L.polyline(busWay).addTo(map);

    setTimeout(function () {
        document.getElementById("InfoPanel").style.display = "none";
    }, 200);

}
var isShowLessMode = false;
function showLessPanel() {
    isShowLessMode = true;
    document.getElementById("InfoPanel").style.height = "5.5rem";
    document.getElementById("ShowLessButton").style.display = "none";
    document.getElementById("ShowMoreButton").style.display = "block";
    document.getElementById("Bottom").style.display = "none";
}

function showMorePanel() {
    isShowLessMode = false;
    document.getElementById("InfoPanel").style.height = infoPanelHeight;
    document.getElementById("ShowLessButton").style.display = "block";
    document.getElementById("ShowMoreButton").style.display = "none";
    document.getElementById("Bottom").style.display = "block";
}

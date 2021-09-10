
    const timeBlock = document.getElementById("currentTime");
    const localTimeBlock = document.getElementById("currentLocalTime");
    const scaleLable = document.getElementById("ScaleInfo");
    const speed = 80;
    var scale = 1;

    
    //Footer menu
    
    let current = new Date();
    timeBlock.innerHTML = current.toLocaleTimeString();

    var localTime = current;
    localTimeBlock.innerHTML = "Local time: "+ current.toLocaleTimeString()

    var normalClock = setInterval(()=>{
        let current = new Date();
        timeBlock.innerHTML = current.toLocaleTimeString();                      
    },1000);

    var localClock = setInterval(() => {
        localTime.setSeconds(localTime.getSeconds() + 1);
        localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
    }, 1000);
    

    
    function stepForward(){
        scale +=1;
        scaleLable.innerHTML = scale +"x";
        setTimeout(clearInterval(localClock),0);
        localClock = setInterval(() => {
            localTime.setSeconds(localTime.getSeconds() + 1);
            localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
        }, 1000/scale);

        setTimeout(clearInterval(normalClock),0);
        normalClock = setInterval(()=>{
            let current = new Date();
            timeBlock.innerHTML = current.toLocaleTimeString();                      
        }, 1000);

        $.each(markers, function (i, marker) {
            marker.options.distance = speed * scale;
        });

        console.log(markers);
    };

    function stepBackward(){
        if (scale > 1) {
            scale -=1;
            scaleLable.innerHTML = scale +"x";
            setTimeout(clearInterval(localClock),0);
            localClock = setInterval(() => {
                localTime.setSeconds(localTime.getSeconds() + 1);
                localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
            }, 1000/scale);

            setTimeout(clearInterval(normalClock),0);
            normalClock = setInterval(()=>{
                let current = new Date();
                timeBlock.innerHTML = current.toLocaleTimeString();                      
            }, 1000);

            $.each(markers, function (i, marker) {
                marker.options.distance = speed * scale;
                
            });
        }           
    };

    function resetScale(){
        scale = 1;
        scaleLable.innerHTML = scale +"x";
        setTimeout(clearInterval(localClock),0);
        setTimeout(clearInterval(normalClock),0);
            
        localTimeBlock.innerHTML = "Local time: "+ current.toLocaleTimeString();
            
        localClock = setInterval(() => {
            localTime.setSeconds(localTime.getSeconds() + 1);
            localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
        }, 1000);

        normalClock = setInterval(()=>{
            let current = new Date();
            timeBlock.innerHTML = current.toLocaleTimeString();                      
        }, 1000);

        $.each(markers, function (i, marker) {
            marker.options.distance = speed * scale;
        });
    };

    function Start(){
        document.getElementById("StartButton").style.display = "none";
        document.getElementById("PauseButton").style.display = "flex";
        scaleLable.innerHTML = scale +"x";
        localClock = setInterval(() => {
                localTime.setSeconds(localTime.getSeconds() + 1);
                localTimeBlock.innerHTML ="Local time: "+ localTime.toLocaleTimeString();
        }, 1000/scale);

        setTimeout(clearInterval(normalClock),0);
        normalClock = setInterval(()=>{
            let current = new Date();
            timeBlock.innerHTML = current.toLocaleTimeString();                      
        }, 1000);

        $.each(markers, function (i, marker) {
            marker.start();
        });
    }

    function Pause(){
        document.getElementById("PauseButton").style.display = "none";
        document.getElementById("StartButton").style.display = "flex";
        scaleLable.innerHTML = "0x";
        setTimeout(clearInterval(localClock), 0);
        $.each(markers, function (i, marker) {
            marker.stop();
        });
    }

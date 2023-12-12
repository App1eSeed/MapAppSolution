//uses classList, setAttribute, and querySelectorAll
//if you want this to work in IE8/9 youll need to polyfill these

function makeOrder() {

    let user = JSON.parse(localStorage.getItem("user"));
    let route = JSON.parse(localStorage.getItem("selectedRoute"));
    let seatNumber = JSON.parse(localStorage.getItem("seatNumber"));
    let userServices = JSON.parse(localStorage.getItem("userServices")); 
   
    switch (localStorage["paymentMethod"]) {           
        case "1":
            $.post("/Order/MakeBookingWithPayment", { }, function (result) {
                window.location.href = "/";
            });
        case "0":
            $.post("/Order/MakeBooking", { user: user, route: route, services: userServices, seatNumber: seatNumber }, function (result) {  
                window.location.href = "/";
            });
        default:
            console.log("smt");
    }
    
    document.getElementById("HomeForm").submit();
}



(function () {
    var d = document,
        accordionToggles = d.querySelectorAll('.js-accordionTrigger'),
        setAria,
        setAccordionAria,
        switchAccordion,
        touchSupported = ('ontouchstart' in window),
        pointerSupported = ('pointerdown' in window);

    skipClickDelay = function (e) {
        e.preventDefault();
        e.target.click();
    }

    setAriaAttr = function (el, ariaType, newProperty) {
        el.setAttribute(ariaType, newProperty);
    };
    setAccordionAria = function (el1, el2, expanded) {
        switch (expanded) {
            case "true":
                setAriaAttr(el1, 'aria-expanded', 'true');
                setAriaAttr(el2, 'aria-hidden', 'false');
                break;
            case "false":
                setAriaAttr(el1, 'aria-expanded', 'false');
                setAriaAttr(el2, 'aria-hidden', 'true');
                break;
            default:
                break;
        }
    };
    //function

    let beforeCollapsed;

    switchAccordion = function (e) {
        console.log(this);
        let user = JSON.parse(localStorage.getItem("user"));
        let user1 = localStorage.getItem("user");
        let user2 = JSON.parse(localStorage.getItem("user"));
        console.log(user);
        console.log(user1);
        console.log("asdasd");
        e.preventDefault();

        if (beforeCollapsed != null) {
            var thisAnswer = beforeCollapsed.target.parentNode.nextElementSibling;
            var thisQuestion = beforeCollapsed.target;
            if (thisAnswer.classList.contains('is-collapsed')) {
                setAccordionAria(thisQuestion, thisAnswer, 'true');
            } else {
                setAccordionAria(thisQuestion, thisAnswer, 'false');
            }
            thisQuestion.classList.toggle('is-collapsed');
            thisQuestion.classList.toggle('is-expanded');
            thisAnswer.classList.toggle('is-collapsed');
            thisAnswer.classList.toggle('is-expanded');

            thisAnswer.classList.toggle('animateIn');
        }
        beforeCollapsed = e;

        var thisAnswer = e.target.parentNode.nextElementSibling;
        var thisQuestion = e.target;
        if (thisAnswer.classList.contains('is-collapsed')) {
            setAccordionAria(thisQuestion, thisAnswer, 'true');
        } else {
            setAccordionAria(thisQuestion, thisAnswer, 'false');
        }
        thisQuestion.classList.toggle('is-collapsed');
        thisQuestion.classList.toggle('is-expanded');
        thisAnswer.classList.toggle('is-collapsed');
        thisAnswer.classList.toggle('is-expanded');

        thisAnswer.classList.toggle('animateIn');

        localStorage.setItem("paymentMethod", this.id);
    };

    for (var i = 0, len = accordionToggles.length; i < len; i++) {
        if (touchSupported) {
            accordionToggles[i].addEventListener('touchstart', skipClickDelay, false);
        }
        if (pointerSupported) {
            accordionToggles[i].addEventListener('pointerdown', skipClickDelay, false);
        }
        accordionToggles[i].addEventListener('click', switchAccordion, false);

    }
})();
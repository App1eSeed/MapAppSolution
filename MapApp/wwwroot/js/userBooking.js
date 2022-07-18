let bookingBlocks = document.getElementsByClassName("UserBooking");

for (const bookingBlock of bookingBlocks) {
	bookingBlock.addEventListener("click", collapseBooking);
}

let beforeBooking;

function collapseBooking(){
    if (beforeBooking == this){
        this.children[1].style.display = "none";
        this.children[0].style.backgroundColor ="white";
        beforeBooking = null;
        return;
    }
    if (beforeBooking != null) {
		beforeBooking.children[1].style.display = "none";
        beforeBooking.children[0].style.backgroundColor ="white";
	} 
    console.log(this.children[1]);
	this.children[1].style.display = "flex";
    this.children[0].style.backgroundColor ="#a07ae31d";
   
	//booking.children[4].style.display = "block";

	beforeBooking = this;
}
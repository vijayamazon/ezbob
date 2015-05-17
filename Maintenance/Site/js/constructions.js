$( document ).ready(function() {
    if(window.location.href.indexOf("everline")>-1){
		$("#header_everline").show();
		$("#footer_everline").show();
	}
	else{
		$("#header_ezbob").show();
		$("#footer_ezbob").show();		
	}
})
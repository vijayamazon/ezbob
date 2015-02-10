function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

$(window).on('load', function() {

	var amount = $('#js-iphone-sliders-bar-active-amount');
	var amount_text = $('#js-iphone-sliders-input-amount');
	var amount_min = 3000;
	var amount_max = 52000;
	var amount_range = amount_max - amount_min;

	var duration = $('#js-iphone-sliders-bar-active-duration');
	var duration_text = $('.js-iphone-sliders-input-duration');
	var duration_min = 1;
	var duration_max = 52;
	var duration_range = duration_max - duration_min;

	var slidersInView = false;
	var s = skrollr.init({
		forceHeight: false,
		smoothScrolling: false,
        render: function(data) {

        	var percentage_amount = parseFloat(amount[0].style.width) / 100;
        	var ff = Math.round(((amount_range * percentage_amount) + amount_min) / 10) * 10;
        	amount_text.text(numberWithCommas(ff));

        	var percentage_duration = parseFloat(duration[0].style.width) / 100;
        	var fo = parseInt((duration_range * percentage_duration) + duration_min);
        	duration_text.text(fo);   
        }
   
	});

	






	if (Modernizr.csstransforms) {

	
		$('.impact-steps-item').each(function(index) {
			if (!s.isMobile()) {
				//Safe to use waypoints
				$(this).waypoint(function(direction) {
					$(this).addClass('animate');
				}, {
					offset: ($.waypoints('viewportHeight') * 0.75)
				});
			} else {
				//Mobile!
				var classname = $(this).attr('class');
				$(this).attr('data-center-top', '@class: '+classname+' animate').attr('data-start', '@class: '+classname);
				s.refresh();
			}
		});

		
		$('.impact-reasons-item').each(function(index) {
			if (!s.isMobile()) {
				//Safe to use waypoints.
				var $item = $(this);
				$item.waypoint(function(direction) {

					if ($(window).width() > 800) {
						var delay = index * 200;
					} else {
						var delay = 0;
					}

					setTimeout(function() {
						$item.addClass('animate');
					}, delay);

				}, {
					offset: ($.waypoints('viewportHeight') * 0.75)
				});
			} else {
				//Mobile!
				var classname = $(this).attr('class');
				$(this).attr('data-bottom', '@class: '+classname+' animate').attr('data-start', '@class: '+classname);
				s.refresh();
			}

		});


		if ($(window).width() > 800) {
			//Only apply the parallax effect to the phone on desktop
			$('.impact-managed-iphone').attr('data-bottom-top', 'transform:translate(0px, 150px)').attr('data-top', 'transform:translate(0px,-75px)');
			s.refresh();
		} else {
			var $amount_slider = $('#js-iphone-sliders-bar-active-amount');
			var data_top = $amount_slider.attr('data-300-top');
			$amount_slider.removeAttr('data-300-top').attr('data-100-top', data_top);
			s.refresh();

		}



	}

});
function InitAmountPeriodSliders(options)
{
	function TooltipLeft(val, min, max, oSlider) {
		return Math.round((val - min) * oSlider.width() / (max - min)) + 5;
	}; // TooltipLeft

	function AmountTooltipLeft(val, oSlider) {
		return TooltipLeft(val, options.amount.min, options.amount.max, oSlider);
	}; // AmountTooltipLeft

	function PeriodTooltipLeft(val, oSlider) {
		return TooltipLeft(val, options.period.min, options.period.max, oSlider);
	}; // PeriodTooltipLeft

	function DisplayAmount(n) {
		function FormatMoney(c, d, t){
			var n = this, 
			c = isNaN(c = Math.abs(c)) ? 2 : c, 
			d = d == undefined ? "," : d, 
			t = t == undefined ? "." : t, 
			s = n < 0 ? "-" : "", 
			i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "", 
			j = (j = i.length) > 3 ? j % 3 : 0;

			return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
		};

		return '£' + FormatMoney.call(n, '0', '.', ',');
	}; // DisplayAmount

	function DisplayPeriod(n) {
		return n + ' month' + (n == 1 ? '' : 's');
	}; // DisplayPeriod

	function CreateSlider(oParent, sAreaName, oDefinitions, oLeftFunc, oTextFunc) {
	    $(oParent).append(
	        $('<div />').addClass(sAreaName + '-area')
	        .append($('<h2 />').addClass(sAreaName + '-caption'))
	        .append($('<div />').addClass(sAreaName + '-min slider-label'))
	        .append($('<div />').addClass(sAreaName + '-max slider-label'))
	        .append($('<div />').addClass(sAreaName + '-tooltip'))
	        .append($('<div />').addClass(sAreaName + '-slider'))
	        .append($('<input />').addClass(sAreaName + '-textbox').attr('ui-event-control-id', 'loan-legal:' + sAreaName.toLowerCase()).val(oDefinitions.start)).attr('type', 'text')
		);

		var oTooltip = $('.' + sAreaName + '-tooltip', oParent);
		var oSlider = $('.' + sAreaName + '-slider', oParent);
		var oTextbox = $('.' + sAreaName + '-textbox', oParent);
		if (oDefinitions.caption) {
		    $('.' + sAreaName + '-caption').text(oDefinitions.caption);
		}
		if (oDefinitions.hasbutton) {
		    $('.' + sAreaName + '-textbox').wrap("<div class='slider-input'></div>");
		    $('.' + sAreaName + '-area').find('.slider-input').prepend(($('<div />').addClass('minus-wrap').append($('<span />').addClass('minus icon-minus'))));
		    if (sAreaName == 'period') {
		       
		        $('.' + sAreaName + '-area').find('.slider-input').append($('<span />').addClass('period-text').text('months'));

		    }
		    $('.' + sAreaName + '-area').addClass('clearfix');
		    $('.' + sAreaName + '-area').find('.slider-input').append(($('<div />').addClass('plus-wrap').append($('<span />').addClass('plus icon-plus'))));
		    $('.' + sAreaName + '-area').find('.' + sAreaName + '-slider').wrap($('<div />').addClass('slider-wrap'));
		    $('.' + sAreaName + '-area').find('.slider-input').insertBefore($('.' + sAreaName + '-area').find('.slider-wrap'));
		  
		   
		}
		if (sAreaName == 'amount')
			oTextbox.autoNumeric($.extend({}, EzBob.moneyFormatNoDecimals, { vMin: oDefinitions.min, vMax: oDefinitions.max }));
		else
			oTextbox.autoNumeric({ vMin: oDefinitions.min, vMax: oDefinitions.max, mDec: 0 });

		var s = oSlider.slider({
			range: 'min',
			min:   oDefinitions.min,
			max:   oDefinitions.max,
			value: oDefinitions.start,
			step: oDefinitions.step,
			
			animate: 'slow',
			create: function() {
				window.setTimeout(
					function() {
						if (oDefinitions.hide)
							$('.' + sAreaName + '-area', oParent).hide();

						oTooltip.css('left', oLeftFunc.call(null, oDefinitions.start, oSlider) + 'px').text(oTextFunc.call(null, oDefinitions.start));
					}, 0
				);
			}, // create
			start: function() {
				oTooltip.fadeIn('fast');
				EzBob.UiAction.saveOne(EzBob.UiAction.evtSlideStart(), oTextbox, true);
			}, // start
			slide: function(event, ui) {
				oTooltip.css('left', oLeftFunc.call(null, ui.value, oSlider) + 'px').text(oTextFunc.call(null, ui.value));
				oTextbox.autoNumericSet(ui.value);

				if ($(oParent).data('status') == 'ready')
					options.callback.call(this, options.container, 'slide');

				EzBob.UiAction.saveOne(EzBob.UiAction.evtSlide(), oTextbox, true);
			}, // slide
			change: function(event, ui) {
				if (ui.value == oDefinitions.min || ui.value == oDefinitions.max)
					oTooltip.fadeOut('fast');
				else
					oTooltip.fadeIn('fast');

				oTooltip.css('left', oLeftFunc.call(null, ui.value, oSlider) + 'px').text(oTextFunc.call(null, ui.value));
				oTextbox.autoNumericSet(ui.value);

				if ($(oParent).data('status') == 'ready')
					options.callback.call(this, options.container, 'change');

				EzBob.UiAction.saveOne(EzBob.UiAction.evtChange(), oTextbox, true);
			}, // change
			stop: function(event, ui){
				oTextbox.autoNumericSet(ui.value);

				if (ui.value == oDefinitions.min || ui.value == oDefinitions.max)
					oTooltip.fadeOut('fast');

				if ($(oParent).data('status') == 'ready')
					options.callback.call(this, options.container, 'stop');

				EzBob.UiAction.saveOne(EzBob.UiAction.evtSlideStop(), oTextbox, true);
			} // stop
		}); // init slider

		$('.' + sAreaName + '-area').find('.plus').click(function () {


		    s.slider('value', s.slider('value') + s.slider("option", "step"));

		});
		$('.' + sAreaName + '-area').find('.minus').click(function () {


		    s.slider('value', s.slider('value') - s.slider("option", "step"));

		});
		oTooltip.hide();
		
		$('.' + sAreaName + '-min', oParent).text(oTextFunc.call(null, oDefinitions.min));
		$('.' + sAreaName + '-max', oParent).text(oTextFunc.call(null, oDefinitions.max));

		oTextbox.change(function() {
			oSlider.slider('value', oTextbox.autoNumericGet());

			if ($(oParent).data('status') == 'ready')
				options.callback.call(this, options.container, 'stop');
		});
	}; // CreateSlider

	var oSlidersContainer = $('<div />').data('status', 'loading').addClass('sliders-container');

	var oContainer = $(options.container);
	
	oContainer.empty().addClass('amount-period-sliders').append(oSlidersContainer);

	CreateSlider(oSlidersContainer, 'amount', options.amount, AmountTooltipLeft, DisplayAmount);
	CreateSlider(oSlidersContainer, 'period', options.period, PeriodTooltipLeft, DisplayPeriod);

	$('.amount-slider', oContainer).slider('value', options.amount.start);
	$('.period-slider', oContainer).slider('value', options.period.start);

	oSlidersContainer.data('status', 'ready');
} // InitAmountPeriodSliders


var EzBob = EzBob || {};

EzBob.Validation = EzBob.Validation || {};

//-----------  On ready  -----------  
$(function() {
	//off ajax cache request
	$.ajaxSetup({
		cache: false
	});

	$.ui.dialog.prototype.options.zIndex = 1040;

	$("[autofocus = autofocus]").focus();
	$('input[placeholder], textarea[placeholder]').placeholder();
	$('.datepicker').datepicker();
	$('#signup-form').validate();

	CheckForActivity();

	$('.faq-page').on('show', function(e) {
		var id = e.target.id;
		var link = $('a[href="#' + id + '"]');
		var b = link.parent().find('b');
		b.toggleClass('down');
	});

	$('.faq-page').on('hide', function(e) {
		var id = e.target.id;
		var link = $('a[href="#' + id + '"]');
		var b = link.parent().find('b');
		b.toggleClass('down');
	});
	$(document).keydown(function (e) {
	   var elem = $(this.activeElement);
	   if (elem.hasClass('clickable-label-wrapp')) {

	        var code = e.which;
	        // 13 = Return, 32 = Space
	        if ((code === 13) || (code === 32)) {
	            console.log('click');
	            elem.parent().click();
	        }
	    }
	})
	$(window).resize(function() {
		var tb = $('.top-buttons'),
            po = $('.top-buttons .popover');

		po.css('width', 'auto');

		if (tb.length == 0 || po.length == 0) return;

		var posX = 0 + tb.width() - po.width();
		po.css('left', posX);

		po.not("input[name='UserName']").find(".arrow").css("left", "40%");
		po.has("input[name='UserName']").find(".arrow").css("left", "84%");
	});

	if ($.fn.dataTableExt) {
		$.extend(jQuery.fn.dataTableExt.oSort, {
			"formatted-num-pre": function(a) {
				a = (a === "-" || a === "") ? 0 : a.replace(/[^\d\-\.]/g, "");
				return parseFloat(a);
			},

			"formatted-num-asc": function(a, b) {
				return a - b;
			},

			"formatted-num-desc": function(a, b) {
				return b - a;
			},
			"date-uk-pre": function(a) {
				var ukDatea = a.split('/');
				return (ukDatea[2] + ukDatea[1] + ukDatea[0]) * 1;
			},

			"date-uk-asc": function(a, b) {
				return ((a < b) ? -1 : ((a > b) ? 1 : 0));
			},

			"date-uk-desc": function(a, b) {
				return ((a < b) ? 1 : ((a > b) ? -1 : 0));
			}
		}); // sorting plugin for formatted numbers
	}
});
//-----------  jQuery extended function  -----------  
(function($) {
	$.fn.extend({
		bobToggleAttr: function(attrib, sw) {
			if (sw) {
				this.removeAttr(attrib);
			} else {
				this.attr(attrib, attrib);
			}
			return this;
		}
	});

	$.fn.beautifullList = function() {

		this.each(function() {
			var el = $(this),
                li = el.find('li'),
                i = li.length;

			strippedList(el);

			while (i--) {
				$(li[i]).on("click", function(e) {
					strippedList(el);
					$(e.currentTarget).attr("selected", "selected");
					el.attr("data", $(e.currentTarget).attr("data"));
				});
			}
		});

		return this;
	};

	function strippedList(el) {
		var li = el.find('li'),
            i = li.length;

		while (i--) {
			if ($(li[i]).attr("selected") != 'defined') {
				$(li[i]).removeAttr("selected");
			}
			if (i % 2) {
				$(li[i]).attr("stripped", "stripped");
			} else {
				$(li[i]).removeAttr("stripped");
			}
		}
	}

	$.fn.splittedDateTime = function() {
		this.each(function() {
			var el = $(this),
                day = el.find('[name="day"]'),
                month = el.find('[name="month"]'),
                year = el.find('[name="year"]'),
                hidden = el.find('input.hidden-field');

			hidden.val(day.val() + "/" + month.val() + "/" + year.val());

			day.on("change", function() {
				var isLeapYear = (year.val() % 4 == 0) && ((year.val() % 100 != 0) || (year.val() % 400 == 0)),
                    monthVal = month.val(),
                    dayVal = day.val();

				switch (monthVal) {
					case "2":
						day.val(((dayVal > 28) && !isLeapYear) || (((dayVal > 29) && isLeapYear)) ? 28 : dayVal);
						break;
					case "4":
					case "6":
					case "9":
					case "11":
						day.val(dayVal > 30 ? 30 : dayVal);
						break;
					default:
						break;
				}

				hidden.val(day.val() + "/" + month.val() + "/" + year.val()).trigger("change");

				hidden.trigger("focusout");
			});

			month.on("change", function() {
				day.trigger("change");
			});

			year.on("change", function() {
				day.trigger("change");
			});

			hidden.on("change:silent", function() {
				hidden.val(day.val() + "/" + month.val() + "/" + year.val());
			});
		});
		return this;
	};

	$.fn.numericOnly = function(maxLength) {
		if (maxLength) {
			this.attr("maxlength", maxLength);
		}
		return this.each(function() {
		    $(this).bind('keyup blur mousedown mouseup', function () {
				var regexp = /[^0-9]/;
				while (regexp.test(this.value)) {
					this.value = this.value.replace(regexp, '');
				}
			});
		});
	};

	$.fn.numericOnlyWithDecimal = function() {
		return this.each(function() {
			$(this).bind('keyup blur', function() {
				var regexp = /[^0-9.]/;
				while (regexp.test(this.value)) {
					this.value = this.value.replace(regexp, '');
				}
			});
		});
	};

	$.fn.alphaOnly = function() {
		return this.each(function() {
			$(this).bind('keyup blur', function() {
				var regexp = /[^A-Za-z\.\s\-]/;
				while (regexp.test(this.value)) {
					this.value = this.value.replace(regexp, '');
				}
			});
		});
	};

	$.fn.withoutSpaces = function() {
		return this.each(function() {
			var el = $(this);
			el.on('keyup blur', function() {
				var regexp = /\s+/g;
				while (regexp.test(this.value)) {
					el.val(el.val().replace(regexp, ""));
				}
			});
		});
	};

	$.fn.moneyFormat = function() {
		this.each(function() {
			$(this).autoNumeric('init', EzBob.moneyFormat);
		});
		return this;
	};

	$.fn.percentFormat = function() {
		return this.each(function() {
			$(this).autoNumeric(EzBob.percentFormat);
		});
	};

	$.fn.monthFormat = function() {
		return this.each(function() {
			$(this).autoNumeric(EzBob.monthFormat);
		});
	};

	$.fn.monthFormatNoDecimals = function() {
		return this.each(function() {
			$(this).autoNumeric(EzBob.monthFormatNoDecimals);
		});
	};

	$.fn.serialFill = function() {
		this.each(function() {
			var el = $(this);
			el.on('keyup', function(e) {
				var target = $(e.target);
				if (checkNumber(e) && target.val().length == 2) {
					$(target.attr('nextSerial')).focus();
				}
			});
		});
		return this;
	};

	$.fn.cashEdit = function() {
		this.each(function() {
			var el = $(this),
                elName = el.attr('name') === undefined ? el.attr("id") : el.attr('name'),
                input = $("<input type='hidden' name='" + elName + "' />");

			if (elName === undefined) {
				return;
			}

			el.parent(".cashControlls").find("input:hidden").remove();
			input.insertAfter(el);

			el.on("change click mouseup keyup", function() { $(input).val(el.autoNumeric('get')); })
                .autoNumeric({ pSign: 's', 'aSep': ',', 'aDec': '.', 'aPad': false, 'mNum': 16 });

			el.removeAttr("name");
		});
		return this;
	};

	$.fn.setPopover = function(placement) {
		placement = EzBob.isNullOrEmpty(placement) ? "right" : placement;

		this.each(function() {
			var el = $(this);
			el.popover({ placement: placement, trigger: 'hover', delay: 200 });

			el.on('click', function() {
				return false;
			});
		});
		return this;
	};

	$.fn.disable = function() {
		return this.each(function() {
			if (typeof this.disabled != "undefined")
				this.disabled = true;
		});
	};

	$.fn.enable = function() {
		return this.each(function() {
			if (typeof this.disabled != "undefined")
				this.disabled = false;
		});
	};
})(jQuery);

//-----------  Helper functions  -----------  
Convert = {};

//small fix for validation select in firefox
var fixSelectValidate = function(el) {
	if ($.browser.mozilla) {
		$(el).on("change", function() {
			$(el).trigger("click");
		});
	}
};

function ValueOrDefault(value, defaultValue) {
	return EzBob.isNullOrEmpty(value) ? defaultValue : value;
}

function Redirect(url) {
	document.location.replace(url);
}

function CheckForActivity() {

	var minute = EzBob.Config.SessionTimeout;
	var timer;
	var timeoutValue = 1000 * 60 * minute;
	if (minute <= 0) return;

	var isUnderWriter = document.location.href.indexOf("Underwriter") > -1;

	if (EzBob.Config.HeartBeatEnabled)
		setInterval(heartBeat, 10000);

	//white list
	if (document.location.href.indexOf("Customer/Start") != -1) {
		return;
	}

	set();

	$("body").on('keyup mousemove', reset);

	function heartBeat() {
		//ping server to keep session alive
		$.get(window.gRootPath + "HeartBeat");
	}

	function timeout() {
		if (isUnderWriter) {
			document.location = window.gRootPath + "Account/LogOffUnderwriter";
		} else {
			document.location = window.gRootPath + "Account/LogOff";
		}
	}

	function reset() {
		clearTimeout(timer);
		set();
	}

	function set() {
		timer = setTimeout(timeout, timeoutValue);
	}
}

SetDefaultDate = function(el, date, isNow) { //el is hidden split input
	if (isNow) {
		var now = new Date(),
            currDate = now.getDate(),
            currMonth = now.getMonth() + 1,
            currYear = now.getFullYear();
		date = currDate + "/" + currMonth + "/" + currYear;
	}
	if (date === undefined) return;
	if (date.indexOf('-') != -1) date = EzBob.formatDateWithoutTime(date);
	var element = typeof el === "object" ? el : $(document.getElementById(el)),
        day = element.parent('.ezDateTime').find('select[name="day"]'),
        month = element.parent('.ezDateTime').find('select[name="month"]'),
        year = element.parent('.ezDateTime').find('select[name="year"]'),
        dateArray = date.split("/");

	element.val(date);
	day.val(dateArray[0] * 1);
	month.val(dateArray[1] * 1);
	year.val(dateArray[2] * 1);

	day.trigger("change:silent");
};

function checkNumber(event) {
	event = (event) ? event : window.event;
	var code = (event.charCode) ? event.charCode : event.keyCode;
	if ((code >= 48 && code <= 57) || (code >= 8 && code <= 32) || (code >= 37 && code <= 40) || (code >= 96 && code <= 105) || code == 46) return true;
	else return false;
}

function scrollTop() {
	$('html,body').animate({ scrollTop: 0 }, 500);
}

printElement = function(id) {
	var newWindow = open('', 'newWindow', 'scrollbars=no,titlebar=no,location=no,menubar=no,status=no');
	newWindow.document.writeln(document.getElementById(id).innerHTML);
	newWindow.document.close();
	newWindow.print();
	newWindow.close();
};

GBPValues = function(val, showCurrencySign) {
	if (val == undefined)
		return '-';

	var moneyFormat = showCurrencySign ? EzBob.moneyFormat : EzBob.moneyFormatNoSign;

	return EzBob.formatPoundsFormat(val, moneyFormat);
};

NegativeNum = function(val) {
	if (val == undefined) {
		return "";
	}

	if (typeof val == 'number') {
		return val < 0 ? "negative" : "";
	}

	if (val.indexOf('-') == 0) {
		return "negative";
	}

	return "";
};

IsInt = function(val, isOnlyPositive) {
	var reg = isOnlyPositive ? /^\d+$/ : /^-?\d+$/;
	return reg.test(val);
};

SerializeArrayToEasyObject = function(array) {
	var easyObject = {};
	$.each(array, function(index, value) {
		easyObject[value.name] = value.value;
	});
	return easyObject;
};

BlockUi = function(action, element) {
	if (arguments.length === 0) {
		action = 'on';
		element = null;
	} // if

	action = (action || 'on').toLowerCase();

	switch (action) {
		case 'on':
			var div = $('<img src="' + window.gRootPath + 'Content/css/images/ajax-loader.gif" />'),

			style = {
				border: 'none',
				padding: '15px',
				backgroundColor: '#000',
				'-webkit-border-radius': '10px',
				'-moz-border-radius': '10px',
				opacity: 0.5,
				color: '#fff',
				top: '40%'
			};

			$.blockUI.defaults.overlayCSS.backgroundColor = '#ffffff';

			var options = { message: div, css: style, baseZ: 1000000, };

			element ? $(element).block(options) : $.blockUI(options);

			$('html, body').css({
				'overflow': 'hidden',
				'height': '100%'
			});

			break;

		default:
			element ? $(element).unblock() : $.unblockUI();

			$('html, body').removeAttr('style');

			break;
	} // switch
}; // BlockUi

UnBlockUi = function(element) {
	BlockUi('off', element);
};

window.console = window.console || {
	log: function() { },
	debug: function() { },
	warn: function() { },
	dir: function() { },
	error: function() { }
};

Array.prototype.clean = function(deleteValue) {
	for (var i = 0; i < this.length; i++) {
		if (this[i] == deleteValue) {
			this.splice(i, 1);
			i--;
		}
	}
	return this;
};

Convert.toBool = function(val) {
	return String(val).toLowerCase() === "true";
};

String.prototype.toBool = function() {
	Convert.toBool(this);
};

String.prototype.isNullOrEmpty = function() {
	return EzBob.isNullOrEmpty(this);
};

String.prototype.format = String.prototype.f = function() {
	var s = this,
        i = arguments.length;

	while (i--) {
		s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
	}
	return s;
};

SetCaptchaMode = function() {
	//EzBob.Config.CaptchaMode = "off";

	switch (EzBob.Config.CaptchaMode) {
		default:
		case 'off':
			EzBob.Captcha = EzBob.EmptyCaptcha;
			break;
		case 'simple':
			EzBob.Captcha = EzBob.SimpleCaptcha;
			break;
		case 'reCaptcha':
			EzBob.Captcha = EzBob.ReCaptcha;
			break;
	} // switch
};

//-----------  EZBob functions  -----------  
EzBob.UpdateBugsIcon = function(element, state) {
	var iconClass;
	element = element.find('i');
	switch (state) {
		case "New":
			iconClass = "fa fa-tag";
			state = "Report"; // for use in tooltip title
			break;
		case "Closed":
			iconClass = "fa fa-check ";
			break;
		case "Reopened":
			iconClass = "fa fa-repeat red";
			break;
		case "Opened":
			iconClass = "fa fa-bug red";
			break;
		default:
			iconClass = "fa fa-bug";
	}
	element.removeClass().addClass(iconClass);
	if (iconClass != "fa fa-bug") {
		element.closest("a").attr("data-original-title", "{0} bug".f(state)).tooltip('fixTitle');
	}
};

EzBob.GlobalUpdateBugsIcon = function(customerId) {
	var req = $.get(window.gRootPath + "Underwriter/Bugs/GetAllForCustomer", { customerId: customerId });
	req.done(function(data) {
		EzBob.UpdateBugsIcons(data);
	});
};

EzBob.UpdateBugsIcons = function(data) {
	if (!data || data.length == 0) {
		EzBob.UpdateBugsIcon($('a[data-bug-type]'), 'NoBug ');
	}
	_.each(data, function(val) {
		var element = val.MarketPlaceId ?
            $('a[data-bug-mp={0}]'.f(val.MarketPlaceId)) :
            (
                val.DirectorId ?
                    $('a[data-credit-bureau-director-id={0}]'.f(val.DirectorId)) :
                    $('a[data-bug-type={0}]'.f(val.Type))
            );
		EzBob.UpdateBugsIcon(element, val.State);
	});
};

EzBob.drawDonut = function(canvasId, fillColor, fillPercent, isClock) {
	var canvas = document.getElementById(canvasId);
	if (!canvas) return false;
	var context = canvas.getContext('2d');
	var x = canvas.width / 2;
	var y = canvas.height / 2;
	var radius = isClock ? 35 : 40;
	var startAngle = 1 * Math.PI;
	var endAngle = (isClock ? 2 : 3) * Math.PI;
	var lineWidth = isClock ? 25 : 15;
	var endEngleData = Math.PI * (1 + fillPercent * (isClock ? 1 : 2));
	context.beginPath();
	context.arc(x, y, radius, startAngle, endAngle, false);
	context.lineWidth = lineWidth;
	context.strokeStyle = '#ebebeb';
	context.stroke();
	context.beginPath();
	context.arc(x, y, radius, startAngle, endEngleData, false);
	context.strokeStyle = fillColor;
	context.lineWidth = lineWidth;
	context.stroke();

	if (isClock) {
		context.beginPath();
		context.moveTo(
			x - (radius + lineWidth / 2) * Math.cos(endEngleData - Math.PI),
			y - (radius + lineWidth / 2) * Math.sin(endEngleData - Math.PI)
		);
		context.lineTo(x, y);
		context.strokeStyle = '#000000';
		context.lineWidth = 2;
		context.stroke();
		context.beginPath();
		context.arc(x, y, 5, startAngle, 3 * Math.PI, false);
		context.lineWidth = 5;
		context.strokeStyle = '#ebebeb';
		context.stroke();
	}

	return true;
},
EzBob.drawDi = function(canvasId, fillColor, fillPercent) {
	var canvas = document.getElementById(canvasId);
	if (!canvas) return false;
	var context = canvas.getContext('2d');
	var x = canvas.width / 2;
	var y = canvas.height;
	context.beginPath();
	context.moveTo(x, y);
	context.lineTo(x, 0);
	context.strokeStyle = '#ebebeb';
	context.lineWidth = 15;
	context.stroke();

	context.beginPath();
	context.moveTo(x, y - y * fillPercent);
	context.lineTo(x, y);
	context.strokeStyle = fillColor;
	context.lineWidth = 15;
	context.stroke();
	return true;
}

EzBob.currentServerDate = function() {
	return moment().utc().add('milliseconds', EzBob.serverOffset || 0);
};

EzBob.currentDateFormatted = function() {
	return moment.utc().format('DD/MM/YYYY');
};

EzBob.roundNumber = function(num, dec) {
	var result = Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
	return result;
};

EzBob.isNullOrEmpty = function(value) {
	return !(value != undefined && value.toString().length > 0);
};

EzBob.SeniorityFormat = function(val, decimalPoint) {
	decimalPoint = EzBob.isNullOrEmpty(decimalPoint) ? 1 : decimalPoint;
	var value = parseInt(val);
	if (isNaN(value)) return "-";

	var years = parseInt(value / 12),
        yearsPart = years > 0 ? years + "Y " : "";

	return yearsPart + EzBob.roundNumber(value % 12, decimalPoint) + "M";
};

EzBob.formatNumberLength = function(num, length) {
	var r = "" + num;
	while (r.length < length) {
		r = "0" + r;
	}
	return r;
};

EzBob.ShowMessage = function(message, title, cbOk, okText, cbCancel, cancelText) {
	return EzBob.ShowMessageEx({
		message: message,
		title: title,
		timeout: 0,
		onOk: cbOk,
		okText: okText,
		onCancel: cbCancel,
		cancelText: cancelText,
		closeOnEscape: true,
	});
}; // EzBob.ShowMessage

EzBob.ShowMessageTimeout = function(message, title, timeout, cbOk, okText, cbCancel, cancelText) {
	return EzBob.ShowMessageEx({
		message: message,
		title: title,
		timeout: timeout,
		onOk: cbOk,
		okText: okText,
		onCancel: cbCancel,
		cancelText: cancelText,
		closeOnEscape: true,
	});
}; // EzBob.ShowMessageTimeout

EzBob.ShowMessageEx = function(args) {
	args.okText = args.okText || 'OK';
   
	var modalpopup = $('<div/>');
	modalpopup.html(args.message);

	var buttonModel = [{
		text: args.okText,

		click: function() {
			if (typeof (args.onOk) === 'function') {
				var okFunc = args.onOk();

				okFunc = (okFunc !== undefined) ? okFunc : true;

				if (okFunc)
					$(this).dialog('close');
			} else
				$(this).dialog('close');
		},

		'class': 'ok-button',
	}];

	if ((args.onCancel != undefined) || (args.cancelText != undefined)) {
		buttonModel.push({
			click: function() {
				if (typeof (args.onCancel) === 'function')
					args.onCancel();

				$(this).dialog('close');
			},

			text: args.cancelText || 'Cancel',
		});
	} // if

	var nTimeout = parseInt(args.timeout || 0);
	if (isNaN(nTimeout))
		nTimeout = 0;

	var fOnOpen;

	if (nTimeout <= 0) {
		fOnOpen = function() {
			if (args.hideClose)
				$(".ui-dialog-titlebar-close", $(this).parent()).hide();
			$('body').addClass('stop-scroll');
		};
	} else {
		modalpopup.attr('data-time-to-close', nTimeout - 1);

		var fTimeoutFunc = (function(modalpopup) {
			return function() {
				if (!modalpopup)
					return;

				if (!modalpopup.hasClass('ui-dialog-content'))
					return;

				var nTime = parseInt(modalpopup.attr('data-time-to-close'));

				if (isNaN(nTime)) {
					modalpopup.dialog('close');
					return;
				} // if

				if (nTime <= 0) {
					modalpopup.dialog('close');
					return;
				} // if

				modalpopup.attr('data-time-to-close', nTime - 1);
				$('.ok-button', modalpopup.dialog('widget')).text(args.okText + ' (' + nTime + ')');

				setTimeout(fTimeoutFunc, 1000);
			};
		})(modalpopup); // fTimeoutFunc

		fOnOpen = function() {
			if (args.hideClose)
				$(".ui-dialog-titlebar-close", $(this).parent()).hide();

			$('.ok-button', modalpopup.dialog('widget')).text(args.okText + ' (' + nTimeout + ')');
			setTimeout(fTimeoutFunc, 1000);
			$('body').addClass('stop-scroll');
		};
	} // if

	var isUW = document.location.href.indexOf("Underwriter") > -1;
	modalpopup.dialog({
		title: args.title,
		width: args.dialogWidth || 350,
		modal: true,
		draggable: isUW, // enable for underwriter
		resizable: isUW, // -"-
		buttons: buttonModel,
		dialogClass: "confirmationDialog " + args.customClass,
		zIndex: 3999,
		open: fOnOpen,
		close: function() {
			modalpopup.remove();
			$(this).remove();
			$('body').removeClass('stop-scroll');
		}, // close
		closeOnEscape: !!args.closeOnEscape,
	});

	//added ezbob style
	if (isUW) {
		modalpopup.parents('.ui-dialog').find("button").addClass('btn btn-primary');
	} else {
		modalpopup.parents('.ui-dialog').find("button").addClass('button btn-green ev-btn-org');
	}
	return modalpopup;
}; // EzBob.ShowMessageEx

EzBob.formatIntWithCommas = function(val) {
	return val.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}; // EzBob.formatIntWithCommas

EzBob.moneyFormat = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '£', mDec: '2', vMax: '999999999999999', vMin: '-999999999999999', 'aNeg': '-', pSign: 'p', };
EzBob.moneyFormat1 = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '£', mDec: '1', vMax: '999999999999999', pSign: 'p', };
EzBob.moneyFormatNoDecimals = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'S', aSign: '£', mDec: '0', vMax: '999999999999999', vMin: '-999999999999999', 'aNeg': '-', pSign: 'p', };
EzBob.moneyFormatNoDecimalsNoSign = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'S', aSign: '', mDec: '0', vMax: '999999999999999', vMin: '-999999999999999', 'aNeg': '-', pSign: 'p', };
EzBob.moneyFormatNoSign = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '', mDec: '2', vMax: '999999999999999', pSign: 'p', };
EzBob.moneyFormatAsInt = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'S', aSign: '£', mDec: '0', vMax: '999999999999999', vMin: '-999999999999999', 'aNeg': '-', pSign: 'p', };
EzBob.moneyFormatAsThousands = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '£', mDec: '1', vMax: '999999999999999', vMin: '-999999999999999', 'aNeg': '-', pSign: 'p', };
EzBob.percentFormat = { 'aSep': '', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '% ', mDec: '2', vMax: '9999999', pSign: 's', };
EzBob.percentFormat1 = { 'aSep': '', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '% ', mDec: '1', vMax: '9999999', pSign: 's' };
EzBob.monthFormat = { 'aSep': '', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: ' Months', mDec: '2', vMax: '9999999', pSign: 's', };
EzBob.monthFormatNoDecimals = { 'aSep': '', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: ' Months', mDec: '2', vMax: '9999999', pSign: 's', };

EzBob.formatPoundsWidhDash = function(val) {
	if (!val && val == 0)
		return '-';

	return EzBob.formatPounds(val);
}; // EzBob.formatPoundsWidhDash

EzBob.formatPounds = function(val) {
	return EzBob.formatPoundsFormat(val, EzBob.moneyFormat);
}; // EzBob.formatPounds

EzBob.formatPoundsNoDecimals = function(val) {
	return EzBob.formatPoundsFormat(Math.round(val), EzBob.moneyFormatNoDecimals);
}; // EzBob.formatPoundsNoDecimals

EzBob.formatPoundsAsInt = function(val) {
	return EzBob.formatPoundsFormat(Math.round(val), EzBob.moneyFormatAsInt);
}; // EzBob.formatPoundsAsInt

EzBob.formatPoundsAsThousands = function(val) {
	if (val < 1000 && val >= 100)
		return EzBob.formatPoundsFormat(Math.round((val / 1000) * 10) / 10, EzBob.moneyFormat1) + 'k';

	return EzBob.formatPoundsFormat(Math.round(val / 1000), EzBob.moneyFormatAsThousands) + 'k';
}; // EzBob.formatPoundsAsThousands

EzBob.formatPoundsAsThousandsNoDecimals = function(val) {
	if (val < 1000 && val >= 100)
		return EzBob.formatPoundsFormat(Math.round((val / 1000) * 10) / 10, EzBob.moneyFormat1) + 'k';

	return EzBob.formatPoundsFormat(Math.round(val / 1000), EzBob.moneyFormatNoDecimals) + 'k';
}; // EzBob.formatPoundsAsThousandsNoDecimals

EzBob.formatPoundsAsThousandsNoDecimalsNoSign = function(val) {
	if (val < 1000 && val >= 100)
		return EzBob.formatPoundsFormat(Math.round((val / 1000) * 10) / 10, EzBob.moneyFormat1) + 'k';

	return EzBob.formatPoundsFormat(Math.round(val / 1000), EzBob.moneyFormatNoDecimalsNoSign) + 'k';
}; // EzBob.formatPoundsAsThousandsNoDecimalsNoSign

EzBob.formatPoundsFormatter = null;

EzBob.formatPoundsFormat = function(val, format) {
	if (!val && val != 0)
		return '-';

	if (!EzBob.formatPoundsFormatter)
		EzBob.formatPoundsFormatter = $('<input type="text"/>').autoNumeric('init', format);
	else
		EzBob.formatPoundsFormatter.autoNumeric('update', format);

	return EzBob.formatPoundsFormatter.autoNumeric('set', val).val();
}; // EzBob.formatPoundsFormat

//parses dateString "DD/MM/YYYY" to date "yyyy-MM-dd"
EzBob.parseDate = function(dateString) {
	if (!dateString) return "";
	return dateString.replace(/(\d{2})\/(\d{2})\/(\d{4})/, '$3-$2-$1');
};
//parses dateString "DD/MM/YYYY" to date "yyyy-MM-dd"
EzBob.parseDateTime = function(dateString) {
	if (!dateString) return "";
	return dateString.replace(/(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})/, '$3-$2-$1T$4:$5:$6');
};

EzBob.formatPercents = function(num, precision) {
	if (num == null || num === "") return "";
	var p = precision || 2;
	return EzBob.roundNumber(num * 100, p) + "%";
};

EzBob.formatPercents1 = function(num) {
	var s = EzBob.formatPercents(num, 2);

	if (s.indexOf('.') < 0)
		s = s.replace(/%$/, '.00%');

	return s;
};

EzBob.formatPercents0 = function(num) {
	if (num === undefined || num == null || num === '')
		return '';

	return EzBob.formatIntWithCommas(parseInt(num * 100)) + "%";
};

EzBob.formatPercentsWithDecimals = function(num, precision) {
	return EzBob.formatPercents(num);
};

EzBob.formatLoanTypeSelection = function(num) {
	switch (num) {
		case 0:
		case '0':
			return 'No';
			break;

		case 1:
		case '1':
			return 'Yes';
			break;

		case 2:
		case '2':
			return 'Forbidden';
			break;

		default:
			return 'UNKNOWN';
			break;
	} // switch
}; // formatLoanTypeSelection

//formats date for user from utc asp.net date
EzBob.formatDate = function(date) {
	if (!date)
		return '';

	var oMoment = moment.utc(date);

	if (oMoment.year() === 1 && oMoment.months() === 0 && oMoment.date() === 1)
		return '';

	return oMoment.format("MMM DD, YYYY");
};

EzBob.formatDateUK = function(date) {
	if (!date)
		return '';

	var oMoment = moment.utc(date);

	if (oMoment.year() === 1 && oMoment.months() === 0 && oMoment.date() === 1)
		return '';

	return EzBob.formatDateHumanFullUK(oMoment);
}; // EzBob.formatDateUK

EzBob.DMYtoUK = function(date) {
	if (!date)
		return '';

	var re = /^(\d+)\/(\d+)\/(\d+)$/;

	var ary = re.exec(date);

	if (!ary)
		return date;

	var obj = [
		parseInt(ary[3], 10),
		parseInt(ary[2], 10),
		parseInt(ary[1], 10),
	];

	var oMoment = moment.utc(obj);

	var result = EzBob.formatDateHumanFullUK(oMoment);

	return result;
}; // EzBob.DMYtoUK

//formats date for user from utc asp.net date
EzBob.formatDate2 = function(date) {
	if (!date) return "";
	return moment.utc(date).format("DD/MM/YYYY");
};

EzBob.formatDate3 = function(date) {
	if (!date) return "";
	return moment(date).format("DD/MM/YYYY");
};

EzBob.formatDateTime3 = function(date) {
	if (!date) return "";
	return moment(date).format("DD/MM/YYYY HH:mm:ss");
};

//formats date for user from utc asp.net date + time
EzBob.formatDateTime = function(date) {
	if (!date) return "";
	return moment.utc(date).format("DD/MM/YYYY HH:mm:ss");
};

EzBob.formatDateTimeUK = function(date) {
	if (!date)
		return "";

	return EzBob.formatDateUK(date) + moment.utc(date).format(' HH:mm:ss');
};

//formats date for user from asp.net date + time as is
EzBob.formatDateTimeAsIs = function(date) {
	if (!date) return "";
	return moment.utc(date).format("DD/MM/YYYY HH:mm:ss");
};

EzBob.formatDateTimeCS = function(date) {
	if (!date) return "";
	return moment.utc(date).format("YYYY-MM-DDTHH:mm:ss");
};

EzBob.datetimeToDate = function(date) {
	if (!date) return "";
	return new Date(date.replace(/(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})/, '$3-$2-$1T$4:$5:$6'));
};

//formats date for user from utc asp.net date without time
EzBob.formatDateWithoutTime = function(date) {
	if (!date) return "";
	return moment.utc(date).format("DD/MM/YYYY");
};

EzBob.formatDateHuman = function(date) {
	if (!date) return "";
	return moment.utc(date).format("MMM D");
};

EzBob.formatDateHumanFull = function(date) {
	if (!date) return "";
	return moment.utc(date).format("MMM D YYYY");
};

EzBob.formatDateHumanFullSuffix = function(date) {
	if (!date)
		return '';

	var normalDate = moment.utc(date);

	var dayOfMonth = normalDate.date();

	var suffix = 'th';

	switch (dayOfMonth) {
	case 11:
	case 12:
	case 13:
		// already set
		break;

	default:
		switch (dayOfMonth % 10) {
		case 1:
			suffix = 'st';
			break;

		case 2:
			suffix = 'nd';
			break;

		case 3:
			suffix = 'rd';
			break;

		default:
			// already set
			break;
		} // switch
		break;
	} // switch

	return dayOfMonth + suffix;
}; // EzBob.formatDateHumanFulSuffix

EzBob.formatDateHumanFullUK = function(date) {
	return EzBob.formatDateHumanFullSuffix(date) + moment.utc(date).format(' MMM YYYY');
}

EzBob.formatDateTimeDelimitedUK = function(date) {
	return EzBob.formatDateHumanFullSuffix(date) + moment.utc(date).format(' MMM | HH:mm');
}

EzBob.formatDateShortCard = function(date) {
	if (!date) return "";
	return moment.utc(date).format("MM/YYYY");
};

EzBob.formatDateMY = function(date) {
	if (!date) return "";
	return moment.utc(date).format("MMM 'YY");
};

EzBob.formatTimeSpan = function(val) {
	if (!val) return "";

	var registered = moment.utc(val);

	var years = moment().diff(registered, 'years');
	if (years > 1) return years + " years";
	if (years > 0) return years + " year";

	var months = moment().diff(registered, 'months');
	if (months > 1) return months + " months";
	if (months > 0) return months + " month";

	var weeks = moment().diff(registered, 'weeks');
	if (weeks > 1) return weeks + " weeks";
	if (weeks > 0) return weeks + " week";

	var days = moment().diff(registered, 'days');
	if (days > 1) return days + " days";
	if (days > 0) return days + " day";

	var hours = moment().diff(registered, 'hours');
	if (hours > 1) return hours + " hours";

	return "less than hour";
};

EzBob.formatTimeFromNow = function(val) {
	if (!val) return "";

	var registered = moment.utc(val, 'DD/MM/YYYY HH:mm:ss');

	if (!registered.isValid()) {
		console.log.apply(console, 'Invalid offer expiration date');
		return "";
	}

	var hours = registered.diff(moment().utc(), 'hours');
	var minutes = registered.diff(moment().utc(), 'minutes');
	
	minutes = minutes - hours * 60;
	
	return (hours > 9 ? '' : '0') + hours + ':' + (minutes > 9 ? '' : '0') + minutes;
};

EzBob.formatMonths = function(num) {
	if (!num) return '';
	return num + ' month' + (num>1 ? 's' : '');
};

EzBob.isDarkColor = function(c) {
	if (!c) return false;
	c = c.substring(1);          // strip #
	var rgb = parseInt(c, 16);   // convert rrggbb to decimal
	var r = (rgb >> 16) & 0xff;  // extract red
	var g = (rgb >> 8) & 0xff;  // extract green
	var b = (rgb >> 0) & 0xff;  // extract blue

	var luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709

	if (luma < 40) {
		return true;
	}
	return false;
};

EzBob.validateLoginForm = function(el) {
	var e = el || $(".simple-login");
	var passPolicy = { required: true, minlength: 6, maxlength: 20 };
	var passPolicyText = EzBob.dbStrings.PasswordPolicyCheck;
	if (EzBob.Config.PasswordPolicyType != 'simple') {
		passPolicy.regex =
        "^.*([a-z]+.*[A-Z]+) |([a-z]+.*[^A-Za-z0-9]+)|([a-z]+.*[0-9]+)|([A-Z]+.*[a-z]+)|([A-Z]+.*[^A-Za-z0-9]+)|([A-Z]+.*[0-9]+)|([^A-Za-z0-9]+.*[a-z]+.)|([^A-Za-z0-9]+.*[A-Z]+)|([^A-Za-z0-9]+.*[0-9]+.)|([0-9]+.*[a-z]+)|([0-9]+.*[A-Z]+)|([0-9]+.*[^A-Za-z0-9]+).*$";
		passPolicy.minlength = 7;
		passPolicyText = "Password has to have 2 types of characters out of 4 (letters,caps,digits,special chars)";
	}
	return e.validate({
		rules: {
			UserName: { required: true, email: true, maxlength: 128 },
			Password: $.extend({}, passPolicy)
		},
		messages: {
			"UserName": { email: EzBob.dbStrings.NotValidEmailAddress, required: EzBob.dbStrings.NotValidEmailAddress },
			"Password": { required: passPolicyText, regex: passPolicyText },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.createPasswordValidationPolicy = function() {
	var passPolicy = { required: true, minlength: 6, maxlength: 60 };
	var passPolicyText = EzBob.dbStrings.PasswordPolicyCheck;

	if (EzBob.Config.PasswordPolicyType !== 'simple') {
		passPolicy.regex = "^.*([a-z]+.*[A-Z]+) |([a-z]+.*[^A-Za-z0-9]+)|([a-z]+.*[0-9]+)|([A-Z]+.*[a-z]+)|([A-Z]+.*[^A-Za-z0-9]+)|([A-Z]+.*[0-9]+)|([^A-Za-z0-9]+.*[a-z]+.)|([^A-Za-z0-9]+.*[A-Z]+)|([^A-Za-z0-9]+.*[0-9]+.)|([0-9]+.*[a-z]+)|([0-9]+.*[A-Z]+)|([0-9]+.*[^A-Za-z0-9]+).*$";
		passPolicy.minlength = 7;
		passPolicyText = "Password has to have 2 types of characters out of 4 (letters,caps,digits,special chars)";
	} // if

	return { policy: passPolicy, text: passPolicyText, };
};

EzBob.validateSignUpForm = function(el) {
	var e = el || $(".signup");

	var oPolicy = EzBob.createPasswordValidationPolicy();

	var passPolicy = oPolicy.policy;
	var passPolicyText = oPolicy.text;

	var passPolicy2 = $.extend({}, passPolicy);
	passPolicy2.equalTo = '#signupPass1';

	return e.validate({
		rules: {
			signupPass1: $.extend({}, passPolicy),
			signupPass2: passPolicy2,
			Email: { required: true, email: true, maxlength: 128 },
			securityQuestion: { required: true },
			SecurityAnswer: { required: true, maxlength: 199 },
			CaptchaInputText: { required: true, minlength: 6, maxlength: 6 },
			amount: { required: true, defaultInvalidPounds: true, regex: "^(?!£0.00$)", autonumericMin: 0, autonumericMax: 1000000000 },
			mobilePhone: { required: true, regex: "^0[0-9]{10}$" },
			mobileCode: { required: true, minlength: 6, maxlength: 6 }
		},
		messages: {
			"Email": { required: EzBob.dbStrings.NotValidEmailAddress, email: EzBob.dbStrings.NotValidEmailAddress },
			"signupPass1": { required: passPolicyText, regex: passPolicyText, minlength: EzBob.Config.Origin == 'everline' ? '' : $.validator.format("Please enter at least {0} characters.") },
			"signupPass2": { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
			"securityQuestion": { required: "This field is required" },
			"SecurityAnswer": { maxlength: "Maximum answer length is 199 characters" },
			"CaptchaInputText": { required: "This field is required" },
			"mobilePhone": { regex: "Please enter a valid UK number" },
			"mobileCode": { regex: "Please enter the code you received" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS,
		ignore: ":not(:visible)"
	});
};

EzBob.validateChangePassword = function(el) {
	var e = el || $("#change-password");

	var oPolicy = EzBob.createPasswordValidationPolicy();

	var passPolicy = oPolicy.policy;
	var passPolicyText = oPolicy.text;

	var passPolicy2 = $.extend({}, passPolicy);
	passPolicy2.equalTo = '#new_password';

	return e.validate({
		rules: {
			password: $.extend({}, passPolicy),
			new_password: $.extend({}, passPolicy),
			new_password2: passPolicy2,
		},
		messages: {
			new_password: { required: passPolicyText, regex: passPolicyText },
			new_password2: { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateChangeSecurityQuestion = function (el) {
	var e = el || $("#change-question");

	var oPolicy = EzBob.createPasswordValidationPolicy();

	var passPolicy = oPolicy.policy;

	return e.validate({
		rules: {
			password: $.extend({}, passPolicy),
			securityQuestion: { required: true },
			answer: { required: true, maxlength: 199 }
		},
		messages: {
			answer: { maxlength: "Maximum answer length is 199 characters" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateRestorePasswordForm = function(el) {
	var e = el || $(".restorePasswordArea");
	return e.validate({
		rules: {
			email: { required: true, email: true },
			Answer: { required: true, maxlength: 199 },
			CaptchaInputText: { required: true, minlength: 6, maxlength: 6 }
		},
		messages: {
			"email": { required: EzBob.dbStrings.NotValidEmailAddress, email: EzBob.dbStrings.NotValidEmailAddress },
			"Answer": { maxlength: "Maximum answer length is 199 characters" },
			"CaptchaInputText": { required: "This field is required" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validatePersonalDetailsForm = function(el) {
	var e = el || $(".PersonalDetailsForm");

    return e.validate({
        rules: {
            FirstName: EzBob.Validation.NameValidationObject,
            MiddleInitial: { required: false, maxlength: 100 },
			Surname: { required: true, maxlength: 100 },
			DateOfBirth: { requiredDate: true, yearLimit: 18 },
			DayTimePhone: { required: true, regex: "^0[0-9]{10}$" },
			MobilePhone: { required: true, regex: "^0[0-9]{10}$" },
			PropertyStatus: { required: true },
			Gender: { required: true },
			MaritalStatus: { required: true },
			TimeAtAddress: { required: true },
			ConsentToSearch: { required: true },
		},
		messages: {
			DateOfBirth: { yearLimit: "The number of full year should be more then 18 year" },
			PropertyStatus: { required: "This field is required" },
			TimeAtAddress: { regex: "This field is required" },
			MaritalStatus: { required: "This field is required" },
			MobilePhone: { regex: "Please enter a valid UK number" },
			DayTimePhone: { regex: "Please enter a valid UK number" },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS,
		ignore: ':hidden:not(#DateOfBirth)',
	});
};

EzBob.validateLimitedCompanyDetailForm = function(el) {
	var e = el || $(".LimitedCompanyDetailForm");
	return e.validate({
		rules: {
			//limited company info
			LimitedCompanyNumber: { required: true, maxlength: 255, regex: "^[a-zA-Z0-9]+$" },
			LimitedCompanyName: { required: true, minlength: 2, maxlength: 255 },
			CapitalExpenditure: { required: true, defaultInvalidPounds: true },
			TotalMonthlySalary: { required: true, defaultInvalidPounds: true, regex: "^(?!£0.00$)", autonumericMin: 0, autonumericMax: 1000000000 },
			LimitedBusinessPhone: { required: true, regex: "^0[0-9]{10}$" }
		},
		messages: {
			LimitedCompanyNumber: { regex: "Please enter a valid company number" },
			CapitalExpenditure: { defaultInvalidPounds: "This field is required" },
			TotalMonthlySalary: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
			LimitedBusinessPhone: { regex: "Please enter a valid UK number" },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS,
		ignore: ":not(:visible)"
	});
};

EzBob.validateNonLimitedCompanyDetailForm = function(el) {
	var e = el || $(".NonLimitedCompanyDetailForm");
	return e.validate({
		rules: {
			//Non limited company info
			NonLimitedCompanyName: { required: true, minlength: 2, maxlength: 255 },
			NonLimitedTimeInBusiness: { required: true },
			CapitalExpenditure: { required: true, defaultInvalidPounds: true },
			TotalMonthlySalary: { required: true, defaultInvalidPounds: true, regex: "^(?!£0.00$)", autonumericMin: 0, autonumericMax: 1000000000 },
			NonLimitedBusinessPhone: { required: true, regex: "^0[0-9]{10}$" }
		},
		messages: {
			CapitalExpenditure: { defaultInvalidPounds: "This field is required" },
			TotalMonthlySalary: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
			NonLimitedBusinessPhone: { regex: "Please enter a valid UK number" },
		},

		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS,
		ignore: ":not(:visible)"
	});
};

EzBob.validateBankDetailsForm = function(el) {
	var e = el || $(".bankAccount");
	return e.validate({
		rules: {
			SortCode: { required: true, minlength: 6, maxlength: 6, digits: true },
			AccountNumber: { required: true, minlength: 8, maxlength: 8, digits: true }
		},
		messages: {
			SortCode: { minlength: "Please enter a valid Sort Code", maxlength: "Please enter a valid SortCode" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateAmazonForm = function(el) {
	var e = el || $(".AmazonForm");

	return e.validate({
		rules: {
			amazonMerchantId: { remote: { url: window.gRootPath + "AmazonMarketPlaces/IsAmazonUserCorrect" }, required: true },
			amazonMarketplaceId: { required: true, rangelength: [10, 15], amazonMPValidator: true },
			amazonMWSAccessToken: { required: true, minlength: 40 }
		},
		messages: {
			amazonMerchantId: { remote: "Account does not exist" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateYourInfoEditForm = function(el) {
	var e = el || $(".editYourInfoForm");

	var isOffline = e.find('.offline').length > 0;

	return e.validate({
		rules: {
			DayTimePhone: { required: true, regex: "^0[0-9]{10}$" },
			MobilePhone: { required: true, regex: "^0[0-9]{10}$" },
			BusinessPhone: { required: true, regex: "^0[0-9]{10}$" },
			OverallTurnOver: { required: true, digits: true, min: 1, max: 1000000000 },
			WebSiteTurnOver: { required: true, digits: true, min: isOffline ? 0 : 1, max: 1000000000 }
		},
		messages: {
			DayTimePhone: { regex: "Please enter a valid UK number" },
			MobilePhone: { regex: "Please enter a valid UK number" },
			BusinessPhone: { regex: "Please enter a valid UK number" }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlight
	});
};

EzBob.validateRollover = function(el) {
	var e = el || $("#validateRollover");

	return e.validate({
		rules: {
			ScheduleId: { required: true },
			PaymentDueDate: { required: true, requiredDate: true },
			Payment: { required: true, number: true },
			MounthCount: { required: true, number: true, min: 1 }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlight
	});
};

EzBob.validateChangeEmailForm = function(el, emailFieldID) {
	var e = el || $('form');
	emailFieldID = emailFieldID || 'edit-email';

	var rules = {};
	rules[emailFieldID] = { required: true, email: true, };

	return e.validate({ rules: rules, });
};

EzBob.validatemanualPaymentForm = function(el, outstandingBbalance, minAmount) {
	var e = el || $('form');

	return e.validate({
		rules: {
			experiedDate: { required: true, requiredDate: true },
			description: { required: true },
			paymentMethod: { required: true, regex: "[a-zA-Z]+" },
			totalSumPaid: { number: true, required: true, positive: true, autonumericMax: outstandingBbalance, autonumericMin: minAmount },
		},
		messages: {
			PaymentMethod: { regex: "This field is required" },
			totalSumPaid: {
				autonumericMax: 'Warning! <br /> Please pay attention that you are trying to enter greater than Outstanding amount relevant for the date of placed payment. ' +
								 'In case the customer made a Bank Transfer in an amount which is higher than £' + outstandingBbalance +
								 ',<br />please, communicate the client and make access funds return after EZBOB LTD access transactions costs deduction.' +
								 ' Then submit to the system only the actual for the day of payment amount.',
				autonumericMin: 'Warning! <br /> Please pay attention that you are trying to enter less than minimal amount relevant for the date of placed payment. ' +
								 'In case the customer made a Bank Transfer in an amount which is less than £' + minAmount +
								 ',<br />please, communicate the client and make access funds beyond the minimal amount.' +
								 ' Then submit to the system only the actual for the day of payment amount.'
			}
		}//,
		//errorPlacement: EzBob.Validation.errorPlacement,
		//unhighlight: EzBob.Validation.unhighlight
	});
};

EzBob.validateEkmShopForm = function(el) {
	var e = el || $('form');

	return e.validate({
		rules: {
			ekm_login: { required: true, minlength: 2, maxlength: 30 },
			ekm_password: { required: true, minlength: 2, maxlength: 30 }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validatePayPointShopForm = function(el) {
	var e = el || $('form');

	return e.validate({
		rules: {
			payPoint_mid: { required: true, minlength: 2, maxlength: 30 },
			payPoint_vpnPassword: { required: true, minlength: 2, maxlength: 30 },
			payPoint_remotePassword: { required: true, minlength: 2, maxlength: 30 }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateHmrcLinkForm = function(el) {
	var e = el || $('form');

	return e.validate({
		rules: {
			hmrc_user_id: { required: true, minlength: 2, maxlength: 300 },
			hmrc_password: { required: true, minlength: 2, maxlength: 300 }
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	});
};

EzBob.validateCGShopForm = function(el, accountType) {
	var v = {
		rules: {},
		messages: {},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS
	};

	var lf = EzBob.CgVendors.pure()[accountType].ClientSide.LinkForm;

	var atlc = accountType.toLowerCase();

	for (var i in lf.Fields) {
		if (!lf.Fields.hasOwnProperty(i))
			continue;

		var fi = lf.Fields[i];

		var r = {};
		var m = {};

		for (var j in fi.ValidationRules)
			if (fi.ValidationRules[j])
				r[j] = fi.ValidationRules[j];

		for (var j in fi.ValidationMessages) {
			var msg = fi.ValidationMessages[j];
			m[msg.PropertyName] = msg.Message;
		} // for

		var pn = atlc + '_' + fi.PropertyName.toLowerCase();

		v.rules[pn] = r;
		v.messages[pn] = m;
	} // for each field

	return el.validate(v);
};

EzBob.escapeRegExp = function(str) {
	return str ? str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&") : '';
}; // EzBob.escapeRegExp

$.validator.addMethod('notEqual', function (value, element, param) {
	if(!$(param).is(':visible')) {
		return true;
	}
	return this.optional(element) || value != $(param).val();
}, 'This has to be different...');

$.validator.addMethod('validateSignerName', function(value, element, params) {
	function innerLog() {
		// console.log.apply(console, Array.prototype.slice.call(arguments));
	} // innerLog

	// ReSharper disable DuplicatingLocalDeclaration
	function checkFL(sSignature, sFirstName, sLastName, sSeparator) {
		innerLog('checkFL(', sSignature, ',', sFirstName, ',', sLastName, ',', sSeparator, ') started...');

		var re = new RegExp('^' + sFirstName + sSeparator + sLastName + '$', 'i');

		if (sSignature.match(re)) {
			innerLog('checkFL(', sSignature, ',', sFirstName, ',', sLastName, ',', sSeparator, ') matches FL.');
			return true;
		} // if

		re = new RegExp('^' + sLastName + sSeparator + sFirstName + '$', 'i');

		if (sSignature.match(re)) {
			innerLog('checkFL(', sSignature, ',', sFirstName, ',', sLastName, ',', sSeparator, ') matches LF.');
			return true;
		} // if

		innerLog('checkFL(', sSignature, ',', sFirstName, ',', sLastName, ',', sSeparator, ') no match.');
		return false;
	} // checkFL

	function checkFML(sSignature, sFirstName, sMiddleName, sLastName, sSeparator) {
		function checkOneFML(sSignature, sFirstName, sMiddleName, sLastName, sSeparator) {
			innerLog('checkOneFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') started...');

			var re = new RegExp('^' + sFirstName + sSeparator + sMiddleName + sSeparator + sLastName + '$', 'i');

			if (sSignature.match(re)) {
				innerLog('checkOneFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') matches FML.');
				return true;
			} // if

			re = new RegExp('^' + sMiddleName + sSeparator + sFirstName + sSeparator + sLastName + '$', 'i');

			if (sSignature.match(re)) {
				innerLog('checkOneFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') matches MFL.');
				return true;
			} // if

			re = new RegExp('^' + sFirstName + sSeparator + sLastName + sMiddleName + sSeparator + '$', 'i');

			if (sSignature.match(re)) {
				innerLog('checkOneFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') matches FLM.');
				return true;
			} // if

			innerLog('checkOneFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') no match.');
			return false;
		} // checkOneFML

		innerLog('checkFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') started...');

		if (checkOneFML(sSignature, sFirstName, sMiddleName, sLastName, sSeparator)) {
			innerLog('checkFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') matches FL with M.');
			return true;
		} // if

		if (checkOneFML(sSignature, sLastName, sMiddleName, sFirstName, sSeparator)) {
			innerLog('checkFML(', sSignature, ',', sFirstName, ',', sMiddleName, ',', sLastName, ',', sSeparator, ') matches LF with M.');
			return true;
		} // if

		return false;
	} // checkFML
	// ReSharper restore DuplicatingLocalDeclaration

	var sSignature = $.trim(value);
	var sNoSpaceSignature = sSignature.replace(/\s+/g, '');

	if (sNoSpaceSignature === '')
		return false;

	var sFirstName = EzBob.escapeRegExp(params[0]);
	var sMiddleName = EzBob.escapeRegExp(params[1]);
	var sLastName = EzBob.escapeRegExp(params[2]);

	var sNoSpaceFirstName = sFirstName.replace(/\s+/g, '');
	var sNoSpaceLastName = sLastName.replace(/\s+/g, '');
	var sNoSpaceMiddleName = sMiddleName.replace(/\s+/g, '');

	if (checkFL(sSignature, sFirstName, sLastName, '\\s+'))
		return true;

	if (sNoSpaceMiddleName === '')
		return checkFL(sNoSpaceSignature, sNoSpaceFirstName, sNoSpaceLastName, '');

	if (checkFML(sSignature, sFirstName, sMiddleName, sLastName, '\\s+'))
		return true;

	return checkFML(sNoSpaceSignature, sNoSpaceFirstName, sNoSpaceMiddleName, sNoSpaceLastName, '');
}, 'Please type your full name as it appears in the agreement.');

EzBob.validateLoanLegalForm = function(el, aryCustomerName) {
	var e = el || $(".LoanLegal");

	return e.validate({
		rules: {
			"preAgreementTermsRead": { required: true },
			"agreementTermsRead": { required: true },
			"euAgreementTermsRead": { required: true },
			"cosmeAgreementTermsRead": { required: true },
			"directorConsentRead": { required: true },
			'signedName': { validateSignerName: aryCustomerName, },
			'notInBankruptcy': { required: true },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlightFS,
		highlight: EzBob.Validation.highlightFS,
		ignore: ":not(:visible)"
	});
};

EzBob.poundToInt = function(sNumWithPounds) {
	return parseInt(sNumWithPounds.replace(/[^0-9\.-]/g, ''));
};

EzBob.poundToNum = function(sNumWithPounds) {
	var x = parseFloat(sNumWithPounds.replace(/[^0-9\.-]/g, ''));
	return isNaN(x) ? 0.0 : x;
};

EzBob.guid = function() {
	return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
		var r = Math.random() * 16 | 0;
		var v = (c == 'x') ? r : (r & 0x3 | 0x8);
		return v.toString(16);
	});
}; // EzBob.guid

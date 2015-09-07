var EzBob = EzBob || {};
EzBob.Validation = EzBob.Validation || {};

//-----------------------------------------------------------------

EzBob.Validation.NameValidationObjectOld = { regex: "^(([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))( ([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))*$", required: true, minlength: 2 };

EzBob.Validation.NameValidationObject = { nameValidator: true, required: true, minlength: 2, maxlength: 100 };

//-----------------------------------------------------------------
EzBob.Validation.addressErrorPlacement = function(el, model, nDirectorID, sTypeOfBusiness) {
	function addressListLength(oModel, nDirID, sBusinessType) {
		if (!nDirID)
			return oModel.length;

		var oBusinessTypeInfo = oModel.get(sBusinessType + 'Info');

		if (!oBusinessTypeInfo || !oBusinessTypeInfo.Directors)
			return 0;

		for (var nIdx = 0; nIdx < oBusinessTypeInfo.Directors.length; nIdx++) {
			var oDirector = oBusinessTypeInfo.Directors[nIdx];

			if (oDirector.Id != nDirID)
				continue;

			if (!oDirector.DirectorAddress)
				return 0;

			return oDirector.DirectorAddress.length;
		} // for

		return 0;
	} // addressListLength

	var $el = $(el);

	$el.on('focusout', function() {
		if (addressListLength(model, nDirectorID, sTypeOfBusiness) === 0) {
			var oButton = $el.find('.addAddress');
			if (EzBob.Config.Origin !== 'everline') {
			    oButton.tooltip({ title: 'Please lookup your post code' });
			}
			

			$el.hover(
				function() { oButton.tooltip('show'); },
				function() { oButton.tooltip('hide'); }
			); // on hover
		} // if
	}); // on focus out

	model.on('change', function() {
		if (addressListLength(model, nDirectorID, sTypeOfBusiness) > 0)
			$el.find('.addAddress').tooltip('destroy');
	}); // on model changed
}; // addressErrorPlacement

EzBob.Validation.errorPlacement = function (error, element) {
    EzBob.Validation.unhighlight(element);
    if (EzBob.Config.Origin === 'everline') {
        if (error.text()) {
        	error.appendTo(element.siblings(".error-wrap"));
        	if ($(element).hasClass('SortCodeSplit')) {
        		error.appendTo(element.parent().siblings(".error-wrap"));
        	}
        }
    } else {
        if (error.text()) {
            //fix for hidden
            if (element.hasClass("hidden-field")) {
                //fix for empty date validation
                if ((element.closest('.ezDateTime').find("select[value=-]")).length > 0) {
                    element = element.closest('.ezDateTime').find("select[value=-]");
                } else {
                    //fix for incorrect date validation
                    if ((element.closest('.ezDateTime').find("select[name='day']")).length > 0) {
                        element = element.closest('.ezDateTime').find("select[name='day'],select[name='month'],select[name='year']");
                    }
                }
                //fix for SortCode
                if ((element.closest('.ezSortCode').find("input:text")).length > 0) {
                    element = element.closest('.ezSortCode').find("input:text");
                }
                //fix for number
                if (element.closest("div").find(".cashInput").length > 0) {
                    element = element.closest("div").find(".cashInput");
                }
            }
            //fix for radio input
            if (element.attr('type') == 'radio') {
                element = element.closest("span");
            }

            element.attr('data-original-title', error.text());
            element.tooltip({
                'trigger': 'hover',
                'title': error.text()
            });

            element.tooltip("enable").tooltip('fixTitle');
            element.addClass('error');
        }
    }
   
};

//-----------------------------------------------------------------
EzBob.Validation.unhighlight = function (element) {
    var $e = $(element);    
    //fix for chosen
    $e = $("#" + $e.attr('id') + "_chzn").length == 0 ? $e : $("#" + $e.attr('id') + "_chzn > a");
    $e.closest('.control-group').removeClass('error');
    $e.closest('.controls').removeClass('error');
    $e.closest('.controls').find("*").removeClass('error');
    $e.removeClass('error');
    $e.tooltip('disable');
    $e.closest('.ezDateTime').find('[data-original-title]').tooltip('disable');
    $e.closest('.ezSortCode').find('[data-original-title]').tooltip('disable');
    $e.closest("div").find(".cashInput").removeClass("error");
    $e.closest("span").removeClass("error");
};


EzBob.Validation.unhighlightFS = function (element) {
    if (EzBob.Config.Origin === 'everline') {
        if ($(element).attr('id') === 'DateOfBirth') {
            $('.form_field_date').css('border-bottom', '2px solid #7ac143');
        }
        else if ($(element).attr('class') === 'address-field') {
            $(element).find('.form_field_left_side:not(.addAddressInput)').css('border-bottom', '2px solid #7ac143');
        }
        else if ($(element).hasClass('SortCodeSplit')) {
        	$('.form_field_look').css('border-bottom', '2px solid #7ac143');
        }
        else {
            $(element).css('border-bottom', '2px solid #7ac143');
        }
    } else {
       
        EzBob.Validation.unhighlight(element);
        var $el = $(element),
            val = $el.val(),
            img = $el.closest('div').find('.field_status');
        if (img.hasClass("required") && !val) {
            img.field_status('set', 'required', 2);
        } else {
            img.field_status('set', 'ok');
        }
    }
 
};

EzBob.Validation.highlightFS = function (element) {
    if (EzBob.Config.Origin == 'everline') {
        if ($(element).attr('id') === 'DateOfBirth') {
            $('.form_field_date').css('border-bottom', '2px solid red');
        } else if ($(element).attr('class') === 'address-field') {
            $(element).find('.form_field_left_side').css('border-bottom', '2px solid red');
        } else if ($(element).hasClass('SortCodeSplit')) {
        	$('.form_field_look').css('border-bottom', '2px solid red');
	    } else {
		    $(element).css('border-bottom', '2px solid red');
	    }

    } else {
      
        var $el = $(element),
            val = $el.val(),
            img = $el.closest('div').find('.field_status');

        if ($el.hasClass('cashInput') && val == '£ ') {
            img.field_status('set', 'required', 2);
            return;
        }

        if (img.hasClass("required") && !val) {
            img.field_status('set', 'required', 2);
        } else if ($el.hasClass('SortCodeSplit')) {
            img.field_status('set', 'required', 2);
        } else if ($el.hasClass('requiredDate') && $el.val().indexOf('-') !== -1) {
            img.field_status('set', 'required', 2);
        } else {
            img.field_status('set', 'fail');
        }
    }
    
};

//Extends validator method 
$.validator.addMethod(
        "regex",
        function (value, element, regexp) {
            var re = new RegExp(regexp);
            return this.optional(element) || re.test(value);
        },
        "Please check your input."
);

$.validator.addMethod(
        "autonumericMin",
        function (value, element, minVal) {
            var amount = parseFloat($(element).autoNumericGet());
            return amount >= minVal;
        },
        "Amount is below limit."
);

$.validator.addMethod(
        "autonumericMax",
        function (value, element, maxVal) {
            var amount = parseFloat($(element).autoNumericGet());
            return amount <= maxVal;
        },
        "Amount is above limit."
);


$.validator.addMethod(
        "positive",
        function (value, element) {
            return parseFloat($(element).val()) >= 0;
        },
        "Should be positive"
);

$.validator.addMethod(
        "minDate",
        function (value, element, minDate) {
            var d1 = moment.utc(minDate).toDate();
            var d2 = moment.utc(value, "DD/MM/YYYY").toDate();
            return d2 >= d1;
        }
);

$.validator.addMethod(
        "nameValidator",
        function (value, element) {
            var trimmed = $.trim(value);
            var re = /^(([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))( ([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))*$/;
            return re.test(trimmed);
        },
        "Please check your input"
);

$.validator.addMethod(
        "amazonMPValidator",
        function (value, element) {
            var trimmed = $.trim(value);
            var re = /^(A13V1IB3VIYZZH)|(A1PA6795UKMFR9)|(APJ6JRA9NG5V4)|(A1RKKUPIHCS9HS)|(A1F83G8C2ARO7P)$/i;
            return re.test(trimmed);
        },
        "Incorrect marketplace ID."
);

$.validator.addMethod(
        "ezbobEmail",
        function (value, element) {
            return this.optional(element) || /^[a-zA-Z][a-zA-Z0-9\.\-\_]{1,28}[a-zA-Z0-9]@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])$/i.test(value);
        },
        "Please use between 3 and 30 characters before @.You can use letter, number and periods."
);

$.validator.addMethod(
    "dateCheck", function (value, element) {
        var res = moment(value, "DD/MM/YYYY").isValid();
        return res;
    }, "Please insert date in format DD/MM/YYYY, for example 21/06/2012"
);

$.validator.addMethod("defaultInvalidPounds", function(value, element) {
    return !(element.value == '£ ');
});

$.validator.addMethod('optionalUrl', function(oValue, oElement) {
	if (this.optional(oElement))
		return true;

	if (/[ "']/.test(oValue))
		return false;

	if (/^[\?\\\.\,\-\*\&\!\@\#\$\%\^\(\)\{\}\[\}\+=;:<>\|`]/.test(oValue))
		return false;

	if ((oValue.indexOf('://') > -1) && !/^https?:\/\//.test(oValue))
		return false;

	if (oValue.indexOf('.') === -1)
		return false;

	return true;
}, 'Please enter a valid URL.');

$.validator.methods.number = function (value, element) {
    return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
};

jQuery.extend(jQuery.validator.messages, {
    required: "This field is required"
});

EzBob.Validation.checkDate = function (value) {
    return /\d+\/\d+\/\d+/.test(value);
};

jQuery.validator.addMethod("requiredDate", EzBob.Validation.checkDate, 'Please enter a valid date.');

EzBob.Validation.validateAndNotify = function (validator) {
    if (!validator.form()) {
        if (validator.errorList && validator.errorList.length > 0) {
            var firstError = validator.errorList[0];
            if (firstError.message) EzBob.App.trigger('error', firstError.message);
        }
        return false;
    }
    return true;
};

EzBob.Validation.checkForm = function (validator) {
    return validator.checkForm();
};

EzBob.Validation.element = function (validator, elem) {
    return validator.element(elem);
};

EzBob.Validation.displayIndication = function (validator, ctrlImgId, validatedElement) {
    var controlImgId = "#" + ctrlImgId;

    var sStatus = validator.element($(validatedElement)) ? 'ok' : 'fail';

    $(controlImgId).field_status('set', sStatus);
};

$.validator.methods.yearLimit = function (value, element, yearCount) {
    var val = moment(value, "DD/MM/YYYY").toDate();
    var now = new Date();
    now.setHours(0);
    var currentFullYear = Math.floor(Math.abs(now - val) / 1000 / 60 / 60 / 24 / 365.25);

    return currentFullYear >= yearCount;
};

//-----------  Director Validation -----------  

EzBob.Validation.checkDirectorName = function (value, elm) {
    if (value.length > 100) {
        return false;
    }

    if (/\.Name$/.test(elm.name)) {
        var trimmed = $.trim(value);
        var re = /^(([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))( ([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))*$/;
        return re.test(trimmed);
    } // if

    return true;
}; // EzBob.Validation.checkDirectorName

EzBob.Validation.checkDirectorSurName = function (value, elm) {
    return value.length >= 1 && value.length <= 100;
}; // EzBob.Validation.checkDirectorSurName

jQuery.validator.addMethod('director_name_part', EzBob.Validation.checkDirectorName, 'Please check your input.');
jQuery.validator.addMethod('director_surname_part', EzBob.Validation.checkDirectorSurName, 'Please check your input.');


EzBob.Validation.checkDirectorGender = function(ignored, elm) {
    return $(elm).closest('.FormRadioCtrl').find('.director_gender:checked').length == 1;
}; // EzBob.Validation.checkDirectorGender

jQuery.validator.addMethod('director_gender', EzBob.Validation.checkDirectorGender, 'Please check your input.');

EzBob.Validation.checkDirectorPhoneNumber = function(value, elm) {
	return (/\.Phone$/.test(elm.name)) ? /^0[0-9]{10}$/.test($.trim(value)) : true;
}; // EzBob.Validation.checkDirectorPhoneNumber

jQuery.validator.addMethod('director_phone', EzBob.Validation.checkDirectorPhoneNumber, 'Please enter a valid UK number.');

EzBob.Validation.checkDirectorEmail = function (value, element) {
    return /^((([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/.test(value);
};

jQuery.validator.addMethod('director_email', EzBob.Validation.checkDirectorEmail, 'Please use between 3 and 30 characters before @.You can use letter, number and periods.');

jQuery.validator.addMethod('director_birth_date', function(value, elm) { return $.validator.methods.yearLimit(value, elm, 18); }, 'Director should be at least 18 years old.');

//-----------  Validate -----------  

var validFunc = function (el) {
    if ($(el).closest('.ezDateTime').length != 0) {
        $(el).closest('.ezDateTime').find("input.hidden-field").valid();
    } else if ($(el).closest('.ezSortCode').length != 0) {
        $(el).closest('.ezSortCode').find("input.hidden-field").valid();
    } else if ($(el).hasClass('addAddressInput')) {
        //do nothing
    } else {
        $(el).valid();
    }
};

$.validator.setDefaults({
    onclick: function (el) {
        if ($(el).is('input:radio')) {
            return $(el).valid();
        }

        if ($(el).is('select') && $(el).closest('.ezDateTime').length == 0) {

            var initialValue = $(el).data("initial-value");
            var currentValue = $(el).val();
            if (initialValue != currentValue) {
                validFunc(el);
            }

        }
        $(el).data("initial-value", $(el).val());
        return false;
    },
    onfocusin: function (el) {
        var $el = $(el);
        $el.data("initial-value", $el.val());
    },
    onfocusout: function (el) {
        var $el = $(el);
        if ($el.val()) {
            var initialValue = $el.data("initial-value");
            var currentValue = $el.val();
            if (initialValue != currentValue) {
                validFunc(el);
            }
        }
        $el.data("initial-value", $el.val());
    },
    onkeyup: function (el, ev) {
        //not tab pressed
        if (ev.keyCode != 9) {
            if ($(el).is('select')) {
                validFunc(el);
            }

            if ($(el).is('input') && $(el).val() == "") {
                validFunc(el);
            }

            if ($(el).hasClass('cashInput') && $(el).val() == "£ ") {
                validFunc(el);
            }

            if ($(el).closest('.ezSortCode').length != 0) {
                validFunc(el);
            }
            /*if ($(el).is('input:text') && !isNotEmptyFunc(el) && ) {
                validFunc(el);
            }*/
        }
    },
    ignore: []
});
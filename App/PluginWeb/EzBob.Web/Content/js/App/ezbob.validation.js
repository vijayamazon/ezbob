var EzBob = EzBob || {}; EzBob.Validation = EzBob.Validation || {};

//-----------------------------------------------------------------

EzBob.Validation.NameValidationObjectOld = { regex: "^(([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))( ([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))*$", required: true, minlength: 2 };

EzBob.Validation.NameValidationObject = { nameValidator: true, required: true, minlength: 2 };

//-----------------------------------------------------------------
EzBob.Validation.errorPlacement = function (error, element) {
    EzBob.Validation.unhighlight(element);
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
    $e.closest("div").find(".cashInput").removeClass("error");
    $e.closest("span").removeClass("error");
};


EzBob.Validation.unhighlightFS = function (element) {
    EzBob.Validation.unhighlight(element);
    var $el = $(element),
        val = $el.val(),
        img = $el.closest('div').find('.field_status');

    if (img.hasClass("required") && !val) {
        img.field_status('set', 'required');
    } else {
        img.field_status('set', 'ok');
    }
};

EzBob.Validation.highlightFS = function (element) {
    $(element).closest('div').find('.field_status').field_status('set', 'fail');
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
        "Loan amount is below limit."
);

$.validator.addMethod(
        "autonumericMax",
        function (value, element, maxVal) {
            var amount = parseFloat($(element).autoNumericGet());
            return amount <= maxVal;
        },
        "Loan amount is above limit."
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
        "Please check your input."
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

EzBob.Validation.checkDirectorName = function(value, elm) {
	if (/\.Name$|\.Surname$/.test(elm.name)) {
		var trimmed = $.trim(value);
		var re = /^(([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))( ([a-zA-Z]*[AEIOUYaeiouy]+[a-zA-Z]*)|(Ng)|(ng))*$/;
		return re.test(trimmed);
	} // if

	return true;
}; // EzBob.Validation.checkDirectorName

jQuery.validator.addMethod("director_name_part", EzBob.Validation.checkDirectorName, 'Please check your input.');

EzBob.Validation.checkDirectorGender = function(ignored, elm) {
	return $(elm).closest('.GenderCtrl').find('.director_gender:checked').length == 1;
}; // EzBob.Validation.checkDirectorGender

jQuery.validator.addMethod("director_gender", EzBob.Validation.checkDirectorGender, 'Please check your input.');

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
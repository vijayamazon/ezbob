/// <reference path="~/Content/js/lib/moment.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="~/Content/js/lib/underscore.js" />
/// <reference path="~/Content/js/lib/jquery.validate.js"/>

var EzBob = EzBob || {};

EzBob.Validation = EzBob.Validation || {};

//-----------  On ready  -----------  
$(function () {
    //off ajax cache request
    $.ajaxSetup({
        cache: false
    });

    $.ui.dialog.prototype.options.zIndex = 1040;
    
    $("[autofocus = autofocus]").focus();
    $('input[placeholder], textarea[placeholder]').placeholder();
    $('.datepicker').datepicker();
    $('#signup-form').validate();

    var contactUs = new EzBob.ContactUsView();
    contactUs.template = $(".contactUs").html();
    contactUs.render();

    CheckForActivity();

    $('.faq-page').on('show', function (e) {
        var id = e.target.id;
        var link = $('a[href="#' + id + '"]');
        var b = link.parent().find('b');
        b.toggleClass('down');
    });

    $('.faq-page').on('hide', function (e) {
        var id = e.target.id;
        var link = $('a[href="#' + id + '"]');
        var b = link.parent().find('b');
        b.toggleClass('down');
    });

    $(window).resize(function () {
        var tb = $('.top-buttons'),
            po = $('.top-buttons .popover');

        po.css('width', 'auto');

        if (tb.length == 0 || po.length == 0) return;

        var posX = 0 + tb.width() - po.width();
        po.css('left', posX);

        po.not("input[name='UserName']").find(".arrow").css("left", "40%");
        po.has("input[name='UserName']").find(".arrow").css("left", "84%");
    });
});
//-----------  jQuery extended function  -----------  
(function ($) {
    $.fn.extend({
        bobToggleAttr: function (attrib, sw) {
            if (sw) {
                this.removeAttr(attrib);
            } else {
                this.attr(attrib, attrib);
            }
            return this;
        }
    });

    $.fn.beautifullList = function () {

        this.each(function () {
            var el = $(this),
                li = el.find('li'),
                i = li.length;

            strippedList(el);

            while (i--) {
                $(li[i]).on("click", function (e) {
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

    $.fn.splittedDateTime = function () {
        this.each(function () {
            var el = $(this),
                day = el.find('[name="day"]'),
                month = el.find('[name="month"]'),
                year = el.find('[name="year"]'),
                hidden = el.find('input[type="hidden"]');

            hidden.val(day.val() + "/" + month.val() + "/" + year.val());

            day.on("change", function () {
                var isLeapYear = (year.val() % 4 == 0) && ((year.val() % 100 != 0) || (year.val() % 400 == 0)),
                    monthVal = month.val(),
                    dayVal = day.val();

                switch (monthVal) {
                    case "2":
                        day.val(((dayVal > 28) && !isLeapYear) || (((dayVal > 29) && isLeapYear)) ? 28 : dayVal);
                        break;
                    case "1":
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

            });

            month.on("change", function () {
                day.trigger("change");
            });

            year.on("change", function () {
                day.trigger("change");
            });

        });
        return this;
    };

    $.fn.numericOnly = function (maxLength) {
        if (maxLength) {
            this.attr("maxlength", maxLength);
        }
        return this.each(function () {
            $(this).bind('keyup blur', function () {
                var regexp = /[^0-9]/;
                while (regexp.test(this.value)) {
                    this.value = this.value.replace(regexp, '');
                }
            });
        });
    };


    $.fn.withoutSpaces = function () {
        this.each(function () {
            var el = $(this);
            el.on('keyup change', function (event) {
                el.val(el.val().replace(/\s/,"" ));
            });
        });
        return this;
    };

    $.fn.moneyFormat = function () {
        this.each(function () {
            $(this).autoNumeric(EzBob.moneyFormat);
        });
        return this;
    };

    $.fn.percentFormat = function () {
        this.each(function () {
            $(this).autoNumeric(EzBob.percentFormat);
        });
        return this;
    };

    $.fn.serialFill = function () {
        this.each(function () {
            var el = $(this);
            el.on('keyup', function (e) {
                var target = $(e.target);
                if (checkNumber(e) && target.val().length == 2) {
                    $(target.attr('nextSerial')).focus();
                }
            });
        });
        return this;
    };

    $.fn.cashEdit = function () {
        this.each(function () {
            var el = $(this),
                elName = el.attr('name') === undefined ? el.attr("id") : el.attr('name'),
                input = $("<input type='hidden' name='" + elName + "' />");

            if (elName === undefined) {
                return;
            }

            el.parent(".cashControlls").find("input:hidden").remove();
            input.insertAfter(el);

            el.on("change click mouseup keyup", function () { $(input).val(el.autoNumericGet()); })
                .autoNumeric({ pSign: 's', 'aSep': ',', 'aDec': '.', 'aPad': false, 'mNum': 16 });

            el.removeAttr("name");
        });
        return this;
    };

    $.fn.setPopover = function (placement) {
        placement = EzBob.isNullOrEmpty(placement) ? "right" : placement;

        this.each(function () {
            var el = $(this);
            el.popover({ placement: placement, trigger: 'hover', delay: 200 });

            el.on('click', function () {
                return false;
            });
        });
        return this;
    };
})(jQuery);

//-----------  Helper functions  -----------  
function ValueOrDefault(value, defaultValue) {
    return EzBob.isNullOrEmpty(value) ? defaultValue : value;
}
    
function Redirect(url) {
    document.location.replace(url);
}

function CheckForActivity() {

    var minute = EzBob.Config.SessionTimeout;

    if (minute <= 0) return;
    
    if (EzBob.Config.HeartBeatEnabled) setInterval(heartBeat, 1000 * 60 * EzBob.Config.SessionTimeout / 2);

    var underwriterParam = document.location.href.indexOf("Underwriter") > -1 ? "?isUnderwriterPage=true" : "";

    //white list
    if (document.location.href.indexOf("Customer/Start") != -1) {
        return;
    }

    var timer;
    var timeoutValue = 1000 * 60 * minute;

    set();

    $("body").on('keyup mousemove', reset);

    function heartBeat() {
        //ping server to keep session alive
        $.get(window.gRootPath + "HeartBeat");
    }

    function timeout() {
        Redirect(window.gRootPath + "Account/LogOff" + underwriterParam);
    }

    function reset() {
        clearTimeout(timer);
        set();
    }

    function set() {
        timer = setTimeout(timeout, timeoutValue);
    }
}

SetDefaultDate = function (el, date, isNow) { //el is hidden split input
    if (isNow) {
        var now = new Date(),
            currDate = now.getDate(),
            currMonth = now.getMonth() + 1,
            currYear = now.getFullYear();
        date = currDate + "/" + currMonth + "/" + currYear;
    }
    if (date === undefined) return;
    if (date.indexOf('-') != -1) date = EzBob.formatDateWithoutTime(date);
    var element = typeof el === "object" ?el : $(document.getElementById(el)),
        day = element.parent('.ezDateTime').find('select[name="day"]'),
        month = element.parent('.ezDateTime').find('select[name="month"]'),
        year = element.parent('.ezDateTime').find('select[name="year"]'),
        dateArray = date.split("/");

    element.val(date);
    day.val(dateArray[0]*1);
    month.val(dateArray[1]*1);
    year.val(dateArray[2]*1);

    day.trigger("change");
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

printElement = function (id) {
    var newWindow = open('', 'newWindow', 'scrollbars=no,titlebar=no,location=no,menubar=no,status=no');
    newWindow.document.writeln(document.getElementById(id).innerHTML);
    newWindow.document.close();
    newWindow.print();
    newWindow.close();
};

GBPValues = function (val, showCurrencySign) {
    if (val == undefined) {
        return "-";
    }
    val = val.toString();
    val = val.replace(',', '.');
    var valSplit = val.split('.');
    val = valSplit[0];
    var length = val.length;

    if (length > 16) {
        console.error("Value:", val, " must be less then 16 characters");
    }

    var retVal = "";
    for (var i = 0; i < length; i++) {
        if (i % 3 == 0 && i != 0) retVal = "," + retVal;
        retVal = val[length - i - 1] + retVal;
    }
    var result = !showCurrencySign ?
        (retVal + (valSplit[1] != undefined ? "." + valSplit[1] : "")) :
        ($("<input />").autoNumeric(EzBob.moneyFormat).autoNumericSet(retVal + (valSplit[1] != undefined ? "." + valSplit[1] : ""))).val();


    return result;
};

SerializeArrayToEasyObject = function (array) {
    var easyObject = {};
    $.each(array, function (index, value) {
        easyObject[value.name] = value.value;
    });
    return easyObject;
};

BlockUi = function (action, element) {
    if (action.toLowerCase() == "on") {
        var div = $("<img src='" + window.gRootPath + "Content/css/images/ajax-loader.gif' />"),
        style = {
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff'
        };
        var options = { message: div, css: style, baseZ: 1000000, };
        element ? $(element).block(options) : $.blockUI(options);
    } else {
        element ? $(element).unblock() : $.unblockUI();
    }
};

window.console = window.console || {
    log: function () { },
    debug: function () { },
    warn: function () { },
    dir: function () { },
    error: function () { }
};

Array.prototype.clean = function (deleteValue) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == deleteValue) {
            this.splice(i, 1);
            i--;
        }
    }
    return this;
};

String.prototype.isNullOrEmpty = function() {
    return EzBob.isNullOrEmpty(this);
};

String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
};

SetCaptchaMode = function () {
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
    }
};

//-----------  EZBob functions  -----------  
EzBob.UpdateBugsIcon = function (element, state) {
    var iconClass;
    element = element.find('i');
    switch (state) {
        case "New":
            iconClass = "icon-tag";
            state = "Report"; // for use in tooltip title
            break;
        case "Closed":
            iconClass = "icon-ok";
            break;
        case "Reopened":
            iconClass = "icon-repeat";
            break;
        case "Opened":
            iconClass = "icon-remove";
            break;
        default:
            iconClass = "";
    }
    element.removeClass().addClass(iconClass);
    element.closest("a").attr("data-original-title", "{0} bug".f(state) ).tooltip('fixTitle');
};

EzBob.GlobalUpdateBugsIcon = function(customerId) {
    var req = $.get( window.gRootPath + "Underwriter/Bugs/GetAllForCustomer", { customerId: customerId });
    req.done(function(data) {
        _.each(data, function (val) {
            var element = val.MarketPlaceId ?
                $('a[data-bug-mp={0}]'.f(val.MarketPlaceId)) :
                (
                    val.DirectorId ?
                    $('a[data-credit-bureau-director-id={0}]'.f(val.DirectorId)) :
                    $('a[data-bug-type={0}]'.f(val.Type))
                );
            EzBob.UpdateBugsIcon(element,val.State);
        });
    });
};

EzBob.currentDateFormatted = function () {
    return moment().format('DD/MM/YYYY');
};

EzBob.roundNumber = function (num, dec) {
    var result = Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
    return result;
};

EzBob.isNullOrEmpty = function (value) {
    return !(value != undefined && value.toString().length > 0);
};

EzBob.SeniorityFormat = function (val, decimalPoint) {
    decimalPoint = EzBob.isNullOrEmpty(decimalPoint) ? 1 : decimalPoint;
    var value = parseInt(val);
    if (isNaN(value)) return "-";

    var years = parseInt(value / 12),
        yearsPart = years > 0 ? years + "Y " : "";

    return yearsPart + EzBob.roundNumber(value % 12, decimalPoint) + "M";
};

EzBob.formatNumberLength = function (num, length) {
    var r = "" + num;
    while (r.length < length) {
        r = "0" + r;
    }
    return r;
};

EzBob.ShowMessage = function (message, title, cbOk, okText, cbCancel, cancelText) {
    var modalpopup = $('<div/>');
    modalpopup.html(message);

    var buttonModel = [{
        text: okText || "OK",
        click: function () {
            $(this).dialog("destroy");
            if (typeof (cbOk) == 'function') {
                cbOk();
            }
        }
    }];
    if (cbCancel != undefined || cancelText != undefined) {
        buttonModel.push({
            click: function () {
                $(this).dialog("destroy");
                if (typeof (cbCancel) == 'function') {
                    cbCancel();
                }
            },
            text: cancelText
        });
    }
    modalpopup.dialog(
        {
            'title': title,
            modal: true,
            draggable: false,
            resizable: false,
            buttons: buttonModel,
            dialogClass: "confirmationDialog",
            zIndex: 3999

        });
    //added ezbob style
    modalpopup.parents('.ui-dialog').find("button").addClass('btn');
};

EzBob.moneyFormat = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '£ ', mDec: '2', vMax: '999999999999999' };
EzBob.moneyFormatNoDecimals = { 'aSep': ',', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '£ ', mDec: '0', vMax: '999999999999999' };
EzBob.percentFormat = { 'aSep': '', 'aDec': '.', 'aPad': true, 'mNum': 16, 'mRound': 'F', aSign: '% ', mDec: '2', vMax: '9999999', pSign: 's' };

EzBob.formatPoundsWidhDash = function (val) {
    if (!val && val == 0) {
        return '-';
    }
    return EzBob.formatPounds(val);
};

EzBob.formatPounds = function (val) {
    return EzBob.formatPoundsFormat(val, EzBob.moneyFormat);
};

EzBob.formatPoundsNoDecimals = function (val) {
    return EzBob.formatPoundsFormat(val, EzBob.moneyFormatNoDecimals);
};

EzBob.formatPoundsFormat = function (val, format) {
    if (!val && val != 0) {
        return '-';
    }
    var target = $('<input type="text"/>');
    return $.fn.autoNumeric.Format(target, val, format);
};

EzBob.formatPoundsWithBrackets = function (val) {
    if (EzBob.isNullOrEmpty(val)) {
        return '-';
    }
    return "(" + EzBob.formatPounds(val) + ")";
};

//formats date for user from utc asp.net date
EzBob.formatDate = function (date) {
    if (!date) return "";
    return moment.utc(date).local().format("MMM DD, YYYY");
};

//formats date for user from utc asp.net date + time
EzBob.formatDateTime = function (date) {
    if (!date) return "";
    return moment.utc(date).local().format("DD/MM/YYYY HH:mm:ss");
};

//formats date for user from utc asp.net date without time
EzBob.formatDateWithoutTime = function (date) {
    if (!date) return "";
    return moment.utc(date).local().format("DD/MM/YYYY");
};

EzBob.formatPercents = function (num, precision) {
    if (num == null || num === "") return "";
    var p = precision || 2;
    return EzBob.roundNumber(num * 100, p) + "%";
};

EzBob.formatDateHuman = function (date) {
    if (!date) return "";
    return moment.utc(date).local().format("MMM D");
};

EzBob.formatDateHumanFull = function (date) {
    if (!date) return "";
    return moment.utc(date).local().format("MMM D YYYY");
};

EzBob.formatTimeSpan = function (val) {
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

//Server Date
var dateDiff = null;
(function () {
    var r = new XMLHttpRequest(),
            start = (new Date).getTime();

    r.open('HEAD', window.gRootPath, false);
    r.onreadystatechange = function () {
        if (r.readyState != 4) {
            return null;
        }
        var latency = (new Date).getTime() - start;
        var timestring = r.getResponseHeader("DATE");

        var systemtime = new Date(timestring);
        systemtime.setMilliseconds(systemtime.getMilliseconds() + (latency / 2));

        dateDiff = systemtime - (new Date);
    };
    r.send(null);
})();

Date.prototype.getServerTime = function () {
    var date = new Date();

    date.setTime(date.getTime() + dateDiff);

    return date;
};

//-----------  Validate  -----------  
$.validator.setDefaults({
    onkeyup: false,
    onChangeFocus: true,
    ignore: ""
});

EzBob.validateLoginForm = function (el) {
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
            UserName: { required: true, email: true },
            Password:  $.extend({}, passPolicy) 
        },
        messages: {
            "UserName": { email: EzBob.dbStrings.NotValidEmailAddress, required: EzBob.dbStrings.NotValidEmailAddress },
            "Password": { required: passPolicyText, regex: passPolicyText },
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateSignUpForm = function (el) {
    var e = el || $(".signup");

    var passPolicy = { required: true, minlength: 6, maxlength: 20 };
    var passPolicyText = EzBob.dbStrings.PasswordPolicyCheck;
    if (EzBob.Config.PasswordPolicyType != 'simple') {
        passPolicy.regex =
        "^.*([a-z]+.*[A-Z]+) |([a-z]+.*[^A-Za-z0-9]+)|([a-z]+.*[0-9]+)|([A-Z]+.*[a-z]+)|([A-Z]+.*[^A-Za-z0-9]+)|([A-Z]+.*[0-9]+)|([^A-Za-z0-9]+.*[a-z]+.)|([^A-Za-z0-9]+.*[A-Z]+)|([^A-Za-z0-9]+.*[0-9]+.)|([0-9]+.*[a-z]+)|([0-9]+.*[A-Z]+)|([0-9]+.*[^A-Za-z0-9]+).*$";
        passPolicy.minlength = 7;
        passPolicyText = "Password has to have 2 types of characters out of 4 (letters,caps,digits,special chars)";
    }
    var passPolicy2 = $.extend({}, passPolicy);
    passPolicy2.equalTo = '#signupPass1';

    return e.validate({
        rules: {
            signupPass1: $.extend({}, passPolicy),
            signupPass2: passPolicy2,
            Email: { required: true, email: true },
            securityQuestion: { required: true },
            SecurityAnswer: { required: true, maxlength: 199 }
        },
        messages: {
            "Email": { required: EzBob.dbStrings.NotValidEmailAddress, email: EzBob.dbStrings.NotValidEmailAddress },
            "signupPass1": { required: passPolicyText, regex: passPolicyText },
            "signupPass2": { equalTo: EzBob.dbStrings.PasswordDoesNotMatch },
            "securityQuestion": { required: "This field is required" },
            "SecurityAnswer": { maxlength: "Maximum answer length is 199 characters" }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateChangePassword = function (el) {
    var e = el || $("#change-password");
    return e.validate({
        rules: {
            password: { required: true },
            new_password: { required: true, minlength: 6, remote: { url: window.gRootPath + "AccountSettings/IsEqualsOldPassword" } },
            new_password2: { required: true, equalTo: '#new_password', minlength: 6 }
        },
        messages: {
            "new_password": { minlength: EzBob.dbStrings.PasswordPolicyCheck, remote: "Equals Old Password" },
            "new_password2": { minlength: EzBob.dbStrings.PasswordPolicyCheck, equalTo: EzBob.dbStrings.PasswordDoesNotMatch }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateDirectorForm = function (el) {
    var e = el;
    return e.validate({
        rules: {
            DirectorName: EzBob.Validation.NameValidationObject,
            DirectorDateOfBirth: { required: true, requiredDate: true }

        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateCompanyDetailForm = function (el) {
    var e = el || $(".CompanyDetailForm");
    return e.validate({
        ignore: "",
        rules: {
            //personal info
            FirstName: EzBob.Validation.NameValidationObject,
            Surname: { required: true },
            DateOfBirth: { required: true, requiredDate: true, yearLimit: 18 },
            DayTimePhone: { required: true, regex: "^0[0-9]{9,10}$" },
            MobilePhone: { required: true, regex: "^0[0-9]{9,10}$" },
            TypeOfBusiness: { required: true },
            ResidentialStatus: { required: true },
            Gender: { regex: "^M|F$", required: true },
            MartialStatus: { required: true },
            OverallTurnOver: { required: true, number: true, min: 1 },
            WebSiteTurnOver: { required: true, number: true, min: 1 },
            TimeAtAddress: { required: true },

            //limited company info
            LimitedCompanyNumber: { required: true, maxlength: 255, regex: "^[a-zA-Z0-9]+$" },
            LimitedCompanyName: { required: true, minlength: 2 },
            LimitedOverallTurnOver: { required: true, number: true },
            LimitedWebSiteTurnOver: { required: true, number: true },
            LimitedBusinessPhone: { required: true, regex: "^0[0-9]{9,10}$" },

            //Non limited company info
            NonLimitedCompanyName: { required: true, minlength: 2 },
            NonLimitedTimeInBusiness: { required: true },
            NonLimitedOverallTurnOver: { required: true, number: true, minlength: 2 },
            NonLimitedWebSiteTurnOver: { required: true, number: true, minlength: 2 },
            NonLimitedTimeAtAddress: { required: true, digits: true },
            NonLimitedBusinessPhone: { required: true, regex: "^0[0-9]{9,10}$" }
        },
        messages: {
            DateOfBirth: { yearLimit: "The number of full year should be more then 18 year" },
            day: { digits: "Incorect day" },
            month: { digits: "Incorect month" },
            year: { digits: "Incorect year" },
            ResidentialStatus: { required: "This field is required" },
            TimeAtAddress: { regex: "This field is required" },
            Gender: { regex: "Gender field is required" },
            MartialStatus: { required: "Martial status field is required" },
            LimitedBusinessPhone: { regex: "Please enter a valid UK number" },
            NonLimitedBusinessPhone: { regex: "Please enter a valid UK number" },
            MobilePhone: { regex: "Please enter a valid UK number" },
            DayTimePhone: { regex: "Please enter a valid UK number" },
            LimitedCompanyNumber: { regex: "Please enter a valid company number" }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateSortCode = function (el) {
    var e = el || $(".bankAccount");
    return e.validate({
        onfocusout: false,
        onfocusin: false,
        onclick: false,
        focusInvalid: false,
        ignoreTitle: true,
        rules: {
            SortCode1: { required: true, minlength: 2, maxlength: 2, digits: true },
            SortCode2: { required: true, minlength: 2, maxlength: 2, digits: true },
            SortCode3: { required: true, minlength: 2, maxlength: 2, digits: true },
            AccountNumber: { required: true, minlength: 8, maxlength: 8, digits: true }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
       // unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateAmazonForm = function (el) {
    var e = el || $(".form-inline");

    return e.validate({
        rules: {
            amazonMerchantId: { remote: { url: window.gRootPath + "AmazonMarketPlaces/IsAmazonUserCorrect", async: false }, required: true },
            amazonMarketplaceId: { required: true }
        },
        messages: {
            amazonMerchantId: { remote: "Account does not exist" }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateYourInfoEditForm = function (el) {
    var e = el || $(".editYourInfoForm");
    return e.validate({
        rules: {
            DayTimePhone: { required: true, regex: "^0[0-9]{9,10}$" },
            MobilePhone: { required: true, regex: "^0[0-9]{9,10}$" },
            BusinessPhone: { required: true, regex: "^0[0-9]{9,10}$" },
            OverallTurnOver: { required: true, digits: true, min:1 },
            WebSiteTurnOver: { required: true, digits: true, min:1 }
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

EzBob.validateRollover = function (el) {
    var e = el || $("#validateRollover");

    return e.validate({
        rules: {
            ScheduleId: { required: true },
            PaymentDueDate: { required: true, requiredDate: true },
            Payment: { required: true, number: true },
            MounthCount: { required: true, number: true, min:1 }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateChangeEmailForm = function(el) {
    var e = el || $('form');

    return e.validate({
        rules: {
            'edit-email' : { required: true, email: true }
        }
    });
};

EzBob.validatemanualPaymentForm = function (el) {
    var e = el || $('form');

    return e.validate({
        rules: {
            experiedDate: { required: true, requiredDate: true },
            description: { required: true },
            paymentMethod: { required: true, regex: "[a-zA-Z]+" },
            totalSumPaid: { required: true, number: true },
        },
        messages: {
            PaymentMethod: { regex: "This field is required" }
        }//,
        //errorPlacement: EzBob.Validation.errorPlacement,
        //unhighlight: EzBob.Validation.unhighlight
    });
};

EzBob.validateEkmShopForm = function (el) {
    var e = el || $('form');

    return e.validate({
        rules: {
            ekm_login: { required: true, minlength: 2, maxlength: 30 },
            ekm_password: { required: true, minlength: 2, maxlength: 30 }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
}

EzBob.validatePayPointShopForm = function (el) {
    var e = el || $('form');

    return e.validate({
        rules: {
            payPoint_mid: { required: true, minlength: 2, maxlength: 30 },
            payPoint_vpnPassword: { required: true, minlength: 2, maxlength: 30 },
            payPoint_remotePassword: { required: true, minlength: 2, maxlength: 30 }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
}

EzBob.validateVolusionShopForm = function(el) {
    var e = el || $('form');

    return e.validate({
        rules: {
            volusion_shopname: { required: true, minlength: 2, maxlength: 300 },
            volusion_url: { required: true, minlength: 10, maxlength: 300 },
            volusion_login: { required: true, minlength: 2, maxlength: 30 },
            volusion_password: { required: true, minlength: 2, maxlength: 300 }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
    });
}
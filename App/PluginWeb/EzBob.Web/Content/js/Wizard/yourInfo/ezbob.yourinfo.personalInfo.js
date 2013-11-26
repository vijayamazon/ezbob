﻿var EzBob = EzBob || {};

EzBob.PersonalInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {

        this.template = _.template($('#personinfo-template').html());
        this.ViewName = "personal";

        this.events = _.extend({}, this.events, {
            "change #TimeAtAddress": "PersonalTimeAtAddressChanged",
            "change #ResidentialStatus": "ResidentialStatusChanged",
            "change #OwnOtherProperty": "OwnOtherPropertyChanged",
            'click label[for="ConsentToSearch"] a': 'showConsent',
            'focus #OverallTurnOver': "overallTurnOverFocus",
            'focus #WebSiteTurnOver': "webSiteTurnOverFocus",

            'change input': 'inputChanged',
            'focusout input': 'inputChanged',
            'keyup input': 'inputChanged',

            'focus select': 'inputChanged',
            'focusout select': 'inputChanged',
            'keyup select': 'inputChanged',
            'click select': 'inputChanged'
        });

        this.constructor.__super__.initialize.call(this);
    },

    inputChanged: function (event) {
        var el = event ? $(event.currentTarget) : null;
        if (el && el.attr('id') == 'MiddleInitial' && el.val() == '') {
            var img = el.closest('div').find('.field_status');
            img.field_status('set', 'empty', 2);
        }

        var enabled = EzBob.Validation.checkForm(this.validator) &&
            this.isPrevAddressValid() &&
            this.isAddressValid() &&
            this.OwnOtherPropertyIsValid();

        $('.continue').toggleClass('disabled', !enabled);
    },

    isAddressValid: function () {
        var oAddress = this.model.get('PersonalAddress');
        return oAddress && (oAddress.length > 0);
    },

    overallTurnOverFocus: function () {
        $("#OverallTurnOver").change();
    },
    webSiteTurnOverFocus: function () {
        $("#WebSiteTurnOver").change();
    },
    ResidentialStatusChanged: function () {
        var oCombo = this.$el.find('#OwnOtherProperty');

        if (oCombo.length < 1)
            return;

        switch (this.$el.find('#ResidentialStatus').val()) {
            case 'Home owner':
                this.$el.find('#OwnOtherPropertyQuestion').text('Do you own another property?');
                oCombo.find('option[value="Yes"]').text('Yes, I own another property.');
                oCombo.find('option[value="No"]').text('No, I don\'t own another property.');
                break;
            default:
                this.$el.find('#OwnOtherPropertyQuestion').text('Do you own a property?');
                oCombo.find('option[value="Yes"]').text('Yes, I own a property.');
                oCombo.find('option[value="No"]').text('No, I don\'t own a property.');
                break;
        } // switch
    },
    PersonalTimeAtAddressChanged: function () {
        this.clearPrevAddressModel();
        this.TimeAtAddressChanged("#PrevPersonAddresses", "#TimeAtAddress");
    },
    OwnOtherPropertyChanged: function (evt) {
        if (this.$el.find('#OwnOtherProperty').val() == 'Yes')
            this.$el.find('#OtherPropertyAddress').parents('div.control-group').show();
        else
            this.$el.find('#OtherPropertyAddress').parents('div.control-group').hide();

        this.inputChanged();
    },
    OwnOtherPropertyIsValid: function () {
        if (!this.model.get('IsOffline'))
            return true;

        switch (this.$el.find('#OwnOtherProperty').val()) {
            case 'Yes':
                {
                    var oModel = this.model.get('OtherPropertyAddress');
                    return oModel && (oModel.length > 0);
                }
            case 'No':
                return true;
            default:
                return false;
        } // switch
    },
    render: function () {
        this.constructor.__super__.render.call(this);

        var personalAddressView = new EzBob.AddressView({ model: this.model.get('PersonalAddress'), name: "PersonalAddress", max: 1 });
        personalAddressView.render().$el.appendTo(this.$el.find('#PersonalAddress'));
        this.addressErrorPlacement(personalAddressView.$el, personalAddressView.model);

        var prevPersonAddressesView = new EzBob.AddressView({ model: this.model.get('PrevPersonAddresses'), name: "PrevPersonAddresses", max: 3 });
        prevPersonAddressesView.render().$el.appendTo(this.$el.find('#PrevPersonAddresses'));
        this.$el.find('#PrevPersonAddresses .addAddressContainer label.attardi-input span').text('Enter previous postcode');
        this.addressErrorPlacement(prevPersonAddressesView.$el, prevPersonAddressesView.model);

        var otherPropertyAddressView = new EzBob.AddressView({ model: this.model.get('OtherPropertyAddress'), name: "OtherPropertyAddress", max: 1 });
        otherPropertyAddressView.render().$el.appendTo(this.$el.find('#OtherPropertyAddress'));
        this.addressErrorPlacement(otherPropertyAddressView.$el, otherPropertyAddressView.model);

        this.model.get('PrevPersonAddresses').on("all", this.PrevModelChange, this);
        this.model.get('PersonalAddress').on("all", this.PersonalAddressModelChange, this);
        this.model.get('OtherPropertyAddress').on("all", this.OtherPropertyAddressModelChange, this);
        this.$el.find(".cashInput").moneyFormat();

        if (!this.model.get('IsOffline'))
            this.$el.find('.offline').remove();

        var oPersonalInfo = this.model.get('CustomerPersonalInfo');
        if (oPersonalInfo) {
            var sGender = '';
            switch (oPersonalInfo.Gender) {
                case 0:
                    sGender = 'M';
                    break;
                case 1:
                    sGender = 'F';
                    break;
            } // switch
            this.$el.find('#FormRadioCtrl_' + sGender).prop('checked', true);

            var aryBirthDate = (this.model.get('BirthDateYMD') || '').split('-');
            if (aryBirthDate.length == 3) {
                this.$el.find('#DateOfBirthDay').val(aryBirthDate[2]);
                this.$el.find('#DateOfBirthMonth').val(aryBirthDate[1]);
                this.$el.find('#DateOfBirthYear').val(aryBirthDate[0]);
            } // if birthdate

            var aryMaritalStatus = ['Married', 'Single', 'Divorced', 'Widow/er', 'Other'];
            if (aryMaritalStatus[oPersonalInfo.MartialStatus])
                this.$el.find('#MartialStatus').val(aryMaritalStatus[oPersonalInfo.MartialStatus]);

            this.$el.find('#TimeAtAddress').val(oPersonalInfo.TimeAtAddress);
            this.$el.find('#ResidentialStatus').val(oPersonalInfo.ResidentialStatus);
            this.$el.find('#DayTimePhone').val(oPersonalInfo.DaytimePhone);
            this.$el.find('#TypeOfBusiness').val(oPersonalInfo.TypeOfBusinessName);
            this.$el.find('#WebSiteTurnOver').autoNumericSet(oPersonalInfo.WebSiteTurnOver);
            this.$el.find('#OverallTurnOver').autoNumericSet(oPersonalInfo.OverallTurnOver);
        } // if has personal info

        this.$el.find('input, select:not(.form_field_date[value="-"])').change();

        this.inputChanged();
    },

    showConsent: function () {
        var consentAgreementModel = new EzBob.ConsentAgreementModel({
            id: this.model.get('Id'),
            firstName: this.$el.find("input[name='FirstName']").val(),
            middleInitial: this.$el.find("input[name='MiddleInitial']").val(),
            surname: this.$el.find("input[name='Surname']").val()
        });

        var consentAgreement = new EzBob.ConsentAgreement({ model: consentAgreementModel });
        EzBob.App.modal.show(consentAgreement);
        return false;
    },
    PersonalAddressModelChange: function (e, el) {
        this.inputChanged();
        this.clearAddressError("#PersonalAddress");
    },
    OtherPropertyAddressModelChange: function (e, el) {
        this.inputChanged();
        this.clearAddressError("#OtherPropertyAddress");
    },

    clearPrevAddressModel: function () {
        this.model.get('PrevPersonAddresses').remove(this.model.get('PrevPersonAddresses').models);
    },

    removeSpaces: function () {
        this.$el.find("input[name='FirstName']").val(this.$el.find("input[name='FirstName']").val().trim());
        this.$el.find("input[name='MiddleInitial']").val(trim(this.$el.find("input[name='MiddleInitial']").val().trim()));
        this.$el.find("input[name='Surname']").val(this.$el.find("input[name='Surname']").val().trim());
    },

    getValidator: function () {
        return EzBob.validatePersonalDetailsForm;
    },

    next: function (e) {
        var $el = $(e.currentTarget);

        if ($el.hasClass("disabled"))
            return false;

        this.trigger('next');
        return false;
    }
});
var EzBob = EzBob || {};

EzBob.PersonalInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {

        this.template = _.template($('#personinfo-template').html());
        this.ViewName = "personal";
        this.PrevAddressValidator = false;
        this.AddressValidator = false;
        this.type = "Personal";

        this.events = _.extend({}, this.events, {
            "change #TimeAtAddress": "PersonalTimeAtAddressChanged",
            "change #OwnOtherProperty": "OwnOtherPropertyChanged",
            'change select[name="TypeOfBusiness"]': "typeChanged",
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
            this.PrevAddressValidator && this.AddressValidator
            && this.OwnOtherPropertyIsValid();

        $('.continue').toggleClass('disabled', !enabled);
    },
    
    overallTurnOverFocus: function () {
        $("#OverallTurnOver").change();
    },
    webSiteTurnOverFocus: function () {
        $("#WebSiteTurnOver").change();
    },
    PersonalTimeAtAddressChanged: function () {
        this.clearPrevAddressModel();
        this.TimeAtAddressChanged("#PrevPersonAddresses", "#TimeAtAddress");
    },
	OwnOtherPropertyChanged: function(evt) {
        if (this.$el.find('#OwnOtherProperty').val() == 'Yes')
            this.$el.find('#OtherPropertyAddress').parents('div.control-group').show();
        else
            this.$el.find('#OtherPropertyAddress').parents('div.control-group').hide();

        this.inputChanged();
    },
	OwnOtherPropertyIsValid: function() {
		switch (this.$el.find('#OwnOtherProperty').val()) {
		case 'Yes': {
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
        var that = this;
        
        var personalAddressView = new EzBob.AddressView({ model: this.model.get('PersonalAddress'), name: "PersonalAddress", max: 1 });
        personalAddressView.render().$el.appendTo(this.$el.find('#PersonalAddress'));
        this.addressErrorPlacement(personalAddressView.$el, personalAddressView.model);

        var prevPersonAddressesView = new EzBob.AddressView({ model: this.model.get('PrevPersonAddresses'), name: "PrevPersonAddresses", max: 3 });
        prevPersonAddressesView.render().$el.appendTo(this.$el.find('#PrevPersonAddresses'));
        this.$el.find('#PrevPersonAddresses .addAddressContainer label.attardi-input span').text('Enter Previous Postcode');
        this.addressErrorPlacement(prevPersonAddressesView.$el, prevPersonAddressesView.model);

        var otherPropertyAddressView = new EzBob.AddressView({ model: this.model.get('OtherPropertyAddress'), name: "OtherPropertyAddress", max: 1 });
        otherPropertyAddressView.render().$el.appendTo(this.$el.find('#OtherPropertyAddress'));
        this.addressErrorPlacement(otherPropertyAddressView.$el, otherPropertyAddressView.model);

        this.model.get('PrevPersonAddresses').on("all", this.PrevModelChange, this);
        this.model.get('PersonalAddress').on("all", this.PersonalAddressModelChange, this);
        this.model.get('OtherPropertyAddress').on("all", this.OtherPropertyAddressModelChange, this);
        this.$el.find("#WebSiteTurnOver").moneyFormat();
        this.$el.find("#OverallTurnOver").moneyFormat();
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
        this.AddressValidator = el.collection && el.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#PersonalAddress");
    },
    OtherPropertyAddressModelChange: function (e, el) {
        this.AddressValidator = el.collection && el.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#OtherPropertyAddress");
    },
    typeChanged: function (e) {
        this.type = e.target.value;
        var buttonName = this.type == "Entrepreneur" ? "Complete" : "Continue";
        this.$el.find('.btn-next').text(buttonName);
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
        if ($el.hasClass("disabled")) return false;
        this.trigger('next', this.type);
        return false;
    }
});
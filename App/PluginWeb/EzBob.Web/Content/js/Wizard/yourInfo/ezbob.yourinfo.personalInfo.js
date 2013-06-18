var EzBob = EzBob || {};

EzBob.PersonalInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {

        this.template = _.template($('#personinfo-template').html());
        this.ViewName = "personal";
        this.PrevAddressValidator = false;
        this.AddressValidator = false;
        this.type = "Personal";

        this.agree = false;

        this.day = false;
        this.month = false;
	    this.year = false;

        this.events = _.extend({}, this.events, {
            "change #TimeAtAddress": "PersonalTimeAtAddressChanged",
            'change select[name="TypeOfBusiness"]': "typeChanged",
            'change input[name="ConsentToSearch"]': 'consentToSearchChanged',
            'click label[for="ConsentToSearch"] a': 'showConsent',
            'change select[id="DateOfBirthYear"]': "dateOfBirthYearChanged",
            'change select[id="DateOfBirthMonth"]': "dateOfBirthMonthChanged",
            'change select[id="DateOfBirthDay"]': "dateOfBirthDayChanged",
            'focus #OverallTurnOver': "overallTurnOverFocus",
            'focus #WebSiteTurnOver': "webSiteTurnOverFocus",
        });

        this.constructor.__super__.initialize.call(this);
    },
    overallTurnOverFocus: function () {
        $("#OverallTurnOver").change();
    },
    webSiteTurnOverFocus: function () {
        $("#WebSiteTurnOver").change();
    },
    dateOfBirthChanged: function () {
	    if (this.year && this.month && this.day) {
		    EzBob.Validation.displayIndication(this.validator, "DateOfBirthImage", "#DateOfBirth");
	    }
    },
    dateOfBirthYearChanged: function () {
    	this.year = true;
    	this.dateOfBirthChanged();
    },
    dateOfBirthMonthChanged: function () {
    	this.month = true;
    	this.dateOfBirthChanged();
    },
    dateOfBirthDayChanged: function () {
    	this.day = true;
    	this.dateOfBirthChanged();
    },
    PersonalTimeAtAddressChanged: function () {
        this.clearPrevAddressModel();
        this.TimeAtAddressChanged("#PrevPersonAddresses", "#TimeAtAddress");
    },

    render: function () {
        this.constructor.__super__.render.call(this);
        var that = this;

        //this.$el.find(':radio[value="' + this.type + '"]').attr('checked', 'checked');

        var personalAddressView = new EzBob.AddressView({ model: this.model.get('PersonalAddress'), name: "PersonalAddress", max: 1 });
        personalAddressView.render().$el.appendTo(this.$el.find('#PersonalAddress'));

        var prevPersonAddressesView = new EzBob.AddressView({ model: this.model.get('PrevPersonAddresses'), name: "PrevPersonAddresses", max: 3 });
        prevPersonAddressesView.render().$el.appendTo(this.$el.find('#PrevPersonAddresses'));
        this.$el.find('#PrevPersonAddresses .addAddressContainer label.attardi-input span').text('Enter Previous Postcode');

        this.model.get('PrevPersonAddresses').on("all", this.PrevModelChange, this);
        this.model.get('PersonalAddress').on("all", this.PersonalAddressModelChange, this);
        this.$el.find("#WebSiteTurnOver").moneyFormat();
        this.$el.find("#OverallTurnOver").moneyFormat();
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
        this.clearAddressError("#PersonalAddress");
    },
    typeChanged: function (e) {
        this.type = e.target.value;
        var buttonName = this.type == "Entrepreneur" ? "Complete" : "Continue";
        this.$el.find('.btn-next').text(buttonName);
        EzBob.Validation.displayIndication(this.validator, "TypeOfBusinessImage", "#TypeOfBusiness", "#RotateImage", "#OkImage", "#FailImage");
    },
    consentToSearchChanged: function (e) {
        this.agree = $(e.target).is(':checked');
        this.$el.find('.btn-next').toggleClass('disabled', !this.agree);
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
        if ($el.hasClass("disabled")) {
            return false;
        }

        scrollTop();
        if (!this.validator.form() || !this.PrevAddressValidator || !this.AddressValidator) {
            if (!this.PrevAddressValidator)
                this.addAddressError("#PrevPersonAddresses");
            if (!this.AddressValidator)
                this.addAddressError("#PersonalAddress");
            if (!this.validator.form())
                EzBob.App.trigger("error", "You must fill in all of the fields.");

            return false;
        }

        EzBob.App.trigger("clear");

        if (!this.agree) return false;

        this.trigger('next', this.type);
        return false;
    }
});
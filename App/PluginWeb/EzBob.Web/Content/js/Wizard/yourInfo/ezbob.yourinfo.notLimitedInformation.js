var EzBob = EzBob || {};

EzBob.NonLimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#nonlimitededinfo-template').html());
        this.ViewName = "NonLimited";
        this.companyAddressValidator = false;

        this.parentView = this.options.parentView;
    }, // initialize

    readyToContinue: function() {
        return this.companyAddressValidator &&
            this.directorsView.validateDuplicates() &&
            this.directorsView.validateAddresses() &&
            (!this.employeeCountView || this.employeeCountView.isValid());
    }, // readyToContinue

    inputChanged: function () {
        this.parentView.inputChanged();
    }, // inputChanged

    propertyOwnedByCompanyChanged: function (event) {
        var toToggle = this.$el.find('#NonLimitedPropertyOwnedByCompany').val() !== 'false';
        this.$el.find('.additionalCompanyAddressQuestions').toggleClass('hide', toToggle);
        this.inputChanged(event);
    }, // propertyOwnedByCompanyChanged

    next: function (e) {
        var $el = $(e.currentTarget);

        if ($el.hasClass("disabled"))
            return false;

        if (!this.parentView.isEnabled())
            return false;

        this.trigger('next');
        return false;
    }, // next

    render: function () {
        var self = this;
        this.constructor.__super__.render.call(this);

        var oAddressContainer = this.$el.find('#NonLimitedCompanyAddress');
        var nonLimitedAddressView = new EzBob.AddressView({
            model: this.model.get('CompanyAddress').reset(),
            name: "NonLimitedCompanyAddress",
            max: 1,
            title: "Enter company postcode",
            uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix'),
        });
        nonLimitedAddressView.render().$el.appendTo(oAddressContainer);
        this.model.get('CompanyAddress').on("all", this.NonLimitedCompanyAddressChanged, this);
        EzBob.Validation.addressErrorPlacement(nonLimitedAddressView.$el, nonLimitedAddressView.model);

        this.employeeCountView = new EzBob.EmployeeCountView({
            model: this.model,
            onchange: $.proxy(self.inputChanged, self),
            prefix: "NonLimited"
        });
        this.employeeCountView.render().$el.appendTo(this.$el.find('.employee-count'));

	    this.directorsView = new EzBob.DirectorMainView({
	    	model: this.model.get('NonLimitedDirectors'),
	    	name: "nonlimitedDirectors",
        	customerInfo: _.extend({},
				this.model.get('CustomerPersonalInfo'),
				{ PostCode: this.model.get('PersonalAddress').models[0].get('Rawpostcode'), }
			),
	    });
        this.directorsView.on("director:change", this.inputChanged, this);
        this.directorsView.on("director:addressChanged", this.inputChanged, this);
        this.directorsView.render().$el.appendTo(this.$el.find('.directors'));

        this.$el.find(".addressCaption").hide();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        EzBob.UiAction.registerView(this);

        return this;
    }, // render

    getValidator: function () {
        return EzBob.validateNonLimitedCompanyDetailForm;
    }, // getValidator

    NonLimitedCompanyAddressChanged: function (evt, oModel) {
        this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#NonLimitedCompanyAddress");
    }, // NonLimitedCompanyAddressChanged

    ownValidationRules: function () {
        return {
            NonLimitedCompanyName: { required: true, minlength: 2 },
            NonLimitedTimeInBusiness: { required: true },
            CapitalExpenditure: { required: true, defaultInvalidPounds: true },
            TotalMonthlySalary: { required: true, defaultInvalidPounds: true, regex: "^(?!£ 0.00$)" },
            OverallTurnOver: { required: true, defaultInvalidPounds: true, regex: "^(?!£ 0.00$)" },
        };
    }, // ownValidationRules

    ownValidationMessages: function () {
        return {
            CapitalExpenditure: { defaultInvalidPounds: "This field is required" },
            TotalMonthlySalary: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
            OverallTurnOver: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
        };
    }, // ownValidationMessages
});
﻿var EzBob = EzBob || {};

EzBob.LimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#limitedinfo-template').html());
        this.ViewName = "Limited";
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

    render: function () {
        var self = this;
        this.constructor.__super__.render.call(this);

        var oAddressContainer = this.$el.find('#LimitedCompanyAddress');
        var limitedAddressView = new EzBob.AddressView({
            model: this.model.get('CompanyAddress').reset(),
            name: "LimitedCompanyAddress",
            max: 1,
            title: "Enter company postcode",
            uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix'),
        });
        limitedAddressView.render().$el.appendTo(oAddressContainer);
        this.model.get('CompanyAddress').on("all", this.LimitedCompanyAddressChanged, this);
        EzBob.Validation.addressErrorPlacement(limitedAddressView.$el, limitedAddressView.model);


        this.employeeCountView = new EzBob.EmployeeCountView({
            model: this.model,
            onchange: $.proxy(self.inputChanged, self),
            prefix: "Limited"
        });
        this.employeeCountView.render().$el.appendTo(this.$el.find('.employee-count'));

        this.directorsView = new EzBob.DirectorMainView({
        	model: this.model.get('LimitedDirectors'),
        	name: 'limitedDirectors',
        	customerInfo: _.extend({},
				this.model.get('CustomerPersonalInfo'),
				{ PostCode: this.model.get('PersonalAddress').models[0].get('Rawpostcode'), }
			),
        });
        this.directorsView.on("director:change", this.inputChanged, this);
        this.directorsView.on("director:addressChanged", this.inputChanged, this);
        this.directorsView.render().$el.appendTo(this.$el.find('.directors'));

        this.$el.find(".addressCaption").hide();
        this.$el.find("#LimitedCompanyNumber").withoutSpaces();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        EzBob.UiAction.registerView(this);

        return this;
    }, // render

    getValidator: function () {
        return EzBob.validateLimitedCompanyDetailForm;
    }, // getValidator

    next: function (e) {
        var $el = $(e.currentTarget);

        if ($el.hasClass("disabled"))
            return false;

        this.trigger('next');
        return false;
    }, // next

    LimitedCompanyAddressChanged: function (evt, oModel) {
        this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#LimitedCompanyAddress");
    }, // LimitedCompanyAddressChanged

    ownValidationRules: function () {
        return {
            LimitedCompanyNumber: { required: true, maxlength: 255, regex: "^[a-zA-Z0-9]+$" },
            LimitedCompanyName: { required: true, minlength: 2 },
            CapitalExpenditure: { required: true, defaultInvalidPounds: true },
            TotalMonthlySalary: { required: true, defaultInvalidPounds: true, regex: "^(?!£ 0.00$)" },
            OverallTurnOver: { required: true, defaultInvalidPounds: true, regex: "^(?!£ 0.00$)" },
        };
    }, // ownValidationRules

    ownValidationMessages: function () {
        return {
            LimitedCompanyNumber: { regex: "Please enter a valid company number" },
            CapitalExpenditure: { defaultInvalidPounds: "This field is required" },
            TotalMonthlySalary: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
            OverallTurnOver: { defaultInvalidPounds: "This field is required", regex: "This field is required" },
        };
    }, // ownValidationMessages
});
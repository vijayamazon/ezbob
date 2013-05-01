var EzBob = EzBob || {};

EzBob.NonLimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#nonlimitededinfo-template').html());
        this.ViewName = "NonLimited";
        this.companyAddressValidator = false;
        this.events = _.extend({}, this.events, {
            'change input[name="NonLimitedCompanyName"]': 'nonLimitedCompanyNameChanged',
            'change input[name="NonLimitedBusinessPhone"]': 'nonLimitedBusinessPhoneChanged',
            'change select[name="NonLimitedTimeAtAddress"]': "nonLimitedTimeAtAddressChanged",
            'change select[name="NonLimitedTimeInBusiness"]': "nonLimitedTimeInBusinessChanged"
        });
    },
    nonLimitedCompanyNameChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "NonLimitedCompanyNameImage", "#NonLimitedCompanyName");
    },
    nonLimitedBusinessPhoneChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "NonLimitedBusinessPhoneImage", "#NonLimitedBusinessPhone");
    },
    nonLimitedTimeAtAddressChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "NonLimitedTimeAtAddressImage", "#NonLimitedTimeAtAddress");
    },
    nonLimitedTimeInBusinessChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "NonLimitedTimeInBusinessImage", "#NonLimitedTimeInBusiness");
    },
    next: function () {
        if (!this.validator.form() || !this.companyAddressValidator) {
            if (!this.companyAddressValidator) this.addAddressError("#NonLimitedCompanyAddress");
            if (!this.validator.form()) EzBob.App.trigger("error", "You must fill in all of the fields.");
            return false;
        }

        this.trigger('next');
        return false;
    },
    render: function () {
        this.constructor.__super__.render.call(this);

        var nonLimitedAddressView = new EzBob.AddressView({ model: this.model.get('NonLimitedCompanyAddress'), name: "NonLimitedCompanyAddress", max: 1 });
        nonLimitedAddressView.render().$el.appendTo(this.$el.find('#NonLimitedCompanyAddress'));
        this.model.get('NonLimitedCompanyAddress').on("change", this.NotLimitedCompanyAddressChanged, this);

        var directorsView = new EzBob.DirectorMainView({ model: this.model.get('NonLimitedDirectors'), name: "nonlimitedDirectors", hidden: this.$el.find('.directorsData') });
        directorsView.render().$el.appendTo(this.$el.find('.directors'));

        this.$el.find(".cashInput").cashEdit();
        this.$el.find(".addressCaption").hide();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        return this;
    },
    NotLimitedCompanyAddressChanged: function (el, e) {
        this.companyAddressValidator = el.collection.length > 0;
        this.clearAddressError("#NonLimitedCompanyAddress");
    }
});
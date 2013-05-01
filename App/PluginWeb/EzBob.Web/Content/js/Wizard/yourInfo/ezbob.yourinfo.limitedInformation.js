var EzBob = EzBob || {};

EzBob.LimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#limitedinfo-template').html());
        this.ViewName = "Limited";
        this.companyAddressValidator = false;

        this.events = _.extend({}, this.events, {
            'change input[name="LimitedCompanyName"]': 'limitedCompanyNameChanged',
            'change input[name="LimitedCompanyNumber"]': 'limitedCompanyNumberChanged',
            'change input[name="LimitedBusinessPhone"]': 'limitedBusinessPhoneChanged'
        });
    },
    limitedCompanyNameChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "LimitedCompanyNameImage", "#LimitedCompanyName");
    },
    limitedCompanyNumberChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "LimitedCompanyNumberImage", "#LimitedCompanyNumber");
    },
    limitedBusinessPhoneChanged: function () {
    	EzBob.Validation.displayIndication(this.validator, "LimitedBusinessPhoneImage", "#LimitedBusinessPhone");
    },
    render: function () {
        this.constructor.__super__.render.call(this);

        var limitedAddressView = new EzBob.AddressView({ model: this.model.get('LimitedCompanyAddress'), name: "LimitedCompanyAddress", max: 1 });
        limitedAddressView.render().$el.appendTo(this.$el.find('#LimitedCompanyAddress'));
        this.model.get('LimitedCompanyAddress').on("change", this.LimitedCompanyAddressChanged, this);

        var directorsView = new EzBob.DirectorMainView({ model: this.model.get('LimitedDirectors'), name: 'limitedDirectors', hidden: this.$el.find('.directorsData') });
        directorsView.render().$el.appendTo(this.$el.find('.directors'));

        this.$el.find(".cashInput").cashEdit();
        this.$el.find(".addressCaption").hide();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });
    },
    next: function () {
        if (!this.validator.form() || !this.companyAddressValidator) {
            if (!this.companyAddressValidator) this.addAddressError("#LimitedCompanyAddress");
            if (!this.validator.form()) EzBob.App.trigger("error", "You must fill in all of the fields.");
            return false;
        }

        this.trigger('next');
        return false;
    },
    LimitedCompanyAddressChanged: function (el, e) {
        this.companyAddressValidator = el.collection.length > 0;
        this.clearAddressError("#LimitedCompanyAddress");

    }
});
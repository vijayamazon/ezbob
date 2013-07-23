var EzBob = EzBob || {};

EzBob.NonLimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function() {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#nonlimitededinfo-template').html());
        this.ViewName = "NonLimited";
        this.companyAddressValidator = false;
        this.events = _.extend({}, this.events, {
            'change input': 'inputChanged',
            'keyup input': 'inputChanged',
            'focusout input': 'inputChanged'
        });
    },
    inputChanged: function () {
        var el = event ? $(event.currentTarget) : null;
        if (el && el.hasClass('nonrequired') && el.val() == '') {
            var img = el.closest('div').find('.field_status');
            img.field_status('set', 'empty', 2);
        }

        var enabled = EzBob.Validation.checkForm(this.validator) && this.companyAddressValidator && this.directorsView.validateAddresses();
        $('.continue').toggleClass('disabled', !enabled);
    },
    next: function(e) {
        var $el = $(e.currentTarget);
        if ($el.hasClass("disabled")) return false;
        this.trigger('next');
        return false;
    },
    render: function() {
        this.constructor.__super__.render.call(this);

        var nonLimitedAddressView = new EzBob.AddressView({ model: this.model.get('NonLimitedCompanyAddress').reset(), name: "NonLimitedCompanyAddress", max: 1 });
        nonLimitedAddressView.render().$el.appendTo(this.$el.find('#NonLimitedCompanyAddress'));
        this.model.get('NonLimitedCompanyAddress').on("all", this.NonLimitedCompanyAddressChanged, this);
        this.addressErrorPlacement(nonLimitedAddressView.$el, nonLimitedAddressView.model);

        this.directorsView = new EzBob.DirectorMainView({ model: this.model.get('NonLimitedDirectors'), name: "nonlimitedDirectors", hidden: this.$el.find('.directorsData'), validator: this.validator });
        this.directorsView.on("director:change", this.inputChanged, this);
        this.directorsView.on("director:addressChanged", this.inputChanged, this);
        this.directorsView.render().$el.appendTo(this.$el.find('.directors'));

        this.$el.find(".cashInput").cashEdit();
        this.$el.find(".addressCaption").hide();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        return this;
    },	

    getValidator: function() {
        return EzBob.validateNonLimitedCompanyDetailForm;
    },

    NonLimitedCompanyAddressChanged: function(evt, oModel) {
        this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#NonLimitedCompanyAddress");
    }
});
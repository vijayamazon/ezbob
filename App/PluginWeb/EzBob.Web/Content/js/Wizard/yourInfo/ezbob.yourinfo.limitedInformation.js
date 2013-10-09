var EzBob = EzBob || {};

EzBob.LimitedInformationView = EzBob.YourInformationStepViewBase.extend({
    initialize: function () {
        this.constructor.__super__.initialize.call(this);
        this.template = _.template($('#limitedinfo-template').html());
        this.ViewName = "Limited";
        this.companyAddressValidator = false;

        this.events = _.extend({}, this.events, {
            'change   input': 'inputChanged',
            'keyup    input': 'inputChanged',
            'focusout input': 'inputChanged',
            'click    input': 'inputChanged',
            'change #LimitedPropertyOwnedByCompany': 'propertyOwnedByCompanyChanged',
            'change   select': 'inputChanged',
            'keyup    select': 'inputChanged',
            'focusout select': 'inputChanged',
            'click    select': 'inputChanged'
        });
    },

    inputChanged: function (event) {
        var el = event ? $(event.currentTarget) : null;

        if (el && el.hasClass('nonrequired') && el.val() == '') {
            var img = el.closest('div').find('.field_status');
            img.field_status('set', 'empty', 2);
        } // if

        this.setContinueStatus();
    },

    setContinueStatus: function () {
        var enabled = EzBob.Validation.checkForm(this.validator) &&
			this.companyAddressValidator &&
			this.directorsView.validateAddresses() &&
			((this.employeeCountView == null) || this.employeeCountView.isValid());

        $('.continue').toggleClass('disabled', !enabled);
    },

    propertyOwnedByCompanyChanged: function (event) {
        var toToggle = this.$el.find('#LimitedPropertyOwnedByCompany').val() != 'false';
        this.$el.find('.additionalCompanyAddressQuestions').toggleClass('hide', toToggle);
        this.inputChanged(event);
    },

    render: function () {
        this.constructor.__super__.render.call(this);

        var limitedAddressView = new EzBob.AddressView({ model: this.model.get('LimitedCompanyAddress').reset(), name: "LimitedCompanyAddress", max: 1 });
        limitedAddressView.render().$el.appendTo(this.$el.find('#LimitedCompanyAddress'));
        this.model.get('LimitedCompanyAddress').on("all", this.LimitedCompanyAddressChanged, this);
        this.addressErrorPlacement(limitedAddressView.$el, limitedAddressView.model);

        this.directorsView = new EzBob.DirectorMainView({ model: this.model.get('LimitedDirectors'), name: 'limitedDirectors', hidden: this.$el.find('.directorsData'), validator: this.validator });
        this.directorsView.on("director:change", this.inputChanged, this);
        this.directorsView.on("director:addressChanged", this.inputChanged, this);
        this.directorsView.render().$el.appendTo(this.$el.find('.directors'));

        if (this.model.get('IsOffline')) {
            this.employeeCountView = new EzBob.EmployeeCountView({
                model: this.model,
                parentView: self,
                onchange: self.setContinueStatus
            });
            this.employeeCountView.render().$el.appendTo(this.$el.find('.employee-count'));
        }
        else {
            this.$el.find('.offline').remove();
            this.employeeCountView = null;
        } // if

        this.$el.find(".cashInput").cashEdit();
        this.$el.find(".addressCaption").hide();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        return this;
    },

    getValidator: function () {
        return EzBob.validateLimitedCompanyDetailForm;
    },

    next: function (e) {
        var $el = $(e.currentTarget);

        if ($el.hasClass("disabled"))
            return false;

        this.trigger('next');
        return false;
    },

    LimitedCompanyAddressChanged: function (evt, oModel) {
        this.companyAddressValidator = oModel.collection && oModel.collection.length > 0;
        this.inputChanged();
        this.clearAddressError("#LimitedCompanyAddress");
    }
});
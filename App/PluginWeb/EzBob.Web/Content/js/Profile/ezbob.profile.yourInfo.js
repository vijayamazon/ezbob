var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.YourInfoMainView = Backbone.View.extend({
    initialize: function() {
        this.template = _.template($('#your-info-template').html());
        this.model.on("reset", this.render, this);
        this.addressValidator = true;
        this.companyAddressValidator = true;
    },

    events: {
        'click .edit-personal': 'editPersonalViewShow',
        'click .submit-personal': 'saveData',
        'click .cancel': 'cancel',
        'change .personEditInput': 'inputChanged',
        'keyup .personEditInput': 'inputChanged'
    },

    cancel: function() {
        this.reload();
    },

    inputChanged: function () {
        var isValid = (!this.validator.form() || !this.addressValidator || !this.companyAddressValidator);
        this.$el.find('.submit-personal').toggleClass('disabled', isValid);
    },

    reload: function () {
        var xhr = this.model.fetch(),
            that = this;
        xhr.done(function () {
            that.render();
            scrollTop();
            that.setInputReadOnly(true);
        });
    },
    
    editPersonalViewShow: function() {
        this.setInputReadOnly(false);
        return false;
    },

    addressModelChange: function() {
        var address = this.model.get('PersonalAddress');
        this.addressValidator = address.length > 0;
        this.setError('#PersonalAddress', !this.addressValidator);
        var typeOfBusinessName = this.model.get('BusinessTypeReduced');

        if (typeOfBusinessName == "Limited") {
            var limitedAddress = this.model.get('LimitedCompanyAddress');
            this.companyAddressValidator = limitedAddress.length > 0;
            this.setError('#LimitedCompanyAddress', !this.companyAddressValidator);

        } else if (typeOfBusinessName == "NonLimited") {
            var nonLimitedAddress = this.model.get('NonLimitedCompanyAddress');
            this.companyAddressValidator = nonLimitedAddress.length > 0;
            this.setError('#NonLimitedAddress', !this.companyAddressValidator);
        }
        this.inputChanged();
    },

    setError: function(element, isError) {
        if (isError) {
            this.addAddressError(element);
        } else {
            this.clearAddressError(element);
        }
    },

    setInputReadOnly: function(isReadOnly) {
        this.$el.find('.personEditInput').attr('readonly', isReadOnly);
        
        this.$el.find('.personEditInput').attr('modifed', !isReadOnly);
        this.$el.find('.addAddressInput').attr('modifed', !isReadOnly);

        if (isReadOnly) {
            this.$el.find('.submit-personal').hide();
            this.$el.find('.cancel').hide();
            this.$el.find('.addressEdit').hide();
            this.$el.find('.addressShow').show();
            this.$el.find('.edit-personal').show();
        } else {
            this.$el.find('.submit-personal').show();
            this.$el.find('.cancel').show();
            this.$el.find('.addressEdit').show();
            this.$el.find('.addressShow').hide();
            this.$el.find('.edit-personal').hide();
        }
    },

    addAddressError: function(el) {
        var error = $('<label class="error" generated="true">This field is required</label>');
        EzBob.Validation.errorPlacement(error, this.$el.find(el));
    },
    clearAddressError: function(el) {
        EzBob.Validation.unhighlight(this.$el.find(el));
    },

    saveData: function() {
        if (!this.validator.form() || !this.addressValidator || !this.companyAddressValidator) {
            EzBob.App.trigger("error", "You must fill in all of the fields.");
            return false;
        }
        var data = this.form.serializeArray(),
            action = this.form.attr('action'),
            that = this;
        var request = $.post(action, data);

        request.success(function() {
            that.reload();
            EzBob.App.trigger('info', "Your information updated successfully");
        });

        request.fail(function() {
            EzBob.App.trigger('error', "Business check service temporary unavaliable, please contact with system administrator", "");
        });
    },

    render: function () {    
        
        this.$el.html(this.template(this.model.toJSON()));
        this.form = this.$el.find('form.editYourInfoForm');
        this.validator = EzBob.validateYourInfoEditForm(this.form);
        this.renderPersonalInfoView();
        this.renderCompanyInfoView();
        this.$el.find('.phonenumber').numericOnly(11);
        this.$el.find('.cashInput').numericOnly(11);
        
        this.setInputReadOnly(true);
        return this;
    },
    
    renderCompanyInfoView: function () {
        this.companyInfoContainer = this.$el.find('.company-info');
        var typeOfBusinessName = this.model.get('BusinessTypeReduced');

        if (typeOfBusinessName == "Limited") {
            var limitedInfoView = new EzBob.Profile.LimitedInfoView({ model: this.model });
            limitedInfoView.render().$el.appendTo(this.companyInfoContainer);
            this.model.get('LimitedCompanyAddress').on("all", this.addressModelChange, this);

        } else if (typeOfBusinessName == "NonLimited") {
            var nonlimitedInfoView = new EzBob.Profile.NonLimitedInfoView({ model: this.model });
            nonlimitedInfoView.render().$el.appendTo(this.companyInfoContainer);
            this.model.get('NonLimitedCompanyAddress').on("all", this.addressModelChange, this);
        }
    },

    renderPersonalInfoView: function () {
        this.personalInfoContainer = this.$el.find('.personal-info');
        var personalInfoView = new EzBob.Profile.PersonalInfoView({ model: this.model });
        personalInfoView.render().$el.appendTo(this.personalInfoContainer);
        this.model.get('PersonalAddress').on("all", this.addressModelChange, this);
    },
});

EzBob.Profile.PersonalInfoView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#personal-info-template').html());
    },
    
    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        var personalAddressView = new EzBob.AddressView({ model: this.model.get('PersonalAddress'), name: "PersonalAddress", max: 10, isShowClear:true });
        personalAddressView.render().$el.appendTo(this.$el.find('#PersonalAddress'));
        return this;
    }
});

EzBob.Profile.LimitedInfoView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#limited-info-template').html());
    },
    
    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        var limitedAddressView = new EzBob.AddressView({ model: this.model.get('LimitedCompanyAddress'), name: "LimitedCompanyAddress", max: 10, isShowClear: true });
        limitedAddressView.render().$el.appendTo(this.$el.find('#LimitedCompanyAddress'));
        return this;
    }
});

EzBob.Profile.NonLimitedInfoView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#nonlimited-info-template').html());
    },

    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        var limitedAddressView = new EzBob.AddressView({ model: this.model.get('NonLimitedCompanyAddress'), name: "NonLimitedCompanyAddress", max: 10, isShowClear: true });
        limitedAddressView.render().$el.appendTo(this.$el.find('#NonLimitedAddress'));
        return this;
    }
});
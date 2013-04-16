(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.YourInfoMainView = (function(_super) {

    __extends(YourInfoMainView, _super);

    function YourInfoMainView() {
      return YourInfoMainView.__super__.constructor.apply(this, arguments);
    }

    YourInfoMainView.prototype.template = "#your-info-template";

    YourInfoMainView.prototype.initialize = function() {
      return this.isAddressValidation = true;
    };

    YourInfoMainView.prototype.events = {
      'click .edit-personal': 'editPersonalViewShow',
      'click .submit-personal': 'saveData',
      'click .cancel': 'reload',
      'change .personEditInput': 'inputChanged',
      'keyup .personEditInput': 'inputChanged'
    };

    YourInfoMainView.prototype.ui = {
      form: "form.editYourInfoForm"
    };

    YourInfoMainView.prototype.setInputReadOnly = function(isReadOnly) {
      this.$el.find('.personEditInput').attr('readonly', isReadOnly).attr('modifed', !isReadOnly);
      this.$el.find('.addAddressInput').attr('modifed', !isReadOnly);
      if (isReadOnly) {
        this.$el.find('.submit-personal, .cancel, .addAddressInput, .addAddress, .removeAddress').hide();
        return this.$el.find('.edit-personal').show();
      } else {
        this.$el.find('.submit-personal, .cancel, .addAddressInput, .addAddress, .removeAddress').show();
        return this.$el.find('.edit-personal').hide();
      }
    };

    YourInfoMainView.prototype.editPersonalViewShow = function() {
      return this.setInputReadOnly(false);
    };

    YourInfoMainView.prototype.addressModelChange = function() {
      var adress, typeOfBusinessName;
      adress = this.model.get('PersonalAddress');
      this.addressValidation(adress, '#PersonalAddress');
      typeOfBusinessName = this.model.get('BusinessTypeReduced');
      if (typeOfBusinessName === "Limited") {
        adress = this.model.get('LimitedCompanyAddress');
        return this.addressValidation(adress, '#LimitedCompanyAddress');
      } else if (typeOfBusinessName === "NonLimited") {
        adress = this.model.get('NonLimitedCompanyAddress');
        return this.addressValidation(adress, '#NonLimitedAddress');
      }
    };

    YourInfoMainView.prototype.addressValidation = function(address, element) {
      this.isAddressValidation = address.length > 0;
      return this.setError(element, !this.isAddressValidation);
    };

    YourInfoMainView.prototype.setError = function(element, isError) {
      if (isError) {
        return this.addAddressError(element);
      } else {
        return this.clearAddressError(element);
      }
    };

    YourInfoMainView.prototype.addAddressError = function(el) {
      var error;
      error = $('<label class="error" generated="true">This field is required</label>');
      return EzBob.Validation.errorPlacement(error, this.$el.find(el));
    };

    YourInfoMainView.prototype.clearAddressError = function(el) {
      return EzBob.Validation.unhighlight(this.$el.find(el));
    };

    YourInfoMainView.prototype.saveData = function() {
      var action, data, request, that,
        _this = this;
      if (!this.validator.form() || !this.isAddressValidation) {
        EzBob.App.trigger("error", "You must fill in all of the fields.");
        return false;
      }
      data = this.ui.form.serializeArray();
      action = this.ui.form.attr('action');
      that = this;
      request = $.post(action, data);
      request.success(function() {
        that.reload();
        return EzBob.App.trigger('info', "Your information updated successfully");
      });
      return request.fail(function() {
        return EzBob.App.trigger('error', "Business check service temporary unavaliable, please contact with system administrator", "");
      });
    };

    YourInfoMainView.prototype.reload = function() {
      var that, xhr,
        _this = this;
      xhr = this.model.fetch();
      that = this;
      return xhr.done(function() {
        that.render();
        scrollTop();
        return that.setInputReadOnly(true);
      });
    };

    YourInfoMainView.prototype.regions = {
      personal: '.personal-info',
      company: '.company-info'
    };

    YourInfoMainView.prototype.inputChanged = function() {
      this.isValid = !this.validator.form() || !this.isAddressValidation;
      return this.$el.find('.submit-personal').toggleClass('disabled', this.isValid);
    };

    YourInfoMainView.prototype.onRender = function() {
      var typeOfBusinessName;
      this.renderPersonal();
      typeOfBusinessName = this.model.get('BusinessTypeReduced');
      if (typeOfBusinessName === "Limited") {
        this.renderLimited();
      } else if (typeOfBusinessName === "NonLimited") {
        this.renderNonLimited();
      }
      this.setInputReadOnly(true);
      this.validator = EzBob.validateYourInfoEditForm(this.ui.form);
      this.$el.find('.phonenumber').numericOnly(11);
      return this.$el.find('.cashInput').numericOnly(11);
    };

    YourInfoMainView.prototype.renderPersonal = function() {
      var personalInfoView;
      personalInfoView = new EzBob.Profile.PersonalInfoView({
        model: this.model
      });
      this.model.get('PersonalAddress').on("all", this.addressModelChange, this);
      return this.personal.show(personalInfoView);
    };

    YourInfoMainView.prototype.renderNonLimited = function() {
      var view;
      view = new EzBob.Profile.NonLimitedInfoView({
        model: this.model
      });
      this.model.get('NonLimitedCompanyAddress').on("all", this.addressModelChange, this);
      return this.company.show(view);
    };

    YourInfoMainView.prototype.renderLimited = function() {
      var view;
      view = new EzBob.Profile.LimitedInfoView({
        model: this.model
      });
      this.model.get('LimitedCompanyAddress').on("all", this.addressModelChange, this);
      return this.company.show(view);
    };

    return YourInfoMainView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.PersonalInfoView = (function(_super) {

    __extends(PersonalInfoView, _super);

    function PersonalInfoView() {
      return PersonalInfoView.__super__.constructor.apply(this, arguments);
    }

    PersonalInfoView.prototype.template = "#personal-info-template";

    PersonalInfoView.prototype.regions = {
      personAddress: '#PersonalAddress'
    };

    PersonalInfoView.prototype.onRender = function() {
      var address;
      address = new EzBob.AddressView({
        model: this.model.get('PersonalAddress'),
        name: "PersonalAddress",
        max: 10,
        isShowClear: true
      });
      this.personAddress.show(address);
      return this;
    };

    return PersonalInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.NonLimitedInfoView = (function(_super) {

    __extends(NonLimitedInfoView, _super);

    function NonLimitedInfoView() {
      return NonLimitedInfoView.__super__.constructor.apply(this, arguments);
    }

    NonLimitedInfoView.prototype.template = "#nonlimited-info-template";

    NonLimitedInfoView.prototype.regions = {
      nonlimitedAddress: '#NonLimitedAddress',
      director: '.director-conteiner'
    };

    NonLimitedInfoView.prototype.onRender = function() {
      var address, directorView, directors;
      address = new EzBob.AddressView({
        model: this.model.get('NonLimitedCompanyAddress'),
        name: "NonLimitedCompanyAddress",
        max: 10,
        isShowClear: true
      });
      this.nonlimitedAddress.show(address);
      directors = this.model.get("NonLimitedInfo").Directors;
      if (directors !== null && directors.length !== 0) {
        directorView = new EzBob.Profile.NonLimitedDirectorInfoView({
          collection: new EzBob.Directors(directors)
        });
        this.director.show(directorView);
      }
      return this;
    };

    return NonLimitedInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.LimitedInfoView = (function(_super) {

    __extends(LimitedInfoView, _super);

    function LimitedInfoView() {
      return LimitedInfoView.__super__.constructor.apply(this, arguments);
    }

    LimitedInfoView.prototype.template = "#limited-info-template";

    LimitedInfoView.prototype.regions = {
      limitedAddress: '#LimitedCompanyAddress',
      director: '.director-conteiner'
    };

    LimitedInfoView.prototype.onRender = function() {
      var address, directorView, directors;
      address = new EzBob.AddressView({
        model: this.model.get('LimitedCompanyAddress'),
        name: "LimitedCompanyAddress",
        max: 10,
        isShowClear: true
      });
      this.limitedAddress.show(address);
      directors = this.model.get("LimitedInfo").Directors;
      if (directors !== null && directors.length !== 0) {
        directorView = new EzBob.Profile.DirectorCompositeView({
          collection: new EzBob.Directors(directors)
        });
        this.director.show(directorView);
      }
      return this;
    };

    return LimitedInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.DirectorInfoView = (function(_super) {

    __extends(DirectorInfoView, _super);

    function DirectorInfoView() {
      return DirectorInfoView.__super__.constructor.apply(this, arguments);
    }

    DirectorInfoView.prototype.template = '#director-info-template';

    DirectorInfoView.prototype.regions = {
      directorAddress: '#DirectorAddress'
    };

    DirectorInfoView.prototype.onRender = function() {
      var address;
      address = new EzBob.AddressView({
        model: this.model.get('DirectorAddress'),
        name: "DirectorAddress",
        max: 10,
        isShowClear: false
      });
      return this.directorAddress.show(address);
    };

    return DirectorInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.DirectorCompositeView = (function(_super) {

    __extends(DirectorCompositeView, _super);

    function DirectorCompositeView() {
      return DirectorCompositeView.__super__.constructor.apply(this, arguments);
    }

    DirectorCompositeView.prototype.template = "#directors-info";

    DirectorCompositeView.prototype.itemView = EzBob.Profile.DirectorInfoView;

    DirectorCompositeView.prototype.itemViewContainer = 'div';

    return DirectorCompositeView;

  })(Backbone.Marionette.CompositeView);

}).call(this);

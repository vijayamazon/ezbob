(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4, _ref5,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.YourInfoMainView = (function(_super) {
    __extends(YourInfoMainView, _super);

    function YourInfoMainView() {
      _ref = YourInfoMainView.__super__.constructor.apply(this, arguments);
      return _ref;
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
        this.$el.find('.submit-personal, .cancel, .addAddressInput, .addAddress, .removeAddress, .attardi-input, .required').hide();
        this.$el.find('textarea').removeClass('form_field').css('margin-top', 0);
        return this.$el.find('.edit-personal').show();
      } else {
        this.$el.find('.submit-personal, .cancel, .removeAddress').show();
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
      var action, data, directors, request, typeOfBusinessName, _ref1,
        _this = this;

      if (!this.validator.form() || !this.isAddressValidation) {
        EzBob.App.trigger("error", "You must fill in all of the fields.");
        return false;
      }
      typeOfBusinessName = this.model.get('BusinessTypeReduced') + "Info";
      if ((_ref1 = this.model.get(typeOfBusinessName)) != null ? _ref1.then : void 0) {
        directors = this.model.get(typeOfBusinessName).Directors;
        _.each(directors, function(val) {
          return _.each(val.DirectorAddress, function(add) {
            return add["DirectorId"] = val.Id;
          });
        });
      }
      data = this.ui.form.serializeArray();
      action = this.ui.form.attr('action');
      request = $.post(action, data);
      request.done(function() {
        _this.reload();
        return EzBob.App.trigger('info', "Your information updated successfully");
      });
      return request.fail(function() {
        return EzBob.App.trigger('error', "Business check service temporary unavaliable, please contact with system administrator", "");
      });
    };

    YourInfoMainView.prototype.reload = function() {
      var _this = this;

      return this.model.fetch().done(function() {
        _this.render();
        scrollTop();
        return _this.setInputReadOnly(true);
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
      this.$el.find('.cashInput').numericOnly(15);
      return $("input.form_field_address_lookup").css("margin-left", "3px");
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
      _ref1 = PersonalInfoView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    PersonalInfoView.prototype.template = "#personal-info-template";

    PersonalInfoView.prototype.regions = {
      personAddress: '#PersonalAddress',
      otherPropertyAddress: '#OtherPropertyAddress'
    };

    PersonalInfoView.prototype.onRender = function() {
      var address, otherAddress;

      address = new EzBob.AddressView({
        model: this.model.get('PersonalAddress'),
        name: "PersonalAddress",
        max: 10,
        isShowClear: true
      });
      this.personAddress.show(address);
      if (this.model.get('IsOffline')) {
        otherAddress = new EzBob.AddressView({
          model: this.model.get('OtherPropertyAddress'),
          name: "OtherPropertyAddress",
          max: 1,
          isShowClear: true
        });
        this.otherPropertyAddress.show(otherAddress);
      } else {
        this.otherPropertyAddress.hide();
      }
      return this;
    };

    return PersonalInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.NonLimitedInfoView = (function(_super) {
    __extends(NonLimitedInfoView, _super);

    function NonLimitedInfoView() {
      _ref2 = NonLimitedInfoView.__super__.constructor.apply(this, arguments);
      return _ref2;
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
        directorView = new EzBob.Profile.DirectorCompositeView({
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
      _ref3 = LimitedInfoView.__super__.constructor.apply(this, arguments);
      return _ref3;
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
      _ref4 = DirectorInfoView.__super__.constructor.apply(this, arguments);
      return _ref4;
    }

    DirectorInfoView.prototype.template = '#director-info-template';

    DirectorInfoView.prototype.regions = {
      directorAddress: '#DirectorAddress'
    };

    DirectorInfoView.prototype.onRender = function() {
      var address;

      address = new EzBob.AddressView({
        model: this.model.get('DirectorAddress'),
        name: "DirectorAddress[" + (this.model.get('Position')) + "]",
        max: 10,
        isShowClear: true,
        directorId: this.model.get('Id')
      });
      return this.directorAddress.show(address);
    };

    return DirectorInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.DirectorCompositeView = (function(_super) {
    __extends(DirectorCompositeView, _super);

    function DirectorCompositeView() {
      _ref5 = DirectorCompositeView.__super__.constructor.apply(this, arguments);
      return _ref5;
    }

    DirectorCompositeView.prototype.template = "#directors-info";

    DirectorCompositeView.prototype.itemView = EzBob.Profile.DirectorInfoView;

    DirectorCompositeView.prototype.itemViewContainer = 'div';

    return DirectorCompositeView;

  })(Backbone.Marionette.CompositeView);

}).call(this);

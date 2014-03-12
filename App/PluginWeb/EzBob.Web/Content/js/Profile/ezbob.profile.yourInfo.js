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

    YourInfoMainView.prototype.template = '#your-info-template';

    YourInfoMainView.prototype.initialize = function() {
      EzBob.App.on('dash-director-address-change', this.directorModelChange, this);
      EzBob.App.on('add-director', this.addDirector, this);
      return EzBob.App.on('add-director-back', this.addDirectorBack, this);
    };

    YourInfoMainView.prototype.events = {
      'click .edit-personal': 'editPersonalViewShow',
      'click .submit-personal': 'saveData',
      'click .cancel': 'reload',
      'change .personEditInput': 'inputChanged',
      'keyup .personEditInput': 'inputChanged'
    };

    YourInfoMainView.prototype.ui = {
      form: 'form.editYourInfoForm'
    };

    YourInfoMainView.prototype.setInputReadOnly = function(isReadOnly) {
      this.$el.find('.personEditInput').attr('readonly', isReadOnly).attr('modifed', !isReadOnly);
      this.$el.find('#PersonalAddress .addAddressInput').attr('modifed', !isReadOnly);
      if (isReadOnly) {
        this.$el.find('.submit-personal, .cancel, .addAddressInput,#PersonalAddress .addAddress, .removeAddress, .attardi-input, .required').hide();
        this.$el.find('textarea').removeClass('form_field').css('margin-top', 0);
        return this.$el.find('.edit-personal').show();
      } else {
        this.$el.find('.submit-personal, .cancel,#PersonalAddress .removeAddress').show();
        return this.$el.find('.edit-personal').hide();
      }
    };

    YourInfoMainView.prototype.editPersonalViewShow = function() {
      return this.setInputReadOnly(false);
    };

    YourInfoMainView.prototype.addressAreValid = function() {
      var address, dir, directors, typeOfBusinessName, _i, _len;
      address = this.model.get('PersonalAddress');
      if (address.length < 1) {
        return false;
      }
      typeOfBusinessName = this.model.get('BusinessTypeReduced');
      if (typeOfBusinessName === 'Limited') {
        address = this.model.get('CompanyAddress');
        if (address.length < 1) {
          return false;
        }
      } else if (typeOfBusinessName === 'NonLimited') {
        address = this.model.get('CompanyAddress');
        if (address.length < 1) {
          return false;
        }
      }
      if (this.model.get(typeOfBusinessName + 'Info')) {
        directors = this.model.get(typeOfBusinessName + 'Info').Directors;
        for (_i = 0, _len = directors.length; _i < _len; _i++) {
          dir = directors[_i];
          if (dir.DirectorAddress.length < 1) {
            return false;
          }
        }
      }
      return true;
    };

    YourInfoMainView.prototype.addDirector = function() {
      return this.ui.form.hide();
    };

    YourInfoMainView.prototype.addDirectorBack = function() {
      return this.ui.form.show();
    };

    YourInfoMainView.prototype.directorModelChange = function(newModel) {
      var directors;
      directors = this.model.get(this.model.get('BusinessTypeReduced') + 'Info').Directors;
      _.each(directors, function(dir) {
        if (dir.Id === newModel.get('Id')) {
          return dir.DirectorAddress = newModel.get('DirectorAddress').models;
        }
      });
      return this.addressModelChange();
    };

    YourInfoMainView.prototype.addressModelChange = function() {
      var directors, self, typeOfBusinessName;
      this.inputChanged();
      this.setInvalidAddressLabel(this.model.get('PersonalAddress'), '#PersonalAddress');
      typeOfBusinessName = this.model.get('BusinessTypeReduced');
      if (typeOfBusinessName === 'Limited') {
        this.setInvalidAddressLabel(this.model.get('CompanyAddress'), '#LimitedCompanyAddress');
      } else if (typeOfBusinessName === 'NonLimited') {
        this.setInvalidAddressLabel(this.model.get('CompanyAddress'), '#NonLimitedAddress');
      }
      self = this;
      if (this.model.get(typeOfBusinessName + 'Info')) {
        directors = this.model.get(typeOfBusinessName + 'Info').Directors;
        return _.each(directors, function(dir) {
          return self.setInvalidAddressLabel(dir.DirectorAddress, '.directorAddress' + dir.Id + ' #DirectorAddress', dir.Id);
        });
      }
    };

    YourInfoMainView.prototype.setInvalidAddressLabel = function(address, element, dirId) {
      if (address.length < 1) {
        return EzBob.Validation.addressErrorPlacement(this.$el.find(element), (dirId ? this.model : address), dirId, this.model.get('BusinessTypeReduced'));
      } else {
        return EzBob.Validation.unhighlight(this.$el.find(element));
      }
    };

    YourInfoMainView.prototype.saveData = function() {
      var action, data, directors, request, typeOfBusinessName,
        _this = this;
      if (!this.validator.form() || !this.addressAreValid()) {
        EzBob.App.trigger('error', 'You must fill in all of the fields.');
        return false;
      }
      typeOfBusinessName = this.model.get('BusinessTypeReduced') + 'Info';
      if (this.model.get(typeOfBusinessName)) {
        directors = this.model.get(typeOfBusinessName).Directors;
        _.each(directors, function(val) {
          return _.each(val.DirectorAddress, function(add) {
            return add['DirectorId'] = val.Id;
          });
        });
      }
      data = this.ui.form.serializeArray();
      action = this.ui.form.attr('action');
      request = $.post(action, data);
      request.done(function() {
        _this.reload();
        return EzBob.App.trigger('info', 'Your information updated successfully');
      });
      return request.fail(function() {
        return EzBob.App.trigger('error', 'Business check service temporary unavaliable, please contact with system administrator', '');
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
      var isInvalid;
      isInvalid = !this.validator.form() || !this.addressAreValid();
      return this.$el.find('.submit-personal').toggleClass('disabled', isInvalid).prop('disabled', isInvalid);
    };

    YourInfoMainView.prototype.onRender = function() {
      var typeOfBusinessName;
      this.renderPersonal();
      typeOfBusinessName = this.model.get('BusinessTypeReduced');
      if (typeOfBusinessName === 'Limited') {
        this.renderLimited();
      } else if (typeOfBusinessName === 'NonLimited') {
        this.renderNonLimited();
      }
      this.setInputReadOnly(true);
      this.validator = EzBob.validateYourInfoEditForm(this.ui.form);
      this.$el.find('.phonenumber').numericOnly(11);
      this.$el.find('.cashInput').numericOnly(15);
      return $('input.form_field_right_side').css('margin-left', '3px');
    };

    YourInfoMainView.prototype.renderPersonal = function() {
      var personalInfoView;
      personalInfoView = new EzBob.Profile.PersonalInfoView({
        model: this.model
      });
      this.model.get('PersonalAddress').on('all', this.addressModelChange, this);
      return this.personal.show(personalInfoView);
    };

    YourInfoMainView.prototype.renderNonLimited = function() {
      var view;
      view = new EzBob.Profile.NonLimitedInfoView({
        model: this.model
      });
      this.model.get('CompanyAddress').on('all', this.addressModelChange, this);
      return this.company.show(view);
    };

    YourInfoMainView.prototype.renderLimited = function() {
      var view;
      view = new EzBob.Profile.LimitedInfoView({
        model: this.model
      });
      this.model.get('CompanyAddress').on('all', this.addressModelChange, this);
      return this.company.show(view);
    };

    return YourInfoMainView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.PersonalInfoView = (function(_super) {

    __extends(PersonalInfoView, _super);

    function PersonalInfoView() {
      return PersonalInfoView.__super__.constructor.apply(this, arguments);
    }

    PersonalInfoView.prototype.template = '#personal-info-template';

    PersonalInfoView.prototype.regions = {
      personAddress: '#PersonalAddress',
      otherPropertyAddress: '#OtherPropertyAddress'
    };

    PersonalInfoView.prototype.onRender = function() {
      var address, otherAddress;
      address = new EzBob.AddressView({
        model: this.model.get('PersonalAddress'),
        name: 'PersonalAddress',
        max: 10,
        isShowClear: true,
        uiEventControlIdPrefix: this.personAddress.getEl(this.personAddress.el).attr('data-ui-event-control-id-prefix')
      });
      this.personAddress.show(address);
      if (this.model.get('IsOffline')) {
        otherAddress = new EzBob.AddressView({
          model: this.model.get('OtherPropertyAddress'),
          name: 'OtherPropertyAddress',
          max: 1,
          isShowClear: true,
          uiEventControlIdPrefix: this.otherPropertyAddress.getEl(this.otherPropertyAddress.el).attr('data-ui-event-control-id-prefix')
        });
        this.otherPropertyAddress.show(otherAddress);
      } else {
        this.$el.find('.offline').remove();
      }
      return this;
    };

    return PersonalInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.NonLimitedInfoView = (function(_super) {

    __extends(NonLimitedInfoView, _super);

    function NonLimitedInfoView() {
      return NonLimitedInfoView.__super__.constructor.apply(this, arguments);
    }

    NonLimitedInfoView.prototype.template = '#nonlimited-info-template';

    NonLimitedInfoView.prototype.regions = {
      nonlimitedAddress: '#NonLimitedAddress',
      director: '.director-container'
    };

    NonLimitedInfoView.prototype.onRender = function() {
      var address, directorView, directors;
      address = new EzBob.AddressView({
        model: this.model.get('CompanyAddress'),
        name: 'NonLimitedCompanyAddress',
        max: 10,
        isShowClear: true,
        uiEventControlIdPrefix: this.nonlimitedAddress.getEl(this.nonlimitedAddress.el).attr('data-ui-event-control-id-prefix')
      });
      this.nonlimitedAddress.show(address);
      directors = this.model.get('CompanyInfo').Directors;
      if (directors !== null && directors.length !== 0) {
        directorView = new EzBob.Profile.DirectorCompositeView({
          collection: new EzBob.Directors(directors)
        });
        this.director.show(directorView);
      }
      if (!this.model.get('IsOffline')) {
        this.$el.find('.offline').remove();
      } else {
        this.$el.find('.notoffline').remove();
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

    LimitedInfoView.prototype.template = '#limited-info-template';

    LimitedInfoView.prototype.regions = {
      limitedAddress: '#LimitedCompanyAddress',
      director: '.director-container'
    };

    LimitedInfoView.prototype.events = {
      "click .add-director": "addDirectorClicked"
    };

    LimitedInfoView.prototype.onRender = function() {
      var address, directorView, directors;
      address = new EzBob.AddressView({
        model: this.model.get('CompanyAddress'),
        name: 'LimitedCompanyAddress',
        max: 10,
        isShowClear: true,
        uiEventControlIdPrefix: this.limitedAddress.getEl(this.limitedAddress.el).attr('data-ui-event-control-id-prefix')
      });
      this.limitedAddress.show(address);
      directors = this.model.get('CompanyInfo').Directors;
      if (directors !== null && directors.length !== 0) {
        directorView = new EzBob.Profile.DirectorCompositeView({
          collection: new EzBob.Directors(directors)
        });
        this.director.show(directorView);
      }
      if (!this.model.get('IsOffline')) {
        this.$el.find('.offline').remove();
      }
      return this;
    };

    LimitedInfoView.prototype.addDirectorClicked = function() {
      var addDirector, directorEl;
      console.log('add director clicked');
      EzBob.App.trigger('add-director');
      directorEl = $('.add-director-container');
      addDirector = new EzBob.Profile.AddDirectorInfoView().render().$el.appendTo(directorEl);
      directorEl.show();
      return false;
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

    DirectorInfoView.prototype.addressModelChange = function() {
      return EzBob.App.trigger('dash-director-address-change', this.model);
    };

    DirectorInfoView.prototype.onRender = function() {
      var address;
      address = new EzBob.AddressView({
        model: this.model.get('DirectorAddress'),
        name: "DirectorAddress[" + (this.model.get('Position')) + "]",
        max: 10,
        isShowClear: true,
        directorId: this.model.get('Id'),
        uiEventControlIdPrefix: this.directorAddress.getEl(this.directorAddress.el).attr('data-ui-event-control-id-prefix')
      });
      this.model.get('DirectorAddress').on('all', this.addressModelChange, this);
      this.directorAddress.show(address);
      return this.$el.find('.addressEdit').addClass('directorAddress' + this.model.get('Id'));
    };

    return DirectorInfoView;

  })(Backbone.Marionette.Layout);

  EzBob.Profile.DirectorCompositeView = (function(_super) {

    __extends(DirectorCompositeView, _super);

    function DirectorCompositeView() {
      return DirectorCompositeView.__super__.constructor.apply(this, arguments);
    }

    DirectorCompositeView.prototype.template = '#directors-info';

    DirectorCompositeView.prototype.itemView = EzBob.Profile.DirectorInfoView;

    DirectorCompositeView.prototype.itemViewContainer = 'div';

    return DirectorCompositeView;

  })(Backbone.Marionette.CompositeView);

  EzBob.Profile.AddDirectorInfoView = (function(_super) {

    __extends(AddDirectorInfoView, _super);

    function AddDirectorInfoView() {
      return AddDirectorInfoView.__super__.constructor.apply(this, arguments);
    }

    AddDirectorInfoView.prototype.template = '#add-director-info-template';

    AddDirectorInfoView.prototype.initialize = function(options) {
      return console.log('init', options);
    };

    AddDirectorInfoView.prototype.region = {
      directorAddress: '.director_address'
    };

    AddDirectorInfoView.prototype.events = {
      "click .directorBack": "directorBack",
      "click .addDirector": "directorAdd",
      'change   input': 'inputChanged',
      'click    input': 'inputChanged',
      'focusout input': 'inputChanged',
      'keyup    input': 'inputChanged',
      'change   select': 'inputChanged',
      'click    select': 'inputChanged',
      'focusout select': 'inputChanged',
      'keyup    select': 'inputChanged'
    };

    AddDirectorInfoView.prototype.addressModelChange = function() {
      return EzBob.App.trigger('dash-director-address-change', this.model);
    };

    AddDirectorInfoView.prototype.render = function() {
      var oFieldStatusIcons;
      console.log('rendering');
      AddDirectorInfoView.__super__.render.call(this);
      oFieldStatusIcons = this.$el.find('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      EzBob.UiAction.registerView(this);
      return console.log('@model', this.model);
      /*
              address = new EzBob.AddressView({
                  name: "DirectorAddress",
                  max: 10,
                  isShowClear: true,
              })
      
              @directorAddress.show(address)
      */

    };

    AddDirectorInfoView.prototype.directorBack = function() {
      this.close();
      return EzBob.App.trigger('add-director-back');
    };

    AddDirectorInfoView.prototype.directorAdd = function() {};

    AddDirectorInfoView.prototype.inputChanged = function() {};

    return AddDirectorInfoView;

  })(Backbone.Marionette.ItemView);

}).call(this);

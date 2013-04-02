(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EmailEditView = (function(_super) {

    __extends(EmailEditView, _super);

    function EmailEditView() {
      return EmailEditView.__super__.constructor.apply(this, arguments);
    }

    EmailEditView.prototype.template = "#email-edit-template";

    EmailEditView.prototype.events = {
      'click .email-confirm-manually': 'confirmManually',
      'click .email-send-new-request': 'sendNewRequest',
      'click .email-change-address': 'changeEmail',
      'keypress input[name="edit-email"]': 'editEmailEnterPressed'
    };

    EmailEditView.prototype.ui = {
      'email': 'input[name="edit-email"]'
    };

    EmailEditView.prototype.confirmManually = function() {
      var xhr,
        _this = this;
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ManuallyConfirm", {
        id: this.model.id
      });
      xhr.success(function() {
        _this.model.fetch();
        return _this.close();
      });
      return false;
    };

    EmailEditView.prototype.sendNewRequest = function() {
      var xhr,
        _this = this;
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/Resend", {
        id: this.model.id
      });
      xhr.success(function() {
        _this.model.fetch();
        return _this.close();
      });
      return false;
    };

    EmailEditView.prototype.changeEmail = function() {
      var xhr,
        _this = this;
      if (!this.validator.form()) {
        return false;
      }
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ChangeEmail", {
        id: this.model.id,
        email: this.ui.email.val()
      });
      xhr.success(function(response) {
        if (response.error !== void 0) {
          EzBob.ShowMessage(response.error);
        }
        _this.model.fetch();
        return _this.close();
      });
      return false;
    };

    EmailEditView.prototype.onRender = function() {
      this.form = this.$el.find('#email-edit-form');
      this.validator = EzBob.validateChangeEmailForm(this.form);
      return this;
    };

    EmailEditView.prototype.editEmailEnterPressed = function(e) {
      if (e.keyCode === 13) {
        return false;
      }
    };

    return EmailEditView;

  })(Backbone.Marionette.ItemView);

}).call(this);

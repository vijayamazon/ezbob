(function() {
  var root;
  var __hasProp = Object.prototype.hasOwnProperty, __extends = function(child, parent) {
    for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; }
    function ctor() { this.constructor = child; }
    ctor.prototype = parent.prototype;
    child.prototype = new ctor;
    child.__super__ = parent.prototype;
    return child;
  }, __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };
  root = typeof exports !== "undefined" && exports !== null ? exports : this;
  root.EzBob = root.EzBob || {};
  EzBob.EmailEditView = (function() {
    __extends(EmailEditView, Backbone.Marionette.ItemView);
    function EmailEditView() {
      EmailEditView.__super__.constructor.apply(this, arguments);
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
      var xhr;
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ManuallyConfirm", {
        id: this.model.id
      });
      xhr.success(__bind(function() {
        this.model.fetch();
        return this.close();
      }, this));
      return false;
    };
    EmailEditView.prototype.sendNewRequest = function() {
      var xhr;
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/Resend", {
        id: this.model.id
      });
      xhr.success(__bind(function() {
        this.model.fetch();
        return this.close();
      }, this));
      return false;
    };
    EmailEditView.prototype.changeEmail = function() {
      var xhr;
      if (!this.validator.form()) {
        return false;
      }
      xhr = $.post(window.gRootPath + "Underwriter/EmailVerification/ChangeEmail", {
        id: this.model.id,
        email: this.ui.email.val()
      });
      xhr.success(__bind(function(response) {
        if (response.error !== void 0) {
          EzBob.ShowMessage(response.error);
        }
        this.model.fetch();
        return this.close();
      }, this));
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
  })();
}).call(this);

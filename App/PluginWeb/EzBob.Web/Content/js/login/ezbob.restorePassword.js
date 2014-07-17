(function() {
  var root,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.ResetPasswordView = (function(_super) {

    __extends(ResetPasswordView, _super);

    function ResetPasswordView() {
      this.focusCaptcha = __bind(this.focusCaptcha, this);

      this.focusAnswer = __bind(this.focusAnswer, this);

      this.focusEmail = __bind(this.focusEmail, this);
      return ResetPasswordView.__super__.constructor.apply(this, arguments);
    }

    ResetPasswordView.prototype.template = "#restore-pass-template";

    ResetPasswordView.prototype.initialize = function() {
      this.mail = void 0;
      this.answerEnabled = true;
      this.emailEnabled = false;
      return this.captchaEnabled = EzBob.Config.CaptchaMode === 'off';
    };

    ResetPasswordView.prototype.focus = null;

    ResetPasswordView.prototype.ui = {
      "questionArea": "#questionArea",
      "questionField": "#questionField",
      "email": "#email",
      "form": "form",
      "getQuestionBtn": "#getQuestion",
      "restoreBtn": "#restore",
      "passwordRestoredArea": ".passwordRestoredArea",
      "restorePasswordArea": ".restorePasswordArea",
      "answer": "#Answer",
      "captcha": "#CaptchaInputText"
    };

    ResetPasswordView.prototype.onRender = function() {
      var notifications;
      this.captcha = new EzBob.Captcha({
        elementId: "captcha",
        tabindex: 11
      });
      this.captcha.render();
      this.ui.email.data("changed", true);
      this.validator = EzBob.validateRestorePasswordForm(this.ui.form);
      this.initStatusIcons();
      $('#email').focus();
      EzBob.UiAction.registerView(this);
      notifications = new EzBob.NotificationsView({
        el: this.$el.find(".errorArea")
      });
      notifications.render();
      return this;
    };

    ResetPasswordView.prototype.events = {
      "click #getQuestion": "getQuestionBtnClicked",
      "click #restore": "restoreClicked",
      "keyup #email": "inputEmailChanged",
      "change #email": "inputEmailChanged",
      "keyup #Answer": "inputAnswerChanged",
      "change #Answer": "inputAnswerChanged",
      "keyup #CaptchaInputText": "inputCaptchaChanged",
      "change #CaptchaInputText": "inputCaptchaChanged"
    };

    ResetPasswordView.prototype.restoreClicked = function(e) {
      var $el,
        _this = this;
      if (this.ui.restoreBtn.hasClass("disabled")) {
        return false;
      }
      $el = $(e.currentTarget);
      if ($el.hasClass("disabled")) {
        return false;
      }
      $el.addClass("disabled");
      this.focus = null;
      return $.post("RestorePassword", this.ui.form.serializeArray()).done(function(data) {
        if (!EzBob.isNullOrEmpty(data.errorMessage) || !EzBob.isNullOrEmpty(data.error)) {
          EzBob.App.trigger("error", data.errorMessage || data.error);
          _this.ui.questionArea.hide();
          _this.ui.email.closest('div').show();
          $('#captcha').show();
          _this.focus = _this.focusCaptcha;
          _this.ui.answer.val('');
          _this.ui.getQuestionBtn.addClass('disabled');
          return false;
        }
        _this.ui.passwordRestoredArea.show();
        _this.ui.restorePasswordArea.hide();
        return scrollTop();
      }).fail(function(data) {
        EzBob.App.trigger("error", data.responceText);
        _this.initStatusIcons();
        return _this.focus = _this.focusCaptcha;
      }).always(function(data) {
        _this.ui.email.data("changed", false);
        _this.emailKeyuped();
        return _this.captcha.reload(_this.focus);
      });
    };

    ResetPasswordView.prototype.focusEmail = function() {
      return $('#email').focus();
    };

    ResetPasswordView.prototype.focusAnswer = function() {
      return $('#Answer').focus();
    };

    ResetPasswordView.prototype.focusCaptcha = function() {
      return $('#CaptchaInputText').focus();
    };

    ResetPasswordView.prototype.inputCaptchaChanged = function() {
      var enabled;
      this.captchaEnabled = this.validator.check($(this.ui.captcha.selector));
      enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
      return this.ui.getQuestionBtn.toggleClass('disabled', !enabled);
    };

    ResetPasswordView.prototype.inputEmailChanged = function() {
      var enabled;
      this.emailEnabled = this.validator.check(this.ui.email);
      enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
      return this.ui.getQuestionBtn.toggleClass('disabled', !enabled);
    };

    ResetPasswordView.prototype.inputAnswerChanged = function() {
      var enabled;
      this.answerEnabled = this.validator.check(this.ui.answer);
      enabled = this.answerEnabled && this.emailEnabled && this.captchaEnabled;
      return this.ui.restoreBtn.toggleClass('disabled', !enabled);
    };

    ResetPasswordView.prototype.emailKeyuped = function() {
      if (this.ui.email.data("changed")) {
        return false;
      }
      this.ui.email.data("changed", true);
      this.ui.questionArea.hide();
      this.ui.getQuestionBtn.show();
      return this.captcha.$el.closest('.control-group').insertAfter(this.ui.email.closest('.control-group'));
    };

    ResetPasswordView.prototype.getQuestionBtnClicked = function() {
      var _this = this;
      if (this.ui.getQuestionBtn.hasClass("disabled")) {
        return false;
      }
      this.mail = this.ui.email.val();
      EzBob.App.trigger('clear');
      this.ui.questionArea.hide();
      this.focus = null;
      return $.post("QuestionForEmail", this.ui.form.serialize()).done(function(response) {
        if (response.broker) {
          document.location.href = "" + window.gRootPath + "Broker#ForgotPassword";
          return true;
        }
        if (!EzBob.isNullOrEmpty(response.errorMessage) || !EzBob.isNullOrEmpty(response.error)) {
          EzBob.App.trigger('error', response.errorMessage || response.error);
          _this.ui.questionArea.hide();
          _this.focus = _this.focusCaptcha;
          _this.ui.getQuestionBtn.addClass('disabled');
          return true;
        }
        if (EzBob.isNullOrEmpty(response.question)) {
          EzBob.App.trigger("warning", "To recover your password security question fields must be completely filled in the account settings");
          _this.ui.questionArea.hide();
          _this.focus = _this.focusEmail;
          return true;
        }
        _this.ui.questionField.text(response.question);
        _this.ui.questionArea.show();
        _this.initStatusIcons('email');
        _this.ui.getQuestionBtn.hide();
        _this.captcha.$el.closest('.control-group').insertAfter(_this.ui.answer.closest('.control-group'));
        _this.ui.email.data("changed", false);
        _this.answerEnabled = false;
        _this.ui.email.closest('div').hide();
        $('#captcha').hide();
        $('#Answer').focus();
        return _this.focus = _this.focusAnswer;
      }).always(function() {
        return _this.captcha.reload(_this.focus);
      });
    };

    ResetPasswordView.prototype.initStatusIcons = function(e) {
      var oFieldStatusIcons;
      oFieldStatusIcons = this.$el.find('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      if (e === 'email') {
        return this.ui.email.change();
      }
    };

    return ResetPasswordView;

  })(Backbone.Marionette.ItemView);

}).call(this);

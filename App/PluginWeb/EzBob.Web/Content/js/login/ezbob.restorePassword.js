(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.ResetPasswordView = (function(_super) {
    __extends(ResetPasswordView, _super);

    function ResetPasswordView() {
      _ref = ResetPasswordView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    ResetPasswordView.prototype.template = "#restore-pass-template";

    ResetPasswordView.prototype.initialize = function() {
      return this.mail = void 0;
    };

    ResetPasswordView.prototype.ui = {
      "questionArea": "#questionArea",
      "questionField": "#questionField",
      "email": "#email",
      "form": "form",
      "getQuestion": "#getQuestion",
      "passwordRestoredArea": ".passwordRestoredArea",
      "restorePasswordArea": ".restorePasswordArea",
      "answer": "#Answer"
    };

    ResetPasswordView.prototype.onRender = function() {
      this.captcha = new EzBob.Captcha({
        elementId: "captcha",
        tabindex: 3
      });
      this.captcha.render();
      this.ui.email.data("changed", true);
      return this;
    };

    ResetPasswordView.prototype.events = {
      "click #getQuestion": "getQuestionClicked",
      "keyup #email": "emailKeyuped",
      "click #restore": "restoreClicked"
    };

    ResetPasswordView.prototype.restoreClicked = function(e) {
      var $el,
        _this = this;

      $el = $(e.currentTarget);
      if ($el.hasClass("disabled")) {
        return false;
      }
      $el.addClass("disabled");
      return $.post("RestorePassword", this.ui.form.serializeArray()).done(function(data) {
        if (!EzBob.isNullOrEmpty(data.errorMessage) || !EzBob.isNullOrEmpty(data.error)) {
          EzBob.App.trigger("error", data.errorMessage || data.error);
          _this.ui.questionArea.css("display", "none");
          return false;
        }
        _this.ui.passwordRestoredArea.css("display", "");
        _this.ui.restorePasswordArea.css("display", "none");
        return scrollTop();
      }).fail(function(data) {
        return EzBob.App.trigger("error", data.responceText);
      }).complete(function() {
        $el.removeClass("disabled");
        _this.ui.email.data("changed", false);
        return _this.emailKeyuped();
      });
    };

    ResetPasswordView.prototype.emailKeyuped = function() {
      if (this.ui.email.data("changed")) {
        return false;
      }
      this.ui.email.data("changed", true);
      this.ui.questionArea.css("display", "none");
      this.ui.getQuestion.css("display", "");
      this.captcha.$el.closest('.control-group').insertAfter(this.ui.email.closest('.control-group'));
      return this.captcha.reload();
    };

    ResetPasswordView.prototype.getQuestionClicked = function() {
      var _this = this;

      this.mail = this.ui.email.val();
      EzBob.App.trigger('clear');
      this.ui.questionArea.css("display", "none");
      this.ui.answer.val("");
      return $.post("QuestionForEmail", this.ui.form.serialize()).done(function(response) {
        if (!EzBob.isNullOrEmpty(response.errorMessage) || !EzBob.isNullOrEmpty(response.error)) {
          EzBob.App.trigger('error', response.errorMessage || response.error);
          _this.ui.questionArea.css("display", "none");
          return true;
        }
        if (EzBob.isNullOrEmpty(response.question)) {
          EzBob.App.trigger("warning", "To recover your password security question fields must be completely filled in the account settings");
          _this.ui.questionArea.css("display", "none");
          return true;
        }
        _this.ui.questionField.text(response.question);
        _this.ui.questionArea.css("display", "");
        _this.ui.getQuestion.css("display", "none");
        _this.captcha.$el.closest('.control-group').insertAfter(_this.ui.answer.closest('.control-group'));
        return _this.ui.email.data("changed", false);
      }).complete(function() {
        return _this.captcha.reload();
      });
    };

    return ResetPasswordView;

  })(Backbone.Marionette.ItemView);

}).call(this);

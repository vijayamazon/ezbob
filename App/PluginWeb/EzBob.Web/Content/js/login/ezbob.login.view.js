(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.LoginView = (function(_super) {
    __extends(LoginView, _super);

    function LoginView() {
      _ref = LoginView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    LoginView.prototype.initialize = function() {
      this.template = _.template($("#login-template").html());
      this.loginModel = new EzBob.LoginModel();
      return this.on("ready", this.ready, this);
    };

    LoginView.prototype.events = {
      "click :submit": "submit",
      "keydown input[name='UserName']": "inputChanged",
      "paste input[name='UserName']": "inputChanged",
      "input input[name='UserName']": "inputChanged",
      "change input[name='UserName']": "emailChanged",
      "keydown input[name='Password']": "inputChanged",
      "paste input[name='Password']": "inputChanged",
      "input input[name='Password']": "inputChanged",
      "change input[name='Password']": "password1Changed"
    };

    LoginView.prototype.render = function() {
      this.$el.html(this.template(this.model.toJSON()));
      this.form = this.$el.find(".simple-login");
      this.validator = EzBob.validateLoginForm(this.form);
      this.$el.find("img[rel]").setPopover("left");
      this.$el.find("li[rel]").setPopover("left");
      return this;
    };

    LoginView.prototype.inputChanged = function() {
      if (EzBob.Validation.checkForm(this.validator)) {
        return $("#loginSubmit.disabled").removeClass("disabled");
      } else {
        return $("#loginSubmit").addClass("disabled");
      }
    };

    LoginView.prototype.emailChanged = function() {
      return EzBob.Validation.displayIndication(this.validator, "EmailImage", "#Email", "#RotateImage", "#OkImage", "#FailImage");
    };

    LoginView.prototype.passwordChanged = function() {
      return EzBob.Validation.displayIndication(this.validator, "Password1Image", "#Password", "#RotateImage", "#OkImage", "#FailImage");
    };

    LoginView.prototype.submit = function() {
      var that;

      if (this.$el.find(":submit").hasClass("disabled")) {
        return false;
      }
      this.blockBtn(true);
      that = this;
      if (!EzBob.Validation.validateAndNotify(that.validator)) {
        that.blockBtn(false);
        return false;
      }
      $.post(that.form.attr("action"), that.form.serialize(), (function(result) {
        if (result.success) {
          that.$el.find("input[type='password'], input[type='text']").tooltip("hide");
          EzBob.App.trigger("clear");
          that.trigger("ready");
          that.trigger("next");
          $.get(window.gRootPath + "Start/TopButton").done(function(dat) {
            return $("#pre_header").html(dat);
          });
        } else {
          if (result.errorMessage) {
            EzBob.App.trigger("error", result.errorMessage);
          }
          that.captcha.reload();
        }
        return that.blockBtn(false);
      }), "json");
      return false;
    };

    LoginView.prototype.ready = function() {
      return this.setReadOnly();
    };

    LoginView.prototype.setReadOnly = function() {
      this.readOnly = true;
      return this.$el.find(":input").not(":submit").attr("disabled", "disabled").attr("readonly", "readonly").css("disabled");
    };

    LoginView.prototype.blockBtn = function(isBlock) {
      BlockUi((isBlock ? "on" : "off"));
      return this.$el.find(":submit").toggleClass("disabled", isBlock);
    };

    return LoginView;

  })(Backbone.View);

  EzBob.LoginModel = (function(_super) {
    __extends(LoginModel, _super);

    function LoginModel() {
      _ref1 = LoginModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    LoginModel.prototype.defaults = {
      completed: false
    };

    return LoginModel;

  })(Backbone.Model);

}).call(this);

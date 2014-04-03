(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CustomerLoginView = (function(_super) {
    __extends(CustomerLoginView, _super);

    function CustomerLoginView() {
      _ref = CustomerLoginView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    CustomerLoginView.prototype.initialize = function() {
      return this.template = _.template($("#customerlogin-template").html());
    };

    CustomerLoginView.prototype.events = {
      "click :submit": "submit",
      "keyup input": "inputChanged",
      "change input": "inputChanged"
    };

    CustomerLoginView.prototype.render = function() {
      var oFieldStatusIcons;

      this.$el.html(this.template());
      this.form = this.$el.find(".simple-login");
      this.validator = EzBob.validateLoginForm(this.form);
      this.$el.find("img[rel]").setPopover("left");
      this.$el.find("li[rel]").setPopover("left");
      oFieldStatusIcons = this.$el.find('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      $('#UserName').focus();
      EzBob.UiAction.registerView(this);
      return this;
    };

    CustomerLoginView.prototype.inputChanged = function() {
      var enabled;

      enabled = EzBob.Validation.checkForm(this.validator);
      return $("#loginSubmit").toggleClass('disabled', !enabled);
    };

    CustomerLoginView.prototype.submit = function() {
      var xhr,
        _this = this;

      if (this.$el.find(":submit").hasClass("disabled")) {
        return false;
      }
      if (!EzBob.Validation.checkForm(this.validator)) {
        return false;
      }
      this.blockBtn(true);
      if (!EzBob.Validation.checkForm(this.validator)) {
        this.blockBtn(false);
        return false;
      }
      xhr = $.post(this.form.attr("action"), this.form.serialize());
      xhr.done(function(result, status) {
        if (status === "success") {
          if (result.success) {
            if (result.broker) {
              return document.location.href = "" + window.gRootPath + "Broker#login";
            } else {
              return document.location.href = "" + window.gRootPath + "Customer/Profile";
            }
          } else {
            EzBob.App.trigger("error", result.errorMessage);
            return _this.blockBtn(false);
          }
        } else {
          if (result.errorMessage) {
            EzBob.App.trigger("error", result.errorMessage);
          }
          return _this.blockBtn(false);
        }
      });
      return false;
    };

    CustomerLoginView.prototype.blockBtn = function(isBlock) {
      BlockUi((isBlock ? "on" : "off"));
      return this.$el.find(":submit").toggleClass("disabled", isBlock);
    };

    return CustomerLoginView;

  })(Backbone.View);

}).call(this);

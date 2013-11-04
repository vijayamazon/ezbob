(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.RecentCustomersModel = (function(_super) {
    __extends(RecentCustomersModel, _super);

    function RecentCustomersModel() {
      _ref = RecentCustomersModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    RecentCustomersModel.prototype.url = window.gRootPath + "Underwriter/Customers/GetRecentCustomers";

    return RecentCustomersModel;

  })(Backbone.Model);

  EzBob.Underwriter.goToCustomerId = (function(_super) {
    __extends(goToCustomerId, _super);

    function goToCustomerId() {
      _ref1 = goToCustomerId.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    goToCustomerId.prototype.initialize = function() {
      var _this = this;

      Mousetrap.bind("ctrl+g", function() {
        _this.render();
        return false;
      });
      return this.on("NotFound", this.notFound);
    };

    goToCustomerId.prototype.template = function() {
      var allOptions, customer, el, recentCustomers, _i, _len;

      recentCustomers = JSON.parse(localStorage.getItem('RecentCustomers'));
      allOptions = '';
      for (_i = 0, _len = recentCustomers.length; _i < _len; _i++) {
        customer = recentCustomers[_i];
        allOptions += '<option value="' + customer.Item1 + '">' + customer.Item2 + '</option>';
      }
      el = $("<div id='go-to-template'/>").html("            <input type='text' class='goto-customerId' autocomplete='off'/>            <br/>            <label>Recent customers:</label>            <select id='recentCustomers'class='selectheight'>" + allOptions + "</select>            <br/>            <div class='error-place' style='color:red'></div>");
      $('body').append(el);
      return el;
    };

    goToCustomerId.prototype.ui = {
      "input": ".goto-customerId",
      "select": "#recentCustomers",
      "template": "#go-to-template",
      "errorPlace": ".error-place"
    };

    goToCustomerId.prototype.onRender = function() {
      var _this = this;

      this.dialog = EzBob.ShowMessage(this.ui.template, "Customer ID?", (function() {
        return _this.okTrigger();
      }), "OK", null, "Cancel");
      this.ui.input.on("keydown", function(e) {
        return _this.keydowned(e);
      });
      this.okBtn = $(".ok-button");
      return this.ui.input.autocomplete({
        source: "" + gRootPath + "Underwriter/Customers/FindCustomer",
        autoFocus: true,
        minLength: 3
      });
    };

    goToCustomerId.prototype.okTrigger = function() {
      var selectVal, val;

      val = this.ui.input.val();
      if (!IsInt(val, true)) {
        val = val.substring(0, val.indexOf(','));
      }
      if (!IsInt(val, true)) {
        selectVal = this.ui.select.val();
        if (!IsInt(selectVal, true)) {
          this.addError("Incorrect input");
          return false;
        }
        val = selectVal;
      }
      this.checkCustomer(val);
      return false;
    };

    goToCustomerId.prototype.keydowned = function(e) {
      this.addError("");
      if (this.okBtn.attr("disabled") === "disabled") {
        return;
      }
      if ($('.ui-autocomplete:visible').length !== 0) {
        return;
      }
      if (e.keyCode === 13) {
        return this.okTrigger();
      }
    };

    goToCustomerId.prototype.addError = function(val) {
      return this.ui.errorPlace.text(val);
    };

    goToCustomerId.prototype.checkCustomer = function(id) {
      var xhr,
        _this = this;

      this.okBtn.attr("disabled", "disabled");
      xhr = $.get("" + window.gRootPath + "Underwriter/Customers/CheckCustomer?customerId=" + id);
      xhr.done(function(res) {
        switch (res.State) {
          case "NotFound":
            _this.addError("Customer id. #" + id + " was not found");
            break;
          case "NotSuccesfullyRegistred":
            _this.addError("Customer id #" + id + " not successfully registered");
            break;
          case "Ok":
            _this.trigger("ok", id);
            _this.dialog.dialog("close");
            break;
        }
      });
      return xhr.complete(function() {
        return _this.okBtn.removeAttr("disabled");
      });
    };

    return goToCustomerId;

  })(Backbone.Marionette.ItemView);

}).call(this);

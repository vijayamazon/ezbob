(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.ApplyForLoanModel = (function(_super) {

    __extends(ApplyForLoanModel, _super);

    function ApplyForLoanModel() {
      return ApplyForLoanModel.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanModel.prototype.defaults = {
      neededCash: 100,
      maxCash: 15000,
      minCash: EzBob.Config.MinLoan,
      agree: false,
      agreement: false,
      CreditSum: 0,
      OfferValid: 0,
      OfferValidMintes: 0
    };

    ApplyForLoanModel.prototype.validate = function(attrs) {
      var val;
      if (typeof attrs.neededCash !== "undefined") {
        val = attrs.neededCash;
        if (isNaN(val)) {
          attrs.neededCash = this.get("minCash");
        }
        if (val > this.get("maxCash")) {
          attrs.neededCash = this.get("maxCash");
        }
        if (val < this.get("minCash")) {
          attrs.neededCash = this.get("minCash");
        }
      }
      return false;
    };

    ApplyForLoanModel.prototype.initialize = function() {
      return this.set({
        neededCash: this.get("maxCash"),
        minCash: (this.get("maxCash") > EzBob.Config.MinLoan ? EzBob.Config.MinLoan : EzBob.Config.XMinLoan)
      });
    };

    return ApplyForLoanModel;

  })(Backbone.Model);

  EzBob.Profile.ApplyForLoanView = (function(_super) {

    __extends(ApplyForLoanView, _super);

    function ApplyForLoanView() {
      return ApplyForLoanView.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanView.prototype.template = "#apply-forloan-template";

    ApplyForLoanView.prototype.initialize = function(options) {
      this.customer = options.customer;
      console.log(this.customer);
      this.model = new EzBob.Profile.ApplyForLoanModel({
        maxCash: this.customer.get("CreditSum"),
        OfferStart: this.customer.get("OfferStart")
      });
      this.model.on("change:neededCash", this.neededCashChanged, this);
      this.model.set({
        "CreditSum": this.customer.get("CreditSum"),
        "OfferValid": this.customer.offerValidFormatted()
      });
      this.recalculateThrottled = _.debounce(this.recalculateSchedule, 250);
      this.timerId = setInterval(_.bind(this.refreshTimer, this), 1000);
      if (this.customer.get("CreditSum") < EzBob.Config.XMinLoan) {
        this.trigger("back");
        document.location.href = "#";
      }
    };

    ApplyForLoanView.prototype.events = {
      "click .submit": "submit",
      "change input[name=\"loanAmount\"]": "loanAmountChanged",
      "change .preAgreementTermsRead": "showSubmit",
      "change .agreementTermsRead": "showSubmit",
      "click .download": "download",
      "click .print": "print"
    };

    ApplyForLoanView.prototype.ui = {
      submit: ".submit",
      agreement: ".agreement"
    };

    ApplyForLoanView.prototype.showSubmit = function() {
      var read, readAgreement, readPreAgreement;
      readPreAgreement = $(".preAgreementTermsRead").is(":checked");
      readAgreement = $(".agreementTermsRead").is(":checked");
      read = readAgreement === true && readPreAgreement === true;
      this.model.set("agree", read);
      return this.$el.find(".submit").toggleClass("disabled", !read);
    };

    ApplyForLoanView.prototype.loanAmountChanged = function(e) {
      var amount;
      amount = this.$el.find("input[name=\"loanAmount\"]").autoNumericGet();
      return this.model.set("neededCash", parseInt(amount, 10));
    };

    ApplyForLoanView.prototype.recalculateSchedule = function(val) {
      var that;
      that = this;
      return $.getJSON(window.gRootPath + "Customer/Schedule/Calculate?amount=" + parseInt(val)).done(function(data) {
        return that.renderSchedule(data);
      });
    };

    ApplyForLoanView.prototype.renderSchedule = function(schedule) {
      var scheduleView;
      scheduleView = new EzBob.LoanScheduleView({
        el: this.$el.find(".loan-schedule"),
        schedule: schedule
      });
      scheduleView.render();
      return this.createAgreementView(schedule.agreement);
    };

    ApplyForLoanView.prototype.neededCashChanged = function() {
      var value;
      value = this.model.get("neededCash");
      this.ui.submit.attr("href", "GetCash/GetTransactionId?loan_amount=" + value);
      this.recalculateThrottled(value);
      this.$el.find("input[name=\"loanAmount\"]").autoNumericSet(value);
      return this.$el.find("#loan-slider").slider("value", value);
    };

    ApplyForLoanView.prototype.onRender = function() {
      var max, min, sliderOptions, updateSlider,
        _this = this;
      updateSlider = function(event, ui) {
        var percent, slider;
        percent = (ui.value - min) / (max - min) * 100;
        slider = _this.$el.find(".ui-slider");
        slider.css("background", "-webkit-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) " + percent + "%,rgba(201,201,201,1) " + percent + "%,rgba(229,229,229,1) 100%)");
        slider.css("background", "-moz-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) " + percent + "%,rgba(201,201,201,1) " + percent + "%,rgba(229,229,229,1) 100%)");
        slider.css("background", "-o-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) " + percent + "%,rgba(201,201,201,1) " + percent + "%,rgba(229,229,229,1) 100%)");
        slider.css("background", "-ms-linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) " + percent + "%,rgba(201,201,201,1) " + percent + "%,rgba(229,229,229,1) 100%)");
        slider.css("-pie-background", "linear-gradient(left, rgba(30,87,153,1) 0%,rgba(41,137,216,1) " + percent + "%,rgba(201,201,201,1) " + percent + "%,rgba(229,229,229,1) 100%)");
        if (ui.value === _this.model.get("neededCash")) {
          return;
        }
        return _this.model.set("neededCash", ui.value);
      };
      max = this.model.get("maxCash");
      min = this.model.get("minCash");
      sliderOptions = {
        max: max,
        min: min,
        value: this.model.get("neededCash"),
        step: EzBob.Config.GetCashSliderStep,
        slide: updateSlider,
        change: updateSlider
      };
      this.$el.find("#loan-slider").slider(sliderOptions);
      this.$el.find("input[name=\"loanAmount\"]").moneyFormat();
      this.neededCashChanged();
      this.$el.find("img[rel]").popover().on("click", function() {
        return false;
      });
      this.$el.find("li[rel]").popover({
        placement: "left"
      });
      return this;
    };

    ApplyForLoanView.prototype.refreshTimer = function() {
      return this.$el.find(".offerValidFor").text(this.customer.offerValidFormatted());
    };

    ApplyForLoanView.prototype.submit = function() {
      var creditSum, max, min, view,
        _this = this;
      creditSum = this.model.get("neededCash");
      max = this.model.get("maxCash");
      min = this.model.get("minCash");
      this.model.set("neededCash", creditSum);
      if (creditSum > max || creditSum < min) {
        return false;
      }
      if (!$(".preAgreementTermsRead").is(":checked") || !$(".agreementTermsRead").is(":checked")) {
        false;
      }
      if (this.customer.get('PayPointCards').length > 0) {
        view = new EzBob.Profile.PayPointCardSelectView();
        view.on('select', function(cardId) {
          return console.log('card id #{cardId} selected');
        });
        EzBob.App.modal.show(view);
        return false;
      }
    };

    ApplyForLoanView.prototype._submit = function() {
      return document.location.href = this.ui.submit.attr("href");
    };

    ApplyForLoanView.prototype.getCurrentViewId = function() {
      var current;
      current = this.$el.find("li.active a").attr("page-name");
      return current;
    };

    ApplyForLoanView.prototype.print = function() {
      printElement(this.getCurrentViewId());
      return false;
    };

    ApplyForLoanView.prototype.download = function() {
      var amount, view;
      amount = parseInt(this.model.get("neededCash"), 10);
      view = this.getCurrentViewId();
      location.href = "" + window.gRootPath + "Customer/Agreement/Download?amount=" + amount + "&viewName=" + view;
      return false;
    };

    ApplyForLoanView.prototype.createAgreementView = function(agreementdata) {
      var bussinessType, isConsumer, view;
      bussinessType = (this.customer.get("CustomerPersonalInfo")).TypeOfBusiness;
      isConsumer = bussinessType === 0 || bussinessType === 4 || bussinessType === 2;
      view = isConsumer ? new EzBob.Profile.ConsumersAgreementView({
        el: this.ui.agreement
      }) : new EzBob.Profile.CompaniesAgreementView({
        el: this.ui.agreement
      });
      view.render(agreementdata);
      return this.showSubmit();
    };

    ApplyForLoanView.prototype.close = function() {
      clearInterval(this.timerId);
      this.model.off();
      return ApplyForLoanView.__super__.close.call(this);
    };

    return ApplyForLoanView;

  })(Backbone.Marionette.ItemView);

  EzBob.Profile.ThankyouLoan = Backbone.View.extend({
    initialize: function() {
      return this.template = _.template($("#thankyouloan-template").html());
    },
    events: {
      "click a": "close"
    },
    render: function() {
      return this.$el.html(this.template());
    },
    close: function() {
      this.trigger("close");
      return false;
    }
  });

  EzBob.GetCashConfirmation = Backbone.Marionette.ItemView.extend({
    template: "#apply-forloan-confirmation-template",
    events: {
      "click a.cancel": "btnClose",
      "click a.save": "btnSave"
    },
    btnClose: function() {
      this.close();
      return false;
    },
    btnSave: function() {
      this.trigger("modal:save");
      this.onOk();
      this.close();
      return false;
    },
    onOk: function() {}
  });

}).call(this);

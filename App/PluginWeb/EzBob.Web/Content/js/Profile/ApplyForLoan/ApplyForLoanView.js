(function() {
  var root,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.ApplyForLoanView = (function(_super) {

    __extends(ApplyForLoanView, _super);

    function ApplyForLoanView() {
      this.loanSelectionChanged = __bind(this.loanSelectionChanged, this);
      return ApplyForLoanView.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanView.prototype.template = "#apply-forloan-template";

    ApplyForLoanView.prototype.initialize = function(options) {
      this.customer = options.customer;
      if (this.customer.get("CreditSum") < EzBob.Config.XMinLoan) {
        this.trigger("back");
        document.location.href = "#";
        return;
      }
      this.fixed = this.customer.get('IsLoanDetailsFixed');
      this.isLoanTypeSelectionAllowed = this.customer.get('IsLoanTypeSelectionAllowed');
      this.currentLoanTypeID = 1;
      this.currentRepaymentPeriod = this.customer.get('LastApprovedRepaymentPeriod');
      this.recalculateThrottled = _.debounce(this.recalculateSchedule, 250);
      this.timerId = setInterval(_.bind(this.refreshTimer, this), 1000);
      this.model.set({
        "CreditSum": this.customer.get("CreditSum"),
        "OfferValid": this.customer.offerValidFormatted()
      });
      if (this.fixed) {
        return;
      }
      this.model.on("change:neededCash", this.neededCashChanged, this);
      return this.isLoanSourceEU = options.model.get("isLoanSourceEU");
    };

    ApplyForLoanView.prototype.events = {
      "click .submit": "submit",
      "change .preAgreementTermsRead": "preAgreementTermsReadChange",
      "change .agreementTermsRead": "showSubmit",
      "change .euAgreementTermsRead": "showSubmit",
      "change .directorConsentRead": "showSubmit",
      "change #signedName": "showSubmit",
      "blur #signedName": "showSubmit",
      "keyup #signedName": "showSubmit",
      "paste #signedName": "showSubmit",
      "click .download": "download",
      "click .print": "print"
    };

    ApplyForLoanView.prototype.ui = {
      submit: ".submit",
      agreement: ".agreement",
      form: "form"
    };

    ApplyForLoanView.prototype.preAgreementTermsReadChange = function() {
      var readPreAgreement;
      readPreAgreement = $(".preAgreementTermsRead").is(":checked");
      $(".agreementTermsRead").attr("disabled", !readPreAgreement);
      if (readPreAgreement) {
        this.$el.find("a[href=\"#tab4\"]").tab("show");
      } else {
        this.$el.find("a[href=\"#tab3\"]").tab("show");
        $(".agreementTermsRead").attr("checked", false);
      }
      return this.showSubmit();
    };

    ApplyForLoanView.prototype.loanSelectionChanged = function(e) {
      var amount;
      this.currentRepaymentPeriod = this.$('#loan-sliders .period-slider').slider('value');
      amount = this.$('#loan-sliders .amount-slider').slider('value');
      this.model.set("neededCash", parseInt(amount, 10));
      this.model.set("loanType", this.currentLoanTypeID);
      this.model.set("repaymentPeriod", this.currentRepaymentPeriod);
      return this.neededCashChanged(true);
    };

    ApplyForLoanView.prototype.showSubmit = function() {
      var enabled;
      enabled = EzBob.Validation.checkForm(this.validator);
      this.model.set("agree", enabled);
      return this.ui.submit.toggleClass("disabled", !enabled);
    };

    ApplyForLoanView.prototype.recalculateSchedule = function(args) {
      var sMoreParams, val,
        _this = this;
      val = args.value;
      /*
          unless args.reloadSelectedOnly is true
          $.getJSON("#{window.gRootPath}Customer/Schedule/CalculateAll?amount=#{parseInt(val)}").done (data) =>
          for loanKey, offer of data
          $('#loan-type-' + loanKey + ' .Interest').text EzBob.formatPounds offer.TotalInterest
          $('#loan-type-' + loanKey + ' .Total').text EzBob.formatPounds offer.Total
      */

      BlockUi("on", this.$el.find('#block-loan-schedule'));
      BlockUi("on", this.$el.find('#block-agreement'));
      sMoreParams = '&loanType=' + this.currentLoanTypeID + '&repaymentPeriod=' + this.currentRepaymentPeriod;
      return $.getJSON(("" + window.gRootPath + "Customer/Schedule/Calculate?amount=" + (parseInt(val))) + sMoreParams).done(function(data) {
        _this.renderSchedule(data);
        BlockUi("off", _this.$el.find('#block-loan-schedule'));
        return BlockUi("off", _this.$el.find('#block-agreement'));
      });
    };

    ApplyForLoanView.prototype.renderSchedule = function(schedule) {
      var scheduleView;
      this.lastPaymentDate = moment(schedule.Schedule[schedule.Schedule.length - 1].Date);
      scheduleView = new EzBob.LoanScheduleView({
        el: this.$el.find(".loan-schedule"),
        schedule: schedule,
        isShowGift: false,
        isShowExportBlock: false,
        isShowExceedMaxInterestForSource: false
      });
      scheduleView.render();
      return this.createAgreementView(schedule.Agreement);
    };

    ApplyForLoanView.prototype.neededCashChanged = function(reloadSelectedOnly) {
      var value;
      this.$el.find('.preAgreementTermsRead, .agreementTermsRead, .euAgreementTermsRead').prop('checked', false);
      value = this.model.get("neededCash");
      this.ui.submit.attr("href", this.model.get("url"));
      return this.recalculateThrottled({
        value: value,
        reloadSelectedOnly: reloadSelectedOnly
      });
    };

    ApplyForLoanView.prototype.onRender = function() {
      var pi, _ref, _ref1,
        _this = this;
      if (this.fixed) {
        this.$(".cash-question").hide();
      }
      if (!((_ref = this.isLoanTypeSelectionAllowed) === 1 || _ref === '1') || this.isLoanSourceEU) {
        this.$('.duration-select-allowed').hide();
      }
      if (!this.isLoanSourceEU) {
        this.$('.eu-agreement-section').hide();
      }
      if (this.model.get('isCurrentCashRequestFromQuickOffer')) {
        this.$('.loan-amount-header-start').text('Confirm loan amount');
      } else {
        this.$('.quick-offer-section').remove();
        InitAmountPeriodSliders({
          container: this.$('#loan-sliders'),
          amount: {
            min: this.model.get('minCash'),
            max: this.model.get('maxCash'),
            start: this.model.get('maxCash'),
            step: 100
          },
          period: {
            min: 3,
            max: 12,
            start: this.model.get('repaymentPeriod'),
            step: 1,
            hide: !((_ref1 = this.isLoanTypeSelectionAllowed) === 1 || _ref1 === '1') || this.isLoanSourceEU
          },
          callback: function(ignored, sEvent) {
            if (sEvent === 'change') {
              return _this.loanSelectionChanged();
            }
          }
        });
      }
      this.neededCashChanged();
      this.$el.find("img[rel]").setPopover('right');
      this.$el.find("li[rel]").setPopover('left');
      pi = this.customer.get('CustomerPersonalInfo');
      this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);
      this.validator = EzBob.validateLoanLegalForm(this.ui.form, [pi.FirstName, pi.Surname]);
      return this;
    };

    ApplyForLoanView.prototype.refreshTimer = function() {
      return this.$el.find(".offerValidFor").text(this.customer.offerValidFormatted());
    };

    ApplyForLoanView.prototype.submit = function(e) {
      var creditSum, enabled, max, min;
      e.preventDefault();
      creditSum = this.model.get("neededCash");
      max = this.model.get("maxCash");
      min = this.model.get("minCash");
      this.model.set("neededCash", creditSum);
      this.model.set("loanType", this.currentLoanTypeID);
      this.model.set("repaymentPeriod", this.currentRepaymentPeriod);
      if (creditSum > max || creditSum < min) {
        return false;
      }
      enabled = EzBob.Validation.checkForm(this.validator);
      if (!enabled) {
        this.showSubmit();
        return false;
      }
      this.trigger("submit");
      return false;
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
      var amount, loanType, repaymentPeriod, view;
      amount = parseInt(this.model.get("neededCash"), 10);
      loanType = this.currentLoanTypeID;
      repaymentPeriod = this.currentRepaymentPeriod;
      view = this.getCurrentViewId();
      location.href = "" + window.gRootPath + "Customer/Agreement/Download?amount=" + amount + "&viewName=" + view + "&loanType=" + loanType + "&repaymentPeriod=" + repaymentPeriod;
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

}).call(this);

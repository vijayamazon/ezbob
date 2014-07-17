(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.ApplyForLoanTopViewModel = (function(_super) {

    __extends(ApplyForLoanTopViewModel, _super);

    function ApplyForLoanTopViewModel() {
      return ApplyForLoanTopViewModel.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanTopViewModel.prototype.defaults = {
      state: "apply"
    };

    return ApplyForLoanTopViewModel;

  })(Backbone.Model);

  EzBob.Profile.ApplyForLoanTopView = (function(_super) {

    __extends(ApplyForLoanTopView, _super);

    function ApplyForLoanTopView() {
      this.createAddBankAccountView = __bind(this.createAddBankAccountView, this);

      this.createApplyForLoanView = __bind(this.createApplyForLoanView, this);
      return ApplyForLoanTopView.__super__.constructor.apply(this, arguments);
    }

    ApplyForLoanTopView.prototype.initialize = function(options) {
      this.template = _.template($("#apply-forloan-top-template").html());
      this.customer = options.customer;
      this.applyForLoanViewModel = new EzBob.Profile.ApplyForLoanModel({
        maxCash: this.customer.get("CreditSum"),
        OfferStart: this.customer.get("OfferStart"),
        loanType: this.customer.get("LastApprovedLoanTypeID"),
        repaymentPeriod: this.customer.get("LastApprovedRepaymentPeriod"),
        isLoanSourceEU: this.customer.get("IsLastApprovedLoanSourceEu"),
        isCurrentCashRequestFromQuickOffer: this.customer.get("IsCurrentCashRequestFromQuickOffer")
      });
      this.states = {
        apply: this.createApplyForLoanView,
        bank: this.createAddBankAccountView
      };
      return this.model.on("change", this.render, this);
    };

    ApplyForLoanTopView.prototype.render = function() {
      var region, view;
      this.$el.html(this.template());
      view = this.states[this.model.get("state")]();
      region = new Backbone.Marionette.Region({
        el: this.$el.find('.apply-for-loan-div')
      });
      region.show(view);
      return false;
    };

    ApplyForLoanTopView.prototype.createApplyForLoanView = function() {
      var view;
      view = new EzBob.Profile.ApplyForLoanView({
        model: this.applyForLoanViewModel,
        customer: this.customer
      });
      view.on("submit", this.amountSelected, this);
      return view;
    };

    ApplyForLoanTopView.prototype.createAddBankAccountView = function() {
      var view,
        _this = this;
      view = new EzBob.BankAccountInfoView({
        model: this.customer
      });
      view.on("back", function() {
        return _this.model.set("state", "apply");
      });
      view.on("completed", function() {
        return _this.submit();
      });
      return view;
    };

    ApplyForLoanTopView.prototype.amountSelected = function() {
      var data, enabled, form, pi, xhr,
        _this = this;
      form = this.$el.find('form');
      pi = this.customer.get('CustomerPersonalInfo');
      this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);
      enabled = EzBob.Validation.checkForm(EzBob.validateLoanLegalForm(form, [pi.FirstName, pi.Surname]));
      if (!enabled) {
        return;
      }
      data = form.serialize();
      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Customer/GetCash/LoanLegalSigned", data);
      xhr.done(function(res) {
        if (res.error) {
          EzBob.App.trigger('error', res.error);
          return;
        }
        if (!_this.customer.get("bankAccountAdded")) {
          _this.model.set("state", "bank");
          return;
        }
        return _this.submit();
      });
      return xhr.always(function() {
        return BlockUi("off");
      });
    };

    ApplyForLoanTopView.prototype.submit = function() {
      var view,
        _this = this;
      view = new EzBob.Profile.PayPointCardSelectView({
        model: this.customer,
        date: this.lastPaymentDate
      });
      if (view.cards.length > 0) {
        view.on('select', function(cardId) {
          var xhr;
          BlockUi("on");
          xhr = $.post("" + window.gRootPath + "Customer/GetCash/Now", {
            cardId: cardId,
            amount: _this.applyForLoanViewModel.get("neededCash")
          });
          xhr.done(function(data) {
            if (data.error !== void 0) {
              return EzBob.ShowMessage(data.error, "Error occured");
            } else {
              return document.location.href = data.url;
            }
          });
          return xhr.complete(function() {
            return BlockUi("off");
          });
        });
        view.on('existing', function() {
          return _this._submit();
        });
        view.on('cancel', function() {
          return _this.model.set("state", "apply");
        });
        EzBob.App.modal.show(view);
        return false;
      } else {
        this._submit();
      }
      return false;
    };

    ApplyForLoanTopView.prototype._submit = function() {
      BlockUi("on");
      this.applyForLoanViewModel.buildUrl();
      return document.location.href = this.applyForLoanViewModel.get('url');
    };

    return ApplyForLoanTopView;

  })(Backbone.Marionette.ItemView);

}).call(this);

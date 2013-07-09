(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter.ProfileView = (function(_super) {
    __extends(ProfileView, _super);

    function ProfileView() {
      _ref = ProfileView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    ProfileView.prototype.initialize = function() {
      return this.template = _.template($("#profile-template-main").html());
    };

    ProfileView.prototype.render = function() {
      var alertPassed, apiChecks, controlButtons, customerRelations, experianInfo, loanInfo, loanhistorys, marketplaces, medalCalculations, messages, paymentAccounts, profileInfo, profileTabs, summaryInfo,
        _this = this;

      this.$el.html(this.template());
      profileInfo = this.$el.find(".profile-person-info");
      loanInfo = this.$el.find(".profile-loan-info");
      summaryInfo = this.$el.find("#profile-summary");
      profileTabs = this.$el.find("#profile-tabs");
      marketplaces = this.$el.find("#marketplaces");
      experianInfo = this.$el.find("#credit-bureau");
      paymentAccounts = this.$el.find("#payment-accounts");
      loanhistorys = this.$el.find("#loanhistorys");
      medalCalculations = this.$el.find("#medal-calculator");
      messages = this.$el.find("#messages");
      apiChecks = this.$el.find("#apiChecks");
      customerRelations = this.$el.find("#customerRelations");
      alertPassed = this.$el.find("#alerts-passed");
      controlButtons = this.$el.find("#controlButtoons");
      this.personalInfoModel = new EzBob.Underwriter.PersonalInfoModel();
      this.profileInfoView = new EzBob.Underwriter.PersonInfoView({
        el: profileInfo,
        model: this.personalInfoModel
      });
      this.personalInfoModel.on("change", this.changeDecisionButtonsState, this);
      this.marketPlaces = new EzBob.Underwriter.MarketPlaces();
      this.marketPlaceView = new EzBob.Underwriter.MarketPlacesView({
        el: marketplaces,
        model: this.marketPlaces
      });
      this.marketPlaceView.on("rechecked", this.mpRechecked, this.marketPlaces);
      this.loanHistory = new EzBob.Underwriter.LoanHistoryModel();
      this.loanHistoryView = new EzBob.Underwriter.LoanHistoryView({
        el: loanhistorys,
        model: this.loanHistory
      });
      this.experianInfoModel = new EzBob.Underwriter.ExperianInfoModel();
      this.experianInfoView = new EzBob.Underwriter.ExperianInfoView({
        el: experianInfo,
        model: this.experianInfoModel
      });
      this.loanInfoModel = new EzBob.Underwriter.LoanInfoModel();
      this.loanInfoView = new EzBob.Underwriter.LoanInfoView({
        el: loanInfo,
        model: this.loanInfoModel,
        personalInfo: this.personalInfoModel
      });
      this.summaryInfoModel = new EzBob.Underwriter.SummaryInfoModel();
      this.summaryInfoView = new EzBob.Underwriter.SummaryInfoView({
        el: summaryInfo,
        model: this.summaryInfoModel
      });
      EzBob.App.vent.on('newCreditLine:done', function() {
        return _this.summaryInfoModel.fetch();
      });
      EzBob.App.vent.on('newCreditLine:pass', function() {
        return _this.summaryInfoModel.fetch();
      });
      this.paymentAccountsModel = new EzBob.Underwriter.PaymentAccountsModel();
      this.paymentAccountsView = new EzBob.Underwriter.PaymentAccountView({
        el: paymentAccounts,
        model: this.paymentAccountsModel
      });
      this.paymentAccountsView.on("rechecked", this.mpRechecked, this.paymentAccountsModel);
      this.medalCalculationModel = new EzBob.Underwriter.MedalCalculationModel();
      this.medalCalculationView = new EzBob.Underwriter.MedalCalculationView({
        el: medalCalculations,
        model: this.medalCalculationModel
      });
      this.crossCheckView = new EzBob.Underwriter.CrossCheckView({
        el: this.$el.find("#customer-info")
      });
      this.messagesModel = new EzBob.Underwriter.MessageModel();
      this.Message = new EzBob.Underwriter.Message({
        el: messages,
        model: this.messagesModel
      });
      this.Message.on("creditResultChanged", this.changedSystemDecision, this);
      this.alertDocsView = new EzBob.Underwriter.AlertDocsView({
        el: this.$el.find("#alert-docs")
      });
      this.ApicCheckLogs = new EzBob.Underwriter.ApiChecksLogs();
      this.ApiChecksLogView = new EzBob.Underwriter.ApiChecksLogView({
        el: apiChecks,
        model: this.ApicCheckLogs
      });
      this.CustomerRelationsData = new EzBob.Underwriter.CustomerRelationsData();
      this.CustomerRelationsView = new EzBob.Underwriter.CustomerRelationsView({
        el: customerRelations,
        model: this.CustomerRelationsData
      });
      this.showed = true;
      this.controlButtons = new EzBob.Underwriter.ControlButtonsView({
        el: controlButtons
      });
      return this;
    };

    ProfileView.prototype.events = {
      "click #RejectBtn": "RejectBtnClick",
      "click #ApproveBtn": "ApproveBtnClick",
      "click #EscalateBtn": "EscalateBtnClick",
      "click #SuspendBtn": "SuspendBtnClick",
      "click #ReturnBtn": "ReturnBtnClick"
    };

    ProfileView.prototype.checkCustomerAvailability = function(model) {
      var data;

      data = model.toJSON();
      if (data.success === false) {
        EzBob.ShowMessage(data.error, "Error", (function() {
          return Redirect("#");
        }), "OK");
        return false;
      } else {
        if (this.showed) {
          this.$el.show();
        }
        return $(".tabbable a[href=\"#profile-summary\"]").tab("show");
      }
    };

    ProfileView.prototype.mpRechecked = function(parameter) {
      var model, umi;

      model = this;
      umi = parameter.umi;
      return model.fetch().done(function() {
        var el, interval;

        el = $("#" + parameter.el.attr("id"));
        el.addClass("disabled");
        return interval = setInterval(function() {
          var req;

          req = $.get(window.gRootPath + "Underwriter/MarketPlaces/CheckForUpdatedStatus", {
            mpId: umi
          });
          return req.done(function(response) {
            if (response.status !== "In progress") {
              clearInterval(interval);
              return model.fetch().done(function() {
                return el.removeClass("disabled");
              });
            }
          });
        }, 1000);
      });
    };

    ProfileView.prototype.disableChange = function(id) {
      return this.show(id);
    };

    ProfileView.prototype.RejectBtnClick = function(e) {
      var functionPopupView;

      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      functionPopupView = new EzBob.Underwriter.RejectedDialog({
        model: this.loanInfoModel
      });
      functionPopupView.render();
      functionPopupView.on("changedSystemDecision", this.changedSystemDecision, this);
      return false;
    };

    ProfileView.prototype.ApproveBtnClick = function(e) {
      var dialog;

      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      if (this.loanInfoModel.get('OfferedCreditLine') <= 0) {
        EzBob.ShowMessage('Wrong Offered credit line value (' + this.loanInfoModel.get('OfferedCreditLine') + '), please enter the valid value (above zero)', 'Error');
        return false;
      }
      if (this.loanInfoModel.get("OfferExpired")) {
        EzBob.ShowMessage("Loan offer has expired. Set new validity date.", "Error");
        return false;
      }
      dialog = new EzBob.Underwriter.ApproveDialog({
        model: this.loanInfoModel
      });
      dialog.render();
      dialog.on("changedSystemDecision", this.changedSystemDecision, this);
      return false;
    };

    ProfileView.prototype.EscalateBtnClick = function(e) {
      var functionPopupView;

      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      functionPopupView = new EzBob.Underwriter.Escalated({
        model: this.loanInfoModel
      });
      functionPopupView.render();
      functionPopupView.on("changedSystemDecision", this.changedSystemDecision, this);
      return false;
    };

    ProfileView.prototype.SuspendBtnClick = function(e) {
      var functionPopupView;

      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      functionPopupView = new EzBob.Underwriter.Suspended({
        model: this.loanInfoModel
      });
      functionPopupView.render();
      functionPopupView.on("changedSystemDecision", this.changedSystemDecision, this);
      return false;
    };

    ProfileView.prototype.ReturnBtnClick = function(e) {
      var functionPopupView;

      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      functionPopupView = new EzBob.Underwriter.Returned({
        model: this.loanInfoModel
      });
      functionPopupView.render();
      functionPopupView.on("changedSystemDecision", this.changedSystemDecision, this);
      return false;
    };

    ProfileView.prototype.changedSystemDecision = function() {
      this.summaryInfoModel.fetch();
      this.personalInfoModel.fetch();
      this.loanInfoModel.fetch();
      this.loanHistory.fetch();
      return this.changeDecisionButtonsState();
    };

    ProfileView.prototype.show = function(id) {
      var xhr,
        _this = this;

      BlockUi("on");
      xhr = $.get("" + window.gRootPath + "Underwriter/Customers/CheckCustomer?customerId=" + id);
      return xhr.done(function(res) {
        BlockUi("off");
        if (res.error) {
          EzBob.ShowMessage(res.error, "Something went wrong");
          _this.router.navigate("", {
            trigger: true,
            replace: true
          });
          return true;
        }
        switch (res.State) {
          case "NotFound":
            EzBob.ShowMessage(res.error, "Customer id. #" + id + " was not found");
            _this.router.navigate("", {
              trigger: true,
              replace: true
            });
            break;
          case "NotSuccesfullyRegistred":
            _this.trigger("customerNotFull", id);
            break;
          case "Ok":
            _this._show(id);
            break;
        }
      });
    };

    ProfileView.prototype._show = function(id) {
      var that;

      this.hide();
      BlockUi("on");
      scrollTop();
      that = this;
      this.customerId = id;
      this.personalInfoModel.set({
        Id: id
      }, {
        silent: true
      });
      this.personalInfoModel.fetch().done(function() {
        return that.changeDecisionButtonsState(that.personalInfoModel.get("Editable"));
      });
      this.loanInfoModel.set({
        Id: id
      }, {
        silent: true
      });
      this.loanInfoModel.fetch();
      this.marketPlaces.customerId = id;
      this.marketPlaces.fetch();
      this.loanHistory.customerId = id;
      this.loanHistoryView.idCustomer = id;
      this.loanHistory.fetch();
      this.experianInfoModel.set({
        Id: id
      }, {
        silent: true
      });
      this.experianInfoModel.fetch();
      this.summaryInfoModel.set({
        Id: id
      }, {
        silent: true
      });
      this.summaryInfoModel.set({
        success: true
      }, {
        silent: true
      });
      this.summaryInfoModel.fetch().complete(function() {
        that.checkCustomerAvailability(that.summaryInfoModel);
        BlockUi("Off");
        EzBob.GlobalUpdateBugsIcon(id);
        if (that.$el.find(".vsplitbar").length === 0) {
          return $("#spl").splitter({
            minLeft: 280,
            sizeLeft: 300,
            minRight: 600
          });
        }
      });
      this.paymentAccountsModel.customerId = id;
      this.paymentAccountsModel.fetch();
      this.medalCalculationModel.set({
        Id: id
      }, {
        silent: true
      });
      this.medalCalculationModel.fetch();
      this.crossCheckView.render({
        customerId: id
      });
      this.messagesModel.set({
        Id: id
      }, {
        silent: true
      });
      this.messagesModel.fetch();
      this.alertDocsView.create(id);
      this.ApicCheckLogs.customerId = id;
      this.ApiChecksLogView.idCustomer = id;
      this.ApicCheckLogs.fetch();
      this.CustomerRelationsData.customerId = id;
      this.CustomerRelationsView.idCustomer = id;
      this.CustomerRelationsData.fetch();
      this.controlButtons.model = new Backbone.Model({
        customerId: id
      });
      return this.controlButtons.render();
    };

    ProfileView.prototype.hide = function() {
      return this.$el.hide();
    };

    ProfileView.prototype.changeDecisionButtonsState = function(isHideAll) {
      var creditResult, disabled;

      disabled = this.personalInfoModel.get("Disabled") === 1;
      creditResult = this.personalInfoModel.get("CreditResult");
      this.$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").toggleClass("disabled", disabled);
      if (isHideAll) {
        this.$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").hide();
      }
      switch (creditResult) {
        case "WaitingForDecision":
          this.$el.find("#ReturnBtn").hide();
          this.$el.find("#RejectBtn").show();
          this.$el.find("#ApproveBtn").show();
          this.$el.find("#SuspendBtn").show();
          if (!escalatedFlag) {
            return this.$el.find("#EscalateBtn").show();
          }
          break;
        case "Rejected":
        case "Approved":
        case "Late":
          this.$el.find("#ReturnBtn").hide();
          this.$el.find("#RejectBtn").hide();
          this.$el.find("#ApproveBtn").hide();
          this.$el.find("#SuspendBtn").hide();
          return this.$el.find("#EscalateBtn").hide();
        case "Escalated":
          this.$el.find("#ReturnBtn").hide();
          this.$el.find("#RejectBtn").show();
          this.$el.find("#ApproveBtn").show();
          this.$el.find("#SuspendBtn").show();
          return this.$el.find("#EscalateBtn").hide();
        case "ApprovedPending":
          this.$el.find("#ReturnBtn").show();
          this.$el.find("#RejectBtn").hide();
          this.$el.find("#ApproveBtn").hide();
          this.$el.find("#SuspendBtn").hide();
          return this.$el.find("#EscalateBtn").hide();
      }
    };

    ProfileView.prototype.updateAlerts = function() {
      return this.alertsModel.fetch();
    };

    return ProfileView;

  })(Backbone.View);

}).call(this);

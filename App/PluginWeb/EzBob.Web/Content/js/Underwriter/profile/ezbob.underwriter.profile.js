(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter.ProfileView = (function(_super) {

    __extends(ProfileView, _super);

    function ProfileView() {
      return ProfileView.__super__.constructor.apply(this, arguments);
    }

    ProfileView.prototype.initialize = function() {
      return this.template = _.template($("#profile-template-main").html());
    };

    ProfileView.prototype.render = function() {
      var alertPassed, apiChecks, controlButtons, customerRelations, experianInfo, fraudDetection, loanInfo, loanhistorys, marketplaces, medalCalculations, messages, paymentAccounts, profileInfo, summaryInfo, that,
        _this = this;
      this.$el.html(this.template());
      profileInfo = this.$el.find(".profile-person-info");
      loanInfo = this.$el.find(".profile-loan-info");
      summaryInfo = this.$el.find("#profile-summary");
      marketplaces = this.$el.find("#marketplaces");
      experianInfo = this.$el.find("#credit-bureau");
      paymentAccounts = this.$el.find("#payment-accounts");
      loanhistorys = this.$el.find("#loanhistorys");
      medalCalculations = this.$el.find("#medal-calculator");
      messages = this.$el.find("#messages");
      apiChecks = this.$el.find("#apiChecks");
      customerRelations = this.$el.find("#customerRelations");
      alertPassed = this.$el.find("#alerts-passed");
      controlButtons = this.$el.find("#controlButtons");
      fraudDetection = this.$el.find("#fraudDetection");
      this.personalInfoModel = new EzBob.Underwriter.PersonalInfoModel();
      this.profileInfoView = new EzBob.Underwriter.PersonInfoView({
        el: profileInfo,
        model: this.personalInfoModel
      });
      this.personalInfoModel.on("change", this.changeDecisionButtonsState, this);
      this.marketPlaces = new EzBob.Underwriter.MarketPlaces();
      this.marketPlaceView = new EzBob.Underwriter.MarketPlacesView({
        el: marketplaces,
        model: this.marketPlaces,
        personalInfoModel: this.personalInfoModel
      });
      that = this;
      this.marketPlaceView.on("rechecked", this.mpRechecked, this.marketPlaces);
      EzBob.App.vent.on('ct:marketplaces.history', function(history) {
        return that.show(that.marketPlaces.customerId, true, history);
      });
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
        personalInfo: this.personalInfoModel,
        parentView: this
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
      this.companyScoreModel = new EzBob.Underwriter.CompanyScoreModel();
      this.companyScoreView = new EzBob.Underwriter.CompanyScoreView({
        el: that.$el.find('#company-score-list'),
        model: this.companyScoreModel
      });
      that.$el.find('a.company-score-tab').on('shown.bs.tab', function(evt) {
        return that.companyScoreView.showAccordion();
      });
      this.crossCheckView = new EzBob.Underwriter.CrossCheckView({
        el: this.$el.find("#customer-info"),
        marketPlaces: this.marketPlaces,
        companyScore: this.companyScoreModel
      });
      that.$el.find('a.cross-check-tab').on('shown.bs.tab', function(evt) {
        return that.crossCheckView.doCrossCheck();
      });
      this.messagesModel = new EzBob.Underwriter.MessageModel();
      this.Message = new EzBob.Underwriter.Message({
        el: messages,
        model: this.messagesModel
      });
      this.Message.on("creditResultChanged", this.changedSystemDecision, this);
      this.alertDocs = new EzBob.Underwriter.Docs();
      this.alertDocsView = new EzBob.Underwriter.AlertDocsView({
        el: this.$el.find("#alert-docs"),
        model: this.alertDocs
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
      this.FraudDetectionLogs = new EzBob.Underwriter.FraudDetectionLogs();
      this.FraudDetectionLogView = new EzBob.Underwriter.FraudDetectionLogView({
        el: fraudDetection,
        model: this.FraudDetectionLogs
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

    ProfileView.prototype.recordRecentCustomers = function(id) {
      var xhr;
      xhr = $.post("" + gRootPath + "Underwriter/Customers/SetRecentCustomer", {
        id: id
      });
      return xhr.done(function(recentCustomersModel) {
        return localStorage.setItem('RecentCustomers', JSON.stringify(recentCustomersModel.RecentCustomers));
      });
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
      return this.show(id, false);
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
      var approveLoanWithoutAMLDialog;
      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      if (this.loanInfoModel.get('InterestRate') <= 0) {
        EzBob.ShowMessage('Wrong Interest Rate value (' + this.loanInfoModel.get('InterestRate') + '), please enter the valid value (above zero)', 'Error');
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
      this.skipPopupForApprovalWithoutAML = this.loanInfoModel.get("SkipPopupForApprovalWithoutAML");
      if (this.loanInfoModel.get("AMLResult") !== 'Passed' && !this.skipPopupForApprovalWithoutAML) {
        approveLoanWithoutAMLDialog = new EzBob.Underwriter.ApproveLoanWithoutAML({
          model: this.loanInfoModel,
          parent: this,
          skipPopupForApprovalWithoutAML: this.skipPopupForApprovalWithoutAML
        });
        EzBob.App.modal.show(approveLoanWithoutAMLDialog);
        return false;
      }
      return this.CheckCustomerStatusAndCreateApproveDialog();
    };

    ProfileView.prototype.CheckCustomerStatusAndCreateApproveDialog = function() {
      var approveLoanForWarningStatusCustomer;
      if (this.personalInfoModel.get("IsWarning")) {
        approveLoanForWarningStatusCustomer = new EzBob.Underwriter.ApproveLoanForWarningStatusCustomer({
          model: this.personalInfoModel,
          parent: this
        });
        EzBob.App.modal.show(approveLoanForWarningStatusCustomer);
        return false;
      }
      return this.CreateApproveDialog();
    };

    ProfileView.prototype.CreateApproveDialog = function() {
      var dialog;
      dialog = new EzBob.Underwriter.ApproveDialog({
        model: this.loanInfoModel
      });
      dialog.on("changedSystemDecision", this.changedSystemDecision, this);
      dialog.render();
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

    ProfileView.prototype.show = function(id, isHistory, history) {
      var fullModel, that, _ref,
        _this = this;
      this.hide();
      BlockUi("on");
      scrollTop();
      that = this;
      this.companyScoreView.hideAccordion();
      this.customerId = id;
      fullModel = new EzBob.Underwriter.CustomerFullModel({
        customerId: id,
        history: (_ref = EzBob.parseDate(history)) != null ? _ref : {
          history: null
        }
      });
      fullModel.fetch().done(function() {
        switch (fullModel.get("State")) {
          case "NotFound":
            EzBob.ShowMessage(res.error, "Customer id. #" + id + " was not found");
            _this.router.navigate("", {
              trigger: true,
              replace: true
            });
            return;
          case "NotSuccesfullyRegistred":
            _this.trigger("customerNotFull", id);
            return;
        }
        _this.personalInfoModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.personalInfoModel.set(fullModel.get("PersonalInfoModel"), {
          silent: true
        });
        _this.personalInfoModel.changeDisabled(true);
        _this.changeDecisionButtonsState(_this.personalInfoModel.get("Editable"));
        _this.personalInfoModel.trigger("sync");
        _this.loanInfoModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.loanInfoModel.set(fullModel.get("ApplicationInfoModel"), {
          silent: true
        });
        _this.loanInfoModel.trigger("sync");
        _this.marketPlaces.customerId = id;
        _this.marketPlaces.history = history;
        _this.marketPlaces.reset(fullModel.get("Marketplaces"), {
          silent: true
        });
        _this.marketPlaces.trigger("sync");
        _this.loanHistory.customerId = id;
        _this.loanHistoryView.idCustomer = id;
        _this.loanHistory.set(fullModel.get("LoansAndOffers"), {
          silent: true
        });
        _this.loanHistory.trigger("sync");
        _this.summaryInfoModel.set({
          Id: id,
          success: true
        }, {
          silent: true
        });
        _this.summaryInfoModel.set(fullModel.get("SummaryModel"), {
          silent: true
        });
        _this.summaryInfoModel.trigger("sync");
        _this.checkCustomerAvailability(_this.summaryInfoModel);
        _this.recordRecentCustomers(id);
        EzBob.UpdateBugsIcons(fullModel.get("Bugs"));
        if (that.$el.find(".vsplitbar").length === 0) {
          $("#spl").splitter({
            minLeft: 280,
            sizeLeft: 300,
            minRight: 600
          });
        }
        _this.experianInfoModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.experianInfoModel.set(fullModel.get("CreditBureauModel"), {
          silent: true
        });
        _this.experianInfoModel.trigger("sync");
        _this.paymentAccountsModel.customerId = id;
        _this.paymentAccountsModel.set(fullModel.get("PaymentAccountModel"), {
          silent: true
        });
        _this.paymentAccountsModel.trigger("sync");
        _this.medalCalculationModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.medalCalculationModel.set(fullModel.get("MedalCalculations"), {
          silent: true
        });
        _this.medalCalculationModel.trigger("sync");
        _this.FraudDetectionLogs.customerId = id;
        _this.FraudDetectionLogView.idCustomer = id;
        _this.FraudDetectionLogs.reset(fullModel.get("FraudDetectionLog"), {
          silent: true
        });
        _this.FraudDetectionLogs.trigger("sync");
        _this.ApicCheckLogs.customerId = id;
        _this.ApiChecksLogView.idCustomer = id;
        _this.ApicCheckLogs.reset(fullModel.get("ApiCheckLogs"), {
          silent: true
        });
        _this.ApicCheckLogs.trigger("sync");
        _this.messagesModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.messagesModel.set({
          attaches: fullModel.get("Messages"),
          silent: true
        });
        _this.messagesModel.trigger("sync");
        _this.CustomerRelationsData.customerId = id;
        _this.CustomerRelationsView.idCustomer = id;
        _this.CustomerRelationsData.reset(fullModel.get("CustomerRelations"), {
          silent: true
        });
        _this.CustomerRelationsData.trigger("sync");
        _this.alertDocs.reset(fullModel.get("AlertDocs"), {
          silent: true
        });
        _this.alertDocsView.create(id);
        _this.alertDocs.trigger("sync");
        _this.companyScoreModel.customerId = id;
        _this.companyScoreModel.set(fullModel.get("CompanyScore"), {
          silent: true
        });
        _this.companyScoreModel.trigger("sync");
        if (isHistory) {
          $('a[href=#marketplaces]').click();
        }
        return BlockUi("Off");
      });
      this.crossCheckView.render({
        customerId: id
      });
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

    ProfileView.prototype.clearDecisionNotes = function() {
      return this.$el.find('#DecisionNotes').empty();
    };

    ProfileView.prototype.appendDecisionNote = function(oNote) {
      return this.$el.find('#DecisionNotes').append(oNote);
    };

    return ProfileView;

  })(Backbone.View);

}).call(this);

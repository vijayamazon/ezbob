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
      var xhr;
      this.template = _.template($("#profile-template-main").html());
      if ((EzBob.CrmActions != null) || EzBob.CrmActions.length === 0) {
        xhr = $.get(window.gRootPath + "Underwriter/CustomerRelations/CrmStatic", function(data) {
          EzBob.CrmActions = data.CrmActions;
          EzBob.CrmStatuses = data.CrmStatuses;
          return EzBob.CrmRanks = data.CrmRanks;
        });
      }
      if ((EzBob.RejectReasons != null) || EzBob.RejectReasons.length === 0) {
        return xhr = $.get(window.gRootPath + "Underwriter/Customers/RejectReasons", function(data) {
          return EzBob.RejectReasons = data.reasons;
        });
      }
    };

    ProfileView.prototype.render = function() {
      var alertPassed, apiChecks, customerRelations, dashboardInfo, experianInfo, fraudDetection, loanhistorys, marketplaces, medalCalculations, messages, paymentAccounts, profileHead, profileInfo, summaryInfo, that,
        _this = this;
      this.$el.html(this.template());
      profileInfo = this.$el.find(".profile-person-info");
      summaryInfo = this.$el.find("#profile-summary");
      dashboardInfo = this.$el.find("#dashboard");
      marketplaces = this.$el.find("#marketplaces");
      experianInfo = this.$el.find("#credit-bureau");
      paymentAccounts = this.$el.find("#payment-accounts");
      loanhistorys = this.$el.find("#loanhistorys");
      medalCalculations = this.$el.find("#medal-calc");
      messages = this.$el.find("#messages");
      apiChecks = this.$el.find("#apiChecks");
      customerRelations = this.$el.find("#customerRelations");
      alertPassed = this.$el.find("#alerts-passed");
      profileHead = this.$el.find("#profileHead");
      fraudDetection = this.$el.find("#fraudDetection");
      this.personalInfoModel = new EzBob.Underwriter.PersonalInfoModel();
      this.profileInfoView = new EzBob.Underwriter.PersonInfoView({
        el: profileInfo,
        model: this.personalInfoModel
      });
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
      this.pricingModelScenarios = new EzBob.Underwriter.PricingModelScenarios();
      this.pricingModelCalculationsModel = new EzBob.Underwriter.PricingModelCalculationsModel();
      this.pricingModelScenarios.fetch();
      this.pricingModelCalculationsView = new EzBob.Underwriter.PricingModelCalculationsView({
        el: this.$el.find("#pricing-calc"),
        model: this.pricingModelCalculationsModel,
        scenarios: this.pricingModelScenarios
      });
      this.companyScoreModel = new EzBob.Underwriter.CompanyScoreModel();
      this.companyScoreView = new EzBob.Underwriter.CompanyScoreView({
        el: that.$el.find('#company-score-list'),
        model: this.companyScoreModel
      });
      that.$el.find('a.company-score-tab').on('shown.bs.tab', function(evt) {
        return that.companyScoreView.redisplayAccordion();
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
      this.signatureMonitorView = new EzBob.Underwriter.SignatureMonitorView({
        el: this.$el.find("#signature-monitor"),
        personalInfoModel: this.personalInfoModel
      });
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
      this.crmModel = new EzBob.Underwriter.CustomerRelationsModel();
      this.CustomerRelationsView = new EzBob.Underwriter.CustomerRelationsView({
        el: customerRelations,
        model: this.crmModel
      });
      this.FraudDetectionLogs = new EzBob.Underwriter.fraudDetectionLogModel();
      this.FraudDetectionLogView = new EzBob.Underwriter.FraudDetectionLogView({
        el: fraudDetection,
        model: this.FraudDetectionLogs
      });
      this.PropertiesModel = new EzBob.Underwriter.Properties();
      this.affordability = new EzBob.Underwriter.Affordability();
      this.fundingModel = new EzBob.Underwriter.FundingModel();
      this.dashboardInfoView = new EzBob.Underwriter.DashboardView({
        el: dashboardInfo,
        model: this.summaryInfoModel,
        crmModel: this.crmModel,
        personalModel: this.personalInfoModel,
        experianModel: this.experianInfoModel,
        companyModel: this.companyScoreModel,
        propertiesModel: this.PropertiesModel,
        mpsModel: this.marketPlaces,
        affordability: this.affordability,
        loanModel: this.loanInfoModel
      });
      this.profileHeadView = new EzBob.Underwriter.ProfileHeadView({
        el: profileHead,
        model: this.summaryInfoModel,
        personalModel: this.personalInfoModel,
        loanModel: this.loanInfoModel,
        medalModel: this.medalCalculationModel,
        parentView: this
      });
      this.showed = true;
      EzBob.handleUserLayoutSetting();
      this.$el.find('.nav-list a[data-toggle="tab"]').on('shown.bs.tab', (function(e) {
        _this.setLastShownProfileSection($(e.target).attr('href').substr(1));
        if ($(e.currentTarget).attr("href") === "#dashboard") {
          return _this.dashboardInfoView.render();
        }
      }));
      return this;
    };

    ProfileView.prototype.setState = function(nCustomerID, sSection) {
      this.lastShownCustomerID = nCustomerID;
      if (!sSection) {
        return this.getLastShownProfileSection(this.$el.find('a.customer-tab:first').attr('href').substr(1));
      }
    };

    ProfileView.prototype.restoreState = function() {
      return this.$el.find('a.customer-tab').filter('[href="#' + this.getLastShownProfileSection(this.$el.find('a.customer-tab:first').attr('href').substr(1)) + '"]').tab('show');
    };

    ProfileView.prototype.setLastShownProfileSection = function(sSection) {
      return localStorage['underwriter.profile.lastShownProfileSection'] = sSection;
    };

    ProfileView.prototype.getLastShownProfileSection = function(sDefault) {
      var sSection;
      sSection = localStorage['underwriter.profile.lastShownProfileSection'];
      if (!sSection) {
        sSection = sDefault;
        this.setLastShownProfileSection(sSection);
      }
      return sSection;
    };

    ProfileView.prototype.events = {
      "click #RejectBtn": "RejectBtnClick",
      "click #ApproveBtn": "ApproveBtnClick",
      "click #EscalateBtn": "EscalateBtnClick",
      "click #SuspendBtn": "SuspendBtnClick",
      "click #ReturnBtn": "ReturnBtnClick",
      'click .add-director': 'addDirectorClicked'
    };

    ProfileView.prototype.addDirectorClicked = function(event) {
      var addDirectorView, customerInfo, director, directorEl,
        _this = this;
      event.stopPropagation();
      event.preventDefault();
      this.crossCheckView.$el.find('.add-director').hide();
      director = new EzBob.DirectorModel();
      directorEl = this.crossCheckView.$el.find('.add-director-container');
      customerInfo = {
        FirstName: this.personalInfoModel.get('FirstName'),
        Surname: this.personalInfoModel.get('Surname'),
        DateOfBirth: this.personalInfoModel.get('DateOfBirth'),
        Gender: this.personalInfoModel.get('Gender'),
        PostCode: this.personalInfoModel.get('PostCode'),
        Directors: this.personalInfoModel.get('Directors')
      };
      addDirectorView = new EzBob.AddDirectorInfoView({
        model: director,
        el: directorEl,
        backButtonCaption: 'Cancel',
        failOnDuplicate: false,
        customerInfo: customerInfo
      });
      addDirectorView.setBackHandler((function() {
        return _this.onDirectorAddCanceled();
      }));
      addDirectorView.setSuccessHandler((function() {
        return _this.onDirectorAdded();
      }));
      addDirectorView.setDupCheckCompleteHandler((function(bDupFound) {
        return _this.onDuplicateCheckComplete(bDupFound);
      }));
      addDirectorView.render();
      addDirectorView.setCustomerID(this.customerId);
      directorEl.show();
      return false;
    };

    ProfileView.prototype.onDuplicateCheckComplete = function(bDupFound) {
      if (bDupFound) {
        return this.crossCheckView.$el.find('.duplicate-director-detected').show();
      } else {
        return this.crossCheckView.$el.find('.duplicate-director-detected').hide();
      }
    };

    ProfileView.prototype.onDirectorAddCanceled = function() {
      this.crossCheckView.$el.find('.add-director-container').hide().empty();
      return this.crossCheckView.$el.find('.add-director').show();
    };

    ProfileView.prototype.onDirectorAdded = function() {
      this.onDirectorAddCanceled();
      return this.show(this.customerId);
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
        return this.restoreState();
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
      $('.editOfferDiv').hide();
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
        EzBob.App.jqmodal.show(approveLoanWithoutAMLDialog);
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
        EzBob.App.jqmodal.show(approveLoanForWarningStatusCustomer);
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
      return this.loanHistory.fetch();
    };

    ProfileView.prototype.show = function(id, isHistory, history) {
      var fullModel, that, _ref,
        _this = this;
      this.hide();
      BlockUi("on");
      scrollTop();
      that = this;
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
        }
        _this.personalInfoModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.personalInfoModel.set(fullModel.get("PersonalInfoModel"), {
          silent: true
        });
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
        _this.marketPlaces.reset(fullModel.get("MarketPlaces"), {
          silent: true
        });
        _this.marketPlaces.trigger("sync");
        _this.affordability.customerId = id;
        _this.affordability.clear().set(fullModel.get("Affordability"), {
          silent: true
        });
        _this.affordability.trigger("sync");
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
        _this.pricingModelCalculationsModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.pricingModelCalculationsModel.set(fullModel.get("PricingModelCalculations"), {
          silent: true
        });
        _this.pricingModelCalculationsModel.trigger("sync");
        _this.PropertiesModel.set({
          Id: id
        }, {
          silent: true
        });
        _this.PropertiesModel.set(fullModel.get("Properties"), {
          silent: true
        });
        _this.PropertiesModel.trigger("sync");
        _this.FraudDetectionLogs.customerId = id;
        _this.FraudDetectionLogView.customerId = id;
        _this.FraudDetectionLogs.set(fullModel.get("FraudDetectionLog"), {
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
        _this.crmModel.customerId = id;
        _this.crmModel.set(fullModel.get("CustomerRelations"), {
          silent: true
        });
        _this.crmModel.trigger("sync");
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
        _this.crossCheckView.marketPlaces = _this.marketPlaces;
        _this.crossCheckView.companyScore = _this.companyScoreModel;
        _this.crossCheckView.experianDirectors = fullModel.get("ExperianDirectors");
        _this.crossCheckView.fullModel = fullModel;
        _this.crossCheckView.render({
          customerId: id
        });
        if (isHistory) {
          $('a[href=#marketplaces]').click();
        }
        $('a.common-bug').attr('data-bug-customer', id);
        _this.signatureMonitorView.reload(id);
        _this.fundingModel.fetch().done(function() {
          var availableFundsNum, availableFundsStr, fundingAlert, reqFunds;
          fundingAlert = _this.$el.find(".fundingAlert");
          availableFundsNum = _this.fundingModel.get('AvailableFunds');
          reqFunds = _this.fundingModel.get('RequiredFunds');
          availableFundsStr = 'Funding ' + EzBob.formatPoundsNoDecimals(availableFundsNum).replace(/\s+/g, '');
          fundingAlert.html(availableFundsStr);
          if (reqFunds > availableFundsNum) {
            return fundingAlert.addClass('red_cell');
          } else {
            return fundingAlert.removeClass('red_cell');
          }
        });
        return BlockUi("Off");
      });
      return EzBob.handleUserLayoutSetting();
    };

    ProfileView.prototype.hide = function() {
      return this.$el.hide();
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

  })(EzBob.View);

}).call(this);

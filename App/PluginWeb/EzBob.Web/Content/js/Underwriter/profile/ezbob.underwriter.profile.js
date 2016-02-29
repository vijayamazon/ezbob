var EzBob = EzBob || {};

EzBob.Underwriter.ProfileView = EzBob.View.extend({
	initialize: function(options) {
		this.template = _.template($('#profile-template-main').html());

		if (!EzBob.CrmActions || EzBob.CrmActions.length === 0) {
			$.get(window.gRootPath + 'Underwriter/CustomerRelations/CrmStatic', function(data) {
				EzBob.CrmActions = data.CrmActions;
				EzBob.CrmStatuses = data.CrmStatuses;
				EzBob.CrmRanks = data.CrmRanks;
			});
		} // if

		if (!EzBob.RejectReasons || EzBob.RejectReasons.length === 0) {
			$.get(window.gRootPath + 'Underwriter/Customers/RejectReasons', function(data) {
				EzBob.RejectReasons = data.reasons;
			});
		} // if

		this.fundingModel = options.fundingModel;
		this.fundingModel.on('change reset sync', this.fillFunds, this);

	}, // initialize

	render: function() {
		var self = this;

		this.$el.html(this.template());

		var profileInfo = this.$el.find('.profile-person-info');
		var summaryInfo = this.$el.find('#profile-summary');
		var dashboardInfo = this.$el.find('#dashboard');
		var marketplaces = this.$el.find('#marketplaces');
		var experianInfo = this.$el.find('#credit-bureau');
		var paymentAccounts = this.$el.find('#payment-accounts');
		var loanhistorys = this.$el.find('#loanhistorys');
		var medalCalculations = this.$el.find('#medal-calc');
		var automationCalculations = this.$el.find('#automation-calc');
		var messages = this.$el.find('#messages');
		var apiChecks = this.$el.find('#apiChecks');
		var customerRelations = this.$el.find('#customerRelations');
		var profileHead = this.$el.find('#profileHead');
		var fraudDetection = this.$el.find('#fraudDetection');
		var properties = this.$el.find('#properties');
		var affordability = this.$el.find('#affordability');
		var logicalGlue = this.$el.find('#logical-glue-history');

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
		this.marketPlaceView.on('rechecked', this.mpRechecked, this.marketPlaces);

		EzBob.App.vent.on('ct:marketplaces.history', function(history) {
			self.show(self.marketPlaces.customerId, true, history);
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

		EzBob.App.vent.on('newCreditLine:updated', function() {
			self.changedSystemDecision();
		});

		EzBob.App.vent.on('newCreditLine:done', function() {
			self.summaryInfoModel.fetch();
		});

		EzBob.App.vent.on('newCreditLine:pass', function() {
			self.summaryInfoModel.fetch();
		});

		this.paymentAccountsModel = new EzBob.Underwriter.PaymentAccountsModel();
		this.paymentAccountsView = new EzBob.Underwriter.PaymentAccountView({
			el: paymentAccounts,
			model: this.paymentAccountsModel
		});

		this.paymentAccountsView.on('rechecked', this.mpRechecked, this.paymentAccountsModel);
		this.paymentAccountsView.on('added', function() {
			self.personalInfoModel.fetch();
		}, this);

		this.medalCalculationModel = new EzBob.Underwriter.MedalCalculationModel();
		this.medalCalculationView = new EzBob.Underwriter.MedalCalculationView({
			el: medalCalculations,
			model: this.medalCalculationModel
		});

		this.logicalGlueView = new EzBob.Underwriter.LogicalGlueHistoryView({
			el: logicalGlue,
			model: this.medalCalculationModel
		});

		this.automationCalculationModel = new EzBob.Underwriter.AutomationCalculationModel();
		this.automationCalculationView = new EzBob.Underwriter.AutomationCalculationView({
		    el: automationCalculations,
		    model: this.automationCalculationModel
		});

		this.pricingModelCalculationsModel = new EzBob.Underwriter.PricingModelCalculationsModel();
		this.pricingModelCalculationsView = new EzBob.Underwriter.PricingModelCalculationsView({
			el: this.$el.find('#pricing-calc'),
			model: this.pricingModelCalculationsModel,
		});

		this.companyScoreModel = new EzBob.Underwriter.CompanyScoreModel();
		this.companyScoreView = new EzBob.Underwriter.CompanyScoreView({
			el: this.$el.find('#company-score-list'),
			model: this.companyScoreModel
		});
		
		this.crossCheckView = new EzBob.Underwriter.CrossCheckView({
			el: this.$el.find('#customer-info')
		});

		this.messagesModel = new EzBob.Underwriter.MessageModel();
		this.Message = new EzBob.Underwriter.Message({
			el: messages,
			model: this.messagesModel
		});

		this.Message.on('creditResultChanged', this.changedSystemDecision, this);

		this.signatureMonitorView = new EzBob.Underwriter.SignatureMonitorView({
			el: this.$el.find('#signature-monitor'),
			personalInfoModel: this.personalInfoModel,
			loanInfoModel: this.loanInfoModel,
		});

		this.alertDocs = new EzBob.Underwriter.Docs();
		this.alertDocsView = new EzBob.Underwriter.AlertDocsView({
			el: this.$el.find('#alert-docs'),
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
			model: this.crmModel,
			isBroker: false
		});

		this.FraudDetectionLogs = new EzBob.Underwriter.fraudDetectionLogModel();
		this.FraudDetectionLogView = new EzBob.Underwriter.FraudDetectionLogView({
			el: fraudDetection,
			model: this.FraudDetectionLogs
		});

		this.PropertiesModel = new EzBob.Underwriter.Properties();
		this.PropertiesView = new EzBob.Underwriter.PropertiesView({
			el: properties,
			model: this.PropertiesModel
		});

		this.affordability = new EzBob.Underwriter.Affordability();
		this.dashboardInfoView = new EzBob.Underwriter.DashboardView({
			el: dashboardInfo,
			model: this.summaryInfoModel,
			crmModel: this.crmModel,
			personalModel: this.personalInfoModel,
			experianModel: this.experianInfoModel,
			companyModel: this.companyScoreModel,
			propertiesModel: this.PropertiesModel,
			affordability: this.affordability,
			loanModel: this.loanInfoModel
		});

		this.profileHeadView = new EzBob.Underwriter.ProfileHeadView({
			el: profileHead,
			model: this.summaryInfoModel,
			loanModel: this.loanInfoModel,
			medalModel: this.medalCalculationModel,
			parentView: this
		});

		this.showed = true;
		this.$el.find('.nav-list a[data-toggle="tab"]').on('show.bs.tab', (function(e) {
			var $content = self.$el.find('.profile-content');
			BlockUi('on', $content);
			self.setLastShownProfileSection($(e.target).attr('href').substr(1));
			var currentTab = $(e.currentTarget).attr('href');
			switch (currentTab) {
				case '#customer-info':
					self.crossCheckView.render({ customerId: self.customerId });
					BlockUi('off', $content);
					break;
			    case '#dashboard':
			        if (self.affordability.customerId !== self.customerId) {
			            BlockUi('on', affordability);
			            self.affordability.customerId = self.customerId;
			            self.affordability.fetch().done(function() {
			                BlockUi('off', $content);
			                BlockUi('off', affordability);
			                self.dashboardInfoView.drawSparklineGraphs();
			            });
			        } else {
			        	BlockUi('off', $content);
			        	self.dashboardInfoView.drawSparklineGraphs();
			        }
					break;
				case '#company-score':
					self.companyScoreView.redisplayAccordion();
					BlockUi('off', $content);
					break;
				case '#fraudDetection':
					self.FraudDetectionLogs.customerId = self.customerId;
					self.FraudDetectionLogs.fetch().done(function() {
						BlockUi('off', $content);
					});
					break;
				case '#apiChecks':
					self.ApicCheckLogs.customerId = self.customerId;
					self.ApicCheckLogs.fetch().done(function() {
						BlockUi('off', $content);
					});
					break;
				case '#messages-tab':
					BlockUi('off', $content);
					BlockUi('on', self.alertDocsView.$el);
					BlockUi('on', self.Message.$el);
					self.alertDocs.customerId = self.customerId;
					self.alertDocs.fetch().done(function () {
						BlockUi('off', self.alertDocsView.$el);
					});

					self.messagesModel.clear({ silent: true });
					self.messagesModel.set({ Id: self.customerId }, { silent: true });
					self.messagesModel.fetch().done(function () {
						BlockUi('off', self.Message.$el);
					});

					self.signatureMonitorView.reload(self.customerId);
					break;
				case '#payment-accounts':
					self.paymentAccountsModel.customerId = self.customerId;
					self.paymentAccountsModel.fetch().done(function() {
						BlockUi('off', $content);
					});
					break;
				case '#loanhistorys':
					self.loanHistory.customerId = self.customerId;
					self.loanHistory.fetch().done(function() {
						BlockUi('off', $content);
					});
					break;
				case '#calculator':
					BlockUi('off', $content);
					BlockUi('on', self.automationCalculationView.$el);
					BlockUi('on', self.pricingModelCalculationsView.$el);

				    self.automationCalculationModel.set({ Id: self.customerId }, { silent: true });
					self.automationCalculationModel.fetch().done(function() {
						BlockUi('off', self.automationCalculationView.$el);
					});

				    self.pricingModelCalculationsModel.set({ Id: self.customerId }, { silent: true });
					self.pricingModelCalculationsModel.fetch().done(function () {
						self.pricingModelCalculationsModel.trigger('fetch');
						BlockUi('off', self.pricingModelCalculationsView.$el);
					});

					break;
				case '#marketplaces':
					self.marketPlaces.customerId = self.customerId;
					self.marketPlaces.fetch().done(function() {
						BlockUi('off', $content);
					});
					break;
				default:
					BlockUi('off', $content);
					break;
			}
		}));

		this.gotoCustomer();

		return this;
	}, // render

	show: function(id) {
		this.hide();

		BlockUi('on');

		scrollTop();

		this.customerId = id;

		var fullModel = new EzBob.Underwriter.CustomerFullModel({
			customerId: id
		});

		var self = this;

		fullModel.fetch().done(function(res) {
			if (fullModel.get('State') === 'NotFound') {
				EzBob.ShowMessage(res.error, 'Customer id. #' + id + ' was not found');
				self.router.navigate('', { trigger: true, replace: true });
				return;
			} // if

			self.$el.show();
			self.recordRecentCustomers(id);
			//console.log('Full customer model is', fullModel);

			self.personalInfoModel.set({ Id: id }, { silent: true });
			self.personalInfoModel.set(fullModel.get('PersonalInfoModel'), { silent: true });
			self.personalInfoModel.trigger('sync');

			self.loanInfoModel.set({ Id: id }, { silent: true });
			self.loanInfoModel.set(fullModel.get('ApplicationInfoModel'), { silent: true });
			self.loanInfoModel.trigger('sync');

			self.loanHistory.customerId = id;
			self.loanHistoryView.idCustomer = id;

			self.summaryInfoModel.set({ Id: id, success: true }, { silent: true });
			self.summaryInfoModel.set(fullModel.get('SummaryModel'), { silent: true });
			self.summaryInfoModel.trigger('sync');

			self.experianInfoModel.set({ Id: id }, { silent: true });
			self.experianInfoModel.set(fullModel.get('CreditBureauModel'), { silent: true });
			self.experianInfoModel.trigger('sync');

			self.medalCalculationModel.set({ Id: id }, { silent: true });
			self.medalCalculationModel.set(fullModel.get('MedalCalculations'), { silent: true });
			self.medalCalculationModel.trigger('sync');

			self.PropertiesModel.set({ Id: id }, { silent: true });
			self.PropertiesModel.set(fullModel.get('Properties'), { silent: true });
			self.PropertiesModel.trigger('sync');

			self.FraudDetectionLogView.customerId = id;
			
			self.ApiChecksLogView.idCustomer = id;
			
			self.crmModel.customerId = id;
			self.crmModel.set(fullModel.get('CustomerRelations'), { silent: true });
			self.crmModel.trigger('sync');

			self.alertDocsView.create(id);
			
			self.companyScoreModel.customerId = id;
			self.companyScoreModel.set(fullModel.get('CompanyScore'), { silent: true });
			self.companyScoreModel.trigger('sync');

			self.crossCheckView.marketPlaces = self.marketPlaces;
			self.crossCheckView.companyScore = self.companyScoreModel;
			self.crossCheckView.experianDirectors = fullModel.get('ExperianDirectors');
			self.crossCheckView.fullModel = fullModel;

			self.PropertiesView.customerId = id;

			$('a.common-bug').attr('data-bug-customer', id);

			self.fillFunds();
			self.fundingModel.fetch();

			EzBob.InitBugs();

			EzBob.UpdateBugsIcons(fullModel.get('Bugs'));

			UnBlockUi();
		}); // fullModel.fetch().done

		EzBob.handleUserLayoutSetting();
	}, // show

	gotoCustomer: function() {
		var goToCustomerId = new EzBob.Underwriter.goToCustomerId();
		goToCustomerId.on('ok', function(id) {
			Redirect(window.gRootPath + 'Underwriter/Customers#profile/' + id);
		});

		$('[id=liClient] > a').unbind('click').on('click', function() {
			goToCustomerId.render();
			return false;
		});
	}, // gotoCustomer

	setState: function(nCustomerID, sSection) {
		this.customerId = nCustomerID;
		if (!sSection)
			this.getLastShownProfileSection(this.$el.find('a.customer-tab:first').attr('href').substr(1));
	}, // setState

	restoreState: function() {
		this.$el.find('a.customer-tab').filter(
			'[href="#' + this.getLastShownProfileSection(this.$el.find('a.customer-tab:first'
			).attr('href').substr(1)) + '"]'
		).tab('show').trigger('show.bs.tab');
		EzBob.handleUserLayoutSetting();
	}, // restoreState

	setLastShownProfileSection: function(sSection) {
		localStorage['underwriter.profile.lastShownProfileSection'] = sSection;
	}, // setLastShownProfileSection

	getLastShownProfileSection: function(sDefault) {
		var sSection = localStorage['underwriter.profile.lastShownProfileSection'];

		if (!sSection) {
			sSection = sDefault;
			this.setLastShownProfileSection(sSection);
		} // if

		return sSection;
	}, // getLastShownProfileSection

	events: {
		'click #RejectBtn': 'RejectBtnClick',
		'click #ApproveBtn': 'ApproveBtnClick',
		'click #EscalateBtn': 'EscalateBtnClick',
		'click #SuspendBtn': 'SuspendBtnClick',
		'click #ReturnBtn': 'ReturnBtnClick',
		'click #SignatureBtn': 'SignatureBtnClick',
		'click .add-director': 'addDirectorClicked'
	}, // events

	addDirectorClicked: function(event) {
		event.stopPropagation();
		event.preventDefault();

		this.crossCheckView.$el.find('.add-director').hide();

		var director = new EzBob.DirectorModel();
		var directorEl = this.crossCheckView.$el.find('.add-director-container');

		var customerInfo = {
			FirstName: this.personalInfoModel.get('FirstName'),
			Surname: this.personalInfoModel.get('Surname'),
			DateOfBirth: this.personalInfoModel.get('DateOfBirth'),
			Gender: this.personalInfoModel.get('Gender'),
			PostCode: this.personalInfoModel.get('PostCode'),
			Directors: this.personalInfoModel.get('Directors')
		};

		var addDirectorView = new EzBob.AddDirectorInfoView({
			model: director,
			el: directorEl,
			backButtonCaption: 'Cancel',
			failOnDuplicate: false,
			customerInfo: customerInfo
		});

		var self = this;

		addDirectorView.setBackHandler(function() { self.onDirectorAddCanceled(); });
		addDirectorView.setSuccessHandler(function() { self.onDirectorAdded(); });
		addDirectorView.setDupCheckCompleteHandler(function(bDupFound) { self.onDuplicateCheckComplete(bDupFound); });

		addDirectorView.render();
		addDirectorView.setCustomerID(this.customerId);

		directorEl.show();

		return false;
	}, // addDirectorClicked

	onDuplicateCheckComplete: function(bDupFound) {
		var oDupDetected = this.crossCheckView.$el.find('.duplicate-director-detected');

		if (bDupFound)
			oDupDetected.show();
		else
			oDupDetected.hide();
	}, // onDuplicateCheckComplete

	onDirectorAddCanceled: function() {
		this.crossCheckView.$el.find('.add-director-container').hide().empty();
		this.crossCheckView.$el.find('.add-director').show();
	}, // onDirectorAddCanceled

	onDirectorAdded: function() {
		this.onDirectorAddCanceled();
		this.show(this.customerId);
	}, // onDirectorAdded

	recordRecentCustomers: function(id) {
		$.post(window.gRootPath + 'Underwriter/CustomerNavigator/SetRecentCustomer', { id: id }).done(function(recentCustomersModel) {
			localStorage.setItem('RecentCustomers', JSON.stringify(recentCustomersModel.RecentCustomers));
		});
	}, // recordRecentCustomers

	mpRechecked: function(parameter) {
		var model = this;
		var umi = parameter.umi;

		model.fetch().done(function() {
			var el = $('#' + parameter.el.attr('id'));
			el.addClass('disabled');

			var interval = setInterval(function() {
				$.get(window.gRootPath + 'Underwriter/MarketPlaces/CheckForUpdatedStatus', { mpId: umi }).done(function(response) {
					if (response.status !== 'In progress') {
						clearInterval(interval);

						model.fetch().done(function() {
							el.removeClass('disabled');
						});
					} // if
				});
			}, 1000);
		});
	}, // mpRechecked

	disableChange: function(id) {
		this.show(id, false);
	}, // disableChange

	RejectBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		var functionPopupView = new EzBob.Underwriter.RejectedDialog({ model: this.loanInfoModel });

		functionPopupView.render();

		functionPopupView.on('changedSystemDecision', this.changedSystemDecision, this);

		return false;
	}, // RejectBtnClick

	ApproveBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		if (this.loanInfoModel.get('InterestRate') <= 0) {
			EzBob.ShowMessage('Wrong Interest Rate value (' + this.loanInfoModel.get('InterestRate') + '), please enter the valid value (above zero)', 'Error');
			return false;
		} // if

		if (this.loanInfoModel.get('OfferedCreditLine') <= 0) {
			EzBob.ShowMessage('Wrong Offered credit line value (' + this.loanInfoModel.get('OfferedCreditLine') + '), please enter the valid value (above zero)', 'Error');
			return false;
		} // if

		if (this.loanInfoModel.get('OfferExpired')) {
			EzBob.ShowMessage('Loan offer has expired. Set new validity date.', 'Error');
			return false;
		} // if

		if (this.personalInfoModel.get('CompanyExperianRefNum') === 'NotFound' && _.contains([1, 3, 5],this.loanInfoModel.get('TypeOfBusiness'))) {
			EzBob.ShowMessage('Customer with limited/pship business type have selected company not found, this must be fixed in order to approve a loan', 'Error');
			return false;
		} // if

		$('.editOfferDiv').addClass('hide');

		$.cookie('editOfferVisible', false);
		$(".profile-content").css({ 'margin-top': ($('#profileHead').height() + 10) + 'px' });

		var skipPopupForApprovalWithoutAml = this.loanInfoModel.get('SkipPopupForApprovalWithoutAML');

		var showBecauseOfAml = (this.loanInfoModel.get('AMLResult') !== 'Passed') && !skipPopupForApprovalWithoutAml;
		var showBecauseOfMultiBrand = this.loanInfoModel.get('IsMultiBranded');
		var showBecauseOfFCAIncompliance = this.personalInfoModel.get('BrokerID') > 0 && !this.personalInfoModel.get('IsBrokerRegulated') && this.personalInfoModel.get('IsRegulated') && this.personalInfoModel.get('IsWizardComplete');

		if (showBecauseOfAml || showBecauseOfMultiBrand) {
			var approveLoanWithoutAMLDialog = new EzBob.Underwriter.ApproveLoanWithoutAML({
				model: this.loanInfoModel,
				parent: this,
				showBecauseOfAml: showBecauseOfAml,
				showBecauseOfMultiBrand: showBecauseOfMultiBrand,
				showBecauseOfFCAIncompliance: showBecauseOfFCAIncompliance
			});

			EzBob.App.jqmodal.show(approveLoanWithoutAMLDialog);
			return false;
		} // if

		this.CheckCustomerStatusAndCreateApproveDialog();
		return false;
	}, // ApproveBtnClick

	CheckCustomerStatusAndCreateApproveDialog: function() {
		if (this.personalInfoModel.get('IsWarning')) {
			var approveLoanForWarningStatusCustomer = new EzBob.Underwriter.ApproveLoanForWarningStatusCustomer({
				model: this.personalInfoModel,
				parent: this,
			});
			EzBob.App.jqmodal.show(approveLoanForWarningStatusCustomer);
			return false;
		} // if

		return this.CreateApproveDialog();
	}, // CheckCustomerStatusAndCreateApproveDialog

	CreateApproveDialog: function() {
		var dialog = new EzBob.Underwriter.ApproveDialog({ model: this.loanInfoModel });
		dialog.on('changedSystemDecision', this.changedSystemDecision, this);
		dialog.render();

		return false;
	}, // CreateApproveDialog

	EscalateBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		var functionPopupView = new EzBob.Underwriter.Escalated({ model: this.loanInfoModel });
		functionPopupView.render();
		functionPopupView.on('changedSystemDecision', this.changedSystemDecision, this);

		return false;
	}, // EscalateBtnClick

	SuspendBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		var functionPopupView = new EzBob.Underwriter.Suspended({ model: this.loanInfoModel });
		functionPopupView.render();
		functionPopupView.on('changedSystemDecision', this.changedSystemDecision, this);

		return false;
	}, // SuspendBtnClick

	ReturnBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		var functionPopupView = new EzBob.Underwriter.Returned({ model: this.loanInfoModel });
		functionPopupView.render();
		functionPopupView.on('changedSystemDecision', this.changedSystemDecision, this);

		return false;
	}, // ReturnBtnClick

	SignatureBtnClick: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return false;

		var functionPopupView = new EzBob.Underwriter.Signature({ model: this.loanInfoModel });
		functionPopupView.render();
		functionPopupView.on('changedSystemDecision', this.changedSystemDecision, this);

		return false;
	}, // SignatureBtnClick

	changedSystemDecision: function() {
		this.summaryInfoModel.fetch();
		this.personalInfoModel.fetch();
		this.loanInfoModel.fetch();
		this.loanHistory.fetch();
	}, // changedSystemDecision

	

	fillFunds: function() {
		var fundingAlert = this.$el.find('.fundingAlert');

		var availableFundsStr = 'Funding ' +
			EzBob.formatPoundsNoDecimals(
				this.fundingModel.get('AvailableFunds')
			).replace(/\s+/g, '');

		fundingAlert.html(availableFundsStr).toggleClass('red_cell', this.fundingModel.notEnoughFunds());
	}, // fillFunds

	hide: function() {
		this.$el.hide();
	}, // hide

	updateAlerts: function() {
		this.alertsModel.fetch();
	}, // updateAlerts

	clearDecisionNotes: function() {
		this.$el.find('#DecisionNotes').empty();
	}, //clearDecisionNotes 

	appendDecisionNote: function(oNote) {
		this.$el.find('#DecisionNotes').append(oNote);
	} // appendDecisionNote
}); // EzBob.Underwriter.ProfileView

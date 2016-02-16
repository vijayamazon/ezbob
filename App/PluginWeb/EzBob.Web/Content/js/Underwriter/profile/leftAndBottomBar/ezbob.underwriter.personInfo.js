var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PersonInfoView = Backbone.Marionette.ItemView.extend({
	template: '#profile-person-info-template',

	initialize: function() {
		this.bindTo(this.model, 'change sync', this.render, this);
	}, // initialize

	onRender: function() {
		
		this.$el.find(".tltp").tooltip({html : true, container: 'body'});
		this.$el.find(".tltp-left").tooltip({ placement: "left", container: 'body' });

		if (this.model.get('IsWizardComplete'))
			this.$el.find('#ForceFinishWizard').addClass('hide');

		this.$el.find('.left-pane-header').toggleClass('alert-external-collection-status', this.model.get('ExternalCollectionStatusName') !== '');

		this.initSwitch(".cciMarkSwitch", this.model.get('IsCciMarkInAlertMode'), this.toggleCciMark);

		this.initSwitch(".blockTakingLoanSwitch", this.model.get('BlockTakingLoan'), this.toggleBlockTakingLoan);

		this.initSwitch(".testUserSwitch", this.model.get('IsTestInAlertMode'), this.toggleIsTest);

		this.initSwitch(".manualDecisionSwitch", this.model.get('IsAvoid'), this.toggleManualDecision);

		this.initSwitch(".lightDarkThemeSwitch", $.cookie('sidebar-color') == 'light', this.toggleTheme);

		if (this.model.get('BrokerName') !== '')
			this.$el.find('#brokerDetailsBtn').removeClass('hide');
	}, // onRender

	initSwitch: function(elemClass, state, func) {
		this.$el.find(elemClass).bootstrapSwitch();

		this.$el.find(elemClass).bootstrapSwitch('setState', state);

		var self = this;

		this.$el.find(elemClass).on('switch-change', function(event, innerState) {
			func.call(self, event, innerState);
		});
	}, // initSwitch

	toggleCciMark: function(event, state) {
		var self = this;

		var id = this.model.get('Id');

		BlockUi();

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleCciMark', {
			id: id
		}).done(function(result) {
			if (result.error)
				EzBob.App.trigger('error', result.error);
			else {
				self.setAlertStatus(result.mark, '.cci-mark-td');
				self.model.set('IsCciMarkInAlertMode', result.mark);
			} // if
		}).always(function() {
			UnBlockUi();
		});
	}, // toggleCciMark

	toggleBlockTakingLoan: function(event, state){
		var self = this;

		var id = this.model.get('Id');

		BlockUi();

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleBlockTakingLoan', {
			id: id
		}).done(function (result) {
			if (result.error)
				EzBob.App.trigger('error', result.error);
			else {
				self.setAlertStatus(result.mark, '.block-taking-loan-td');
				self.model.set('BlockTakingLoan', result.mark);
			} // if
		}).always(function () {
			UnBlockUi();
		});
	},

	toggleIsTest: function(event, state) {
		var self = this;

		var id = this.model.get('Id');

		BlockUi();

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleIsTest', {
			id: id
		}).done(function(result) {
			if (result.error)
				EzBob.App.trigger('error', result.error);
			else {
				self.setAlertStatus(result.isTest, '.is-test-td');
				self.model.set('IsTestInAlertMode', result.isTest);
			}
		}).always(function() {
			UnBlockUi();
		});
	}, // toggleIsTest

	toggleManualDecision: function(event, state) {
		var self = this;
		var id = this.model.get('Id');

		BlockUi();

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/AvoidAutomaticDecision', {
			id: id,
			enabled: state.value
		}).done(function(result) {
			if (result.error)
				EzBob.App.trigger('error', result.error);
			else
				self.model.set('IsAvoid', result.status);
		}).always(function() {
			UnBlockUi();
		});
	}, // toggleManualDecision

	toggleTheme: function (event, state) {
	    $("#main-container").toggleClass("sidebar-light", state.value);
	    if (state.value) {
	    	$.cookie('sidebar-color', 'light', { expires: moment().add(3, 'y').toDate() });
	    } else {
	    	$.cookie('sidebar-color', 'dark', { expires: moment().add(3, 'y').toDate() });
	    }
	},
	setAlertStatus: function(isAlert, td) {
		this.$el.find(td).toggleClass('red_cell', isAlert);
	}, // setAlertStatus

	events: {
		"click button[name=\"changeCustomerStatus\"]": "changeCustomerStatus",
		"click button[name=\"changeExternalCollectionStatus\"]": "changeExternalCollectionStatus",
		"click button[name=\"editEmail\"]": "editEmail",
		"click button[name=\"changeMobile\"]": "editPhone",
		"click button[name=\"changeDaytime\"]": "editPhone",
		"click button[name=\"brokerDetails\"]": "brokerDetails",
		"click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked",
		'click [name="TrustPilotStatusUpdate"]': 'updateTrustPilotStatus',
		'click #MainStrategyHidden': 'activateMainStratgey',
		'click #ForceFinishWizard': 'activateFinishWizard',
		'click .reset-password-123456': 'resetPassword123456',
		'click .change-broker': 'startChangeBroker',
		'click .go-to-broker': 'goToBroker',
		"click button[name=\"verifyMobile\"]": "verifyPhone",
		"click button[name=\"verifyDaytime\"]": "verifyPhone",
	}, // events

	goToBroker: function() {
		event.preventDefault();
		event.stopPropagation();

		window.location = '#broker/' + $(event.target).data('broker-id');

		return false;
	}, // goToBroker

	resetPassword123456: function() {
		event.preventDefault();
		event.stopPropagation();

		var self = this;

		EzBob.ShowMessage(
			"Do you really want to reset this customer password?",
			"Please confirm",
			function() {
				$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ResetPassword123456", {
					nCustomerID: self.model.get('Id')
				});

				EzBob.ShowMessageTimeout('Customer password has been reset to 123456.', 'Success', 3);
			}, // on ok
			"Reset",
			null,
			"Keep"
		);

		return false;
	}, // resetPassword123456

	selectBrokerEntry: function(oDiv, nBrokerID) {
		var oLst = oDiv.find('.broker-entry');
		oLst.filter('.active').removeClass('active');
		oLst.filter('.brkr' + nBrokerID).addClass('active');
	}, // selectBrokerEntry

	changeBroker: function(oDlg, oDiv) {
		var oEntry = oDiv.find('.broker-entry').filter('.active');

		if (oEntry.length !== 1) {
			oDlg.dialog('close');
			return;
		} // if

		BlockUi();

		var self = this;

		var oRequest = $.post(window.gRootPath + 'Brokers/AttachCustomer', {
			nCustomerID: this.model.get('Id'),
			nBrokerID: oEntry.data('broker-id'),
		});

		oRequest.done(function(oResponse) {
			UnBlockUi();

			if (oResponse.success) {
				oDlg.dialog('close');
				self.model.fetch();
			} // if

			if (oResponse.error)
				EzBob.ShowMessge(oResponse.error, 'Error');
			else
				EzBob.ShowMessge("Could not change customer's broker", 'Error');
		});

		oRequest.fail(function() {
			EzBob.ShowMessge("Failed to change customer's broker", 'Error');
			UnBlockUi();
		});
	}, // changeBroker

	startChangeBroker: function() {
		BlockUi();

		var oSelectBrokerEntry = _.bind(this.selectBrokerEntry, this);
		var oChangeBroker = _.bind(this.changeBroker, this);

		var self = this;

		var oRequest = $.getJSON(window.gRootPath + 'Grids/Load', {
			grid: 'UwGridBrokers',
			includeTestCustomers: false,
			includeAllCustomers: false,
		});

		oRequest.done(function(oResponse) {
			var customerOrigin = self.model.get('Origin');

			oResponse.aaData = _.filter(oResponse.aaData, function(brk) { return brk.Origin === customerOrigin; });

			oResponse.aaData.sort(function(a, b) {
				var sA = a.ContactName + a.FirmaName;
				var sB = b.ContactName + b.FirmaName;
				return sA.localeCompare(sB);
			});

			var oDiv = $('<div title="Select broker" class=broker-select-dlg></div>');

			oDiv.append(
				$('<div>No broker</div>')
					.data('broker-id', 0)
					.addClass('broker-entry brkr0')
					.click(function() { oSelectBrokerEntry(oDiv, 0); })
			);

			for (var i = 0; i < oResponse.aaData.length; i++) {
				var oBroker = oResponse.aaData[i];

				oDiv.append(
					$('<div></div>')
						.text(oBroker.ContactName + ' of ' + oBroker.FirmName)
						.data('broker-id', oBroker.BrokerID)
						.addClass('broker-entry brkr' + oBroker.BrokerID)
						.click((function(nBrkrID) {
							return function() { oSelectBrokerEntry(oDiv, nBrkrID); };
						})(oBroker.BrokerID))
				);
			} // for i

			UnBlockUi();

			var oDlg = oDiv.dialog({
				autoOpen: true,
				modal: true,
				width: 400,
				height: 400,
				buttons: [
					{
						text: 'Cancel',
						click: function() { oDlg.dialog('close'); },
						'class': 'btn btn-primary',
					},
					{
						text: 'Attach to broker',
						click: function() {
							oChangeBroker(oDlg, oDiv);
						},
						'class': 'btn btn-primary',
					},
				], // buttons
			});

			oSelectBrokerEntry(oDiv, self.model.get('BrokerID'));
		});

		oRequest.fail(function() {
			UnBlockUi();
			EzBob.ShowMessage('Failed to load list of brokers.', 'Error');
		});
	}, // startChangeBroker

	activateMainStratgey: function () {
	    var customerID = this.model.get('Id');
		$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ActivateMainStrategy", {
		    customerId: customerID
		});

		EzBob.ShowMessageTimeout('Executed main strategy for customer ' + customerID, '', 3, function () { }, 'Cool');

	}, //activateMainStratgey

	activateFinishWizard: function() {
		this.$el.find('#ForceFinishWizard').addClass('hide');

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/ActivateFinishWizard', {
			customerId: this.model.get('Id')
		});

		EzBob.ShowMessage('Finish wizard is in progress, please wait', 'Ok');
	}, // activateFinishWizard

	updateTrustPilotStatus: function() {
		var self = this;
		
		var d = new EzBob.Dialogs.ComboEdit({
			model: this.model,
			propertyName: 'TrustPilotStatusName',
			title: 'Trust Pilot Status',
			width: 500,
			postValueName: 'status',
			comboValues: this.model.get('TrustPilotStatusList'),
			url: "Underwriter/ApplicationInfo/UpdateTrustPilotStatus",
			data: {
				id: this.model.get('Id')
			},
		});

		d.render();

		d.on('done', function() { self.model.fetch(); });
	}, // updateTrustPilotStatus

	changeFraudStatusManualyClicked: function() {
		var self = this;

		var fraudStatusModel = new EzBob.Underwriter.FraudStatusModel({
			customerId: this.model.get('Id'),
			currentStatus: this.model.get('FraudCheckStatusId')
		});

		BlockUi("on");

		fraudStatusModel.fetch().done(function() {
			var fraudStatusLayout = new EzBob.Underwriter.FraudStatusLayout({
				model: fraudStatusModel
			});

			fraudStatusLayout.render();

			EzBob.App.jqmodal.show(fraudStatusLayout);

			BlockUi("off");

			fraudStatusLayout.on('saved', function() {
				var currentStatus = fraudStatusModel.get('currentStatus');
				self.model.set('FraudCheckStatusId', currentStatus);
				self.model.set('FraudCheckStatus', fraudStatusModel.get('currentStatusText'));
				self.setAlertStatus(currentStatus !== 0, '.fraud-status', '.fraud-status-td');
			});
		});
	}, // changeFraudStatusManualyClicked

	serializeData: function() {
		var sEmailState = this.model.get('EmailState');

		var sIcon = (
			sEmailState === 'Confirmed' || sEmailState === 'ManuallyConfirmed' || sEmailState === 'ImplicitlyConfirmed'
		) ? 'fa fa-check-circle' : 'fa fa-question-circle';
		
		return {
			data: this.model.toJSON(),
			icon: sIcon,
			getIcon: function() { return this.icon; },
		};
	}, // serializeData

	changeCustomerStatus: function() {
		var self = this;
		
		var collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel({
			customerId: this.model.get('Id'),
			CurrentStatus: this.model.get('CustomerStatusId')
		});

		collectionStatusModel.fetch().done(function() {
			var collectionStatusLayout = new EzBob.Underwriter.CollectionStatusLayout({
				model: collectionStatusModel
			});

			collectionStatusLayout.render();
			
			EzBob.App.jqmodal.show(collectionStatusLayout);
			collectionStatusLayout.on('saved', function() {
				self.model.fetch();
			});
		});
	}, // changeCustomerStatus

	changeExternalCollectionStatus: function() {
		var self = this;
		var blank = Object({ text: '', value: '' });
		if (this.model.get('ExternalCollectionStatusesList')[0].value !== '') {
			this.model.get('ExternalCollectionStatusesList').unshift(blank);
		}
		
		var d = new EzBob.Dialogs.ComboEdit({
			model: this.model,
			propertyName: 'ExternalCollectionStatusID',
			title: 'External Collection Status',
			width: 500,
			postValueName: 'externalStatusID',
			comboValues: this.model.get('ExternalCollectionStatusesList'),
			url: "Underwriter/ApplicationInfo/ChangeExternalCollectionStatus",
			data: {
				id: this.model.get('Id')
			},
		});

		d.render();

		d.on('done', function() { self.model.fetch(); });
	}, // changeExternalCollectionStatus

	brokerDetails: function() {
		this.$el.find('.broker-details-rows').toggle();
		this.$el.find('#brokerDetailsBtn i').toggleClass("fa-plus fa-minus");
	}, // brokerDetails

	editEmail: function() {
		var view = new EzBob.EmailEditView({ model: this.model });

		EzBob.App.jqmodal.show(view);

		view.on("showed", function() { return view.$el.find("input").focus(); });

		return false;
	}, // editEmail

	editPhone: function (ev) {
	    var view = new EzBob.PhoneEditView({ model: this.model, PhoneType: $(ev.currentTarget).attr('data-phoneType') });
	    EzBob.App.jqmodal.show(view);
        return false;
	}, // editPhone

	verifyPhone: function(elem) {
	    var that = this;
	    BlockUi();
	    var phoneType;
		var previousStatus;
		if (elem.currentTarget.name === 'verifyDaytime') {
			phoneType = 'Daytime';
			previousStatus = this.model.get('DaytimePhoneVerified');
		}
	    else if (elem.currentTarget.name === 'verifyMobile') {
	    	phoneType = 'Mobile';
	    	previousStatus = this.model.get('MobilePhoneVerified');
	    } else {
	    	EzBob.ShowMessage("Unknown phone type", "Error", function() {
	    		UnBlockUi();
	    	});
		    return false;
	    }
		
		$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/VerifyPhone", { customerId: this.model.get('Id'), phoneType: phoneType, verifiedPreviousState: previousStatus })
		 .done(function() {
	        UnBlockUi();
	        that.model.fetch();
	    });
	    return false;
	},
}); // EzBob.Underwriter.PersonInfoView

EzBob.Underwriter.PersonalInfoModel = Backbone.Model.extend({
	idAttribute: "Id",

	urlRoot: window.gRootPath + 'Underwriter/CustomerInfo/Index',

	initialize: function() {
		this.on("change:FraudCheckStatusId", this.changeFraudCheckStatus, this);

		this.changeFraudCheckStatus();

		if (EzBob.Underwriter.StaticData) {
			this.initCollectionStatuses();
		}
	}, // initialize

	initCollectionStatuses: function(){
		if (this.StatusesArr === void 0) {
			EzBob.Underwriter.StaticData = EzBob.Underwriter.StaticData;
			this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
		}
		this.StatusesArr = {};
		var _ref = this.statuses.models;
		var _results = [];
		for (var _i = 0, _len = _ref.length; _i < _len; _i++) {
			var status = _ref[_i];
			_results.push(this.StatusesArr[status.get('Id')] = status.get('Name'));
		} // for
		return _results;
	},

	changeFraudCheckStatus: function() {
		this.set("FraudHighlightCss", this.get("FraudCheckStatusId") === 2 ? 'red_cell' : '');
	}, // changeFraudCheckStatus
}); // EzBob.Underwriter.PersonalInfoModel


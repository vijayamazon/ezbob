var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PersonInfoView = Backbone.Marionette.ItemView.extend({
	template: '#profile-person-info-template',

	initialize: function() {
		this.bindTo(this.model, 'change sync', this.render, this);
	}, // initialize

	onRender: function() {
		this.$el.find(".tltp").tooltip();
		this.$el.find(".tltp-left").tooltip({ placement: "left" });

		if (this.model.get('IsWizardComplete'))
			this.$el.find('#ForceFinishWizard').addClass('hide');

		this.initSwitch(".cciMarkSwitch", this.model.get('IsCciMarkInAlertMode'), this.toggleCciMark);

		this.initSwitch(".testUserSwitch", this.model.get('IsTestInAlertMode'), this.toggleIsTest);

		this.initSwitch(".manualDecisionSwitch", this.model.get('IsAvoid'), this.toggleManualDecision);

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

	setAlertStatus: function(isAlert, td) {
		this.$el.find(td).toggleClass('red_cell', isAlert);
	}, // setAlertStatus

	events: {
		"click button[name=\"changeDisabledState\"]": "changeDisabledState",
		"click button[name=\"editEmail\"]": "editEmail",
		"click button[name=\"brokerDetails\"]": "brokerDetails",
		"click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked",
		'click [name="TrustPilotStatusUpdate"]': 'updateTrustPilotStatus',
		'click #MainStrategyHidden': 'activateMainStratgey',
		'click #ForceFinishWizard': 'activateFinishWizard',
		'click .reset-password-123456': 'resetPassword123456',
	}, // events

	resetPassword123456: function() {
		event.preventDefault();
		event.stopPropagation();
		
		$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ResetPassword123456", {
			nCustomerID: this.model.get('Id')
		});

		EzBob.ShowMessageTimeout('Customer password has been reset to 123456.', 'Success', 3);

		return false;
	}, // resetPassword123456

	activateMainStratgey: function() {
		$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ActivateMainStrategy", {
			customerId: this.model.get('Id')
		});
	}, //activateMainStratgey

	activateFinishWizard: function() {
		var self = this;
		EzBob.ShowMessage("Finish wizard is in progress, refresh in a couple of seconds", "Ok", function() {
			$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ActivateFinishWizard", {
				customerId: self.model.get('Id')
			});
		});
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

	templateHelpers: {
		getIcon: function() {
			return (
				this.EmailState === "Confirmed" || this.EmailState === "ManuallyConfirmed"
			) ? "fa fa-check-circle" : "fa fa-question-circle";
		},
	}, // templateHelpers

	serializeData: function() {
		return {
			data: this.model.toJSON(),
			getIcon: this.templateHelpers.getIcon,
		};
	}, // serializeData

	changeDisabledState: function() {
		var self = this;

		var collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel({
			customerId: this.model.get('Id'),
			currentStatus: this.model.get('CustomerStatusId')
		});

		var prevStatus = this.model.get('CustomerStatusId');

		var customerId = this.model.get('Id');

		BlockUi("on");

		collectionStatusModel.fetch().done(function() {
			var collectionStatusLayout = new EzBob.Underwriter.CollectionStatusLayout({
				model: collectionStatusModel
			});

			collectionStatusLayout.render();

			EzBob.App.jqmodal.show(collectionStatusLayout);

			BlockUi("off");

			collectionStatusLayout.on('saved', function() {
				var newStatus = collectionStatusModel.get('currentStatus');

				$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/GetIsStatusWarning", {
					status: newStatus
				}).done(function() {
					BlockUi("on");

					self.model.fetch();

					$.post("" + window.gRootPath + "Underwriter/ApplicationInfo/LogStatusChange", {
						newStatus: newStatus,
						prevStatus: prevStatus,
						customerId: customerId,
					}).done(function() {
						return BlockUi("off");
					});
				});
			});
		});
	}, // changeDisabledState

	brokerDetails: function() {
		this.$el.find('.broker-details-rows').toggle("fast");
		return this.$el.find('#brokerDetailsBtn i').toggleClass("fa-plus fa-minus");
	}, // brokerDetails

	editEmail: function() {
		var view = new EzBob.EmailEditView({ model: this.model });

		EzBob.App.jqmodal.show(view);

		view.on("showed", function() { return view.$el.find("input").focus(); });

		return false;
	}, // editEmail
}); // EzBob.Underwriter.PersonInfoView

EzBob.Underwriter.PersonalInfoModel = Backbone.Model.extend({
	idAttribute: "Id",

	urlRoot: window.gRootPath + 'Underwriter/CustomerInfo/Index',

	initialize: function() {
		this.on("change:FraudCheckStatusId", this.changeFraudCheckStatus, this);

		this.changeFraudCheckStatus();

		if (this.StatusesArr === void 0)
			this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;

		this.StatusesArr = {};
		var _ref = this.statuses.models;
		var _results = [];
		for (var _i = 0, _len = _ref.length; _i < _len; _i++) {
			var status = _ref[_i];
			_results.push(this.StatusesArr[status.get('Id')] = status.get('Name'));
		} // for
		return _results;
	}, // initialize

	changeFraudCheckStatus: function() {
		this.set("FraudHighlightCss", this.get("FraudCheckStatusId") === 2 ? 'red_cell' : '');
	}, // changeFraudCheckStatus
}); // EzBob.Underwriter.PersonalInfoModel

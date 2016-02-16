var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.FundingView = Backbone.Marionette.ItemView.extend({
	template: "#funding-template",

	initialize: function() {
		this.model.on("change reset", this.render, this);
	},

	events: {
		"click #addFundsBtn": "addFunds",
		"click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds",
		"click #pacnetTopUpRequestBtn": "pacnetTopUpRequest",
		"click #pacnetConfMailBtn": "pacnetConfMail"
	},

	addFunds: function(e) {
		var that = this;

		var d = new EzBob.Dialogs.PacentManual({
			model: this.model,
			title: "Pacnet Balance - Add Manual Funds",
			width: 400,
			postValueName: "amount",
			url: "Underwriter/Funding/SavePacnetManual",
			data: {
				limit: EzBob.Config.PacnetBalanceMaxManualChange
			},
			min: EzBob.Config.PacnetBalanceMaxManualChange * -1,
			max: EzBob.Config.PacnetBalanceMaxManualChange,
			required: true
		});

		d.render();

		d.on("done", function() {
			that.model.fetch();
		});
	},

	cancelManuallyAddedFunds: function(e) {
		var that = this;

		var d = new EzBob.Dialogs.CheckBoxEdit({
			model: this.model,
			propertyName: "UseSetupFee",
			title: "Pacnet Balance - Clear Manual Funds",
			width: 400,
			checkboxName: "I am sure",
			postValueName: "isSure",
			url: "Underwriter/Funding/DisableCurrentManualPacnetDeposits",
			data: {
				isSure: this.model.get("IsSure")
			}
		});

		d.render();

		d.on("done", function() {
			that.model.fetch();
		});
	},

	pacnetTopUpRequest: function(e) {
		var that = this;

		var d = new EzBob.Dialogs.PacentManual({
			model: this.model,
			title: "Pacnet Balance - Funds",
			buttonName: 'Send SMS for Approval',
			width: 400,
			postValueName: "amount",
			url: "Underwriter/Funding/TopUpRequest",
			data: {
				limit: EzBob.Config.PacnetBalanceMaxManualChange
			},
			min: 0,
			required: true,
		});

		d.render();

		d.on("done", function() {
			that.model.fetch();
		});
	},

	pacnetConfMail: function(e) {
		var that = this;

		var d = new EzBob.Dialogs.PacentManual({
			model: this.model,
			title: "Pacnet Balance - Funds",
			buttonName: 'Send Mail to Pacnet',
			width: 400,
			postValueName: "amount",
			url: "Underwriter/Funding/SendForPacnetConfirm",
			data: {
				limit: EzBob.Config.PacnetBalanceMaxManualChange
			},
			min: 0,
			required: true,
		});

		d.render();

		d.on("done", function() {
			that.model.fetch();
		});
	},

	onRender: function() {

	},

	hide: function() {
		this.$el.hide();
	},

	show: function() {
		this.$el.show();
	},

	serializeData: function() {
		return {
			model: this.model
		};
	},
});

EzBob.Underwriter.FundingModel = Backbone.Model.extend({
	urlRoot: function() {
		return "" + window.gRootPath + "Underwriter/Funding/GetCurrentFundingStatus";
	},

	notEnoughFunds: function() {
		return this.get('RequiredFunds') > this.get('AvailableFunds');
	},
});

var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ConfigSchedulerModel = Backbone.Model.extend({
	idAttribute: 'InvestorID',
	urlRoot: '' + gRootPath + 'Underwriter/Investor/GetSchedulerData'
});

EzBob.Underwriter.ConfigSchedulerView = Backbone.Marionette.ItemView.extend({
	template: '#config-scheduler-template',
	initialize: function(options) {
		this.investorID = options.investorID;
		this.model.on("change", this.render, this);
	},

	ui: {
		'form': '#configSchedulerForm',
		'monthlyFundingCapital': '#monthlyFundingCapital',
		'fundsTransferSchedule': '#fundsTransferSchedule',
		'fundsTransferDate': '#fundsTransferDate',
		'repaymentsTransferSchedule': '#repaymentsTransferSchedule',
		'money': '.cashInput'
	},//ui   

	events: {
		"click #config-scheduler-cancel-btn": "cancelConfigScheduler",
		"click #config-scheduler-submit-btn": "submitConfigScheduler"
	},

	onRender: function() {
		
		this.populateFields();

		this.ui.form.validate({
			rules: {
				fundsTransferSchedule: { required: true },
				monthlyFundingCapital: { required: true, autonumericMin: 0, autonumericMax: 100000000 },
				fundsTransferDate: { required: true, digits: true, min: 1, max: 31 },
				repaymentsTransferSchedule: { required: true }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ':not(:visible)'
		});

		this.ui.fundsTransferDate.numericOnly(2);

		return this;
	},

	cancelConfigScheduler: function() {
		this.$el.remove();
		return false;
	},

	populateFields: function() {
		this.ui.money.moneyFormat();
		var data = this.model.get("SchedulerObject");

		this.ui.monthlyFundingCapital.val(EzBob.formatPounds(data.MonthlyFundingCapital));
		this.ui.fundsTransferSchedule.val(data.FundsTransferSchedule);
		this.ui.fundsTransferDate.val(data.FundsTransferDate);
		this.ui.repaymentsTransferSchedule.val(data.RepaymentsTransferSchedule);
	},

	submitConfigScheduler: function() {
		
		if (!this.ui.form.valid()) {
			return false;
		}

		var amount = this.ui.monthlyFundingCapital.autoNumeric('get');

		BlockUi();
		var submitParam = {
			investorID: this.investorID,monthlyFundingCapital: amount,
			fundsTransferSchedule: this.ui.fundsTransferSchedule.val(),
			fundsTransferDate: this.ui.fundsTransferDate.val(),
			repaymentsTransferSchedule: this.ui.repaymentsTransferSchedule.val()
		};

		var self = this;
		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/SubmitSchedulerData', submitParam);


		xhr.done(function(res) {
			UnBlockUi();
			if (res.success) {
				EzBob.ShowMessage('Scheduler updated successfully', 'Done', null, 'Ok');
			} else {
				EzBob.ShowMessage("Failed updating scheduler", 'Failed', null, 'Ok');
			}
		});

		xhr.always(function() {
			UnBlockUi();
		});

		return false;

	},

	show: function() {

		return this.$el.show();
	},
});
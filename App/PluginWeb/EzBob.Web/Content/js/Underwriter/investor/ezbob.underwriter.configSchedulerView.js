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
		'fundsTransferScheduleOption': '#fundsTransferSchedule option',
		'fundsTransferDate': '#fundsTransferDate',
		'fundsTransferDateOption': '#fundsTransferDate option',
		'repaymentsTransferSchedule': '#repaymentsTransferSchedule',
		'repaymentsTransferScheduleOption': '#repaymentsTransferSchedule option',
		'money': '.cashInput'
	},//ui   

	events: {
		"click #config-scheduler-cancel-btn": "cancelConfigScheduler",
		"click #config-scheduler-submit-btn": "submitConfigScheduler",
		"change #fundsTransferSchedule": "resetFundsTransferDateList"
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

		this.setFundsTransferDateList(data.FundsTransferSchedule);

		this.ui.monthlyFundingCapital.val(EzBob.formatPounds(data.MonthlyFundingCapital));
		this.ui.fundsTransferScheduleOption.each(function() {
			 this.selected = (this.text == data.FundsTransferSchedule);
		});
		this.$el.find('#fundsTransferDate option').each(function() {
			 this.selected = (this.value == data.FundsTransferDate);
		});
		this.ui.repaymentsTransferScheduleOption.each(function() {
			 this.selected = (this.text == data.RepaymentsTransferSchedule);
		});
	},

	submitConfigScheduler: function() {
		
		if (!this.ui.form.valid()) {
			return false;
		}

		var amount = this.ui.monthlyFundingCapital.autoNumeric('get');

		BlockUi();
		var submitParam = {
			investorID: this.investorID,
			monthlyFundingCapital: amount,
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

	resetFundsTransferDateList: function() {
		this.ui.fundsTransferDate.empty();
		this.setFundsTransferDateList(this.ui.fundsTransferSchedule.val());
	},

	setFundsTransferDateList: function(data) {
		
		var weekDateList = ['Mon', 'Tue', 'Wed', 'Thu'];
		if (data)
			var scheduleDateList = data === 'month' ? [1, 2, 5, 10, 15, 20, 30] : [1, 2, 3, 4];

		var self = this;
		_.each(scheduleDateList, function(i, k) {
			self.ui.fundsTransferDate.append(
				$('<option></option>').val(i).text(data === 'week' ? weekDateList[k] : i)
			);
		}); // for each
	},

	show: function() {

		return this.$el.show();
	},
});
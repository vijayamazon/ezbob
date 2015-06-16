var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.LoanOptionsModel = Backbone.Model.extend({
	urlRoot: function() {
		return "" + window.gRootPath + "Underwriter/LoanOptions/Index?loanId=" + this.loanId;
	}
});

EzBob.Underwriter.LoanOptionsView = Backbone.Marionette.ItemView.extend({
	template: '#loan-options-template',

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: "Loan Options",
			position: "center",
			draggable: false,
			dialogClass: "loan-options-popup",
			width: 950
		};
	},

	initialize: function() {
		this.loanOptions = new Backbone.Model(this.model.get('Options'));
		this.modelBinder = new Backbone.ModelBinder();
		return this;
	},

	ui: {
		dateInput: '.date-input'
	},

	bindings: {
		Id: {
			selector: "input[name='Id']"
		},
		LoanId: {
			selector: "input[name='LoanId']"
		},
		AutoPayment: {
			selector: "input[name='AutoPayment']"
		},
		StopAutoChargeDate: {
			selector: "input[name='StopAutoChargeDate']",
			converter: EzBob.BindingConverters.dateTime
		},
		ReductionFee: {
			selector: "input[name='ReductionFee']"
		},
		LatePaymentNotification: {
			selector: "input[name='LatePaymentNotification']"
		},
		EmailSendingAllowed: {
			selector: "#EmailSendingAllowed"
		},
		MailSendingAllowed: {
			selector: "#MailSendingAllowed"
		},
		SmsSendingAllowed: {
			selector: "#SmsSendingAllowed"
		},
		AutoLateFees: {
			selector: "input[name='AutoLateFees']"
		},
		StopLateFeeFromDate: {
			selector: "input[name='StopLateFeeFromDate']",
			converter: EzBob.BindingConverters.dateTime
		},
		StopLateFeeToDate: {
			selector: "input[name='StopLateFeeToDate']",
			converter: EzBob.BindingConverters.dateTime
		},
	},

	events: {
		'change #cais-flags': 'changeFlags',
		'change #CaisAccountStatus': 'changeAccountStatus',
		"click .btnOk": "onSave"
	},

	changeFlags: function() {
		this.loanOptions.set('ManualCaisFlag', this.$el.find("#cais-flags option:selected").val());
		var index = this.$el.find("#cais-flags option:selected").attr('data-id');
		var curentFlag = this.model.get('ManualCaisFlags')[index];
		this.$el.find('.cais-comment').html('<h5>' + curentFlag.ValidForRecordType + '</h5>' + curentFlag.Comment);
	},

	changeAccountStatus: function() {
		var tmp = $("#CaisAccountStatus option:selected").val();

		$("#defaultExplanation").toggle(tmp === '8');

		this.loanOptions.set('CaisAccountStatus', $("#CaisAccountStatus option:selected").val());
	},

	save: function() {
		var postData = this.loanOptions.toJSON();
		var action = "" + window.gRootPath + "Underwriter/LoanOptions/Save";

		$.post(action, postData);

		return false;
	},

	onCancel: function() {
		this.close();
	},

	onSave: function() {
		this.save();
		this.close();
	},

	onRender: function() {
		this.modalOptions = {
			show: true,
			keyboard: false,
			width: 700
		};
		this.loanOptions.set('StopLateFeeFromDate', this.getDate(this.loanOptions.get('StopLateFeeFromDate')));
		this.loanOptions.set('StopLateFeeToDate', this.getDate(this.loanOptions.get('StopLateFeeToDate')));
		this.loanOptions.set('StopAutoChargeDate', this.getDate(this.loanOptions.get('StopAutoChargeDate')));

		this.modelBinder.bind(this.loanOptions, this.el, this.bindings);
		this.$el.find("#CaisAccountStatus option[value='" + (this.loanOptions.get('CaisAccountStatus')) + "']").attr('selected', 'selected');
		this.$el.find("#cais-flags option[value='" + (this.loanOptions.get('ManualCaisFlag')) + "']").attr('selected', 'selected');
		this.ui.dateInput.datepicker({ format: 'dd/mm/yyyy' });
		this.changeFlags();
	},

	getDate: function(date) {
		if (date == null || date === '')
			return null;

		return moment(date).toJSON();
	},
	serializeData: function() {
		var data = {
			ManualCaisFlags: this.model.get('ManualCaisFlags'),
			CalculatedStatus: this.model.get('CalculatedStatus'),
			autoChageEnabled: this.loanOptions.get('AutoPayment'),
			stopAutoChargeDate: EzBob.formatDate3(this.loanOptions.get('StopAutoChargeDate')),
			autoLateFeeEnabled: this.loanOptions.get('AutoLateFees'),
			stopLateFeeFromDate: EzBob.formatDate3(this.loanOptions.get('StopLateFeeFromDate')),
			stopLateFeeToDate: EzBob.formatDate3(this.loanOptions.get('StopLateFeeToDate'))
		};

		return data;
	}

});

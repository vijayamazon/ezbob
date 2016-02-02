var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ApproveLoanWithoutAML = EzBob.BoundItemView.extend({
	template: '#approve-loan-without-aml-template',

	initialize: function (options) {
		this.model = options.model;
		this.parent = options.parent;
		this.showBecauseOfAml = options.showBecauseOfAml;
		this.showBecauseOfMultiBrand = options.showBecauseOfMultiBrand;
		this.showBecauseOfFCAIncompliance = options.showBecauseOfFCAIncompliance;

		EzBob.Underwriter.ApproveLoanWithoutAML.__super__.initialize.apply(this, arguments);
	}, // initialize

	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: 'Warning',
			position: 'center',
			draggable: false,
			dialogClass: 'warning-aml-status-popup',
			width: 600,
		};
	}, // jqoptions

	onRender: function() {
		this.$el.find('.aml-area').toggleClass('hide', !this.showBecauseOfAml);
		this.$el.find('.multi-brand-area').toggleClass('hide', !this.showBecauseOfMultiBrand);
		this.$el.find('.fca-incompliance-area').toggleClass('hide', !this.showBecauseOfFCAIncompliance);
	}, // onRender

	onSave: function () {
		var isChecked = $('#isDoNotShowAgain').is(':checked');

		this.model.set('SkipPopupForApprovalWithoutAML', isChecked);

		BlockUi('on');

		var xhr = $.post('' + window.gRootPath + 'Underwriter/ApplicationInfo/SaveApproveWithoutAML/', {
			customerId: this.model.get('CustomerId'),
			doNotShowAgain: isChecked
		});
		
		var that = this;

		xhr.complete(function () {
			UnBlockUi();
			that.close();
			that.parent.CheckCustomerStatusAndCreateApproveDialog();
		});
	}, // onSave
}); // EzBob.Underwriter.ApproveLoanWithoutAML

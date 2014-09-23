var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ApproveLoanWithoutAML = EzBob.BoundItemView.extend({
	template: '#approve-loan-without-aml-template',
	initialize: function (options) {
		this.model = options.model;
		this.parent = options.parent;
		this.skipPopupForApprovalWithoutAML = options.skipPopupForApprovalWithoutAML;
		return this;
	},
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: "Warning",
			position: "center",
			draggable: false,
			dialogClass: "warning-aml-status-popup",
			width: 600
		};
	},
	onSave: function () {
		var isChecked, that, xhr;
		isChecked = $('#isDoNotShowAgain').is(':checked');
		this.model.set('SkipPopupForApprovalWithoutAML', isChecked);
		BlockUi("on");
		that = this;
		xhr = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/SaveApproveWithoutAML/", {
			customerId: this.model.get('CustomerId'),
			doNotShowAgain: isChecked
		});
		
		return xhr.complete(function () {
			BlockUi("off");
			that.close();
			that.parent.CheckCustomerStatusAndCreateApproveDialog();
			return false;
		});
	}
});

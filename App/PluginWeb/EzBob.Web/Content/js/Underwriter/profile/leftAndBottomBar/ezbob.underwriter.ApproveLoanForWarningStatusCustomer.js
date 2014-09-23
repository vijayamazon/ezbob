var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ApproveLoanForWarningStatusCustomer = EzBob.BoundItemView.extend({
	template: '#approve-loan-for-warning-status-customer',
	
	initialize: function (options) {
		this.model = options.model;
		this.parent = options.parent;
		return this;
	},
	
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: "Warning",
			position: "center",
			draggable: false,
			dialogClass: "warning-customer-status-popup",
			width: 600
		};
	},
	
	serializeData: function () {
		return {
			m: this.model.toJSON()
		};
	},
	
	onSave: function () {
		this.close();
		this.parent.CreateApproveDialog();
		return false;
	}
});

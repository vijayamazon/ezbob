﻿var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ApproveLoanForWarningStatusCustomer = EzBob.BoundItemView.extend({
	template: '#approve-loan-for-warning-status-customer',
	
	initialize: function (options) {
		this.model = options.model;
		this.parent = options.parent;

		EzBob.Underwriter.ApproveLoanForWarningStatusCustomer.__super__.initialize(this, arguments);
	}, // initialize
	
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: 'Warning',
			position: 'center',
			draggable: false,
			dialogClass: 'warning-customer-status-popup',
			width: 600,
		};
	}, // jqoptions
	
	serializeData: function () {
		return { m: this.model.toJSON(), };
	}, // serializeData
	
	onSave: function () {
		this.close();
		this.parent.CreateApproveDialog();
		return false;
	}, // onSave
}); // EzBob.Underwriter.ApproveLoanForWarningStatusCustomer

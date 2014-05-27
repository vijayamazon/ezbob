var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddCustomerRelationsEntry = EzBob.BoundItemView.extend({
	template: '#add-customer-relations-entry-template',

	events: {
		'keyup #Comment': 'commentKeyup'
	}, // events

	jqoptions: function() {
		return {
			modal: true,
			resizable: true,
			title: 'CRM - add entry',
			position: 'center',
			draggable: true,
			dialogClass: 'customer-relations-popup',
			width: 600
		};
	}, // jqoptions

	initialize: function(options) {
		this.onsave = options.onsave;
		this.onbeforesave = options.onbeforesave;
		this.customerId = this.model.customerId;
		this.url = window.gRootPath + 'Underwriter/CustomerRelations/SaveEntry/';

		EzBob.Underwriter.AddCustomerRelationsEntry.__super__.initialize.call(this);
	}, // initialize

	onRender: function() {
		this.ui.Action.prop('selectedIndex', 1);
	}, // onRender

	serializeData: function() {
		return {
			actions: EzBob.CrmActions,
			statuses: EzBob.CrmStatuses,
			ranks: EzBob.CrmRanks,
		};
	}, // serializeData

	commentKeyup: function(el) {
		return this.ui.Comment.val(this.ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
	}, // commentKeyup

	ui: {
		Incoming: '#Incoming_I',
		Status: '#Status',
		Action: '#Action',
		Rank: '#Rank',
		Comment: '#Comment',
	}, // ui

	onSave: function() {
		if (this.ui.Status[0].selectedIndex === 0)
			return false;

		if (this.ui.Action[0].selectedIndex === 0)
			return false;

		if (this.ui.Rank[0].selectedIndex === 0)
			return false;

		BlockUi();

		var opts = {
			isIncoming: this.ui.Incoming[0].checked,
			action: this.ui.Action[0].value,
			status: this.ui.Status[0].value,
			rank: this.ui.Rank[0].value,
			comment: this.ui.Comment.val(),
			customerId: this.customerId
		};

		if (this.onbeforesave)
			this.onbeforesave(opts);

		var self = this;

		var xhr = $.post(this.url, opts);

		xhr.done(function(r) {
			if (r.success)
				self.model.fetch();
			else {
				if (r.error)
					EzBob.ShowMessage(r.error, 'Error');
			} // if

			self.close();
		});

		xhr.always(function() {
			return UnBlockUi();
		});

		return false;
	}, // onSave
}); // EzBob.Underwriter.AddCustomerRelationsEntry

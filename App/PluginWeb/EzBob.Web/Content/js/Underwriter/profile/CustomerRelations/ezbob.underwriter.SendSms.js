var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SendSms = EzBob.BoundItemView.extend({
	template: '#send-sms-template',

	events: {
		'keyup #Content': 'contentKeyup',
		'click .default-sms': 'defaultSms'
	}, // events

	jqoptions: function() {
		return {
			modal: true,
			resizable: true,
			title: 'CRM - send SMS',
			position: 'center',
			draggable: true,
			dialogClass: 'send-sms-popup',
			width: 600
		};
	}, // jqoptions

	initialize: function(options) {
		this.onsave = options.onsave;
		this.onbeforesave = options.onbeforesave;
		this.customerId = this.model.customerId;
		this.customerName = this.model.get('CustomerName');
		this.phone = this.model.get('Phone');
		this.isPhoneVerified = this.model.get('IsPhoneVerified');
		this.url = window.gRootPath + 'Underwriter/CustomerRelations/SendSms/';
		this.isBroker = options.isBroker;

		EzBob.Underwriter.AddCustomerRelationsFollowUp.__super__.initialize.apply(this, arguments);
	}, // initialize

	serializeData: function() {
		return {
			customerName: this.customerName,
			phone: this.phone
		};
	},

	onRender: function() {
		EzBob.Underwriter.AddCustomerRelationsFollowUp.__super__.onRender.apply(this, arguments);

		this.ui.Content.focus();
		this.ui.Phone.numericOnly(11);
		if (this.isPhoneVerified && this.phone) {
			this.ui.Phone.attr('readonly', 'readonly');
		}
	}, // onRender

	contentKeyup: function(el) {
	    return this.ui.Content.val(this.ui.Content.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
	}, // commentKeyup

	defaultSms: function (el) {
	    this.ui.Content.val($(el.currentTarget).data('default'));
	},

	ui: {
		Phone: '#Phone',
		Content: '#Content',
		Default: '.default-sms'
	}, // ui

	onSave: function() {
		if (!this.ui.Phone.val())
			return false;

		if (!this.ui.Content.val())
			return false;

		BlockUi();

		var opts = {
			phone: this.ui.Phone.val(),
			content: this.ui.Content.val(),
			customerId: this.customerId,
			isBroker: this.isBroker,
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
}); // EzBob.Underwriter.AddCustomerRelationsFollowUp

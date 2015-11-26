var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorContactView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-contact-template',
	initialize: function (options) {
		this.model.on('change reset', this.render, this);
		this.stateModel = options.stateModel;
	},//initialize

	events: {
		'click #investorContactBack': 'back',
		'click #investorContactSubmit': 'submit',
	},

	ui: {
		'form': 'form#addEditInvestorContactForm',
		'phone': '.phone',
		'numeric': '.numeric',
		'ContactPersonalName': '#ContactPersonalName',
		'ContactLastName': '#ContactLastName',
		'ContactEmail': '#ContactEmail',
		'Role': '#Role',
		'ContactMobile': '#ContactMobile',
		'ContactOfficeNumber': '#ContactOfficeNumber',
		'Comment': '#Comment',
		'IsPrimary': '#IsPrimary',
		'IsActive': '#IsActive',
		'InvestorContactID': '#InvestorContactID',
	},//ui

	onRender: function () {
		this.ui.phone.mask('0?9999999999', { placeholder: ' ' });
		this.ui.phone.numericOnly(11);
		this.ui.numeric.numericOnly(20);

		this.editID = this.stateModel.get('editID');
		var self = this;
		if (this.editID) {
			var contact = _.find(this.model.get('Contacts'), function (contact) { return contact.InvestorContactID == self.editID; });
			if (contact) {
				this.ui.ContactPersonalName.val(contact.ContactPersonalName).change();
				this.ui.ContactLastName.val(contact.ContactLastName).change();
				this.ui.ContactEmail.val(contact.ContactEmail).change();
				this.ui.Role.val(contact.Role).change();
				this.ui.ContactMobile.val(contact.ContactMobile).change();
				this.ui.ContactOfficeNumber.val(contact.ContactOfficeNumber).change();
				this.ui.Comment.val(contact.Comment).change();
				this.ui.IsPrimary.prop('checked', contact.IsPrimary).change();
				this.ui.IsActive.prop('checked', contact.IsActive).change();
				this.ui.InvestorContactID.val(contact.InvestorContactID).change();
			}
		}

		this.ui.form.validate({
			rules: {
				ContactPersonalName: { required: true },
				ContactLastName: { required: true },
				ContactEmail: { required: true, email: true, },
				Role: { required: false, },
				ContactMobile: { required: true, regex: '^0[0-9]{10}$' },
				ContactOfficeNumber: { required: true, regex: '^0[0-9]{10}$' },
				Comment: { required: false, },
			},
			messages: {
				"ContactMobile": { regex: 'Please enter a valid UK number' },
				"ContactOfficeNumber": { regex: 'Please enter a valid UK number' }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
			ignore: ':not(:visible)'
		});
	},//onRender

	submit: function () {
		if (!this.ui.form.valid()) {
			return false;
		}
		BlockUi();
		var data = this.ui.form.serializeArray();
		data.push({ name: 'InvestorID', value: this.model.get('InvestorID') });

		var self = this;

		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/AddEditInvestorContact', data);
		xhr.done(function (res) {
			if (res.success) {
				EzBob.ShowMessage('Contact added/updated successfully', 'Done', null, 'Ok');
				self.model.fetch();
				self.trigger('back');
			} else {
				EzBob.ShowMessage(res, 'Failed saving investor contact', null, 'Ok');
			}
		});

		xhr.fail(function (res) {
			EzBob.ShowMessage(res, 'Failed saving investor contact', null, 'Ok');
		});
		xhr.always(function () {
			UnBlockUi();
		});
		return false;
	},

	back: function () {
		this.trigger('back');
		return false;
	}
});//EzBob.Underwriter.ManageInvestorContactView

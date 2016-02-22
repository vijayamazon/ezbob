var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorContactView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-contact-template',
	initialize: function (options) {
	    this.model.on('change reset', this.render, this);
	    this.model.set('InvestorID', options.InvestorID, { silent: true });
	   
	    this.model.set('Contact', options.Contact, { silent: true });
	    this.model.set('EditID', options.EditID, { silent: true });
	},//initialize

	events: {
	    'click #investorContactCancel': 'cancel',
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
	    this.contact = this.model.get('Contact');
		this.editID = this.model.get('EditID');
		var self = this;
		if (this.editID) {
		
		    if (this.contact) {
		        this.ui.ContactPersonalName.val(this.contact.get('ContactPersonalName')).change();
			    this.ui.ContactLastName.val(this.contact.get('ContactLastName')).change();
			    this.ui.ContactEmail.val(this.contact.get('ContactEmail')).prop('readonly', 'readonly').change();
		        this.ui.Role.val(this.contact.get('Role')).change();
		        this.ui.ContactMobile.val(this.contact.get('ContactMobile')).change();
		        this.ui.ContactOfficeNumber.val(this.contact.get('ContactOfficeNumber')).change();
		        this.ui.Comment.val(this.contact.get('Comment')).change();
		        this.ui.IsPrimary.val(this.contact.get('IsPrimary')).change();
		        this.ui.IsActive.val(this.contact.get('IsActive')).change();
		        this.ui.InvestorContactID.val(this.contact.get('InvestorContactID')).change();
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
		var investorID = this.model.get('InvestorID');
		var data = this.ui.form.serializeArray();
		data.push({ name: 'InvestorID', value: investorID });
	    
		var self = this;

		var xhr = $.post('' + window.gRootPath + 'Underwriter/Investor/ManageInvestorContact', data);
		xhr.done(function (res) {
			if (res.success) {
				EzBob.ShowMessage('Contact added/updated successfully', 'Done', null, 'Ok');
				self.trigger('cancel');
				$(".manage-investor-row[data-id='" + investorID + "']").trigger('click', true);
				
			} else {
			    EzBob.ShowMessage(res.error, 'Failed saving investor contact', null, 'Ok');
               
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

	cancel: function () {
	    this.remove();
	    this.unbind();
	    this.model.unbind("change reset", this.modelChanged);
	  
	    $('.add-contact-row').show();
		return false;
	}
});//EzBob.Underwriter.ManageInvestorContactView

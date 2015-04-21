var EzBob = EzBob || {};

EzBob.PhoneEditView = Backbone.Marionette.ItemView.extend({
	template: "#phone-edit-template",
	initialize: function(options) {
	    this.phoneType = options.PhoneType;
	},

	events: {
	    'click .change-phone': 'changePhone',
	    'keyup  input': 'inputChanged',
	    'change input': 'inputChanged',
	}, // events

	ui: {
	    'phone': 'input[name="edit-phone"]',
	    'form': '#phone-edit-form',
	    'changePhoneButton': '.change-phone'
	}, // ui

	serializeData: function () {
	    return {
	        Phone: '0',
	        PhoneType: this.phoneType
	    };
	},

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: "Change Phone",
			position: "center",
			draggable: false,
			width: 530,
			dialogClass: "phone-email-popup"
		};
	}, // jqoptions
    
	inputChanged: function () {
	    var enabled = this.validator.checkForm();
	    this.ui.changePhoneButton.toggleClass('disabled', !enabled);
	},

	changePhone: function () {
	    if (!this.validator.checkForm()) {
	        return false;
	    }

	    BlockUi();
		var xhr = $.post(window.gRootPath + "Underwriter/CustomerInfo/ChangePhone", {
			customerID: this.model.id,
			phoneType: this.phoneType,
			newPhone: this.ui.phone.val()
		});

		var self = this;

		xhr.success(function(response) {
			if (!response.success) {
				if (response.error)
					EzBob.ShowMessage(response.error);
			} // if

			self.model.fetch();
			self.close();
		});
	    xhr.complete(function() {
	        UnBlockUi();
	    });

		return false;
	}, // changePhone

	onRender: function() {
	    this.validator = this.validateChangePhoneForm(this.ui.form);
	    this.ui.phone.mask('0?9999999999', { placeholder: ' ' });
	    this.ui.phone.numericOnly(11);
	    this.ui.phone.focus();
	    this.inputChanged();
		return this;
	}, // onRender

	validateChangePhoneForm:  function(el) {
	    var e = el || $('form');

	    return e.validate({
	        rules: {
	            'edit-phone': { required: true, regex: "^0[0-9]{10}$" }
	        },
	        errorPlacement: EzBob.Validation.errorPlacement,
	        unhighlight: EzBob.Validation.unhighlight
	    });
	}//validateChangePhoneForm

}); // EzBob.PhoneEditView

var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AddBankAccount = EzBob.BoundItemView.extend({
	template: '#add-bank-account-template',
	
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: "Add Bank Account",
			position: "center",
			draggable: false,
			dialogClass: "add-bank-account-popup",
			width: 600
		};
	},
	
	events: {
		"click .check-account": "check"
	},
	
	bindings: {
		BankAccount: {
			selector: "input[name='number']"
		},
		SortCode: {
			selector: "input[name='sortcode']"
		},
		AccountType: {
			selector: "select[name='accountType']"
		}
	},
	
	initialize: function (options) {
		this.model = new Backbone.Model({
			customerId: options.customerId,
			SortCode: "",
			BankAccount: "",
			AccountType: ""
		});
		EzBob.Underwriter.AddBankAccount.__super__.initialize.apply(this, arguments);
		return this;
	},
	
	onRender: function () {
		this.validator = this.$el.find('form').validate({
			rules: {
				number: {
					required: true,
					number: true,
					minlength: 8,
					maxlength: 8
				},
				sortcode: {
					required: true,
					number: true,
					minlength: 6,
					maxlength: 6
				},
				accountType: {
					required: true
				}
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight
		});
		
		EzBob.Underwriter.AddBankAccount.__super__.onRender.apply(this, arguments);
		return this;
	},
	
	onSave: function () {
		var xhr;
		if (!this.validator.form()) {
			alertify.error("invalid input");
			return false;
		}
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/TryAddBankAccount", this.model.toJSON());
		var self = this;
		xhr.done(function (r) {
			if (r.error != null) {
				alertify.error(r.error);
				return false;
			}
			self.trigger('saved');
			self.close();
			return false;
		});
		
		xhr.complete(function () {
			BlockUi("off");
		});
		return false;
	},
	
	check: function () {
		var xhr;

		if (!this.validator.form()) {
			alertify.error("invalid input");
			return false;
		}
		
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/CheckBankAccount", this.model.toJSON());
		xhr.done(function (r) {
			var view;
			if (r.error != null) {
				alertify.error(r.error);
				return false;
			}
			view = new EzBob.Underwriter.BankAccountDetailsView({
				model: new Backbone.Model(r)
			});
			EzBob.App.jqmodal.show(view);
			return false;
		});

		xhr.complete(function () {
			BlockUi("off");
		});

		return false;
	}
});

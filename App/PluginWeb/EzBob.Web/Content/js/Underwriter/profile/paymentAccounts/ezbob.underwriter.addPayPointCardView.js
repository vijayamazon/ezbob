var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PayPointCardModel = Backbone.Model.extend();

EzBob.Underwriter.AddPayPointCardView = Backbone.Marionette.ItemView.extend({
	template: '#add-paypoint-card-template',
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: "Add paypoint card",
			position: "center",
			draggable: false,
			dialogClass: "add-paypoint-popup",
			width: 620
		};
	},
	
	events: {
		'click .btn-primary': 'save'
	},
	
	ui: {
		'transactionid': 'input[name="transactionid"]',
		'cardno': 'input[name="cardno"]',
		'expiredate': 'input[name="expiredate"]'
	},
	
	onRender: function () {
		this.setValidator();
		this.ui.expiredate.mask('99/99');
		return this;
	},
	
	save: function () {
		if (!this.validator.form()) {
			return false;
		}
		this.model.set({
			'transactionid': this.ui.transactionid.val(),
			'cardno': this.ui.cardno.val(),
			'expiredate': moment(this.ui.expiredate.val(), 'MM/YY').toDate().toJSON()
		});
		this.trigger('save');
		this.close();
		return false;
	},
	
	setValidator: function () {
		this.validator = this.$el.find('form').validate({
			rules: {
				transactionid: {
					required: true
				},
				cardno: {
					required: true,
					number: true,
					minlength: 4
				},
				expiredate: {
					required: true,
					regex: "^(0[1-9]|1[012])/\([0-9]{2})"
				}
			}
		});
	}
});

var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CreditLineEditDialog = Backbone.Marionette.ItemView.extend({
	template: '#credit-line-edit-dialog-template',

	initialize: function(options) {
		this.model = options.model;
		this.modelBinder = new Backbone.ModelBinder();
		this.method = null;
		this.medal = null;
		this.value = null;
	}, // initialize

	events: {
		'click .btnOk': 'save',
		'click .suggested-amount-link': 'suggestedAmountClicked',
		"keydown": "onEnterKeydown",
		'change .percent': 'percentChanged',
	}, // events

	ui: {
		form: "form",
		amount: "#edit-offer-amount",
	}, // ui

	jqoptions: function() {
		return {
			modal: true,
			resizable: true,
			title: "Offer credit line edit",
			position: "center",
			draggable: true,
			dialogClass: "credit-line-edit-popup",
			width: 800,
		};
	}, // jqoptions

	percentChanged: function(event) {
		var $elem = $(event.target);

		var percent = $elem.autoNumericGet() / 100;

		var method = $elem.data('method');

		var value = $elem.data('value');

		var link = this.$el.find('a.Manual' + method);

		link.text("Manual offer " + EzBob.formatPoundsNoDecimals(value * percent) + " (" + EzBob.formatPercents(percent) + ")")
			.data('value', value * percent).data('percent', percent);
	}, // percentChanged

	onEnterKeydown: function(event) {
		if (event.keyCode === 13) {
			this.ui.amount.change().blur();
			this.save();
			return false;
		} // if

		return true;
	}, // onEnterKeydown

	save: function() {
		if (!this.validator.checkForm())
			return;

		var _this = this;

		var post = $.post("" + window.gRootPath + "ApplicationInfo/ChangeCashRequestOpenCreditLine", this.getPostData());

		post.done(function() {
			_this.model.fetch();
			_this.trigger('done');
		});

		this.close();
	}, // save

	suggestedAmountClicked: function(el) {
		var $elem = $(el.currentTarget);

		this.method = $elem.data('method');
		this.medal = $elem.data('medal');
		this.value = $elem.data('value');
		this.percent = $elem.data('percent');
		this.ui.amount.val(this.value).change().blur();

		this.save();

		return false;
	}, // suggestedAmountClicked

	getPostData: function() {
		var m = this.model.toJSON();

		return {
			id: m.CashRequestId,
			amount: m.amount,
			method: this.method,
			medal: this.medal,
			value: this.value,
			percent: this.percent,
		};
	}, // getPostData

	bindings: {
		amount: {
			selector: "#edit-offer-amount",
			converter: EzBob.BindingConverters.moneyFormat,
		},
	}, // bindings

	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		this.$el.find("#edit-offer-amount").autoNumeric(EzBob.moneyFormat);
		this.$el.find(".percent").autoNumeric(EzBob.percentFormat).blur();

		if (this.$el.find("#edit-offer-amount").val() === "-")
			this.$el.find("#edit-offer-amount").val("");

		this.validator = this.ui.form.validate({
			rules: {
				editOfferAmount: {
					required: true,
					autonumericMin: EzBob.Config.XMinLoan,
					autonumericMax: EzBob.Config.MaxLoan,
				},
			},
			messages: {
				editOfferAmount: {
					autonumericMin: "Offer is below limit.",
					autonumericMax: "Offer is above limit.",
				},
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight
		});
	}, // onRender

	serializeData: function() {
		var res = {
			m: this.model.toJSON()
		};

		for (var i = 0; i < res.m.SuggestedAmounts.length; i++) {
			var fa = res.m.SuggestedAmounts[i];

			if (fa.Value <= 0)
				fa.Value = 0;
		} // for

		return res;
	}, // serializeData
}); // EzBob.Underwriter.CreditLineEditDialog

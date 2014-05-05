var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.PayEarlyView = Backbone.View.extend({
	className: 'd-widget',

	initialize: function() {
		this.template = _.template($('#d-payEarly-template').html());

		this.lateTemplate = _.template($('#d-lateLoan-template').html());

		this.model.on(
			'change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans change:HasRollovers',
			this.render,
			this
		);
	}, // initialize

	render: function() {
		if (this.model.get('LateLoans') > 0)
			this.$el.html(this.lateTemplate(this.model.toJSON()));
		else {
			this.$el.html(this.template(this.model.toJSON()));
			this.$el.find('input .money').moneyFormat();
			this.$el.toggle(this.model.get('ActiveLoans').length > 0);
		} // if

		if (this.model.get('HasRollovers'))
			this.$el.find('button').text('Pay Roll Over');

		EzBob.UiAction.registerView(this);
		EzBob.UiAction.registerChildren('#payEarly-modal');

		return this;
	}, // render

	events: {
		'click button': 'payEarly'
	}, // events

	payEarly: function() {
		if (this.model.get('TotalBalance'))
			window.location.href = "#PayEarly";
		else
			$('#payEarly-modal').modal();
	}, // payEarly
});
var EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.RequestAcceptTermsView = EzBob.Broker.BaseView.extend({
	initialize: function(options) {
		EzBob.Broker.RequestAcceptTermsView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-requestacceptterms');

		this.termsID = options.termsID;

		this.terms = options.terms;
	}, // initialize

	events: function() {
		var evt = {};

		evt['click #AcceptTermsButton'] = 'submit';

		return evt;
	}, // events

	render: function() {
		if (this.terms)
			this.$el.find('.terms-and-conditions').html(this.terms);

		if (this.termsID)
			this.$el.find('#AcceptTermsButton').data('terms-id', this.termsID);

		this.$el.find('.subsection-saving').hide().removeClass('hide');

		EzBob.UiAction.registerView(this);
	}, // render

	submit: function(event) {
		var nTermsID = this.$el.find('#AcceptTermsButton').data('terms-id');
		console.log('submit accept version', nTermsID, 'by', this.router.getAuth());

		$.post('' + window.gRootPath + 'Broker/BrokerHome/AcceptTerms', {
			nTermsID: this.$el.find('#AcceptTermsButton').data('terms-id'),
			sContactEmail: this.router.getAuth(),
		});

		this.router.followReturnUrl();
	}, // submit
}); // EzBob.Broker.RequestAcceptTermsView

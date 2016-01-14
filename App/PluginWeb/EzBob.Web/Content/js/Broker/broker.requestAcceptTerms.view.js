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
		evt['click #ReAgreeToTerms'] = 'validate';

		return evt;
	}, // events

	validate: function() {
		this.setSomethingEnabled(
			this.$el.find('#AcceptTermsButton'),
			!!this.$el.find('#ReAgreeToTerms:checked').length
		);
	}, // validate

	render: function() {
		if (this.terms)
			this.$el.find('.terms-and-conditions').html(this.terms);

		this.$el.find('#AcceptTermsButton').data(
			'terms-id',
			this.$el.find('.TermsIDToAccept').data('terms-id')
		);

		if (this.termsID)
			this.$el.find('#AcceptTermsButton').data('terms-id', this.termsID);

		this.$el.find('.subsection-saving').hide().removeClass('hide');

		this.validate();

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

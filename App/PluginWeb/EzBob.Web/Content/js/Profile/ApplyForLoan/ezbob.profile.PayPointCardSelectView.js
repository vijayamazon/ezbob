var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.PayPointCardSelectView = Backbone.Marionette.ItemView.extend({
	template: '#PayPointCardSelectViewTemplate',

	initialize: function() {
		this.cards = [];

		var oAllCards = this.model.get('PayPointCards');

		var len = oAllCards.length;

		for (var i = 0; i < len; i++) {
			var oCard = oAllCards[i];

			if ((oCard.ExpireDate != null) && (moment(oCard.ExpireDate).toDate() > moment(this.options.date).toDate()))
				this.cards.push(oCard);
		} // for
	}, // initialize

	ui: {
		cont: '.btn-continue',
	}, // ui

	events: {
		'change input[name="cardOptions"]': 'optionsChanged',
		'click .btn-continue': 'next',
		'click .cancel': 'cancel',
	}, // events

	optionsChanged: function() {
		this.onRender();
	}, // optionsChanged

	onRender: function() {
		var val = this.getCardType();
		var select = this.$el.find('select');

		if (val === 'useExisting') {
			select.removeAttr('readonly disabled');
			this.ui.cont.text('Confirm');
		}
		else {
			this.ui.cont.text('Continue');
			select.attr({
				'readonly': 'readonly',
				'disabled': 'disabled',
			});
		} // if

		EzBob.UiAction.registerView(this);

		return this;
	}, // onRender

	getCardType: function() {
		return this.$el.find('input[name="cardOptions"]:checked').val();
	}, // getCardType

	hasCards: function() {
		return this.cards.length > 1;
	}, // hasCards

	serializeData: function() {
		return {
			cards: this.cards,
		};
	}, // serializeData

	next: function() {
		var val = this.getCardType();

		if (val === 'useExisting')
			this.trigger('select', this.$el.find('option:selected').val());
		else
			this.trigger('existing');

		this.close();
		$('body').removeClass('stop-scroll');
		return false;
	}, // next

	cancel: function() {
		this.trigger('cancel');

		this.close();
		$('body').removeClass('stop-scroll');
		return false;
	}, // cancel

	jqoptions: function () {
		return {
			width: '560px',
			maxWidth: '100%',
			modal: true,
			title: 'Select a card',
			resizable: false,
			draggable: false
		};
	}, // jqoptions
}); // EzBob.Profile.PayPointCardSelectView

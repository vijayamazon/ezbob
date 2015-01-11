var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.Ny2015ScratchView = EzBob.View.extend({
	initialize: function() {
		this.$mainPage = $('.main-page');
		this.$el = $('#ny2015scratch');

		var eventHandler = _.bind(this.onScratch, this);

		this.$scratchArea = this.$el.find('.scratch-area');
		this.$scratchArea.wScratchPad({
			size: 7,
			bg: window.gRootPath + 'Content/img/ny2015scratch/win.png',
			fg: window.gRootPath + 'Content/img/ny2015scratch/scratch.png',
			realtime: false,
			scratchDown: eventHandler,
			scratchMove: eventHandler,
			scratchUp: eventHandler,
		});

		this.$won = this.$el.find('.won');
		this.$decline = this.$el.find('.decline');
		this.$close = this.$el.find('.close-lost');
		this.$amount = this.$won.find('.amount');

		this.isOpen = false;
	}, // initialize

	events: {
		'click .decline-game': 'decline',
		'click .claim': 'claim',
		'click .leave': 'hide',
	}, // events

	decline: function() {
		// TODO: notify server about this

		this.hide();
	}, // decline

	claim: function() {
		// TODO: notify server about this

		this.hide();
	}, // claim

	render: function() {}, // render

	onScratch: function(evt, pct) {
		this.$decline.hide();

		if (this.isOpen)
			return;

		if (pct < 45)
			return;

		this.isOpen = true;

		this.$scratchArea.wScratchPad('clear');

		// TODO: if lost - { this.$close.show(); return; }

		var sAmount = '' + 1234; // TODO: real amount

		this.$amount.empty();

		for (var i = 0; i < sAmount.length; i++) {
			this.$amount.append(
				$('<img>').attr('src', window.gRootPath + 'Content/img/ny2015scratch/' + sAmount[i] + '.png')
			);
		} // for

		this.$won.show();
	}, // onScratch

	show: function() {
		this.$mainPage.hide();
		this.$won.hide();
		this.$el.show();
	}, // show

	hide: function() {
		this.$won.hide();
		this.$el.hide();
		this.$mainPage.show();
	}, // hide
}); // EzBob.Profile.Ny2015ScratchView

var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.Ny2015ScratchView = EzBob.View.extend({
	initialize: function(options) {
		this.customerID = options.customerID;
		this.playerID = options.playerID;

		this.$mainPage = $('.main-page');
		this.$el = $('#ny2015scratch');

		this.$scratchArea = this.$el.find('.scratch-area');

		this.$won = this.$el.find('.won');
		this.$decline = this.$el.find('.decline');
		this.$close = this.$el.find('.close-lost');
		this.$amount = this.$won.find('.amount');

		this.isOpen = false;
		this.hasWon = false;
		this.amount = 0;
	}, // initialize

	events: {
		'click .decline-game': 'decline',
		'click .leave': 'hide',
	}, // events

	decline: function() {
		$.post(
			window.gRootPath + 'Customer/Ny2015Scratch/Decline',
			{ playerID: this.playerID, }
		);

		this.hide();
	}, // decline

	render: function() {
		this.$scratchArea.empty();

		if (!this.playerID) {
			this.hide();
			return;
		} // if

		var self = this;

		var request = $.getJSON(
			window.gRootPath + 'Customer/Ny2015Scratch/PlayLottery',
			{ playerID: this.playerID, }
		);

		request.fail(function() {
			self.hasWon = false;
			self.amount = 0;
			self.hide();
		});

		request.done(function(response) {
			var eventHandler = _.bind(self.onScratch, self);

			if (response.PlayedNow || (response.StatusID === 5)) {
				self.show();

				self.hasWon = !!response.PrizeID;
				self.amount = response.Amount;

				var picName = self.hasWon ? 'win' : 'sorry';

				self.$scratchArea.wScratchPad({
					size: 7,
					bg: window.gRootPath + 'Content/img/ny2015scratch/' + picName + '.png',
					fg: window.gRootPath + 'Content/img/ny2015scratch/scratch.png',
					realtime: false,
					scratchDown: eventHandler,
					scratchMove: eventHandler,
					scratchUp: eventHandler,
				});
			} else {
				self.hasWon = false;
				self.amount = 0;
				self.hide();
			} // if
		});
	}, // render

	onScratch: function(evt, pct) {
		this.$decline.hide();

		if (this.isOpen)
			return;

		if (pct < 45)
			return;

		this.isOpen = true;

		this.$scratchArea.wScratchPad('clear');

		$.post(
			window.gRootPath + 'Customer/Ny2015Scratch/Claim',
			{ playerID: this.playerID, }
		);

		if (!this.hasWon) {
			this.$close.show();
			return;
		} // if

		var sAmount = '' + this.amount;

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
		this.$scratchArea.empty();
		this.$won.hide();
		this.$el.hide();
		this.$mainPage.show();
	}, // hide
}); // EzBob.Profile.Ny2015ScratchView

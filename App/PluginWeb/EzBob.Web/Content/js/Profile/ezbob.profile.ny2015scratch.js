var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.Ny2015ScratchView = EzBob.View.extend({
	initialize: function(options) {
		this.customerID = options.customerID;
		this.playerID = options.playerID;

		this.$mainPage = $('.main-page');
		this.$el = $('#ny2015scratch');

		this.$scratchArea = this.$el.find('.scratch-area');

		this.$alphaScratchArea = this.$el.find('.alpha-scratch-area');
		this.$betaScratchArea = this.$el.find('.beta-scratch-area');
		this.$gammaScratchArea = this.$el.find('.gamma-scratch-area');

		this.$won = this.$el.find('.won');
		this.$decline = this.$el.find('.decline');
		this.$close = this.$el.find('.close-lost');
		this.$amount = this.$won.find('.amount');

		this.isOpen = false;

		this.isAlphaOpen = false;
		this.isBetaOpen = false;
		this.isGammaOpen = false;

		this.hasWon = false;
		this.amount = 0;

		this.pctToScratch = 40;
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
		this.$alphaScratchArea.empty();
		this.$betaScratchArea.empty();
		this.$gammaScratchArea.empty();

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
			if (response.PlayedNow || (response.StatusID === 5)) {
				self.show();

				self.hasWon = !!response.PrizeID;
				self.amount = response.Amount;

				self.$scratchArea.wScratchPad(self.getScratchArgs('onScratch', self.hasWon));

				self.$alphaScratchArea.wScratchPad(self.getScratchArgs('onAlphaScratch'));
				self.$betaScratchArea.wScratchPad(self.getScratchArgs('onBetaScratch'));
				self.$gammaScratchArea.wScratchPad(self.getScratchArgs('onGammaScratch'));
			} else {
				self.hasWon = false;
				self.amount = 0;
				self.hide();
			} // if
		});
	}, // render

	getScratchArgs: function(upEventHandlerName, hasWon) {
		return {
			size: 7,
			bg: window.gRootPath + 'Content/img/ny2015scratch/' + (hasWon ? 'win' : 'sorry') + '.png',
			fg: window.gRootPath + 'Content/img/ny2015scratch/scratch.png',
			realtime: false,
			scratchUp: _.bind(this[upEventHandlerName], this),
		};
	}, // getScratchArgs

	onAlphaScratch: function(evt, pct) {
		this.onScratchOther(pct, 'Alpha');
	}, // onAlphaScratch

	onBetaScratch: function(evt, pct) {
		this.onScratchOther(pct, 'Beta');
	}, // onBetaScratch

	onGammaScratch: function(evt, pct) {
		this.onScratchOther(pct, 'Gamma');
	}, // onGammaScratch

	onScratchOther: function(pct, name) {
		var areaName = '$' + name.toLowerCase() + 'ScratchArea';
		var flagName = 'is' + name + 'Open';

		this.$decline.hide();


		if (this[flagName])
			return;

		if (pct < this.pctToScratch)
			return;

		this[flagName] = true;

		this[areaName].wScratchPad('clear');

		this.showClose();
	}, // onScratchOther

	onScratch: function(evt, pct) {
		this.$decline.hide();

		if (this.isOpen)
			return;

		if (pct < this.pctToScratch)
			return;

		this.isOpen = true;

		this.$scratchArea.wScratchPad('clear');

		$.post(
			window.gRootPath + 'Customer/Ny2015Scratch/Claim',
			{ playerID: this.playerID, }
		);

		if (!this.hasWon) {
			this.showClose();
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

	showClose: function() {
		if (this.isOpen && this.isAlphaOpen && this.isBetaOpen && this.isGammaOpen)
			this.$close.show();
	}, // showClose

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

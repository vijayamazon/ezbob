var EzBob = EzBob || {};
EzBob.ScratchCards = EzBob.ScratchCards || {};

EzBob.ScratchCards.Ny2015 = EzBob.ScratchCards.Base.extend({
	initialize: function(options) {
		this.customerID = options.customerID;
		this.playerID = options.playerID;
		this.controllerName = window.gRootPath +
			(options.customerMode ? 'Customer/Scratch' : 'Broker/BrokerLottery') + '/';

		this.$mainPage = $(options.mainPageClass);
		this.$el = $('#ny2015scratch');

		this.$scratchArea = null;
		this.$alphaScratchArea = null;
		this.$betaScratchArea = null;
		this.$gammaScratchArea = null;

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
		$.post(this.controllerName + 'Decline', {
			playerID: this.playerID,
			userID: this.customerID,
		});

		this.hide();
	}, // decline

	render: function() {
		this.$scratchArea = this.renewArea('scratch-area');
		this.$alphaScratchArea = this.renewArea('alpha-scratch-area');
		this.$betaScratchArea = this.renewArea('beta-scratch-area');
		this.$gammaScratchArea = this.renewArea('gamma-scratch-area');

		if (!this.playerID) {
			this.hide();
			return;
		} // if

		var self = this;

		var request = $.getJSON(this.controllerName + 'PlayLottery', {
			playerID: this.playerID,
			userID: this.customerID,
		});

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

				self.$scratchArea.wScratchPad(self.getScratchArgs('onScratch', '2', self.hasWon));

				self.$alphaScratchArea.wScratchPad(self.getScratchArgs('onAlphaScratch', '1'));
				self.$betaScratchArea.wScratchPad(self.getScratchArgs('onBetaScratch', '3'));
				self.$gammaScratchArea.wScratchPad(self.getScratchArgs('onGammaScratch', '4'));
			} else {
				self.hasWon = false;
				self.amount = 0;
				self.hide();
			} // if
		});
	}, // render

	getScratchArgs: function(upEventHandlerName, idx, hasWon) {
		return {
			size: 7,
			bg: window.gRootPath + 'Content/img/ny2015scratch/' + (hasWon ? 'win' : 'sorry') + '_' + idx + '.png',
			fg: window.gRootPath + 'Content/img/ny2015scratch/scratch_' + idx + '.png',
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

		$.post(this.controllerName + 'Claim', {
			playerID: this.playerID,
			userID: this.customerID,
		});

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
		EzBob.ScratchCards.Ny2015.__super__.show.call(this);

		this.$mainPage.hide();
		this.$won.hide();
		this.$el.show();
	}, // show

	hide: function() {
		this.$scratchArea = this.renewArea('scratch-area');
		this.$alphaScratchArea = this.renewArea('alpha-scratch-area');
		this.$betaScratchArea = this.renewArea('beta-scratch-area');
		this.$gammaScratchArea = this.renewArea('gamma-scratch-area');

		this.$won.hide();
		this.$el.hide();
		this.$mainPage.show();

		EzBob.ScratchCards.Ny2015.__super__.hide.call(this);
	}, // hide

	renewArea: function(className) {
		var newArea = $('<div></div>').attr('class', className);

		this.$el.find('.' + className).replaceWith(newArea);

		return newArea;
	}, // renewArea
}); // EzBob.ScratchCards.Ny2015

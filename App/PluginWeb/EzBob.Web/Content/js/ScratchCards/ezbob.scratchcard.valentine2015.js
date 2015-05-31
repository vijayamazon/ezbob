var EzBob = EzBob || {};
EzBob.ScratchCards = EzBob.ScratchCards || {};

EzBob.ScratchCards.Valentine2015 = EzBob.View.extend({
	initialize: function(options) {
		this.customerID = options.customerID;
		this.playerID = options.playerID;
		this.controllerName = window.gRootPath +
			(options.customerMode ? 'Customer/Scratch' : 'Broker/BrokerLottery') + '/';

		this.$mainPage = $(options.mainPageClass);
		this.$el = $('#valentine2015scratch');

		this.$scratchArea = null;
		this.$alphaScratchArea = null;
		this.$betaScratchArea = null;

		this.$won = this.$el.find('.won');
		this.$decline = this.$el.find('.decline');
		this.$close = this.$el.find('.close-lost');
		this.$city = this.$won.find('.city');

		this.isOpen = false;

		this.isAlphaOpen = false;
		this.isBetaOpen = false;

		this.hasWon = false;
		this.amount = 0;

		this.pctToScratch = 60;
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

				self.$scratchArea.wScratchPad(self.getScratchArgs('onScratch', self.hasWon, self.amount));

				self.$alphaScratchArea.wScratchPad(self.getScratchArgs('onAlphaScratch'));
				self.$betaScratchArea.wScratchPad(self.getScratchArgs('onBetaScratch'));
			} else {
				self.hasWon = false;
				self.amount = 0;
				self.hide();
			} // if
		});
	}, // render

	getScratchArgs: function(upEventHandlerName, hasWon, amount) {
		return {
			size: 7,
			bg: window.gRootPath + 'Content/img/valentine2015scratch/' + (hasWon ? 'prize_' + amount : 'sorry') + '.png',
			fg: window.gRootPath + 'Content/img/valentine2015scratch/scratch_card.png',
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

		var city = '';

		switch (this.amount) {
		case '1':
		case 1:
			city = 'Barcelona';
			break;

		case '2':
		case 2:
			city = 'Paris';
			break;

		case '3':
		case 3:
			city = 'Rome';
			break;
		} // switch

		this.$city.empty().text(city);

		this.$won.show();
	}, // onScratch

	showClose: function() {
		if (this.isOpen && this.isAlphaOpen && this.isBetaOpen)
			this.$close.show();
	}, // showClose

	show: function() {
		this.$mainPage.hide();
		this.$won.hide();
		this.$el.show();
	}, // show

	hide: function() {
		this.$scratchArea = this.renewArea('scratch-area');
		this.$alphaScratchArea = this.renewArea('alpha-scratch-area');
		this.$betaScratchArea = this.renewArea('beta-scratch-area');

		this.$won.hide();
		this.$el.hide();
		this.$mainPage.show();
	}, // hide

	renewArea: function(className) {
		var newArea = $('<div></div>').attr('class', className);

		this.$el.find('.' + className).replaceWith(newArea);

		return newArea;
	}, // renewArea
}); // EzBob.Profile.Valentine2015ScratchView

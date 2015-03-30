var EzBob = EzBob || {};
EzBob.ScratchCards = EzBob.ScratchCards || {};

EzBob.ScratchCards.Easter2015 = EzBob.ScratchCards.Base.extend({
	initialize: function(options) {
		this.customerID = options.customerID;
		this.playerID = options.playerID;
		this.controllerName = window.gRootPath +
			(options.customerMode ? 'Customer/Scratch' : 'Broker/BrokerHome') + '/';

		this.$mainPage = $(options.mainPageClass);
		this.$el = $('#easter2015scratch');

		this.$opening = this.$el.find('.opening');
		this.$playground = this.$el.find('.playground');

		this.$notYet = this.$el.find('.not-yet');

		this.$notNow = this.$el.find('.leave');

		this.$decline = this.$el.find('.decline-game');

		this.$won = this.$el.find('.won');

		this.hasWon = false;
		this.amount = 0;

		this.isPlaying = false;

		this.playLater = false;

		this.nativeGeometries = {
			bg: { width: 1920.0, height: 1200.0, top: 0.0, left: 0.0, },
			notYet: { width: 802.0, height: 563.0, top: 0.0, left: 0.0, },
			notNow: { width: 198.0, height: 60.0, top: 0.0, left: 0.0, },
			eggs: {
				golden: { width: 315.0, height: 433.0, top: 691.0, left: 88.0, },
				magenta: { width: 181.0, height: 232.0, top: 838.0, left: 584.0, },
				pink: { width: 142.0, height: 178.0, top: 953.0, left: 1086.0, }
			},
		};

		this.resizeEventName = 'resize.easter2015Scratch';

		$(window).on(this.resizeEventName, _.bind(this.onResize, this));
	}, // initialize

	remove: function() {
		$(window).off(this.resizeEventName);

		// don't forget to call the original remove() function
		EzBob.ScratchCards.Base.__super__.remove.call(this);
	}, // remove

	events: {
		'click .decline-game': 'decline',
		'click .leave': 'leave',
		'click .opening': 'goPlay',
		'resize body': 'positionEggs',
		'click .egg': 'eggClicked',
		'click .not-yet': 'hideNotYet',
		'click .won': 'hide',
	}, // events

	leave: function() {
		this.playLater = true;
		this.hide();
	}, // leave

	goPlay: function() {
		var self = this;

		this.isPlaying = true;

		this.$opening.fadeOut(function() {
			self.$playground.fadeIn(function() {
				self.positionEggs();
			});
		});
	}, // goPlay

	onResize: function () {
		if (this.isPlaying)
			this.positionEggs();
		else
			this.positionOpening();
	}, // onResize

	positionOpening: function() {
		var rp = this.getRatio('$opening');
		this.positionNotNow(rp.paddingX, rp.ratio);
	}, // positionOpening

	positionNotNow: function(paddingX, ratio) {
		var geometry = this.nativeGeometries.notNow;

		var newHeight = parseInt(geometry.height * ratio);
		var newWidth = parseInt(geometry.width * ratio);

		this.$notNow.css({
			right: parseInt(paddingX + 10 * ratio) + 'px',
			width: newWidth  + 'px',
			height: newHeight + 'px',
			top: parseInt(this.nativeGeometries.bg.height * ratio - 10 * ratio - newHeight) + 'px',
			'background-size': newWidth + 'px ' + newHeight + 'px',
		});
	}, // positionNotNow

	getRatio: function(pageName) {
		var widthRatio = this[pageName].width() / this.nativeGeometries.bg.width;
		var heightRatio = this[pageName].height() / this.nativeGeometries.bg.height;

		var ratio = widthRatio < heightRatio ? widthRatio : heightRatio;

		var paddingX = (this[pageName].width() - this.nativeGeometries.bg.width * ratio) / 2.0;

		return { ratio: ratio, paddingX: paddingX, };
	}, // getRatio

	positionEggs: function() {
		var rp = this.getRatio('$playground');

		var geometry;

		for (var egg in this.nativeGeometries.eggs) {
			geometry = this.nativeGeometries.eggs[egg];

			this.$el.find('.egg-' + egg).css({
				width: parseInt(geometry.width * rp.ratio) + 'px',
				height: parseInt(geometry.height * rp.ratio) + 'px',
				top: parseInt(geometry.top * rp.ratio) + 'px',
				left: parseInt(rp.paddingX + geometry.left * rp.ratio) + 'px',
			}).show();
		} // for

		geometry = this.nativeGeometries.notYet;

		this.$notYet.find('img').css({
			width: parseInt(geometry.width * rp.ratio) + 'px',
			height: parseInt(geometry.height * rp.ratio) + 'px',
		});

		this.positionNotNow(rp.paddingX, rp.ratio);
	}, // positionEggs

	decline: function() {
		this.playLater = true;
		this.hide();

		$.post(this.controllerName + 'Decline', {
			playerID: this.playerID,
			userID: this.customerID,
		});
	}, // decline

	eggClicked: function() {
		this.$decline.hide();

		var $egg = $(event.target);

		var hasPrize = $egg.data('has-prize') === 'yes';

		$egg.remove();

		if (hasPrize) {
			$.post(this.controllerName + 'Claim', {
				playerID: this.playerID,
				userID: this.customerID,
			});

			this.$playground.hide();
			this.$won.show();
		} else
			this.showNotYet();
	}, // eggClicked

	showNotYet: function() {
		this.$notYet.fadeIn();
	}, // showNotYet

	hideNotYet: function() {
		this.$notYet.fadeOut();
	}, // hideNotYet

	render: function() {
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

				self.$won.css('background-image', 'url(/Content/img/easter2015scratch/' + self.amount + '.jpg)');
			} else {
				self.hasWon = false;
				self.amount = 0;
				self.hide();
			} // if
		});
	}, // render

	show: function() {
		if (this.playLater) {
			this.hide();
			return;
		} // if

		EzBob.ScratchCards.Easter2015.__super__.show.call(this);

		this.$mainPage.hide();

		this.$el.show();
		this.$opening.show();

		this.$playground.hide();

		this.$notYet.hide();

		this.positionOpening();
	}, // show

	hide: function() {
		EzBob.ScratchCards.Easter2015.__super__.hide.call(this);

		this.$mainPage.show();

		this.$el.hide();
		this.$opening.hide();

		this.$playground.hide();

		this.$notYet.hide();
	}, // hide
}); // EzBob.Profile.Easter2015ScratchView

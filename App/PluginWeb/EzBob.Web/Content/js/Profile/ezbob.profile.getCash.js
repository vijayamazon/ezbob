var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.GetCashModel = Backbone.Model.extend({
	initialize: function(options) {
		var that = this;
		this.customer = options.customer;

		this.isRequestInProgress = false;

		setInterval(function() { that.refresh(); }, 1000);
	}, // initialize

	refresh: function() {
		var that = this;
		var tempCustomer = null;

		if (this.customer.get('state') === 'wait' && !that.isRequestInProgress) {
			tempCustomer = new EzBob.CustomerModel();
			tempCustomer.id = that.customer.id;
			this.isRequestInProgress = true;

			tempCustomer.fetch({
				success: function() {
					if (tempCustomer.get('state') !== 'wait') {
						that.customer.fetch({
							success: function() {
								that.isRequestInProgress = false;
							}
						});
						return;
					}
					that.isRequestInProgress = false;
				} // success
			}); // fetch
		} // if
	}, // refresh
}); // EzBob.Profile.GetCashModel

EzBob.Profile.GetCashView = Backbone.View.extend({
	className: "d-widget",

	initialize: function(options) {
		this.templates = {
			"get": _.template($('#d-getCash-template').html()),
			"apply": _.template($('#d-getCash-template-apply').html()),
			"wait": _.template($('#d-getCash-template-wait').html()),
			"disabled": _.template($('#d-getCash-template-wait').html()),
			//"bad": _.template($('#d-getCash-template-bad').html()),
			"bad": _.template($('#d-getCash-template-apply').html()),
			"late": _.template($('#d-getCash-template-late').html())
		};

		this.customer = options.customer;

		this.customer.on('change:state', this.render, this);

		setInterval(_.bind(this.refreshTimer, this), 1000);

		var that = this;

		window.YodleeRefreshAccountRetry = function() {
			that.attemptsLeft = (that.attemptsLeft || 5) - 1;
			return {
				url: that.$el.find('#refreshYodleeBtn').attr('href'),
				attemptsLeft: that.attemptsLeft
			};
		}; // window.YodleeRefreshAccountRetry

		window.YodleeAccountUpdateError = function(msg) {
			$.colorbox.close();
			EzBob.App.trigger('error', msg);
		}; // window.YodleeAccountUpdateError
	}, // initialize

	events: {
		'click button.get-cash': 'getCash',
		'click button.apply-for-loan': 'applyForALoan',
	}, // events

	getCash: function() {
		if (this.customer.hasLateLoans())
			return;

		if (this.customer.get('state') !== 'get')
			return;

		window.location.href = "#GetCash";
	}, // getCash

	applyForALoan: function() {
		if (this.customer.get('CustomerStatusName') === 'Legal' || this.customer.get('CustomerStatusName') === 'Default')
			return;

		if (this.customer.hasLateLoans())
			return;

		var sState = this.customer.get('state');

		if (sState !== 'apply' && sState !== 'bad' && sState !== 'disabled')
			return;

		var self = this;

		this.$el.find('.trustpilot-ezbob').dialog({
			autoOpen: true,
			modal: true,
			width: 720,
			resizable: false,
			close: function(evt, ui) {
				$(this).dialog('destroy');
				self.$el.append(this);
			}, // on close
			open: function(evt, ui) {
				var me = $(this);

				if (self.model.get('customer').get('IsOffline')) {
					var oImg = $('div.rate img', me);
					var sSrc = oImg.attr('src');

					sSrc = sSrc.replace(/\.png$/, '-cartless.png');

					oImg.attr('src', sSrc);
				} // if

				$('a.trustpilot-rate', me).click(function() { me.dialog('close'); self.trustpilotRate(); });
				$('a.trustpilot-skip', me).click(function() { me.dialog('close'); self.doApplyForALoan(); });

				$('*:focus', me).blur();
			}, // on open
		}); // dialog
	}, // applyForALoan

	trustpilotRate: function() {
		window.open('http://www.trustpilot.com/evaluate/ezbob.com');
		this.doApplyForALoan();
	}, // trustpilotRate

	doApplyForALoan: function() {
		var that = this;

		this.trigger('applyForLoan');

		BlockUi('on');

		$.post(window.gRootPath + 'Customer/Profile/ApplyForALoan')
			.done(function(result) {
				if (result.hasYodlee) {
					var url = '' + window.gRootPath + 'Customer/YodleeMarketPlaces/RefreshYodlee';
					that.$el.find('#refreshYodleeBtn').attr('href', url);
					that.$el.find('.refresh_yodlee_help').colorbox({ href: '#refresh_yodlee_help', inline: true, transition: 'none', open: true });
				} // if

				if (result.hasBadEkm) {
					that.$el.find('#refresh_ekm_login').val(result.ekm).change();
					that.$el.find('.refresh_ekm_help').colorbox({ href: '#refresh_ekm_help', inline: true, transition: 'none', open: true });
					return;
				} // if

				that.customer.set('state', 'wait');
			})
			.always(function() {
				BlockUi('off');
			});
	}, // doApplyForALoan

	render: function() {
		var state = this.customer.get('state');

		var data = this.model.toJSON();

		data.state = state;
		data.countDown = this.customer.offerValidFormatted();
		data.availableCredit = this.customer.get('CreditSum');
		data.offerStart = this.customer.get('OfferStart');
		data.creditResult = this.customer.get('CreditResult');

		this.$el.html(this.templates[state](data));

		this.$el.find('button').popover({ placement: 'top' });

		return this;
	}, // render

	refreshTimer: function() {
		this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted() + " hrs");
	}, // refreshTimer
}); // EzBob.Profile.GetCashView

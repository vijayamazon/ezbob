EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.Router = Backbone.Router.extend({
	routes: {
		'': 'signup', // this entry must be the first
		'signup': 'signup',
		'login': 'login',
		'dashboard': 'dashboard',
		'forgotten': 'forgotten',
		'ForgotPassword': 'forgotten',
		'add': 'addCustomer',
		'bank': 'addBank',
		'customer/:customerId': 'showCustomer',
		'lead/:leadId': 'showLead',
		'*z': 'dashboard', // this entry must be the last
	}, // routes

	initialize: function() {
		this.returnUrl = null;

		this.setAuth($('body').attr('data-auth'));
		$('body').removeAttr('data-auth');

		$('#user-menu').hide().removeClass('hide');

		this.scratchView = null;
	}, // initialize

	followReturnUrl: function() {
		if (!this.getReturnUrl()) {
			var self = this;

			if (this.getAuth())
				this.setReturnUrl(function() { self.dashboard(); });
			else
				this.setReturnUrl(function() { self.login(); });
		} // if

		var oReturnUrl = this.setReturnUrl(null);

		oReturnUrl();
	}, // followReturnUrl

	getReturnUrl: function() {
		return this.returnUrl;
	}, // getReturnUrl

	setReturnUrl: function(oUrl) {
		var oReturnUrl = this.returnUrl;
		this.returnUrl = oUrl;
		return oReturnUrl;
	}, // setReturnUrl

	getAuth: function() {
		return this.authEmail;
	}, // getAuth

	setAuth: function(sAuth, oBrokerProperties) {
		this.brokerProperties = null;

		this.authEmail = sAuth || '';
		var oElm = $('#user-menu .log-off');
		oElm.tooltip('destroy');
		oElm.tooltip({ placement: 'bottom', title: this.authEmail }).tooltip('fixTitle').tooltip('enable');

		if (oBrokerProperties)
			this.setBrokerProperties(oBrokerProperties);
		else
			this.loadBrokerProperties();
	}, // setAuth

	getBrokerProperties: function() {
		return this.brokerProperties;
	}, // getBrokerProperties

	setBrokerProperties: function(oBrokerProperties) {
		this.brokerProperties = null;

		if (!oBrokerProperties)
			return;

		if ('object' !== typeof oBrokerProperties)
			return;

		if (oBrokerProperties.ContactEmail !== this.authEmail)
			return;

		this.brokerProperties = oBrokerProperties;
		this.trigger('broker-properties-updated');

		// console.log('prop set to', this.getBrokerProperties());
		// console.log('auth email is', this.authEmail);

		var props = this.getBrokerProperties();

		if (!props.LotteryPlayerID)
			return;

		this.scratchView = EzBob.ScratchCards.Select(props.LotteryCode, {
			customerID: props.BrokerID,
			playerID: props.LotteryPlayerID,
			customerMode: false,
			mainPageClass: '#wrapper',
		});

		if (this.scratchView)
			this.scratchView.render();
	}, // setBrokerProperties

	isMyBroker: function(oBrokerProperties) {
		if (!oBrokerProperties)
			return false;

		if ('object' !== typeof oBrokerProperties)
			return false;

		if (oBrokerProperties.ContactEmail !== this.authEmail)
			return false;

		return true;
	}, // isMyBroker

	loadBrokerProperties: function() {
		if (this.isForbidden())
			return;

		if (!this.getAuth())
			return;

		var oRequest = $.getJSON(
			'' + window.gRootPath + 'Broker/BrokerHome/LoadProperties',
			{ sContactEmail: this.getAuth(), }
		);

		var self = this;

		oRequest.success(function(res) {
			if (res.success) {
				self.setBrokerProperties(res.properties);
				return;
			} // if

			if (res.error)
				console.error('Failed to load broker properties:', res.error);
			else
				console.error('Failed to load broker properties.');
		}); // on success

		oRequest.fail(function() {
			console.error('Failed to load broker properties.');
		});
	}, // loadBrokerProperties

	logoff: function() {
		if (this.scratchView) {
			this.scratchView.hide();
			this.scratchView = null;
		} // if

		if (this.views.dashboard)
			this.views.dashboard.clear();

		this.setAuth();
		this.login();

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerAccount/Logoff?sContactEmail=' + encodeURIComponent(this.getAuth()));

		oRequest.success(function(res) {
			if (res.antiforgery_token)
				EzBob.Csrf.updateToken(res.antiforgery_token);
		}); // on success
	}, // logoff

	signup: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else {
			this.createView('signup', EzBob.Broker.SignupView);
			this.show('signup', 'log-in', 'signup');
			this.navigate('signup');
		} // if
	}, // signup

	login: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth()) {
			var sRat = $('body').attr('data-request-accept-terms');

			if (sRat) {
				$('body').removeAttr('data-request-accept-terms');

				if (sRat === 'yes')
					this.requestAcceptTerms();
				else
					this.showDashboard();
			}
			else
				this.showDashboard();
		}
		else {
			this.createView('login', EzBob.Broker.LoginView);
			this.show('login', 'sign-up', 'login');
		} // if
	}, // login

	requestAcceptTerms: function(nTermsID, sTerms) {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth()) {
			this.createView('requestAcceptTerms', EzBob.Broker.RequestAcceptTermsView, {
				termsID: nTermsID,
				terms: sTerms,
			});
			this.show('requestAcceptTerms', 'log-off', 'requestAcceptTerms');
		}
		else
			this.login();
	}, // requestAcceptTerms

	forgotten: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		if (this.getAuth())
			this.showDashboard();
		else {
			this.createView('forgotten', EzBob.Broker.ForgottenView);
			this.show('forgotten', 'sign-up', 'forgotten');
		} // if
	}, // forgotten

	dashboard: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		var self = this;
		this.setReturnUrl(function() { self.dashboard(); });

		if (this.getAuth())
			this.showDashboard();
		else
			this.login();
	}, // dashboard

	showDashboard: function() {
		this.createView('dashboard', EzBob.Broker.DashboardView);
		this.show('dashboard', 'log-off', 'dashboard');
		this.navigate('dashboard');
	}, // showDashboard

	addCustomer: function() {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		var self = this;
		this.setReturnUrl(function() { self.addCustomer(); });

		if (this.getAuth()) {
			this.createView('addCustomer', EzBob.Broker.AddCustomerView);
			this.show('add-customer', 'log-off', 'addCustomer');
		}
		else
			this.login();
	}, // addCustomer

	addBank: function() {
	    if (this.isForbidden()) {
	        this.forbidden();
	        return;
	    } // if

	    var self = this;
	    this.setReturnUrl(function() { self.addBank(); });

	    if (this.getAuth()) {
	        this.createView('addBank', EzBob.Broker.AddBankView);
	        this.show('add-bank', 'log-off', 'addBank');
	    }
	    else
	        this.login();
	}, // addCustomer

	showCustomer: function(customerId) {
		if (this.isForbidden()) {
			this.forbidden();
			return;
		} // if

		var self = this;
		this.setReturnUrl((function(nCustID) {
			return function() { self.showCustomer(nCustID); };
		})(customerId));

		if (this.getAuth()) {
			if (this.views) {
				if (this.views.customer)
					this.views.customer.clear();

				this.views.customer = null;
			} // if

			this.createView('customer', EzBob.Broker.CustomerDetailsView, { customerid: customerId });
			this.show('customer-details', 'log-off', 'customer');
			this.navigate('customer/' + customerId);
		}
		else
			this.login();
	}, // showCustomer

	showLead: function (leadId) {
	    if (this.isForbidden()) {
	        this.forbidden();
	        return;
	    } // if

	    var self = this;
	    this.setReturnUrl((function (nLeadID) {
	        return function () { self.showLead(nLeadID); };
	    })(leadId));

	    if (this.getAuth()) {
	        if (this.views) {
	            if (this.views.lead)
	                this.views.lead.clear();

	            this.views.lead = null;
	        } // if

	        this.createView('lead', EzBob.Broker.LeadDetailsView, { leadid: leadId });
	        this.show('lead-details', 'log-off', 'lead');
	        this.navigate('lead/' + leadId);
	    }
	    else
	        this.login();
	}, // showCustomer

	forbidden: function() {
		this.show(this.forbiddenSection());
	}, // forbidden

	isForbidden: function() {
		return '-' === this.getAuth();
	}, // isForbidden

	show: function(sSectionName, sButtonClass, sViewName) {
		sSectionName = sSectionName.toLowerCase();
		var oSection;

		if (sSectionName === this.forbiddenSection())
			oSection = $('.section-' + sSectionName);
		else {
			if (this.isForbidden())
				sSectionName = this.forbiddenSection();

			oSection = $('.section-' + sSectionName);

			if (!oSection.length) {
				sSectionName = this.forbiddenSection();
				oSection = $('.section-' + sSectionName);
			} // if
		} // if

		if (sSectionName === this.forbiddenSection())
			sButtonClass = null;

		var self = this;

		var oShowPage = function() {
			var oView = null;

			if (sViewName && self.views && self.views[sViewName]) {
				oView = self.views[sViewName];

				if (oView.setSidebar)
					oView.setSidebar($('.common-customer-sidebar'));
				
				oView.render();
			}
			else
				$('#hidden-container').append($('.common-customer-sidebar'));

			oSection.fadeIn(400, function() {
				if (oView)
					oView.onFocus();

				self.currentView = oView;
			});
		}; // oShowPage

		var oActive = $('.page-section:visible');

		if (oActive.length) {
			if (this.currentView)
				this.currentView.onBlur();

			oActive.fadeOut(400, oShowPage);
		}
		else
			oShowPage();

		var oMenu = $('#user-menu');

		if (sButtonClass) {
			oMenu.show();
			$('.menu-btn', oMenu).hide();
			$('.' + sButtonClass, oMenu).show();
		}
		else {
			oMenu.hide();
			$('.menu-btn', oMenu).hide();
		} // if
	}, // show

	forbiddenSection: function() { return 'forbidden'; }, // forbiddenSection

	createView: function(sViewName, oViewType, oViewTypeArgs) {
		if (!this.views)
			this.views = {};

		if (!this.views[sViewName]) {
			var oArgs = $.extend({}, oViewTypeArgs || {}, { router: this, });
			this.views[sViewName] = new oViewType(oArgs);

			if (!this.views[sViewName].router.timeStamp) {
				var d = new Date();
				this.views[sViewName].router.timeStamp = d;
				// console.log('Router timestamp set to', d);
			} // if
		} // if
	}, // createView
}); // EzBob.Broker.Router

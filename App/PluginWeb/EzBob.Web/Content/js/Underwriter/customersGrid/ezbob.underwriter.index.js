﻿(function() {
	//count updater
	var counterTimer;

	function updateCounters() {
		var isTestChecked = $('#include-test-customers:checked').length > 0;
		var xhr = $.get(window.gRootPath + 'Underwriter/Customers/GetCounters', { isTest: isTestChecked });

		xhr.done(function(response) {
			_.each(response, function(val) {
				$('#' + val.Name + '-count').text(val.Count);
			});
		});
	} // updateCounters

	var TUnderwriterRouter = Backbone.Router.extend({
		initialize: function() {
			this.views = {
				grids: {
					view: new EzBob.Underwriter.GridsView({ el: $('#grids-view') }),
					isRendered: false,
					menuItem: 'liApplications',
				},
				profile: {
					view: new EzBob.Underwriter.ProfileView({ el: $('#profile-view') }),
					isRendered: false,
					menuItem: 'liApplications',
				},
				strategySettings: {
					view: new EzBob.Underwriter.StrategySettingsView({ el: $('#settings-view') }),
					isRendered: false,
					menuItem: 'liStrategySettings',
				},
				strategyAutomation: {
					view: new EzBob.Underwriter.StrategyAutomationView({ el: $('#automation-view') }),
					isRendered: false,
					menuItem: 'liAutomation',
				},
				support: {
					view: new EzBob.Underwriter.SupportView({ el: $('#support-area') }),
					isRendered: false,
					menuItem: 'liSupport',
				},
				fraud: {
					view: new EzBob.Underwriter.FraudView({ el: $('#fraud-area') }),
					isRendered: false,
					menuItem: 'liFraud',
				},
				report: {
				    view: new EzBob.Underwriter.ReportView({ el: $('#report-view') }),
				    isRendered: false,
				    menuItem: 'liReports',
				},
				funding: {
				    view: new EzBob.Underwriter.FundingView({ el: $('#funding-view') }),
				    isRendered: false,
				    menuItem: 'liFunding',
				}
			}; // views
		}, // initialize

		routes: {
			'': 'customers',
			'customers/:type': 'customers',
			'profile/:id': 'profile',
			'profile/:id/': 'profile',
			'profile/:id/:section': 'profile',
			'profile/:id/:section/': 'profile',
			'settings': 'settings',
			'automation': 'automation',
			'support': 'support',
			'fraud': 'fraud',
			'report': 'report',
			'funding' : 'funding',
			'*z': 'customers',
		}, // routes

		handleRoute: function(sViewName, id, type) {
			var oView = this.views[sViewName];

			this.switchMenuTo(oView.menuItem);

			if (!oView.isRendered) {
				oView.isRendered = true;
				oView.view.render();
			} // if

			this.hideAll();

			oView.view.show(id, type);
		}, // handleRoute

		customers: function(type) {
			if (!type)
				type = localStorage.getItem('uw_grids_last_shown-' + $('#uw-name-and-icon').attr('data-uw-id'));

			counterTimer = counterTimer || setInterval(updateCounters, 5000);

			this.handleRoute('grids', null, type);
		}, // customers

		profile: function(id, section) {
			clearInterval(counterTimer);
			counterTimer = null;

			this.handleRoute('profile', id);

			this.views.profile.view.showed = true;

			this.views.profile.view.setState(id, section);
			this.views.profile.view.restoreState();
		}, // profile

		settings: function() {
			this.handleRoute('strategySettings');
		}, // settings

		automation: function() {
			this.handleRoute('strategyAutomation');
		}, // automation

		support: function() {
			this.handleRoute('support');
		}, // support

		fraud: function() {
			this.handleRoute('fraud');
		}, // fraud
		
		report: function () {
		    this.handleRoute('report');
		}, // fraud

		funding: function () {
		    this.handleRoute('funding');
		}, // funding

		switchMenuTo: function(name) {
			$('.navbar ul.nav > li[id]').removeClass('active');
			$('#' + name).addClass('active');
		}, // switchMenuTo

		hideAll: function() {
			for (var i in this.views)
				this.views[i].view.hide();

			this.views.profile.view.showed = false;
		}, // hideAll
	}); // TUnderwriterRouter

	var oRouter = new TUnderwriterRouter();

	oRouter.views.grids.view.router = oRouter;

	oRouter.views.profile.view.router = oRouter;

	Backbone.history.start();

	oRouter.views.support.view.on('rechecked', function(options) {
		var umi = options.umi;

		var el = $('[umi=' + umi + ']');

		var status = el.parent().prev();

		oRouter.views.support.view.model.fetch().done(function() {
			var interval = setInterval(function() {
				var req = $.get(window.gRootPath + 'Underwriter/MarketPlaces/CheckForUpdatedStatus', { mpId: umi });

				return req.done(function(response) {
					status.text(response.status);

					if (response.status !== 'In progress') {
						clearInterval(interval);

						oRouter.views.support.view.model.fetch().done(function() {
							el.removeClass('disabled');

							if (response.status === 'Done')
								EzBob.ShowMessage('The MP ' + umi + ' rechecked successfully', '', null, 'OK');

							oRouter.views.support.view.model.trigger('change');
						});
					} // if
				}); // done
			}, 1000); // setInterval
		}); // on fetch done
	}); // on rechecked

	var recentCustomersModel = new EzBob.Underwriter.RecentCustomersModel();

	recentCustomersModel.fetch().done(function() {
		localStorage.setItem('RecentCustomers', JSON.stringify(recentCustomersModel.get('RecentCustomers')));

		var a = new EzBob.Underwriter.goToCustomerId();

		a.on('ok', function(id) {
			oRouter.views.profile.view.router.navigate('profile/' + id, { trigger: true, replace: true });
		});

		$('#liClient > a').on('click', function() {
			a.render();
			return false;
		});
	}); // recent customers model fetch done

	var aryInitialCustomerID = window.location.search.match(/[\?&]customerid=(\d+)/);

	if (aryInitialCustomerID)
		oRouter.navigate('#profile/' + aryInitialCustomerID[1], { trigger: true });
})();
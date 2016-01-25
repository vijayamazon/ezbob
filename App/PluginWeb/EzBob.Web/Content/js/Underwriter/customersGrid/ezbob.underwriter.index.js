(function() {
	//count updater
	var counterTimer;

	function updateCounters() {
		var isTestChecked = $('#include-test-customers:checked').length > 0;
		var xhr = $.get(window.gRootPath + 'Underwriter/Grids/GetCounters', { isTest: isTestChecked });

		xhr.done(function(response) {
			_.each(response, function(val) {
				$('#' + val.Name + '-count').text(val.Count);
			});
		});
	} // updateCounters

	var TUnderwriterRouter = Backbone.Router.extend({
		initialize: function() {
			var oFundingModel = new EzBob.Underwriter.FundingModel();

			function fundingModelFetchDone() {
				$("[id='liFunding'] span,[id='liFunding'] i").toggleClass('red_cell', oFundingModel.notEnoughFunds());
			} // fundingModelFetchDone

			oFundingModel.fetch().done(function() {
				fundingModelFetchDone();
				setInterval(
					function() { oFundingModel.fetch().done(function() { fundingModelFetchDone(); }); },
					oFundingModel.get('RefreshInterval') || 5000
				);
			});

			this.views = {
				grids: {
					view: new EzBob.Underwriter.GridsView({ el: $('#grids-view'), }),
					isRendered: false,
				},
				profile: {
					view: new EzBob.Underwriter.ProfileView({ el: $('#profile-view'), fundingModel: oFundingModel, }),
					isRendered: false,
				},
				strategySettings: {
					view: new EzBob.Underwriter.StrategySettingsView({ el: $('#settings-view'), }),
					isRendered: false,
				},
				strategyAutomation: {
					view: new EzBob.Underwriter.StrategyAutomationView({ el: $('#automation-view'), }),
					isRendered: false,
				},
				support: {
					view: new EzBob.Underwriter.SupportView({ el: $('#support-area'), }),
					isRendered: false,
				},
				fraud: {
					view: new EzBob.Underwriter.FraudView({ el: $('#fraud-area'), }),
					isRendered: false,
				},
				report: {
					view: new EzBob.Underwriter.ReportView({ el: $('#report-view'), }),
					isRendered: false,
				},
				funding: {
					view: new EzBob.Underwriter.FundingView({ el: $('#funding-view'), model: oFundingModel, }),
					isRendered: false,
				},
				broker: {
					view: new EzBob.Underwriter.BrokerProfileView({ el: $('#broker-profile-view'), }),
					isRendered: false,
				},
				addInvestor: {
					view: new EzBob.Underwriter.AddInvestorView({ el: $('#add-investor-view'), }),
					isRendered: false,
				},
			
				manageInvestors: {
				    view: new EzBob.Underwriter.ManageInvestorsView({ el: $('#investors-view'), }),
				    isRendered: false,
				},
				configInvestor: {
					view: new EzBob.Underwriter.ConfigInvestorView({ el: $('#config-investor-view'), }),
					isRendered: false,
				},

				accountingInvestor: {
					view: new EzBob.Underwriter.AccountingInvestorView({ el: $('#accounting-investor-view'), model: new EzBob.Underwriter.AccountingInvestorModel() }),
					isRendered: false,
				},

				portfolioInvestor: {
					view: new EzBob.Underwriter.PortfolioInvestorView({ el: $('#portfolio-investor-view'), }),
					isRendered: false,
				},

				statisticsInvestor: {
					view: new EzBob.Underwriter.StatisticsInvestorView({ el: $('#statistics-investor-view'), }),
					isRendered: false,
				},
				
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
			'funding': 'funding',
			'broker/:id': 'broker',
			'broker/:id/': 'broker',
			'broker/:id/:section': 'broker',
			'broker/:id/:section/': 'broker',
			'addInvestor': 'addInvestor',
			'manageInvestor': 'manageInvestors',
	
			'configInvestor': 'configInvestor',
			'configInvestor/:id': 'configInvestor',
			'configInvestor/:id/': 'configInvestor',
			'accountingInvestor': 'accountingInvestor',
			'portfolioInvestor': 'portfolioInvestor',
			'statisticsInvestor': 'statisticsInvestor',
			'*z': 'customers',
		}, // routes

		handleRoute: function(sViewName, id, type, forceRender) {
			var oView = this.views[sViewName];
			if (!oView.isRendered || forceRender) {
				oView.isRendered = true;
				oView.view.render();
				EzBob.handleUserLayoutSetting();
			} // if

			this.hideAll();

			oView.view.show(id, type);
		}, // handleRoute

		customers: function(type) {
			if (!type)
				type = localStorage.getItem('uw_grids_last_shown-' + $('#uw-name-and-icon').attr('data-uw-id'));

			counterTimer = counterTimer || setInterval(updateCounters, 5000);

			this.handleRoute('grids', null, type);

			var a = new EzBob.Underwriter.goToCustomerId();

			a.on('ok', function(id) {
				oRouter.views.profile.view.router.navigate('profile/' + id, { trigger: true, replace: true });
			});

			$('[id="liClient"] > a').unbind("click").on('click', function() {
				a.render();
				return false;
			});
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

		report: function() {
			this.handleRoute('report');
		}, // fraud

		funding: function() {
			this.handleRoute('funding');
		}, // funding

		broker: function(id, section) {
			this.handleRoute('broker', id, section);
		}, // broker

		addInvestor: function () {
			this.handleRoute('addInvestor');
		},

	
		manageInvestors: function () {
		    this.handleRoute('manageInvestors');
		},

		configInvestor: function (id) {
			this.handleRoute('configInvestor', id);
		},

		accountingInvestor: function() {
			this.handleRoute('accountingInvestor');
		},

		portfolioInvestor: function() {
			this.handleRoute('portfolioInvestor');
		},

		statisticsInvestor: function() {
			this.handleRoute('statisticsInvestor');
		},

		hideAll: function() {
			for (var i in this.views)
				this.views[i].view.hide();

			this.views.profile.view.showed = false;
		}, // hideAll
	}); // TUnderwriterRouter

	var oRouter = new TUnderwriterRouter();

	for (var i in oRouter.views)
		oRouter.views[i].view.router = oRouter;

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
	}); // recent customers model fetch done

	var aryInitialCustomerID = window.location.search.match(/[\?&]customerid=(\d+)/);

	if (aryInitialCustomerID)
		oRouter.navigate('#profile/' + aryInitialCustomerID[1], { trigger: true });
})();
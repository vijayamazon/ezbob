var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.GetCashModel = Backbone.Model.extend({
	initialize: function(options) {
		this.customer = options.customer;

		this.isRequestInProgress = false;
		this.trustPilotClaim = false;
		this.refreshInterval = null;

		this.startRefresh();
	}, // initialize

	startRefresh: function() {
		var that = this;

		$.post(window.gRootPath + 'Customer/CustomerStatus/GetRefreshInterval').done(function (result) {
			if (!that.refreshInterval) {
				that.refreshInterval = setInterval(
					_.bind(that.refresh, that),
					Math.max(parseInt(result.Interval) || 0, 15000) // do not allow refresh more frequently than 15 sec
				);
			} // if
		});
	}, // startRefresh

	refresh: function() {
		var that = this;

		if (!that.isRequestInProgress) {
			that.isRequestInProgress = true;

			var oRequest = $.post(
				window.gRootPath + 'Customer/CustomerStatus/GetCustomerStatus',
				{ customerId: that.customer.get('Id') }
			);
			
			oRequest.done(function(result) {
				if (result.State !== that.previousState) {
					that.previousState = result.State;

					that.customer.fetch({
						success: function() { that.isRequestInProgress = false; },
					});

					return;
				} // if

				that.isRequestInProgress = false;
			});

			oRequest.fail(function() {
				// In case of error:
				that.isRequestInProgress = false;

				// 1. stop refreshing
				if (that.refreshInterval) {
					clearInterval(that.refreshInterval);
					that.refreshInterval = null;
				} // if

				// 2. restart refreshing after five minutes
				setTimeout(_.bind(that.start, that), 300000);
			});
		} // if
	}, // refresh
}); // EzBob.Profile.GetCashModel

EzBob.Profile.GetCashView = Backbone.View.extend({
	className: "d-widget",

	initialize: function(options) {
		this.templates = {
			"get":      { template: _.template($('#d-getCash-template')      .html()), name: 'getCash' },
			"apply":    { template: _.template($('#d-getCash-template-apply').html()), name: 'apply' },
			"wait":     { template: _.template($('#d-getCash-template-wait') .html()), name: 'wait' },
			"disabled": { template: _.template($('#d-getCash-template-bad')  .html()), name: 'bad' },
			"bad":      { template: _.template($('#d-getCash-template-apply').html()), name: 'apply' },
			"late":     { template: _.template($('#d-getCash-template-late') .html()), name: 'late' }
		};

		this.refreshAccountData = null;

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
		'click button.choose-amount-wait': 'maybeShowCreditLine',
		'click button.apply-for-loan': 'applyForALoan',
	}, // events

	getCash: function () {
		if (this.customer.get('state') !== 'get')
			return;

		if (this.customer.get('IsAlibaba'))
			this.showCreditLine();
		else {
			if (this.customer.hasLateLoans())
				return;

			window.location.href = '#GetCash';
		} // if
	}, // getCash

	maybeShowCreditLine: function() {
		if (this.customer.get('IsAlibaba'))
			this.showCreditLine();
	}, // maybeShowCreditLine

	showCreditLine: function() {
		var clView = new EzBob.Profile.ReviewSignCreditLineView({
			customerID: this.customer.get('Id'),
			cashRequestID: this.customer.get('LastCashRequestID'),
			signedLegalID: this.customer.get('SignedLegalID'),

			firstName: this.customer.get('FirstName'),
			middleName: this.customer.get('MiddleName'),
			lastName: this.customer.get('LastName'),

			approvedAmount: this.customer.get('LastApprovedAmount'),
			repaymentPeriod: this.customer.get('LastApprovedRepaymentPeriod'),
			loanTypeID: this.customer.get('LastApprovedLoanTypeID'),

			isPersonal: this.customer.get('BusinessTypeReduced') === 'Personal',

			alibabaCreditFacilityTemplate: this.customer.get('AlibabaCreditFacilityTemplate'),
		});
		clView.render();
	}, // showCreditLine

	applyForALoan: function() {
		UnBlockUi();
		this.sliders.changeLoanAmount('saveOnly');

		if (this.customer.get('IsDefaultCustomerStatus'))
			return;

		if (this.customer.hasLateLoans())
			return;

		var sState = this.customer.get('state');

		if (sState !== 'apply' && sState !== 'bad' && sState !== 'disabled')
			return;

		var isTrustPilotEnabled = this.customer.get('TrustPilotReviewEnabled') &&
			this.customer.get('hasLoans') &&
			(this.customer.get('Origin') === 'ezbob');

		if (isTrustPilotEnabled) {
			var nTrustPilotStatusID = this.customer.get('TrustPilotStatusID');

			if (nTrustPilotStatusID === 0) {
				this.openTrustPilotDlg();
				return;
			} // if never left review
		} // if review enabled

		this.checkTurnoverIsNotExpired();
	}, // applyForALoan

	openTrustPilotDlg: function() {
		var self = this;

		this.$el.find('.trustpilot-ezbob').dialog({
			autoOpen: true,
			modal: true,
			width: 500,
			resizable: false,
			closeOnEscape: false,
			close: function() {
				$(this).dialog('destroy');
				self.$el.append(this);
				$('body').removeClass('stop-scroll');
			}, // on close
			open: function() {
				var me = $(this);
				$('body').addClass('stop-scroll');
				$('.ui-dialog-titlebar', me.parent()).hide();

				$('a.trustpilot-rate', me).click(function() {
					me.dialog('close');
					window.open('http://www.trustpilot.com/evaluate/ezbob.com');
					self.trustPilotClaim = true;
				});

				$('a.trustpilot-skip', me).click(function() {
					me.dialog('close');
					self.checkTurnoverIsNotExpired();
				});

				$('*:focus', me).blur();
			}, // on open
		}); // dialog
	}, // openTrustPilotDlg

	checkTurnoverIsNotExpired: function () {
		if (this.customer.get('IsTurnoverExpired')) {
			this.trigger('turnover');
			return;
		}
		
		this.doApplyForALoan();
	},//checkTurnoverIsNotExpired

	doApplyForALoan: function () {
		var that = this;

		this.makeTargeting();

		BlockUi();

		if (this.trustPilotClaim)
			$.post(window.gRootPath + 'Customer/Profile/ClaimsTrustPilotReview');

		EzBob.App.Iovation.callIovation('getcash');

		var oRequest = $.post(window.gRootPath + 'Customer/Profile/ApplyForALoan');

		oRequest.done(function(oResponse) {
			if (oResponse.error) {
				EzBob.App.trigger('error', oResponse.error);
				return;
			} // if

			if (!oResponse.good_to_go) {
				var oDlg = that.$el.find('#refresh-accounts-dlg');

				oDlg.find('.skip-refresh-accounts').click(function(e) {
					e.preventDefault();
					e.stopPropagation();

					EzBob.UiAction.saveOne('click', this);

					that.refreshAccountData = { type: 'skip-refresh', };

					$.colorbox.close();
				});

				var oList = oDlg.find('.account-list').empty();
				var updatesNeeded = false;

				var sDisplayName;
				if (oResponse.has_hmrc) {
					updatesNeeded = true;
					for (var i = 0; i < oResponse.linked_hmrc.length; i++) {
						sDisplayName = oResponse.linked_hmrc[i];

						oList.append(that.createUpdateEntry(
							'HMRC account <strong>' + sDisplayName + '</strong> password',
							'hmrc-list-item',
							{
								type: 'linked-hmrc',
								displayName: sDisplayName,
							}
						));
					} // for each linked hmrc
				} // if

				if (oResponse.HasUploadedHmrc && !oResponse.vat_return_is_up_to_date) {
					updatesNeeded = true;
					oList.append(that.createUpdateEntry(
						'your VAT return data', 'vat-return-list-item', { type: 'vat-return', }
					));
				} // if

				if (oResponse.has_ekm) {
					updatesNeeded = true;

					// ReSharper disable once MissingHasOwnPropertyInForeach
					for (sDisplayName in oResponse.ekms) {
						var sErrorMsg = oResponse.ekms[sDisplayName];

						oList.append(that.createUpdateEntry(
							'ekmpowershop account <strong>' + sDisplayName + '</strong> password',
							'ekm-list-item',
							{
								type: 'ekm',
								displayName: sDisplayName,
								errorMsg: sErrorMsg,
							}
						));
					} // for each EKM
				} // if has EKM

				if (oResponse.has_yodlee) {
					updatesNeeded = true;
					oList.append(that.createUpdateEntry(
						'your bank account', 'yodlee-list-item', { type: 'bank', }
					));
				} // if has yodlee

				if (updatesNeeded) {
					that.$el.find('.refresh-account-help').colorbox({
						href: '#refresh-accounts-dlg',
						inline: true,
						open: true,
						maxWidth: '100%',
						maxHeight: '100%',
						onOpen: function() {
							$('body').addClass('stop-scroll');
						},
						onClosed: function() {
							that.refreshAccount();
							$('body').removeClass('stop-scroll');
						},
						close: '<i class="pe-7s-close"></i>',
					});
				} else {
					that.directApplyForLoan();
				}

				return;
			} // if
			
			that.customer.set('state', 'wait');
		});

		oRequest.always(function() {
			UnBlockUi();
		});
	}, // doApplyForALoan

	createUpdateEntry: function(sText, sUiEventControlID, oRefreshData) {
		var self = this;

		var oBtn = $('<button>Update</button>')
			.addClass('button btn-green ev-btn-org')
			.attr(
				'ui-event-control-id', 'refresh-account:' + sUiEventControlID
			).click(function(e) {
				e.preventDefault();
				e.stopPropagation();

				EzBob.UiAction.saveOne('click', this);

				self.refreshAccountData = oRefreshData;

				$.colorbox.close();
			});

		return $('<li />').append(oBtn).append($('<span />').html(' ' + sText));
	}, // createUpdateEntry

	refreshAccount: function() {
		$.colorbox.remove();

		if (!this.refreshAccountData)
			return;

		switch (this.refreshAccountData.type) {
		case 'ekm':
			this.refreshEkm();
			break;

		case 'bank':
			this.refreshYodlee();
			break;

		case 'linked-hmrc':
			this.refreshLinkedHmrc();
			break;

		case 'vat-return':
			this.refreshVatReturn();
			break;

		case 'skip-refresh':
			this.directApplyForLoan();
			break;
		} // switch

		this.refreshAccountData = null;
	}, // refreshAccount

	directApplyForLoan: function() {
		var that = this;
		BlockUi('on');

		var oRequest = $.post(window.gRootPath + 'Customer/Profile/DirectApplyForLoan');

		oRequest.done(function(oResponse) {
			if (oResponse.error) {
				EzBob.App.trigger('error', oResponse.error);
				return;
			} // if

			that.customer.set('state', 'wait');
		});

		oRequest.always(function() {
			UnBlockUi();
		});
	}, // directApplyForLoan

	refreshVatReturn: function () {
		this.uploadUi = new EzBob.HmrcUploadUi({
			chartMonths: this.options.chartMonths,
			formID: 'hmrcAccountUpload',
			uploadUrl: '/Customer/Hmrc/SaveFile',
			loadPeriodsUrl: '/Customer/Hmrc/LoadPeriods',
			isUnderwriter: false,
			uiEventControlIDs: {
				form: 'hmrc-cash-request:dropzone',
				backBtn: 'hmrc-cash-request:upload_back',
				doneBtn: 'hmrc-cash-request:do_upload',
			},
			classes: {
			    backBtn: 'button btn-grey back clean-btn',
				doneBtn: 'button btn-green ev-btn-org',
			},
			clickBack: _.bind(this.backFromUploadFiles, this, false),
			clickDone: _.bind(this.backFromUploadFiles, this, true),
		});

		this.uploadUi.$el = $('#refresh-vat-return');

		this.uploadUi.$el.outerWidth(this.$el.outerWidth());
		this.uploadUi.$el.outerHeight($(window).height() * 0.75);

		this.uploadUi.render();

		this.$el.find('.refresh-account-help').colorbox({
			href: '#refresh-vat-return',
			inline: true,
			open: true,
			maxWidth: '100%',
			maxHeight: '100%',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$.colorbox.remove();
				$('body').removeClass('stop-scroll');
			},
			close: '<i class="pe-7s-close"></i>',
		});
	}, // refreshVatReturn

	backFromUploadFiles: function(bRecheck) {
		$.colorbox.close();

		if (bRecheck)
			this.applyForALoan();
	}, // backFromUploadFiles

	refreshYodlee: function() {
		this.$el.find('#refreshYodleeBtn').attr('href', '' + window.gRootPath + 'Customer/YodleeMarketPlaces/RefreshYodlee');
		this.$el.find('.refresh-account-help').colorbox({
			href: '#refresh_yodlee_help',
			inline: true,
			open: true,
			maxWidth: '100%',
			maxHeight: '100%',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$.colorbox.remove(); 
				$('body').removeClass('stop-scroll');
			},
			close: '<i class="pe-7s-close"></i>',
		});
	}, // refreshYodlee

	refreshEkm: function() {
		this.$el.find('#update-ekm-error').text(this.refreshAccountData.errorMsg);
		this.$el.find('#refresh_ekm_login').val(this.refreshAccountData.displayName).change();
		this.$el.find('#refresh_ekm_password').val('').change().focus();

		if (!this.ekmRefreshButtonEventSet) {
			this.$el.find('#refreshEkmBtn').click(_.bind(this.validateAndUpdateAccount, this, 'ekm'));
			this.ekmRefreshButtonEventSet = true;
		} // if

		this.$el.find('.refresh-account-help').colorbox({
			href: '#refresh_ekm_help',
			inline: true,
			open: true,
			maxWidth: '100%',
			maxHeight: '100%',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$.colorbox.remove(); 
				$('body').removeClass('stop-scroll');
			},
			close: '<i class="pe-7s-close"></i>',
		});
	}, // refreshEkm

	refreshLinkedHmrc: function() {
		this.$el.find('#update-hmrc-error').text("Invalid username or password.");
		this.$el.find('#refresh_hmrc_login').val(this.refreshAccountData.displayName).change();
		this.$el.find('#refresh_hmrc_password').val('').change().focus();

		if (!this.hmrcRefreshButtonEventSet) {
			this.$el.find('#refreshHmrcBtn').click(_.bind(this.validateAndUpdateAccount, this, 'hmrc'));
			this.hmrcRefreshButtonEventSet = true;
		} // if

		this.$el.find('.refresh-account-help').colorbox({
			href: '#refresh_hmrc_help',
			inline: true,
			open: true,
			maxWidth: '100%',
			maxHeight: '100%',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$.colorbox.remove(); 
				$('body').removeClass('stop-scroll');
			},
			close: '<i class="pe-7s-close"></i>',
		});
	}, // refreshLinkedHmrc

	validateAndUpdateAccount: function(sAccountType) {
		var sErrorMsg;
		var sAction;
		var sLogin;
		var sPassword;

		switch (sAccountType) {
		case 'ekm':
			sErrorMsg = '#update-ekm-error';
			sAction = 'Customer/EkmMarketPlaces/Update';
			sLogin = '#refresh_ekm_login';
			sPassword = '#refresh_ekm_password';
			break;

		case 'hmrc':
			sErrorMsg = '#update-hmrc-error';
			sAction = 'Customer/CGMarketPlaces/Update';
			sLogin = '#refresh_hmrc_login';
			sPassword = '#refresh_hmrc_password';
			break;

		default:
			return;
		} // switch

		$(sErrorMsg).empty();

		BlockUi();

		var oRequest = $.post('' + window.gRootPath + sAction, {
			name: $(sLogin).val(),
			password: $(sPassword).val(),
		});

		var self = this;

		oRequest.done(function(res) {
			if (res.success) {
				$.colorbox.close();
				self.applyForALoan();
				return;
			} // if

			if (res.error)
				$(sErrorMsg).html(res.error);

			UnBlockUi();
		});

		oRequest.fail(function() {
			UnBlockUi();
		});
	}, // validateAndUpdateAccount

	makeTargeting: function() {
		var that = this;

		$.post(window.gRootPath + 'Customer/Profile/EntrepreneurTargeting').done(function(result) {
			if (result.companyTargeting) {
				if (EzBob.Config.TargetsEnabledEntrepreneur) {
					var req = $.get(
						window.gRootPath + 'Account/CheckingCompany',
						{
							companyName: result.companyName,
							postcode: result.companyPostcode,
							filter: result.companyType,
							refNum: result.companyNumber,
						}
					);

					scrollTop();
					BlockUi();

					req.done(function(reqData) {
						if (reqData) {
							switch (reqData.length) {
							case 0:
								break;

							case 1:
								that.saveTargeting(reqData[0]);
								break;

							default:
								var companyTargets = new EzBob.companyTargets({ model: reqData });

								companyTargets.render();

								companyTargets.on('BusRefNumGot', function(targetingData) {
									that.saveTargeting(targetingData);
								});

								break;
							} // switch reqData.length
						} // if
					}); // on done

					req.error(function() { UnBlockUi(); });
				} // if
			} // if company targeting
		});
	}, // makeTargeting

	saveTargeting: function(targetingData) {
		if (!targetingData || targetingData.BusRefNum === 'skip')
			targetingData = { BusRefNum: 'NotFound' };

		$.post(window.gRootPath + 'Customer/Profile/SaveTargeting', targetingData);
	}, // saveTargeting

	render: function() {
		var state = this.customer.get('state');

		if (this.previousState == undefined)
			this.previousState = state;

		var data = this.model.toJSON();
		data.state = state;
		data.countDown = this.customer.offerValidFormatted();
		data.availableCredit = this.customer.get('CreditSum');
		data.offerStart = this.customer.get('OfferStart');
		data.creditResult = this.customer.get('CreditResult');

		this.$el.html(this.templates[state].template(data));

		this.$el.find('button').popover({ placement: 'top' });

		if (this.customer.get('IsAlibaba')) {
			var buttonText = (this.customer.get('SignedLegalID') > 0) ? 'Review credit line' : 'Sign agreement';

			this.$el.find('button.get-cash').text(buttonText);
			this.$el.find('button.choose-amount-wait').text(buttonText);
		} // if

		EzBob.UiAction.registerView(this);

		this.$el.find('.trustpilot-ezbob').hide();

		if (!(this.customer.get('ActiveLoans').length > 0)) {
			$('.apply-for-loan').removeClass('clean-btn').addClass('ev-btn-org');
			$('.choose-amount-wait').removeClass('clean-btn').addClass('ev-btn-org');
		}

		if (this.templates[state].name === 'apply') {
			var slidersEl = this.$el.find('.request-cash-sliders');
			this.slidersModel = new EzBob.SlidersModel({ Id: this.customer.get('Id') });
			this.sliders = new EzBob.SlidersView({ el: slidersEl, model: this.slidersModel, type: 'dashboardRequestLoan' });
			this.slidersModel.fetch();
		}

		return this;
	}, // render

	refreshTimer: function() {
		this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted() + " hrs");
	}, // refreshTimer
}); // EzBob.Profile.GetCashView

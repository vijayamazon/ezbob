var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.GetCashModel = Backbone.Model.extend({
	initialize: function(options) {
		this.customer = options.customer;

		this.isRequestInProgress = false;

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
			"get": _.template($('#d-getCash-template').html()),
			"apply": _.template($('#d-getCash-template-apply').html()),
			"wait": _.template($('#d-getCash-template-wait').html()),
			"disabled": _.template($('#d-getCash-template-wait').html()),
			//"bad": _.template($('#d-getCash-template-bad').html()),
			"bad": _.template($('#d-getCash-template-apply').html()),
			"late": _.template($('#d-getCash-template-late').html())
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
		UnBlockUi();

		if (this.customer.get('IsDefaultCustomerStatus'))
			return;

		if (this.customer.hasLateLoans())
			return;

		var sState = this.customer.get('state');

		if (sState !== 'apply' && sState !== 'bad' && sState !== 'disabled')
			return;

		if (this.customer.get('TrustPilotReviewEnabled') && this.customer.get("hasLoans") && this.customer.get('Origin') === 'ezbob') {
			var nTrustPilotStatusID = this.customer.get('TrustPilotStatusID');

			if (nTrustPilotStatusID === 0) {
				this.openTrustPilotDlg();
				return;
			} // if never left review
		} // if review enabled

		this.doApplyForALoan();
	}, // applyForALoan

	openTrustPilotDlg: function() {
		var self = this;

		this.$el.find('.trustpilot-ezbob').dialog({
			autoOpen: true,
			modal: true,
			width: 500,
			resizable: false,
			closeOnEscape: false,
			close: function(evt, ui) {
				$(this).dialog('destroy');
				self.$el.append(this);
			}, // on close
			open: function(evt, ui) {
				var me = $(this);

				$('.ui-dialog-titlebar', me.parent()).hide();

				$('a.trustpilot-rate', me).click(function() {
					me.dialog('close');
					window.open('http://www.trustpilot.com/evaluate/ezbob.com');
					self.doApplyForALoan(true);
				});

				$('a.trustpilot-skip', me).click(function() {
					me.dialog('close');
					self.doApplyForALoan();
				});

				$('*:focus', me).blur();
			}, // on open
		}); // dialog
	}, // openTrustPilotDlg

	doApplyForALoan: function(bClaims) {
		var that = this;

		this.makeTargeting();

		this.trigger('applyForLoan');

		BlockUi();

		if (bClaims)
			$.post(window.gRootPath + 'Customer/Profile/ClaimsTrustPilotReview');

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
			            onClosed: function() {
			                that.refreshAccount();
			            },
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
			.addClass('button btn-green')
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
			    backBtn: 'button btn-grey back',
				doneBtn: 'button btn-green',
			},
			clickBack: _.bind(this.backFromUploadFiles, this, false),
			clickDone: _.bind(this.backFromUploadFiles, this, true),
		});

		this.uploadUi.$el = $('#refresh-vat-return');

		this.uploadUi.$el.outerWidth(this.$el.outerWidth());
		this.uploadUi.$el.outerHeight($(window).height() * 0.75);

		this.uploadUi.render();

		this.$el.find('.refresh-account-help').colorbox({ href: '#refresh-vat-return', inline: true, open: true, onClosed: function() { $.colorbox.remove(); }, });
	}, // refreshVatReturn

	backFromUploadFiles: function(bRecheck) {
		$.colorbox.close();

		if (bRecheck)
			this.applyForALoan();
	}, // backFromUploadFiles

	refreshYodlee: function() {
		this.$el.find('#refreshYodleeBtn').attr('href', '' + window.gRootPath + 'Customer/YodleeMarketPlaces/RefreshYodlee');
		this.$el.find('.refresh-account-help').colorbox({ href: '#refresh_yodlee_help', inline: true, open: true, onClosed: function() { $.colorbox.remove(); }, });
	}, // refreshYodlee

	refreshEkm: function() {
		this.$el.find('#update-ekm-error').text(this.refreshAccountData.errorMsg);
		this.$el.find('#refresh_ekm_login').val(this.refreshAccountData.displayName).change();
		this.$el.find('#refresh_ekm_password').val('').change().focus();

		if (!this.ekmRefreshButtonEventSet) {
			this.$el.find('#refreshEkmBtn').click(_.bind(this.validateAndUpdateAccount, this, 'ekm'));
			this.ekmRefreshButtonEventSet = true;
		} // if

		this.$el.find('.refresh-account-help').colorbox({ href: '#refresh_ekm_help', inline: true, open: true, onClosed: function() { $.colorbox.remove(); }, });
	}, // refreshEkm

	refreshLinkedHmrc: function() {
		this.$el.find('#update-hmrc-error').text("Invalid username or password.");
		this.$el.find('#refresh_hmrc_login').val(this.refreshAccountData.displayName).change();
		this.$el.find('#refresh_hmrc_password').val('').change().focus();

		if (!this.hmrcRefreshButtonEventSet) {
			this.$el.find('#refreshHmrcBtn').click(_.bind(this.validateAndUpdateAccount, this, 'hmrc'));
			this.hmrcRefreshButtonEventSet = true;
		} // if

		this.$el.find('.refresh-account-help').colorbox({ href: '#refresh_hmrc_help', inline: true, open: true, onClosed: function() { $.colorbox.remove(); }, });
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

									companyTargets.on('BusRefNumGetted', function(targetingData) {
										that.saveTargeting(targetingData);
									});

									break;
							} // switch reqData.length
						} // if
					}); // on done

					req.always(function() {
						UnBlockUi();
					});
				} // if
			}
		});
	}, // makeTargeting

	saveTargeting: function(targetingData) {
		if (!targetingData || targetingData.BusRefNum == 'skip')
			targetingData = { BusRefNum: "NotFound" };

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

		this.$el.html(this.templates[state](data));

		this.$el.find('button').popover({ placement: 'top' });
	    
        if(this.customer.get("IsAlibaba")) {
            this.$el.find('button.get-cash').text('Get a loan');
            this.$el.find('button.choose-amount-wait').text('Get a loan');
        }

		EzBob.UiAction.registerView(this);

		this.$el.find('.trustpilot-ezbob').hide();

		return this;
	}, // render

	refreshTimer: function() {
		this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted() + " hrs");
	}, // refreshTimer
}); // EzBob.Profile.GetCashView

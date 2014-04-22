var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.GetCashModel = Backbone.Model.extend({
    initialize: function (options) {
        var that = this;
        this.customer = options.customer;

        this.isRequestInProgress = false;

        $.post(window.gRootPath + 'Customer/Profile/GetRefreshInterval')
            .done(function(result) {
                setInterval(function() { that.refresh(); }, result.Interval);
            });
    }, // initialize

    refresh: function () {
        var that = this;
        if (this.customer.get('state') === 'wait' && !that.isRequestInProgress) {
            $.post(window.gRootPath + 'Customer/Profile/GetCustomerStatus', { customerId: that.customer.get('Id') })
                .done(function(result) {
                    if (result.State !== 'wait') {
                        that.customer.fetch({
                            success: function() {
                                that.isRequestInProgress = false;
                            }
                        });
                        return;
                    }
                    that.isRequestInProgress = false;
                });
        } // if
    }, // refresh
}); // EzBob.Profile.GetCashModel

EzBob.Profile.GetCashView = Backbone.View.extend({
    className: "d-widget",

    initialize: function (options) {
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

        window.YodleeRefreshAccountRetry = function () {
            that.attemptsLeft = (that.attemptsLeft || 5) - 1;
            return {
                url: that.$el.find('#refreshYodleeBtn').attr('href'),
                attemptsLeft: that.attemptsLeft
            };
        }; // window.YodleeRefreshAccountRetry

        window.YodleeAccountUpdateError = function (msg) {
            $.colorbox.close();
            EzBob.App.trigger('error', msg);
        }; // window.YodleeAccountUpdateError
    }, // initialize

    events: {
        'click button.get-cash': 'getCash',
        'click button.apply-for-loan': 'applyForALoan',
    }, // events

    getCash: function () {
        if (this.customer.hasLateLoans())
            return;

        if (this.customer.get('state') !== 'get')
            return;

        window.location.href = "#GetCash";
    }, // getCash

    applyForALoan: function () {
        if (this.customer.get('CustomerStatusName') === 'Legal' || this.customer.get('CustomerStatusName') === 'Default')
            return;

        if (this.customer.hasLateLoans())
            return;

        var sState = this.customer.get('state');

        if (sState !== 'apply' && sState !== 'bad' && sState !== 'disabled')
            return;

        if (this.customer.get('TrustPilotReviewEnabled') && this.customer.get("hasLoans")) {
            var nTrustPilotStatusID = this.customer.get('TrustPilotStatusID');

            if (nTrustPilotStatusID === 0) {
                this.openTrustPilotDlg();
                return;
            } // if never left review
        } // if review enabled

        this.doApplyForALoan();
    }, // applyForALoan

    openTrustPilotDlg: function () {
        var self = this;

        this.$el.find('.trustpilot-ezbob').dialog({
            autoOpen: true,
            modal: true,
            width: 500,
            resizable: false,
            closeOnEscape: false,
            close: function (evt, ui) {
                $(this).dialog('destroy');
                self.$el.append(this);
            }, // on close
            open: function (evt, ui) {
                var me = $(this);

                $('.ui-dialog-titlebar', me.parent()).hide();

                $('a.trustpilot-rate', me).click(function () {
                    me.dialog('close');
                    window.open('http://www.trustpilot.com/evaluate/ezbob.com');
                    self.doApplyForALoan(true);
                });

                $('a.trustpilot-skip', me).click(function () {
                    me.dialog('close');
                    self.doApplyForALoan();
                });

                $('*:focus', me).blur();
            }, // on open
        }); // dialog
    }, // openTrustPilotDlg

    doApplyForALoan: function (bClaims) {
        var that = this;

        this.makeTargeting();
        
        this.trigger('applyForLoan');
        BlockUi('on');
        if (bClaims) {
            $.post(window.gRootPath + 'Customer/Profile/ClaimsTrustPilotReview');
        }

        $.post(window.gRootPath + 'Customer/Profile/ApplyForALoan')
            .done(function(result) {
                if (result.hasYodlee) {
                    var url = '' + window.gRootPath + 'Customer/YodleeMarketPlaces/RefreshYodlee';
                    that.$el.find('#refreshYodleeBtn').attr('href', url);
                    that.$el.find('.refresh_yodlee_help').colorbox({ href: '#refresh_yodlee_help', inline: true, open: true });
                } // if

                if (result.hasBadEkm) {
                    that.$el.find('#refresh_ekm_login').val(result.ekm).change();
                    that.$el.find('.refresh_ekm_help').colorbox({ href: '#refresh_ekm_help', inline: true, open: true });
                    return;
                } // if

                that.customer.set('state', 'wait');
            })
            .always(function() {
                BlockUi('off');
            });
    }, // doApplyForALoan

    makeTargeting: function() {
        var that = this;

        $.post(window.gRootPath + 'Customer/Profile/EntrepreneurTargeting')
            .done(function (result) {
                if (result.entrepreneurTargeting) {
                    var postcode, companyName, sCompanyFilter;
                    if (EzBob.Config.TargetsEnabledEntrepreneur) {
                        console.log(that.model);
                        postcode = that.model.get('customer').get('PersonalAddress').models[0].get('Postcode');
                        companyName = (function () {
                            var cpi = that.model.get('customer').get('CustomerPersonalInfo');
                            return cpi.FirstName + ' ' + cpi.Surname;
                        })(this.model);
                        sCompanyFilter = 'N';
                        var req = $.get(window.gRootPath + 'Account/CheckingCompany', { companyName: companyName, postcode: postcode, filter: sCompanyFilter, refNum: '' });
                        scrollTop();
                        BlockUi();

                        req.done(function (reqData) {
                            if (reqData) {
                                switch (reqData.length) {
                                    case 0: break;
                                    case 1: that.saveTargeting(reqData[0]); break;
                                    default:
                                        var companyTargets = new EzBob.companyTargets({ model: reqData });
                                        companyTargets.render();
                                        companyTargets.on('BusRefNumGetted', function (targetingData) {
                                            that.saveTargeting(targetingData);
                                        });
                                        break;
                                } // switch reqData.length
                            } // if
                        }); // on done

                        req.always(function () {
                            UnBlockUi();
                        });
                    } // if
                }
            });
    },
    saveTargeting: function(targetingData) {
        if (!targetingData) {
            targetingData = { BusRefNum: "NotFound" };
        }
        $.post(window.gRootPath + 'Customer/Profile/SaveTargeting', targetingData);
    },
    
    render: function () {
        var state = this.customer.get('state');
        var data = this.model.toJSON();
        data.state = state;
        data.countDown = this.customer.offerValidFormatted();
        data.availableCredit = this.customer.get('CreditSum');
        data.offerStart = this.customer.get('OfferStart');
        data.creditResult = this.customer.get('CreditResult');

        this.$el.html(this.templates[state](data));
        this.$el.find('button').popover({ placement: 'top' });
        EzBob.UiAction.registerView(this);
        this.$el.find('.trustpilot-ezbob').hide();
        return this;
    }, // render

    refreshTimer: function () {
        this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted() + " hrs");
    }, // refreshTimer
}); // EzBob.Profile.GetCashView

﻿var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.GetCashModel = Backbone.Model.extend({
    initialize: function (options) {
        var that = this;
        this.customer = options.customer;

        this.isRequestInProgress = false;

        setInterval(function () {
            that.refresh();
        }, 1000);
    },
    refresh: function () {
        var that = this;
        var tempCustomer = null;

        if (this.customer.get('state') == 'wait' && !that.isRequestInProgress) {
            tempCustomer = new EzBob.CustomerModel();
            tempCustomer.id = that.customer.id;
            this.isRequestInProgress = true;
            tempCustomer.fetch({ success: function () {
                if (tempCustomer.get('state') != 'wait') {
                    that.customer.fetch({success:function () {
                        that.isRequestInProgress = false;
                    }});
                    return;
                }
                that.isRequestInProgress = false;
            }
            });
        }
    }
});

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
        };
        window.YodleeAccountUpdateError = function (msg) {
            $.colorbox.close();
            EzBob.App.trigger('error', msg);
        };
    },
    events: {
        'click button.get-cash': 'getCash',
        'click button.apply-for-loan': 'applyForALoan',
    },
    getCash: function () {
        if (this.customer.hasLateLoans()) return;
        if (this.customer.get('state') != 'get') return;
        
        window.location.href = "#GetCash";
    },
    applyForALoan: function () {
        var that = this;

        if (this.customer.get('CustomerStatusName') === 'Legal' || this.customer.get('CustomerStatusName') === 'Default') return;
        if (this.customer.hasLateLoans()) return;
        if (this.customer.get('state') != 'apply' && this.customer.get('state') != 'bad' && this.customer.get('state') != 'disabled') return;
        
        this.trigger('applyForLoan');
        BlockUi('on');
        $.post(window.gRootPath + "Customer/Profile/ApplyForALoan")
            .done(function (result) {
                if (result.hasYodlee) {
                    var url = "" + window.gRootPath +"Customer/YodleeMarketPlaces/RefreshYodlee";
                    that.$el.find("#refreshYodleeBtn").attr("href", url);
                    that.$el.find('.refresh_yodlee_help').colorbox({ href: "#refresh_yodlee_help", inline: true, transition: 'none', open: true });
                }
                if (result.hasBadEkm) {
                    that.$el.find('#refresh_ekm_login').val(result.ekm).change();
                    that.$el.find('.refresh_ekm_help').colorbox({ href: "#refresh_ekm_help", inline: true, transition: 'none', open: true });
                    return;
                }
                that.customer.set('state', 'wait');
            })
            .always(function () {
                BlockUi('off');
            });
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
        
        return this;
    },
    refreshTimer: function () {
        this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted() + " hrs");
    }
});
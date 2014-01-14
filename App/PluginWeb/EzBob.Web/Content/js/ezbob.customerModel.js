﻿var EzBob = EzBob || {};

EzBob.LoanModel = Backbone.Model.extend({
    defaults: {
    },
    idAttribute: 'Id'
});

EzBob.Loans = Backbone.Collection.extend({
    model: EzBob.LoanModel
});

EzBob.RolloverModel = Backbone.Model.extend({

});

EzBob.Rollovers = Backbone.Collection.extend({
    model: EzBob.RolloverModel
});

EzBob.CustomerModel = Backbone.Model.extend({
    initialize: function () {
        this.init();
    },
    init: function () {

        this.set('Loans', new EzBob.Loans(this.get('Loans')));

        this.set('PersonalAddress', new EzBob.AddressModels(this.get('PersonalAddress')));
        this.set('PrevPersonAddresses', new EzBob.AddressModels(this.get('PrevPersonAddresses')));
        this.set('OtherPropertyAddress', new EzBob.AddressModels(this.get('OtherPropertyAddress')));
        this.set('CompanyAddress', new EzBob.AddressModels(this.get('CompanyAddress')));

        //this.set('LimitedDirectors', new EzBob.Directors(this.get('LimitedDirectors')));
        //this.set('NonLimitedDirectors', new EzBob.Directors(this.get('NonLimitedDirectors')));

        this.set('hasLateLoans', this.hasLateLoans());
        this.set('hasLoans', this.get('Loans').length > 0);
        this.set('ActiveLoans', this.getActiveLoans());

        this.set('ActiveRollovers', new EzBob.Rollovers(this.get('ActiveRollovers')));

        this.calculateState();
    },

    fetch: function (options) {
        var that = this;
        return this.constructor.__super__.fetch.call(this, {
            success: function () {
                that.init();
                that.trigger('fetch');
                if (options && options.success) {
                    options.success.call();
                }
            }
        });
    },

    url: function () {
        return window.gRootPath + "Customer/Profile/Details";
    },
    hasLateLoans: function () {
        var loans = this.get('Loans');
        if (!loans || !loans.any) return false;
        var r = loans.filter(function (loan) { return loan.get('Status') == "Late"; });

        var due = _.reduce(r, function (memo, value) {
            return memo + value.get('Late');
        }, 0);

        this.set({ 'TotalDue': due, 'LateLoans': r.length });

        return r.length > 0;
    },
    getActiveLoans: function () {
        var loans = this.get('Loans');
        var liveLoans = loans.filter(function (l) {
            var status = l.get('Status');
            return status == "Live" || status == "Late" || status == "Collection" || status == "Legal";
        });
        return liveLoans;
    },
	serverTimeZone: function() {
		var ary = $('#current-server-time-zone').text().match(/^([+-])(\d\d):(\d\d)$/);

		if (!ary)
			return 0;

		var sSign = ary[1];
		var nHours = parseInt(ary[2]);
		var nMinutes = parseInt(ary[3]);

		var nOffset = nHours * 60 + nMinutes;

		if ((sSign == '+') && (nOffset > 0))
			nOffset = -nOffset;

		return nOffset;
	},
    offerValid: function () {
        var offerStart = this.get('OfferStart'),
            offerValidUntil = this.get('OfferValidUntil');

        if (!offerStart || !offerValidUntil) return { S: 0, H: 0, M: 0, TotalSeconds: 0 };

        var start = moment(offerStart);
        var end = moment(offerValidUntil);

        var now = moment(new Date());
        now = moment(now - now.zone() + this.serverTimeZone());

        if (start > now && start < end) return { S: 0, H: 0, M: 0, TotalSeconds: 0, NotStarted: true};
        if (now > end) return { S: 0, H: 0, M: 0, TotalSeconds: 0, Expired : true };

        var seconds = Math.max(0, end.diff(now, 'seconds')),
            H = Math.max(0, Math.floor(seconds / (60 * 60))),
            M = Math.max(0, Math.floor((seconds / (60)) % (60))),
            S = Math.max(0, seconds % 60);
        return { S: S, H: H, M: M, TotalSeconds: seconds };
    },
    offerValidFormatted: function () {
        var offer = this.offerValid();
        return "" + offer.H + ":" + EzBob.formatNumberLength(offer.M, 2) + ":" + EzBob.formatNumberLength(offer.S, 2);
    },
    calculateState: function () {
        var availableCredit = this.get('CreditSum'),
            creditResult = this.get('CreditResult'),
            status = this.get('Status'),
            offerValid = this.offerValid(),
            hasFunds = availableCredit >= EzBob.Config.XMinLoan,
            isDisabled = this.get('IsDisabled');
        
        if (isDisabled) {
            this.set('state', 'disabled');
            return;
        }

        if (this.hasLateLoans()) {
            this.set('state', 'late');
            return;
        }

        if (!creditResult || creditResult == 'WaitingForDecision') {
            this.set('state', 'wait');
            return;
        }
        
        if (status == 'Rejected') {
            this.set('state', 'bad');
            return;
        }
        
        if (status == 'Manual') {
            this.set('state', 'wait');
            return;
        }

        if (hasFunds && offerValid.TotalSeconds > 0 && (status == 'Approved') ) {
            this.set('state', 'get');
            return;
        }
        
        if (hasFunds && offerValid.NotStarted && status == 'Approved') {
            this.set('state', 'wait');
            return;
        }

        if (!hasFunds || offerValid.TotalSeconds <= 0) {
            this.set('state', 'apply');
            return;
        }
    }
});

EzBob.AddressModel = Backbone.Model.extend({
    defaults: {
        Found: "",
        Credits_display_text: "",
        Accountadminpage: "",
        Errormessage: "",
        Id: "",
        Organisation: "",
        Line1: "",
        Line2: "",
        Line3: "",
        Town: "",
        County: "",
        Postcode: "",
        Country: "",
        Rawpostcode: "",
        Deliveryposuffix: "",
        Nohouseholds: "",
        Smallorg: "",
        Pobox: "",
        Mailsortcod: "",
        Udprn: ""
    },

    url: function () {
        var rootPath = window.gRootPath;
        if (window.gRootPath == undefined) {
            console.warn("window.gRootPath is not initialized!");
            rootPath = "/";
        }
        return rootPath+"Postcode/GetFullAddressFromPostCode?id=" + this.get('id');
    },

    initialize: function () {

    }
});
//-----------------------------------------------------------------------------
EzBob.AddressModels = Backbone.Collection.extend({
    addresses: EzBob.AddressModel
});
//-----------------------------------------------------------------------------
EzBob.DirectorModel = Backbone.Model.extend({
    initialize: function () {
        this.set('DirectorAddress', new EzBob.AddressModels(this.get('DirectorAddress')));
    }
});

EzBob.Directors = Backbone.Collection.extend({ model: EzBob.DirectorModel });
//-----------------------------------------------------------------------------
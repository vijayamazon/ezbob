var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ProfileRouter = Backbone.Router.extend({
    initialize: function (options) {
        this.widgets = {};
        this.menu = $('.profile-menu');
        this.profileWidgets = $('.profile-widgets');
        
        var that = this;
        this.model = options;
        this.stores = new EzBob.Profile.StoresView({ model: options });

        this.accounts = new EzBob.Profile.PaymentAccountsView({ model: options });

        this.accountActivityView = new EzBob.Profile.AccountActivityView({ model: options });
        
        this.perksView = new EzBob.Profile.PerksView();
        
        this.companyDirectorsView = new EzBob.Profile.CompanyDirectorsView({ model: options });

        var accountSettings = new EzBob.Profile.AccountSettingsModel(options.get('AccountSettings'));
        accountSettings.set('userName', options.get('userName'));
        this.accountSettingsView = new EzBob.Profile.SettingsMasterView({ model: accountSettings });

        this.YourDetails = new EzBob.Profile.YourInfoMainView({ model: options });

        this.widgets.PaymentAccounts = this.accounts;
        this.widgets.YourStores = this.stores;
        this.widgets.AccountActivity = this.accountActivityView;
        this.widgets.Settings = this.accountSettingsView;
        this.widgets.YourDetails = this.YourDetails;
        this.widgets.Perks = this.perksView;
        this.widgets.CompanyDirectors = this.companyDirectorsView;

        _.each(this.widgets, function (w) {
            w.render().$el.hide().appendTo(that.profileWidgets);
        });

        if (EzBob.Config.ShowChangePasswordPage) {
            this.settings();
            this.widgets.Settings.editPassword();
        }

        EzBob.App.on('ct:profile:show', this.ctNavigate, this);

        EzBob.App.on('ct:profile:payEarly', this.payEarly, this);
        EzBob.App.on('ct:profile:getCash', this.getCash, this);
        EzBob.App.on('ct:profile:loanDetails', this.loanDetails, this);
        EzBob.App.on('ct:profile:turnover', this.turnover, this);

        this.on('all', this.storeRoute, this);
        this.previousViews = [];
    },

    storeRoute: function () {
        this.previousViews.push(Backbone.history.fragment);
    },

    previous: function () {
        if (this.previousViews.length > 1) {
            this.navigate(this.previousViews[this.previousViews.length - 3], true);
        }
    },

    ctNavigate: function (name) {
        //this.navigate(name, { trigger: true });
        var handler = this.routes[name];
        if (handler) {
            this[handler]();
        }
    },
    activate: function (name) {
        if (!this.widgets[name]) {
            console.warn('no such widget!');
            return;
        }

        EzBob.CT.recordEvent('ct:profile:show', name);

        EzBob.App.GA.trackPage('/Customer/Profile/' + name);

        this.profileWidgets.children().hide();
        this.widgets[name].$el.show();
        this.menu.find('.nav li.active').removeClass('active');
        this.menu.find('.nav li:has(a[href="#' + name + '"])').addClass('active');
        this.trigger('menuWidgetShown');
        this.marketing(name);
    },

    routes: {
        "": "decide",
        "AccountActivity": "accountActivity",
        "YourDetails": "YourDetails",
        "YourStores": "yourStores",
        "PaymentAccounts": "paymentAccounts",
        "Settings": "settings",
        "GetCash": "getCash",
        "PayEarly/:id": "payEarly",
        "PayEarly": "payEarly",
        "LoanDetails/:id": "loanDetails",
        "Perks": "perks",
        "CompanyDirectors": "companyDirectors",
        "Turnover": "turnover",
        "Turnover/:id": "turnover",
    },
    decide: function () {
        // Workaround for non-standard navigation
        var isInHmrcUpload = $("#uploadFilesDiv").attr('style') == "display: block;";
        var isInHmrcLink = $("#linkAccountDiv").attr('style') == "display: block;";
        if (isInHmrcUpload || isInHmrcLink) {
            return false;
        }

        if (this.model.get('hasLoans')) {
            this.activate("AccountActivity");
        } else {
            this.activate("YourStores");
        }
    },
    accountActivity: function () {
    	this.activate("AccountActivity");
    	this.removeSteps('accountActivity');
    },
    YourDetails: function () {
    	this.activate("YourDetails");
    	this.removeSteps('YourDetails');
    },
    yourStores: function () {
    	this.activate("YourStores");
    	this.removeSteps('YourStores');
    },
    paymentAccounts: function () {
    	this.activate("PaymentAccounts");
    	this.removeSteps('PaymentAccounts');
    },
    settings: function () {
        this.activate("Settings");
        this.marketing("Settings");
        this.removeSteps('Settings');
    },
    perks: function() {
    	this.activate("Perks");
    	this.removeSteps('Perks');
    },
    getCash: function () {
        EzBob.CT.recordEvent('ct:profile:getCash');
        this.trigger('getCash');
        this.marketing("GetCash");
    },
    payEarly: function (id) {
        EzBob.CT.recordEvent('ct:profile:payEarly', id);
        this.trigger('payEarly', id);
        this.marketing("PayEarly");
        this.removeSteps('PayEarly');
    },
    loanDetails: function (id) {
        EzBob.CT.recordEvent('ct:profile:loanDetails', id);
        this.trigger('details', id);
        this.marketing("LoanDetails");
        this.removeSteps('LoanDetails');
    },
    turnover: function (id) {
    	EzBob.CT.recordEvent('ct:profile:turnover', id);
    	this.trigger('turnover', id);
    	this.marketing("Turnover");
    	this.removeSteps('Turnover');
    },
    companyDirectors: function() {
    	this.activate("CompanyDirectors");
    	this.removeSteps('CompanyDirectors');
    },
    removeSteps: function(caller) {
		$('.dashboard-steps-container').empty();
	},
    marketing: function (page) {
        var marketing;
        var isDashboard = true;
        switch (page) {
            case "AccountActivity":
                marketing = EzBob.dbStrings.MarketingDashboardAccountActivity;
                break;
            case "YourDetails":
                marketing = EzBob.dbStrings.MarketingDashboardYourDetails;
                break;
            case "YourStores":
                marketing = EzBob.dbStrings.MarketingDashboardYourStores;
                break;
            case "PaymentAccounts":
                marketing = EzBob.dbStrings.MarketingDashboardPaymentAccounts;
                break;
            case "PayEarly":
                marketing = EzBob.dbStrings.MarketingDashboardPayEarly;
                break;
            case "Settings":
                marketing = EzBob.dbStrings.MarketingDashboardSettings;
                break;
            case "LoanDetails":
                marketing = EzBob.dbStrings.MarketingDashboardLoanDetails;
                break;
            case "GetCash":
                marketing = EzBob.dbStrings.MarketingDashboardGetCash;
                break;
            default:
                isDashboard = false;
                marketing = EzBob.dbStrings.MarketingDefault;
                break;
        }
        if (isDashboard && marketing) {
            $("#defaultMarketing").hide();
            $("#marketingProggress").show().html(marketing);
        } else {
            $("#defaultMarketing").show();
            $("#marketingProggress").hide().html(marketing);
        }
    }
});
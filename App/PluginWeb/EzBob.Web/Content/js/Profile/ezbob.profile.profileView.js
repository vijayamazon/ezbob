var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ProfileView = Backbone.View.extend({
    initialize: function (options) {
        if (typeof orddb == 'undefined') { orddb = Math.random() * 10000000000000000; }
        var profileTemplate = $(_.template($("#profile-main-template").html(), { orddb: orddb }));
        $('#profile-main-to-be-replaced').html(profileTemplate);
        this.profileMain = $('#profile-main');

        this.customer = options;
        this.payEarly = new EzBob.Profile.PayEarlyView({ model: options });

        this.getCashModel = new EzBob.Profile.GetCashModel({ customer: options });
        this.getCashView = new EzBob.Profile.GetCashView({ model: this.getCashModel, customer: options });

        this.signWidget = new EzBob.Profile.SignWidget({ customerModel: options });
        this.signWidget.on('payEarly', this.makePayment, this);

        this.processingMessageView = new EzBob.Profile.ProccessingMessageView({ model: options });
        this.processingMessageContainer = $('.proccessing-message').empty();

        this.widgetsContainer = $('.d-widgets').empty();

        this.getCashRegion = new Backbone.Marionette.Region({
            el: "#get-cash"
        });

        this.signContainer = $("#message-sign");

        this.processingMessageView.on('getCash', this.getCash, this);
        this.processingMessageView.on('payEarly', this.makePayment, this);

        this.router = new EzBob.Profile.ProfileRouter(options);
        this.router.on("details", this.loanDetails, this);
        this.router.on("getCash", this.getCash, this);
        this.router.on("payEarly", this.makePayment, this);
        this.router.on("menuWidgetShown", this.menuWidgetShown, this);
    },

    render: function () {
        this.profileMain.show();

        this.payEarly.render();
        this.getCashView.render();
        this.signWidget.render();
        this.processingMessageView.render();

        this.getCashView.$el.appendTo(this.widgetsContainer);
        this.payEarly.$el.appendTo(this.widgetsContainer);
        this.signWidget.$el.appendTo(this.signContainer);
        this.processingMessageView.$el.appendTo(this.processingMessageContainer);
        /*
        if (this.customer.get('state') == 'get' && !this.customer.get('hasLoans')) {
            window.location.href = "#GetCash";
        }
        */
        if (this.customer.get('hasLoans')) {
            window.location.href = "#AccountActivity";
        }
        return this;
    },
    loanDetails: function (id) {

        EzBob.App.GA.trackPage('/Customer/Profile/LoanDetails');

        var loan = new EzBob.Profile.LoanModel();
        loan.loanId = id;
        loan.fetch();

        var loanDetailView = new EzBob.Profile.LoanDetailsView({ model: loan, customer: this.customer });
        loanDetailView.render();

        this.profileMain.hide();
        this.getCashRegion.show(loanDetailView);
        this.marketing("LoanDetails");
    },
    getCash: function () {
        $(document).attr("title", "Get Cash: Select Loan Amount | EZBOB");
        EzBob.App.GA.trackPage('/Customer/Profile/GetCash');

        var applyForLoanView = new EzBob.Profile.ApplyForLoanTopView({ customer: this.customer, model: new EzBob.Profile.ApplyForLoanTopViewModel() });

        applyForLoanView.on('back', this.applyForLoanBack, this);
        applyForLoanView.on('submit', this.applyForLoanSubmit, this);

        this.getCashRegion.show(applyForLoanView);
        this.profileMain.hide();
        this.marketing("GetCash");
    },
    menuWidgetShown: function () {
        $(document).attr("title", "Dashboard: User Dashboard | EZBOB");
        this.getCashRegion.close();
        this.profileMain.show();
        scrollTop();
        this.marketing("Dashboard");
    },
    applyForLoanBack: function () {
        this.router.navigate("");
        this.menuWidgetShown();
    },

    payErlyBack: function () {
        this.router.previous();
    },

    applyForLoanSubmit: function (creditSum) {
        this.applyForLoanBack();
        this.getCashModel.set('availableCredit', this.getCashModel.get('availableCredit') - creditSum);
    },
    makePayment: function (id) {
        var title = (this.customer.get("hasLateLoans") ? "Pay Late:" : "Pay Early:") + " User Payment | EZBOB";
        $(document).attr("title", title);

        EzBob.App.GA.trackPage('/Customer/Profile/MakePayment');

        var payEarlyView = new EzBob.Profile.MakeEarlyPayment({ el: this.payEarlyDiv, customerModel: this.customer, loanId: id });
        payEarlyView.on('submit back', this.payErlyBack, this);

        this.getCashRegion.show(payEarlyView);
        this.profileMain.hide();
        this.marketing("MakePayment");
    },

    marketing: function (page) {
        var marketing;
        var isDashboard = true;
        switch (page) {
            case "Dashboard":
                marketing = EzBob.dbStrings.MarketingDashboard;
                break;
            case "MakePayment":
                marketing = EzBob.dbStrings.MarketingDashboardMakePayment;
                break;
            case "GetCash":
                marketing = EzBob.dbStrings.MarketingDashboardGetCash;
                break;
            case "LoanDetails":
                marketing = EzBob.dbStrings.MarketingDashboardLoanDetails;
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

EzBob.Profile.ProfileRouter = Backbone.Router.extend({
    initialize: function (options) {
        this.widgets = {};
        this.menu = $('.profile-menu');
        this.profileWidgets = $('.profile-widgets');
        
        var that = this;

        this.stores = new EzBob.Profile.StoresView({ model: options });

        this.accounts = new EzBob.Profile.PaymentAccountsView({ model: options });

        this.accountActivityView = new EzBob.Profile.AccountActivityView({ model: options });
        
        this.inviteFriendView = new EzBob.Profile.InviteFriendView({ model: options });

        this.perksView = new EzBob.Profile.PerksView();
        //this.accountSummary = new EzBob.Profile.AccountSummaryView({ model: options });

        var accountSettings = new EzBob.Profile.AccountSettingsModel(options.get('AccountSettings'));
        accountSettings.set('userName', options.get('userName'));
        this.accountSettingsView = new EzBob.Profile.SettingsMasterView({ model: accountSettings });

        this.YourDetails = new EzBob.Profile.YourInfoMainView({ model: options });

        //this.widgets.AccountSummary = this.accountSummary;
        this.widgets.PaymentAccounts = this.accounts;
        this.widgets.YourStores = this.stores;
        this.widgets.AccountActivity = this.accountActivityView;
        this.widgets.Settings = this.accountSettingsView;
        this.widgets.YourDetails = this.YourDetails;
        this.widgets.InviteFriend = this.inviteFriendView;
        this.widgets.Perks = this.perksView;

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
        //"AccountSummary": "accountSummary",
        "": "yourStores",
        "AccountActivity": "accountActivity",
        "YourDetails": "YourDetails",
        "YourStores": "yourStores",
        "PaymentAccounts": "paymentAccounts",
        "Settings": "settings",
        "GetCash": "getCash",
        "PayEarly/:id": "payEarly",
        "PayEarly": "payEarly",
        "LoanDetails/:id": "loanDetails",
        "InviteFriend": "inviteFriend",
        "Perks":"perks"
    },
    //    accountSummary: function () {
    //        this.activate("AccountSummary");
    //    },
    accountActivity: function () {
        this.activate("AccountActivity");
    },
    YourDetails: function () {
        this.activate("YourDetails");
    },
    yourStores: function () {
        this.activate("YourStores");
    },
    paymentAccounts: function () {
        this.activate("PaymentAccounts");
    },
    settings: function () {
        this.activate("Settings");
        this.marketing("Settings");
    },
    inviteFriend: function() {
        this.activate("InviteFriend");
    },
    perks: function() {
        this.activate("Perks");
    },
    getCash: function () {
        EzBob.CT.recordEvent('ct:profile:getCash');
        EzBob.App.GA.trackEvent('Loan Approved', 'Choose Amount', 'Request Cash');
        this.trigger('getCash');
        this.marketing("GetCash");
    },
    payEarly: function (id) {
        EzBob.CT.recordEvent('ct:profile:payEarly', id);
        this.trigger('payEarly', id);
        this.marketing("PayEarly");
    },
    loanDetails: function (id) {
        EzBob.CT.recordEvent('ct:profile:loanDetails', id);
        this.trigger('details', id);
        this.marketing("LoanDetails");
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
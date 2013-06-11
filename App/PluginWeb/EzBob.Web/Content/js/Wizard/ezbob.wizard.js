///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/controls/ezbob.notifications.js" />
var EzBob = EzBob || {};

EzBob.WizardRouter = Backbone.Router.extend({
    initialize: function (options) {
        this.topNavigationEnabled = options.topNavigationEnabled;
        this.maxStepNum = options.maxStepNum;
    },
    routes: {
        "": "SignUp",
        "SignUp": "SignUp",
        "ShopInfo": "ShopInfo",
        //"PaymentAccounts": "PaymentAccounts",
        "YourDetails": "YourDetails"
    },
    defaultRoute: function () {
        this.activate("SignUp");
    },
    SignUp: function () {
        this.trigger("SignUp");
    },
    ShopInfo: function () {
        if (!this.topNavigationEnabled) {
            if (this.maxStepNum >= 1) {
                this.trigger("ShopInfo");
            } else {
                this.navTo(this.maxStepNum);
            }
        } else {
            this.trigger("ShopInfo");
        }
    },
    //PaymentAccounts: function () {
    //    if (!this.topNavigationEnabled) {
    //        if (this.maxStepNum >= 2) {
    //            this.trigger("PaymentAccounts");
    //        } else {
    //            this.navTo(this.maxStepNum);
    //        }
    //    } else {
    //        this.trigger("PaymentAccounts");
    //    }
    //},
    YourDetails: function () {
        if (!this.topNavigationEnabled) {
            if (this.maxStepNum >= 2) {
                this.trigger("YourDetails");
            } else {
                this.navTo(this.maxStepNum);
            }
        } else {
            this.trigger("YourDetails");
        }
    },
    navTo: function (i) {
        if (!this.topNavigationEnabled) {
            i = i > this.maxStepNum ? this.maxStepNum : i;
        }
        switch (i) {
            case 0:
                $(document).attr("title", "Step 1: Sign Up | EZBOB");
                EzBob.App.GA.trackPage('/Customer/Wizard/SignUp');
                EzBob.App.trigger("wizard:progress", 0);
                this.navigate("SignUp", { trigger: true });
                break;
            case 1:
                $(document).attr("title", "Step 2: Link Your Accounts | EZBOB");
                EzBob.App.GA.trackPage('/Customer/Wizard/Shops');
                EzBob.App.trigger("wizard:progress", 30);
                this.navigate("ShopInfo", { trigger: true });
                break;
                //case 2:
                //    $(document).attr("title", "Wizard 3: Link Your Payment Accounts | EZBOB");
                //    EzBob.App.GA.trackPage('/Customer/Wizard/PaymentAccounts');
                //    this.navigate("PaymentAccounts", { trigger: true });
                //    break;
            case 2:
                $(document).attr("title", "Step 3: Fill Personal Details | EZBOB ");
                EzBob.App.GA.trackPage('/Customer/Wizard/PersonalDetails');
                EzBob.App.trigger("wizard:progress", 70);
                this.navigate("YourDetails", { trigger: true });
                break;
            default:
        }
    }
});


EzBob.Wizard = Backbone.View.extend({
    initialize: function (options) {
        this.progress = 0;
        this.topNavigationEnabled = EzBob.Config.WizardTopNaviagtionEnabled;
        this.template = _.template($('#wizard-template').html());
        this.progressTemplate = _.template($('#progress-indicator').html());
        this.model = options.model;
        this.steps = options.steps;
        this.stepModels = this.model.get('stepModels');
        this.model.on('change', this.stepChanged, this);
        var that = this;

        _.each(this.steps, function (s, i) {
            s.view.on('ready', _.bind(that.ready, that, i));
            s.view.on('next', that.next, that);
            s.view.on('previous', that.previous, that);
            s.view.on('linkClick', that.linkClick, that);
        });
        
        EzBob.App.on('wizard:progress', that.progressChanged, that);

        this.router = new EzBob.WizardRouter({ topNavigationEnabled: this.topNavigationEnabled, maxStepNum: this.model.get("ready") != undefined ? this.model.get("ready").clean(undefined).length : 0 });
        this.router.on("SignUp", this.SignUpRoute, this);
        this.router.on("ShopInfo", this.ShopInfoRoute, this);
        //this.router.on("PaymentAccounts", this.PaymentAccountsRoute, this);
        this.router.on("YourDetails", this.YourDetailsRoute, this);
        Backbone.history.start();
    },
    SignUpRoute: function () {
        this.model.changePage(0);
    },
    ShopInfoRoute: function () {
        this.model.changePage(1);
        this.steps[1].view.StoreInfoView.back();
    },
    //PaymentAccountsRoute: function () {
    //    this.model.changePage(2);
    //},
    YourDetailsRoute: function () {
        this.model.changePage(2);
    },
    render: function () {
        var template = this.template();
        this.$el.html(template);
        
        this.renderSteps();
        this.stepChanged();

        var notifications = new EzBob.NotificationsView({ el: this.$el.find('.notifications') });
        notifications.render();

        if (!this.topNavigationEnabled) {
            this.$el.find('.wizard-steps > ul li').css('cursor', 'default');
        }

        this.router.navTo(this.model.get("ready") != undefined ? this.model.get("ready").clean(undefined).length : 0);

        return this;
    },
    events: {
        "click .wizard-steps > ul li": "linkClick"
    },
    linkClick: function (e) {
        if (!EzBob.Config.WizardTopNaviagtionEnabled) return false;
        var allowed = this.model.get('allowed');
        var newCurrent = $(e.currentTarget).data('step-num');
        if (newCurrent > allowed) return false;
        this.router.navTo(newCurrent);
        return false;
    },
    addStep: function (title, view) {
    	var num = this.steps.length;
    	this.steps.push({ num: num++, title: title, view: view });
    },
    ready: function (num) {
        var ready = this.model.get("ready") || new Array(this.model.get("total") + 1);
        ready[num] = true;
        for (var i = 0; i < ready.length; i++) {
            if (!ready[i]) break;
        }
        i = Math.max(i, this.model.get('allowed'));
        this.model.set({ "ready": ready, "allowed": i });

        if (!this.steps[num].ready) {
            this.steps[num].ready = true;
        }

        this.router.maxStepNum = this.model.get("ready") != undefined ? this.model.get("ready").clean(undefined).length : 0;
        
        this.stepChanged();
    },

    next: function () {
        scrollTop();
        var current = this.model.get('current'),
            allowed = this.model.get('allowed'),
            total = this.model.get('total');
        if (total == current - 1) {
            this.model.trigger('completed');
        }
        if (current == allowed) {
            return;
        }
        this.model.set('current', ++current);
        this.router.navTo(current);
    },

    previous: function () {
        scrollTop();

        var current = this.model.get('current');

        if (current == 0) {
            return;
        }
        this.model.set('current', --current);
        this.router.navTo(current);
    },
    renderSteps: function () {
        var ul = this.$el.find('.pages');
        _.each(this.steps, function (s) {
            var view = s.view.render();

            view.$el.hide().appendTo(ul);
            view.$el.find(".chzn-select").chosen({ disable_search_threshold: 10 });
            if (view.$el.find('#captcha').length > 0) {
                view.captcha = new EzBob.Captcha({ elementId: "captcha", tabindex: 6 });
                view.captcha.render();
            }
        });
    },
    stepChanged: function () {
        var current = this.model.get('current'),
            allowed = this.model.get('allowed');

        var data = {
            steps: this.steps,
            current: current,
            progress: this.progress
        };

        var marketing;
        var isWizard = true;
        switch (this.progress) {
            case 0:
                marketing = EzBob.dbStrings.MarketingWizardStep1;
                break;
            case 30:
                marketing = EzBob.dbStrings.MarketingWizardStep2;
                break;
            case 70:
                marketing = EzBob.dbStrings.MarketingWizardStep3;
                break;
            case 90:
                marketing = EzBob.dbStrings.MarketingWizardStep35;
                break;
            case 100:
                marketing = EzBob.dbStrings.MarketingWizardStep4;
                break;
            default:
                isWizard = false;
                marketing = EzBob.dbStrings.MarketingDefault;
                break;
        }
        if (isWizard) {
            this.$el.find("#defaultMarketing").hide();
            this.$el.find("#marketingProggress").show().html(marketing);
        } else {
            this.$el.find("#defaultMarketing").show();
            this.$el.find("#marketingProggress").hide().html(marketing);
        }
        
        this.$el.find(".wizard-progress").html(this.progressTemplate(data));

        this.$el.find('.pages > div').hide().eq(current).show();
        if (this.steps[current]) this.$el.find('.wizard-header').text(this.steps[current].header);
    },
    progressChanged: function(progress) {
        this.progress = progress;
        this.stepChanged();
    }
});

EzBob.WizardStepModel = Backbone.Model.extend({
    defaults: {
        completed: false
    }
});

EzBob.WizardSteps = Backbone.Collection.extend({
    model: EzBob.WizardStepModel
});

EzBob.WizardModel = Backbone.Model.extend({
    defaults: {
        current: 0,
        allowed: 0
    },
    initialize: function () {
        EzBob.App.on('ct:wizard_page', this.changePage, this);
        this.on('change current', this.currentChanged, this);
    },
    changePage: function (i) {
        var allowed = this.get('allowed');
        this.set({
            current: i,
            allowed: Math.max(i, allowed)
        });
    },
    currentChanged: function () {
        EzBob.CT.recordEvent('ct:wizard_page', this.get('current'));
    }
});
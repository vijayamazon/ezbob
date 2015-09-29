var EzBob = EzBob || {};

EzBob.WizardRouter = Backbone.Router.extend({
    initialize: function (options) {
        this.stepList = options.stepList;

        for (var i = 0; i < this.stepList.length; i++) {
            var s = this.stepList[i];
            this.route(s.name, s.name);
        } // for
    }, // initialize

    navTo: function (i) {
        var oStep = this.stepList[i];
        this.navigate(oStep.name);
        this.trigger(oStep.name);
    } // navTo
}); // EzBob.WizardRouter

function HeartOfActivity() {
    var sessionTimeout = EzBob.Config.SessionTimeout;

    if (sessionTimeout <= 0)
        return;

    var heartInterval;

    if (EzBob.Config.HeartBeatEnabled)
        heartInterval = setInterval(heartBeat, 5000);

    var timer;
    var timeoutValue = 1000 * 60 * sessionTimeout;

    set();

    function heartBeat() {
        $.get(window.gRootPath + 'HeartBeat');
    } // heartBeat

    function timeout() {
        reset();
        document.location = window.gRootPath + 'Account/LogOff';
    } // timeout

    function reset() {
        clearInterval(heartInterval);
        clearTimeout(timer);
    } // reset

    function set() {
        timer = setTimeout(timeout, timeoutValue);
    } // set
} // HeartOfActivity

EzBob.WizardView = EzBob.View.extend({
    initialize: function (options) {
        this.customer = options.customer;

        if ((this.customer.get('Id')) !== 0)
            HeartOfActivity();

        var storeInfoStepModel = new EzBob.StoreInfoStepModel(this.customer);

        var oWss = {
            views: {
                signup: new EzBob.QuickSignUpStepView({ model: this.customer }),
                link: new EzBob.StoreInfoStepView({ model: storeInfoStepModel }),
                details: new EzBob.PersonalInformationStepView({ model: this.customer }),
                companydetails: new EzBob.CompanyDetailsStepView({ model: this.customer }),
            }
        };

        var oConfiguredStepSequence = new EzBob.WizardStepSequence(oWss);

        this.stepModels = new EzBob.WizardSteps([
            this.customer,
            storeInfoStepModel,
            new EzBob.PersonalInformationStepModel(),
            new EzBob.CompanyDetailsStepModel()
        ]);

        this.model = new EzBob.WizardModel({ stepModels: this.stepModels, total: this.stepModels.length });
        this.vip = new EzBob.VipModel();
        this.progress = 0;
        this.topNavigationEnabled = EzBob.Config.WizardTopNaviagtionEnabled;
        this.template = _.template($('#wizard-template').html());
        this.progressTemplate = _.template($('#progress-indicator').html());

        this.steps = [];
        this.stepsByName = {};
        this.stepsOrderByName = {};

        var sSequenceName = 'offline'; // todo make one

        for (var i = 0; i < oConfiguredStepSequence[sSequenceName].length; i++) {
            var oStep = oConfiguredStepSequence[sSequenceName][i];

            oStep.view.on('next', this.next, this);
            oStep.view.on('previous', this.previous, this);
            oStep.view.on('jump-to', this.jumpTo, this);

            this.steps[i] = oStep;
            this.stepsByName[oStep.name] = oStep;
            this.stepsOrderByName[oStep.name] = i;
        } // for

        EzBob.App.on('wizard:progress', this.progressChanged, this);
        this.router = new EzBob.WizardRouter({
            stepList: this.steps
        });

        this.router.on('all', this.onRoute, this);

        Backbone.history.start({ silent: true });
    }, // initialize

    events: {
    	'click .broker-finish-wizard-later button': 'brokerFinishWizardLater',
    	//'click #privacy_policy': 'privacyPolicyClicked'
    }, // events

    brokerFinishWizardLater: function (event) {
        this.setSomethingEnabled($(event.currentTarget), false);
        location.assign(window.gRootPath + 'Broker/BrokerHome/FinishWizardLater');
    }, // brokerFinishWizardLater

    privacyPolicyClicked: function() {
		/*
    	var privacyPolicyLink = this.$el.find('#privacy_policy').attr('data-href');
    	$.colorbox({
		    href: privacyPolicyLink,
		    iframe: true,
		    width: '600px',
		    maxWidth: '100%',
		    height: '600px',
		    maxHeight: '100%',
		    close: '<i class="pe-7s-close"></i>',
            className : 'iframe-popup'
	    });
		*/
    },

    jumpTo: function (sLastSavedStepName, fCallback) {
        if (!this.stepsOrderByName.hasOwnProperty(sLastSavedStepName))
            return;

        var nStepPos = this.stepsOrderByName[sLastSavedStepName];

        if (nStepPos < this.steps.length)
            this.model.set('current', nStepPos);
        else
            this.model.trigger('completed');
    }, // jumpTo

    onRoute: function (sEventName) {
        var oStep = this.stepsByName[sEventName];

        if (!oStep)
            return;

        this.model.changePage(oStep.num);
        this.stepChanged();
        oStep.onFocus();
    }, // onRoute

    render: function () {
        var template = this.template();
        this.$el.html(template);
        this.stepChanged();

        var notifications = new EzBob.NotificationsView({ el: this.$el.find('.notifications') });
        notifications.render();
        $('.footer-navigator').hide();
		$('footer.location-customer-everline .privacy-and-cookie-policy').show();
        if ($('.broker-finish-wizard-later').length)
            $('#user-menu').hide();
        $('.find-out-more').hide();
        
        var that = this;

        if (!this.customer.get('IsBrokerFill') && !this.customer.get('IsWhiteLabel')) {
            this.vip.fetch().done(function() {
                if (that.vip.get('VipEnabled') && !that.vip.get('RequestedVip')) {
                    that.VipView = new EzBob.VipView({ model: that.vip, el: that.$el.find('.vip-container') });
                    that.VipView.render();
                }
            });
        }
        return this;
    }, // render
    
    handleTopNavigation: function (e) {
        if (!this.topNavigationEnabled)
            return;

        var newCurrent = $(e.currentTarget).data('step-num');
        if (this.steps[newCurrent])
            this.router.navTo(newCurrent);
    }, // handelTopNavigation

    next: function () {
        scrollTop();

        var current = parseInt(this.model.get('current'), 10),
            total = parseInt(this.model.get('total'), 10);

        if (current >= total)
            this.model.trigger('completed');

        if (current < total - 1) {
            ++current;
            this.model.set('current', current);
        } // if

        this.router.navTo(current);
    }, // next

    previous: function () {
        scrollTop();

        var current = parseInt(this.model.get('current'), 10);

        if (current === 0)
            return;

        this.model.set('current', --current);
        this.router.navTo(current);
    }, // previous

    renderStep: function (current) {
        var oPagesContainer = this.$el.find('.pages');

        var view = this.steps[current].view;

        view.render();

        if (!view.readyToProceed)
            return false;

        EzBob.UiAction.registerView(view);

        view.$el.attr('data-wizard-page-rendered', current).hide().appendTo(oPagesContainer);
        view.$el.find('.chzn-select').chosen({ disable_search_threshold: 10 });

        if (view.$el.find('#captcha').length > 0) {
            view.captcha = new EzBob.Captcha({ elementId: 'captcha', tabindex: 12 });
            view.captcha.render();
        } // if

        return true;
    }, // renderStep

    stepChanged: function () {
        
            var current = this.model.get('current');
            if (!this.renderStep(current))
                return;

            var currStep = this.steps[current];
            var address = this.customer.get('PersonalAddress');
            var postcode = '';
            if (address && address.models && address.models.length > 0) {
                postcode = this.customer.get('PersonalAddress').models[0].get('Postcode') || '';
            }

            var personalInfo = this.customer.get('CustomerPersonalInfo');
			var requestedLoan = this.customer.get('RequestedLoan') || {};
            EzBob.App.GA.trackPage(currStep.trackPage, currStep.documentTitle,
            {
            	Amount: requestedLoan.Amount || '',
            	Length: requestedLoan.Term || '',
                Gender: personalInfo ? personalInfo.GenderName || '' : '',
                Age: personalInfo ? personalInfo.Age || '' : '',
                Postcode: postcode,
                TypeofBusiness: personalInfo ? personalInfo.TypeOfBusinessDescription || '' : '',
                IndustryType: personalInfo ? personalInfo.IndustryTypeDescription || '' : '',
                LeadID: this.customer.get('RefNumber') || ''
            });
            $(document).attr('title', currStep.documentTitle);
            EzBob.App.trigger('wizard:progress', currStep.progress);

            var marketing = EzBob.dbStrings.MarketingDefault;
            var isWizard = false;

            if (this.progress === 100) {
                isWizard = true;
                marketing = EzBob.dbStrings.MarketingWizardStepDone;
            }
            else if (this.steps[current]) {
                isWizard = true;
                marketing = EzBob.dbStrings[this.steps[current].marketingStrKey] || marketing;
            } // if

            if (isWizard) {
                this.$el.find('#defaultMarketing').hide();
                this.$el.find('#marketingProggress').show().html(marketing);
            }
            else {
                this.$el.find('#defaultMarketing').show();
                this.$el.find('#marketingProggress').hide().html(marketing);
            } // if

            this.$el.find('.wizard-progress').html(this.progressTemplate({
                steps: this.steps,
                current: current,
                progress: this.progress,
                caption: currStep.documentTitle
            }));

            if (this.topNavigationEnabled)
                this.$el.find('li[data-step-num]').click($.proxy(this.handleTopNavigation, this));
            else
                this.$el.find('li[data-step-num]').css('cursor', 'default');

            this.$el.find('.pages > div').hide().filter('[data-wizard-page-rendered="' + current + '"]').show();

            if (this.steps[current])
                this.$el.find('.wizard-header').text(this.steps[current].header);
            var $numofsteps = $('ul.application_steps li').size();
            var $proggressLine = this.$el.find("ul.application_steps li.complete .progress-line-complete");
            var $proggresLineCurrent = this.$el.find("ul.application_steps li.current .progress-line-current");
            var $circleCurrent = this.$el.find("ul.application_steps li.current .inner-circle");
            var $circleCurrentev = this.$el.find("ul.application_steps li.current .progress-circle");
            $circleCurrent.removeClass('current');
            $circleCurrentev.removeClass('current');
            if (EzBob.Config.Origin === 'everline') {
                var sterpval= ((100 / $numofsteps) - 2) ;
                var movepercentage = (sterpval) * (current + 1);
                if (current != 0) {
                    $('.progress-bar .green-line').css("width", (movepercentage - sterpval) + '%');
                } else {
                    $circleCurrentev.addClass('current');
                }
             
                $('.progress-bar .green-line').show().animate({ width: movepercentage + '%' }, 800, function() {
                    $circleCurrentev.addClass('current');
                });
             


            } else {
               
                $proggressLine.last().css("width", "35%");
                $proggresLineCurrent.hide().css("width", "0");

                if (this.$el.find("ul.application_steps li:first").hasClass("current"))
                    $proggresLineCurrent.show().css("width", "35%");

                $proggressLine.show().animate({ width: "110%" }, 1000, function () {
                    $circleCurrent.addClass('current');
                    $proggresLineCurrent.show().animate({ width: "35%" }, 800, function () { });
                });
            }
      
    }, // stepChanged

    progressChanged: function (progress) {
        progress = parseInt(progress || 0, 10);

        if (progress < 0)
            progress = 0;

        if (progress > 100)
            progress = 100;

        this.progress = progress;
    }, // progressChanged
}); // EzBob.WizardView

EzBob.WizardModel = Backbone.Model.extend({
    defaults: {
        current: 0
    }, // defaults

    initialize: function () {
        EzBob.App.on('ct:wizard_page', this.changePage, this);
        this.on('change current', this.currentChanged, this);
    }, // initialize

    changePage: function (i) {
        this.set('current', i);
    }, // changeRange

    currentChanged: function () {
        EzBob.CT.recordEvent('ct:wizard_page', this.get('current'));
    } // currentChanged
}); // EzBob.WizardModel

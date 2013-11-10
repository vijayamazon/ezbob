///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/controls/ezbob.notifications.js" />
///<reference path="~/Content/js/Wizard/yourInfo/ezbob.steps.personinfo.js" />
///<reference path="~/Content/js/Wizard/ezbob.wizard.signupstep.js" />
///<reference path="~/Content/js/Wizard/accounts/ezbob.accounts.js" />

var EzBob = EzBob || {};

EzBob.WizardRouter = Backbone.Router.extend({
	initialize: function (options) {
		this.topNavigationEnabled = options.topNavigationEnabled;
		this.maxStepNum = options.maxStepNum;
		this.stepList = options.stepList;

		this.stepsByName = {};

		var self = this;

		for (var i = 0; i < this.stepList.length; i++) {
			var s = this.stepList[i];

			this.stepsByName[s.name] = s;

			this.route(s.name, s.name, (function(sName) {
				return function() { self.navByName(sName); };
			})(s.name));
		} // for
	}, // initialize

	navByName: function(sStepName) {
		var oStep = this.stepsByName[sStepName];

		if (oStep.num == 0) {
			this.trigger(sStepName);
			return;
		} // if first step

		if (!this.topNavigationEnabled) {
			if (this.maxStepNum >= oStep.num)
				this.trigger(sStepName);
			else
				this.navTo(this.maxStepNum);
		}
		else
			this.trigger(sStepName);
	},

	navTo: function (i) {
		if (!this.topNavigationEnabled)
			i = i > this.maxStepNum ? this.maxStepNum : i;

		var oStep = this.stepList[i];

		$(document).attr('title', oStep.documentTitle);
		EzBob.App.GA.trackPage(oStep.trackPage);
		EzBob.App.trigger('wizard:progress', oStep.progress);
		this.navigate(oStep.name, { trigger: true });
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
	}

	function timeout() {
		reset();
		document.location = window.gRootPath + 'Account/LogOff';
	}

	function reset() {
		clearInterval(heartInterval);
		clearTimeout(timer);
	}

	function set() {
		timer = setTimeout(timeout, timeoutValue);
	}
} // HeartOfActivity

EzBob.WizardView = Backbone.View.extend({
	initialize: function (options) {
		this.customer = options.customer;

		var modelArgs = {
			ebayMarketPlaces: options.ebayMarketPlaces,
			amazonMarketPlaces: options.amazonMarketPlaces,
			ekmMarketPlaces: options.ekmMarketPlaces,
			freeagentMarketPlaces: options.freeagentMarketPlaces,
			sageMarketPlaces: options.sageMarketPlaces,
			yodleeAccounts: options.yodleeAccounts,
			paypointMarketPlaces: options.paypointMarketPlaces
		};

		if ((this.customer.get('Id')) != 0)
			HeartOfActivity();

		var cgShops = options.cgShops;

		for (var i in cgShops) {
			if (!cgShops.hasOwnProperty(i))
				continue;

			var o = cgShops[i];

			if (!modelArgs[o.storeInfoStepModelShops])
				modelArgs[o.storeInfoStepModelShops] = [];

			modelArgs[o.storeInfoStepModelShops].push(o);
		} // for each cg shop

		if (!this.customer.get('IsOffline') && (EzBob.getCookie('isoffline') == 'yes'))
			this.customer.set('IsOffline', true);

		modelArgs.isOffline = this.customer.get('IsOffline');
		modelArgs.isProfile = this.customer.get('IsProfile');
		var storeInfoStepModel = new EzBob.StoreInfoStepModel(modelArgs);

		var oWss = {
			views: {
				signup:  new EzBob.QuickSignUpStepView({ model: this.customer }),
				link:    new EzBob.StoreInfoStepView({ model: storeInfoStepModel }),
				details: new EzBob.YourInformationStepView({ model: this.customer }),
				success: new EzBob.ThankYouWizardPage({ model: this.customer }),
			}
		};

		var oConfiguredStepSequence = new EzBob.WizardStepSequence(oWss);

		this.stepModels = new EzBob.WizardSteps([this.customer, storeInfoStepModel, new EzBob.YourInformationStepModel()]);

		this.model = new EzBob.WizardModel({ stepModels: this.stepModels, total: this.stepModels.length });
		this.model.on('change', this.stepChanged, this);

		this.progress = 0;
		this.topNavigationEnabled = EzBob.Config.WizardTopNaviagtionEnabled;
		this.template = _.template($('#wizard-template').html());
		this.progressTemplate = _.template($('#progress-indicator').html());

		this.steps = [];
		this.stepsByName = {};

		var sSequenceName = this.customer.get('IsOffline') ? 'offline' : 'online';

		for (var i = 0; i < oConfiguredStepSequence[sSequenceName].length; i++) {
			var oStep = oConfiguredStepSequence[sSequenceName][i];

			oStep.view.on('ready', _.bind(this.ready, this, i));
			oStep.view.on('next', this.next, this);
			oStep.view.on('previous', this.previous, this);
			oStep.view.on('linkClick', this.linkClick, this);

			this.steps[i] = oStep;
			this.stepsByName[oStep.name] = oStep;
		} // for

		EzBob.App.on('wizard:progress', this.progressChanged, this);

		this.router = new EzBob.WizardRouter({
			topNavigationEnabled: this.topNavigationEnabled,
			maxStepNum: this.model.get('ready') != undefined ? this.model.get('ready').clean(undefined).length : 0,
			stepList: this.steps
		});

		this.router.on('all', this.onRoute, this);
		EzBob.App.on('instep-progress-changed', this.inStepProgressChanged, this);

		Backbone.history.start({ silent: true });
	},

	inStepProgressChanged: function(inStepSectionID) {
		var current = this.model.get('current');
		var nextStepProgress = (current >= this.steps.length - 1) ? 100 : this.steps[current + 1].progress;
		this.steps[current].onInStepProgressChanged(inStepSectionID, nextStepProgress);
	},

	onRoute: function(sEventName) {
		var oStep = this.stepsByName[sEventName];

		if (!oStep)
			return;

		this.model.changePage(oStep.num);
		oStep.onFocus();
	},

	render: function () {
		var template = this.template();
		this.$el.html(template);

		this.renderSteps();
		this.stepChanged();

		var notifications = new EzBob.NotificationsView({ el: this.$el.find('.notifications') });
		notifications.render();

		if (!this.topNavigationEnabled)
			this.$el.find('.wizard-steps > ul li').css('cursor', 'default');

		this.router.navTo(this.model.get('ready') != undefined ? this.model.get('ready').clean(undefined).length : 0);

		return this;
	},

	events: {
		'click .wizard-steps > ul li': 'linkClick'
	},

	linkClick: function (e) {
		if (!EzBob.Config.WizardTopNaviagtionEnabled)
			return false;

		var newCurrent = $(e.currentTarget).data('step-num');

		if (this.steps[newCurrent])
			this.router.navTo(newCurrent);

		return false;
	},

	ready: function (num) {
		var ready = this.model.get('ready') || new Array(this.model.get('total') + 1);
		ready[num] = true;

		for (var i = 0; i < ready.length; i++)
			if (!ready[i])
				break;

		this.model.set('ready', ready);

		if (!this.steps[num].ready)
			this.steps[num].ready = true;

		this.router.maxStepNum = this.model.get('ready') != undefined ? this.model.get('ready').clean(undefined).length : 0;

		this.stepChanged();
	},

	next: function () {
		scrollTop();

		var current = this.model.get('current'),
			total = this.model.get('total');

		if (total == current - 1)
			this.model.trigger('completed');

		++current;

		this.model.set('current', current);
		this.router.navTo(current);
	},

	previous: function () {
		scrollTop();

		var current = this.model.get('current');

		if (current == 0)
			return;

		this.model.set('current', --current);
		this.router.navTo(current);
	},

	renderSteps: function () {
		var ul = this.$el.find('.pages');

		_.each(this.steps, function (s) {
			var view = s.view.render();

			view.$el.hide().appendTo(ul);
			view.$el.find('.chzn-select').chosen({ disable_search_threshold: 10 });
			if (view.$el.find('#captcha').length > 0) {
				view.captcha = new EzBob.Captcha({ elementId: 'captcha', tabindex: 12 });
				view.captcha.render();
			}
		});
	},

	stepChanged: function () {
		var current = this.model.get('current');

		var data = {
			steps: this.steps,
			current: current,
			progress: this.progress
		};

		var marketing = EzBob.dbStrings.MarketingDefault;
		var isWizard = false;

		if (this.progress == 100) {
			isWizard = true;
			marketing = EzBob.dbStrings.MarketingWizardStepDone;
		}
		else if (this.steps[current]) {
			isWizard = true;
			marketing = EzBob.dbStrings[this.steps[current].marketingStrKey] || marketing;
		}

		if (isWizard) {
			this.$el.find('#defaultMarketing').hide();
			this.$el.find('#marketingProggress').show().html(marketing);
		} else {
			this.$el.find('#defaultMarketing').show();
			this.$el.find('#marketingProggress').hide().html(marketing);
		}

		this.$el.find('.wizard-progress').html(this.progressTemplate(data));

		this.$el.find('.pages > div').hide().eq(current).show();
		if (this.steps[current])
			this.$el.find('.wizard-header').text(this.steps[current].header);
	},

	progressChanged: function (progress) {
		progress = parseInt(progress || 0);

		if (progress < 0)
			progress = 0;

		if (progress > 100)
			progress = 100;

		this.progress = progress;
		this.stepChanged();
	}
}); // EzBob.WizardView

EzBob.WizardModel = Backbone.Model.extend({
	defaults: {
		current: 0
	},

	initialize: function () {
		EzBob.App.on('ct:wizard_page', this.changePage, this);
		this.on('change current', this.currentChanged, this);
	},

	changePage: function (i) {
		this.set('current', i);
	},

	currentChanged: function () {
		EzBob.CT.recordEvent('ct:wizard_page', this.get('current'));
	}
});

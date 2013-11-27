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

		$(document).attr('title', oStep.documentTitle);
		EzBob.App.GA.trackPage(oStep.trackPage);
		EzBob.App.trigger('wizard:progress', oStep.progress);
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

		if ((this.customer.get('Id')) !== 0)
			HeartOfActivity();

		if (!this.customer.get('IsOffline') && (EzBob.getCookie('isoffline') === 'yes'))
			this.customer.set('IsOffline', true);

		var modelArgs = {
			mpAccounts: this.customer.get('mpAccounts'),
			isOffline: this.customer.get('IsOffline'),
			isProfile: this.customer.get('IsProfile'),
		};

		var storeInfoStepModel = new EzBob.StoreInfoStepModel(modelArgs);

		var oWss = {
			views: {
				signup: new EzBob.QuickSignUpStepView({ model: this.customer }),
				link: new EzBob.StoreInfoStepView({ model: storeInfoStepModel }),
				details: new EzBob.PersonalInformationStepView({ model: this.customer }),
				companydetails: new EzBob.CompanyDetailsStepView({ model: this.customer }),
				success: new EzBob.ThankYouWizardPage({ model: this.customer }),
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

			this.steps[i] = oStep;
			this.stepsByName[oStep.name] = oStep;
		} // for

		EzBob.App.on('wizard:progress', this.progressChanged, this);

		this.router = new EzBob.WizardRouter({
			stepList: this.steps
		});

		this.router.on('all', this.onRoute, this);

		Backbone.history.start({ silent: true });
	},

	onRoute: function (sEventName) {
		console.log('onRoute: name');
		var oStep = this.stepsByName[sEventName];

		if (!oStep)
			return;

		this.model.changePage(oStep.num);
		this.stepChanged();
		oStep.onFocus();
	},

	render: function () {
		var template = this.template();
		this.$el.html(template);

		this.stepChanged();

		var notifications = new EzBob.NotificationsView({ el: this.$el.find('.notifications') });
		notifications.render();

		return this;
	},

	handleTopNavigation: function (e) {
		if (!this.topNavigationEnabled)
			return;

		var newCurrent = $(e.currentTarget).data('step-num');
		if (this.steps[newCurrent])
			this.router.navTo(newCurrent);
	}, // handelTopNavigation

	ready: function (num) {
		var ready = this.model.get('ready') || new Array(this.model.get('total') + 1);
		ready[num] = true;

		this.model.set('ready', ready);

		if (!this.steps[num].ready)
			this.steps[num].ready = true;
	}, // ready

	next: function () {
		scrollTop();

		var current = parseInt(this.model.get('current'), 10),
			total = parseInt(this.model.get('total'), 10);

		if (total === current - 1)
			this.model.trigger('completed');

		++current;

		this.model.set('current', current);
		this.router.navTo(current);
	}, // next

	previous: function () {
		scrollTop();

		var current = parseInt(this.model.get('current'), 10);

		if (current === 0)
			return;

		this.model.set('current', --current);
		this.router.navTo(current);
	},

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

		var data = {
			steps: this.steps,
			current: current,
			progress: this.progress
		};

		var marketing = EzBob.dbStrings.MarketingDefault;
		var isWizard = false;

		if (this.progress === 100) {
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
		if (this.topNavigationEnabled)
			this.$el.find('li[data-step-num]').click($.proxy(this.handleTopNavigation, this));
		else
			this.$el.find('li[data-step-num]').css('cursor', 'default');

		this.$el.find('.pages > div').hide().filter('[data-wizard-page-rendered="' + current + '"]').show();
		if (this.steps[current])
			this.$el.find('.wizard-header').text(this.steps[current].header);
	},

	progressChanged: function (progress) {
		progress = parseInt(progress || 0, 10);

		if (progress < 0)
			progress = 0;

		if (progress > 100)
			progress = 100;

		this.progress = progress;
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

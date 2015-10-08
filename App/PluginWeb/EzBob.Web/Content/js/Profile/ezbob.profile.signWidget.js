var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SignModel = Backbone.Model.extend({
	defaults: {
		color: 'green',
		text: 'Welcome back',
	}
});

EzBob.Profile.SignWidget = Backbone.View.extend({
	initialize: function(options) {
		this.templates = {
			standard: _.template($('#sign-template').html()),
			welcome: _.template($('#sign-welcome-template').html())
		};
		this.customerModel = options.customerModel;
		this.model = new EzBob.Profile.SignModel();
		this.model.on('change', this.render, this);
		this.customerModel.on('change:TotalBalance', this.processBalanceChanged, this);
		this.customerModel.on('change:state', this.processBalanceChanged, this);
		this.customerModel.on('change:HasRollovers', this.processBalanceChanged, this);
		this.balanceChanged();
	}, // initialize

	render: function() {
		this.$el.html(this.templates[this.model.get('signTemplate')](this.model.toJSON()));
		EzBob.UiAction.registerView(this);
		return this;
	}, // render

	events: {
		'click a.pay-early': 'click'
	}, // events

	click: function() {
		this.trigger('payEarly');
		window.location.href = '#PayEarly';
		return false;
	}, // click

	processBalanceChanged: function() {
		this.balanceChanged();
		EzBob.UiAction.registerView(this);
	}, // processBalanceChanged

	balanceChanged: function() {
		var balance = this.customerModel.get('TotalBalance');
		var state = this.customerModel.get('state');
		var hasLoans = this.customerModel.get('hasLoans');
		var isNew = !hasLoans;
		var hasRollOver = this.customerModel.get('HasRollovers');
		var name = this.customerModel.get('CustomerPersonalInfo') != null ? this.customerModel.get('CustomerPersonalInfo').FirstName : '';
		var isEarly = this.customerModel.get('IsEarly');
		var isAlibaba = this.customerModel.get('IsAlibaba');
		var hasChance = this.customerModel.get('HasApprovalChance');
		if (!hasRollOver && state === 'late') {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', payment is required',
				signTemplate: 'welcome'
			});
			return;
		}

		if (hasRollOver) {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', you have a rollover payment pending</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (balance > 0) {
			var valueOfText;
			if (isEarly) {
				valueOfText = '<span>' + this.getNameSpan(name) + ', pay early &amp; save';
			} else {
				valueOfText = '<span>' + this.getNameSpan(name) + ', payment is required';
			}
			this.model.set({
				color: 'green',
				text: valueOfText,
				signTemplate: 'welcome'
			});
			return;
		}

		if (state === 'get' && !isNew) {
			this.model.set({
				color: 'green',
				text:
					'<span>' + this.getNameSpan(name) + ', congratulations, your credit is ' +
					(isAlibaba ? 'approved' : 'ready to be taken') +
					'</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (state === 'get' && isNew) {
			this.model.set({
				color: 'green',
				text:
					'<span>' + this.getNameSpan(name) + ', congratulations, credit is approved' +
					(isAlibaba ? '' : ' and can be taken') +
					'</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (state === 'bad' && hasLoans) {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', welcome back</span>',
				signTemplate: 'welcome'
			});
			return;
		}
		
		if (state === 'bad' && !hasLoans && !hasChance) {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', unfortunately we cannot offer you financing at this time</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (state === 'bad' && !hasLoans && hasChance) {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', we don\'t have enough data to approve your loan</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (state === 'apply') {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', request cash to get funding today!</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (!isNew && state === 'wait') {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', your application is under review, we will revert as soon as possible</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		if (isNew && state === 'wait') {
			this.model.set({
				color: 'green',
				text: '<span>' + this.getNameSpan(name) + ', your application is under review, we will revert as soon as possible</span>',
				signTemplate: 'welcome'
			});
			return;
		}

		this.model.set({
			color: 'green',
			text: '<span>' + this.getNameSpan(name) + ', welcome back</span>',
			signTemplate: 'welcome'
		});
	}, // balanceChanged

	getNameSpan: function (name) {
		return '<span class="client-name">' + name + '</span>';
	}//getNameSpan
}); //EzBob.Profile.SignWidget View
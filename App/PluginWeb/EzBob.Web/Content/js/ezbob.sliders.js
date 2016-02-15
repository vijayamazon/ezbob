var EzBob = EzBob || {};

EzBob.SlidersModel = Backbone.Model.extend({
	idAttribute: 'Id',
	defaults: {
		'Amount': 20000,
		'Term': 15,
		'InterestRate' : 0.0175
	},
	initialize: function(options){
		if (options.Amount > 0) {
			this.set('Amount', options.Amount);
		}

		if (options.Term > 0) {
			this.set('Term', options.Term);
		}
	},
	urlRoot: window.gRootPath + 'Customer/CustomerRequestedLoan/RequestedLoan'
});

EzBob.SlidersView = Backbone.Marionette.ItemView.extend({
	template: '#sliders-template',
    initialize: function (options) {
    	this.model.on('change', this.render, this);
	    this.type = options.type;
        return this;
    },
    events: {
    	'click #changeLoanAmount': 'changeLoanAmount',
    	'click .close': 'closeClicked',
    	'click .cancel': 'closeClicked'
    },
    ui: {
    	'amount': '.amount',
    	'interest': '.interest',
    	'total': '.total',
    	'interestRate': '.interest-rate',
    	'dashboardRequestSummary': '.total-wrapper',
    	'sliderWrapper': '.slider-wrapper',
    	'calcSlider': '#calc-slider',
    	'totalTooltip': '.total-repayment-tooltip'
    },
    serializeData: function () {
	    return { type: this.type };
    },
    onRender: function () {
        var amountCaption = (EzBob.Config.Origin === 'everline' ? 'How much do you need?' : 'Amount');
        var periodCaption = (EzBob.Config.Origin === 'everline' ? 'How long do you want it for?' : 'Time');
        var self = this;
		
	    var startValues = this.adjustValues();

	    InitAmountPeriodSliders({
        	container: this.ui.calcSlider,
        	el: this.$el,
            amount: {
            	min: this.model.get('MinLoanAmount'),
            	max: this.model.get('MaxLoanAmount'),
            	start: startValues.startAmount,
                step: 1000,
                caption: amountCaption,
                hasbutton: true,
                uiEvent: 'requested-loan:'
            },
            period: {
            	min: this.model.get('MinTerm'),
            	max: this.model.get('MaxTerm'),
            	start: startValues.startTerm,
                step: 1,
                hide: false,
                caption: periodCaption,
                hasbutton: true,
                uiEvent: 'requested-loan:'
            },
            callback: function(ignored, sEvent) {
                if (sEvent === 'slide' || sEvent === 'change')
                    self.loanSelectionChanged();
            }
        });

        if (this.type == 'dashboardRequestLoan' && EzBob.Config.Origin !== 'everline') {
        	this.ui.dashboardRequestSummary.appendTo(this.ui.sliderWrapper);
        	var interestRate = this.model.get('InterestRate') ? this.model.get('InterestRate') : 0.0175;
        	this.ui.totalTooltip.tooltip({ title: '*Actual loan amount is subject to status. Loan estimate based on ' + EzBob.formatPercents(interestRate) + ' interest charge per month. Interest rate varies between 1.75% - 2.25% per month and setup fee of 2% - 7% may be charged depending on your business risk rating.', container: 'body', viewport: '.request-summary-title' });
        } else {
	        this.ui.dashboardRequestSummary.remove();
        }
        this.loanSelectionChanged();

        EzBob.UiAction.registerView(this);
        return this;
    },

    changeLoanAmount: function(ev) {
    	var currentTerm = $('#calc-slider .period-slider').slider('value');
    	var currentAmount = $('#calc-slider .amount-slider').slider('value');

    	this.model.set({ 'Amount': currentAmount }, { silent: true });
    	this.model.set({ 'Term': currentTerm }, { silent: true });

    	var self = this;
    	this.model.off('change', this.render, this);
    	this.model
			.save()
			.done(function () {
				if(ev !== 'saveOnly') self.closeClicked();
			})
			.fail(function () {
				if (ev !== 'saveOnly') self.closeClicked();
			});
    },

	adjustValues: function(){
		var startAmount;
		if (this.model.get('Amount')) {
			startAmount = this.model.get('Amount') > this.model.get('MinLoanAmount') ? this.model.get('Amount') : this.model.get('MinLoanAmount');
			startAmount = startAmount > this.model.get('MaxLoanAmount') ? this.model.get('MaxLoanAmount') : startAmount;
		} else
			startAmount = this.model.get('MinLoanAmount');

		var startTerm;
		if (this.model.get('Term')) {
			startTerm = this.model.get('Term') > this.model.get('MinTerm') ? this.model.get('Term') : this.model.get('MinTerm');
			startTerm = startTerm > this.model.get('MaxTerm') ? this.model.get('MaxTerm') : startTerm;
		} else
			startTerm = this.model.get('MinTerm');
		return { startTerm: startTerm, startAmount: startAmount };
	},

	closeClicked: function () {
    		this.trigger('requested-amount-changed');
    		this.model.off('change', this.render, this);
    		this.close();
		},

    loanSelectionChanged: function() {
    	var currentTerm = $('#calc-slider .period-slider').slider('value');
    	var currentAmount = $('#calc-slider .amount-slider').slider('value');
	    var interestRate = this.model.get('InterestRate');
	    var calc = this.calcRepaymentsTable(currentTerm, currentAmount, interestRate);

    	this.ui.amount.text(EzBob.formatPounds(calc.principal));
    	this.ui.interest.text(EzBob.formatPounds(calc.interest));
    	this.ui.total.text(EzBob.formatPounds(calc.total));
    	this.ui.interestRate.text(EzBob.formatPercents(calc.interestRate));
    },

    calcRepaymentsTable: function(loan_period, loan_amount, interest_rate) {
    	var total_repayment = 0;
    	var total_interest = 0;
    	// calculate the repayments table
    	for (var i = 0; i < loan_period; i++) {
    		var month_id = i;
    		var principal = loan_amount / loan_period;
    		var remaining_balance = loan_amount * (loan_period - month_id) / loan_period;
    		var interest = remaining_balance * interest_rate;
    		var total_monthly = principal + interest;
			
    		total_repayment += total_monthly;
    		total_interest += interest;
    	}
		
    	return {
    		interest: total_interest.toPrecision(10),
    		interestRate: interest_rate,
    		principal: loan_amount,
    		total: total_repayment,
    		units: loan_period
    	};
    },

    remove: function () {
    	this.$el.empty().off(); /* off to unbind the events */
    	return this;
    }
});

EzBob.TakeLoanSlidersView = Backbone.Marionette.ItemView.extend({
	template: '#sliders-template',
	initialize: function () {
		this.type = 'take-loan-sliders';
		return this;
	},

	serializeData: function () {
		return { type: this.type };
	},

	onRender: function () {
		var amountCaption = (EzBob.Config.Origin === 'everline' ? 'How much do you need?' : 'Amount');
		var periodCaption = (EzBob.Config.Origin === 'everline' ? 'How long do you want it for?' : 'Time');

		InitAmountPeriodSliders({
			container: this.$el.find('#calc-slider'),
			el: this.$el,
			amount: {
				min: this.model.get('minCash'),
				max: this.model.get('maxCash'),
				start: this.model.get('neededCash'),
				step: 100,
				caption: amountCaption,
				hasbutton: true,
				uiEvent: 'loan-legal:'
			},
			period: {
				min: this.model.get('isLoanSourceCOSME') ? 15 : 3,
				max: this.model.get('approvedRepaymentPeriod'),
				start: this.model.get('repaymentPeriod'),
				step: 1,
				caption: periodCaption,
				hasbutton: true,
				hide: !this.model.get('isCustomerRepaymentPeriodSelectionAllowed'),
				uiEvent: 'loan-legal:'
			},
			callback: function(ignored, sEvent) {
				if (sEvent === 'change')
					EzBob.App.trigger('loanSelectionChanged');
			} // callback
		});

		this.$el.find('.wizard-request-loan-section').remove();
		this.$el.find('.calc-header').remove();
		EzBob.App.trigger('loanSelectionChanged');
		EzBob.UiAction.registerView(this);
		return this;
	},
});



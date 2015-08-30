var EzBob = EzBob || {};

EzBob.SlidersModel = Backbone.Model.extend({
	idAttribute: 'Id',
	defaults: {
		'Amount': 20000,
		'Term': 15,
		'InterestRate' : 0.02
	},
	urlRoot: window.gRootPath + 'Customer/CustomerRequestedLoan/RequestedLoan'
});

EzBob.SlidersView = Backbone.Marionette.ItemView.extend({
	template: '#sliders-template',
    initialize: function () {
        this.model.on('change', this.render, this);
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
		'interestRate': '.interest-rate'
    },
    onRender: function () {
        var amountCaption = (EzBob.Config.Origin === 'everline' ? 'How much do you need?' : 'Amount');
        var periodCaption = (EzBob.Config.Origin === 'everline' ? 'How long do you want it for?' : 'Time');
        var self = this;
        InitAmountPeriodSliders({
            container: this.$('#calc-slider'),
            amount: {
                min: 1000,
                max: 120000,
                start: this.model.get('Amount'),
                step: 1000,
                caption: amountCaption,
                hasbutton: true,
                uiEvent: 'requested-loan:'
            },

            period: {
                min: 3,
                max: 24,
                start: this.model.get('Term'),
                step: 1,
                hide: false,
                caption: periodCaption,
                hasbutton: true,
                uiEvent: 'requested-loan:'
            },
            callback: function(ignored, sEvent) {
                if (sEvent === 'slide')
                    self.loanSelectionChanged();
            }
        });
        this.loanSelectionChanged();

        EzBob.UiAction.registerView(this);
        return this;
    },

    changeLoanAmount: function(){
    	var currentTerm = this.$('#calc-slider .period-slider').slider('value');
    	var currentAmount = this.$('#calc-slider .amount-slider').slider('value');

    	this.model.set({ 'Amount': currentAmount }, { silent: true });
    	this.model.set({ 'Term': currentTerm }, { silent: true });

	    var self = this;
	    this.model.save().done(function () {
		    self.trigger('requested-amount-changed');
	    });
    },

    closeClicked: function(){
    	this.trigger('requested-amount-changed');
    },

    loanSelectionChanged: function() {
    	var currentTerm = this.$('#calc-slider .period-slider').slider('value');
    	var currentAmount = this.$('#calc-slider .amount-slider').slider('value');
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



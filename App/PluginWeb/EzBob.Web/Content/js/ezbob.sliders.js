var EzBob = EzBob || {};

EzBob.SlidersModel = Backbone.Model.extend({
    initialize: function () {
        this.set('amount', 20000);
        this.set('term', 15);
	    this.set('interestRate', 0.02);
    },
});

EzBob.SlidersView = Backbone.Marionette.ItemView.extend({
    template: '#sliders-template',

    initialize: function () {
        this.model.on('change', this.render, this);
        return this;
    },
    events: {
       
    },
    ui: {
    	'amount': '.amount',
    	'interest': '.interest',
    	'total': '.total',
		'interestRate': '.interest-rate'
    },
    onRender: function () {
        console.log('this.model', this.model.get('amount'), this.model.get('term'));
        var self = this;
        InitAmountPeriodSliders({
            container: this.$('#calc-slider'),
            amount: {
                min: 1000,
                max: 120000,
                start: this.model.get('amount'),
                step: 1000,
                caption: 'How much do you need?',
                hasbutton : true
            },

            period: {
                min: 3,
                max: 24,
                start: this.model.get('term'),
                step: 1,
                hide: false,
                caption: 'How long do you want it for?',
                hasbutton: true
            },
            callback: function(ignored, sEvent) {
                if (sEvent === 'change')
                    self.loanSelectionChanged();
            }
        });
	    this.loanSelectionChanged();
        return this;
    },

    loanSelectionChanged: function() {
    	var currentRepaymentPeriod = this.$('#calc-slider .period-slider').slider('value');
    	var currentAmount = this.$('#calc-slider .amount-slider').slider('value');
	    var interestRate = this.model.get('interestRate');
    	var calc = this.calcRepaymentsTable(currentRepaymentPeriod, currentAmount, interestRate);

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

    jqoptions: function () {
        return {
            autoOpen: true,
            title: 'Change requested loan amount',
            modal: true,
            resizable: true,
            width: 940,
            maxWidth: '100%',
            height: 515,
            closeOnEscape: true,
        }
    },
});



var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.MakeEarlyPaymentModel = Backbone.Model.extend({
	defaults: {
		amount: 0,
		paymentType: 'loan',
		loanPaymentType: 'full',
		rolloverPaymentType: 'minimum',
		defaultCard: true,
		url: '#',
		isPayTotal: true,
		isPayRollover: false,
		isPayLoan: false,
		isPayTotalLate: false,
		isNextInterest: false,
	}, // defaults

	initialize: function() {
		this.get('customer').on('fetch', this.recalculate, this);
		this.on('change:amount change:paymentType change:loan change:loanPaymentType', this.changed, this);
		this.on('change:paymentType', this.paymentTypeChanged, this);
		this.on('change:loanPaymentType', this.loanPaymentTypeChanged, this);
		this.on('change:rolloverPaymentType', this.rolloverPaymentTypeChanged, this);
		this.on('change:loan', this.loanChanged, this);
		this.recalculate();
	}, // initialize

	recalculate: function() {
		var customer = this.get('customer');
		var liveRollovers = customer.get('ActiveRollovers');
		var liveLoans = customer.get('ActiveLoans');
		var total = customer.get('TotalEarlyPayment');
		var loan = (liveLoans ? liveLoans[0] : null);
		var currentRollover = this.calcCurrentRollover(loan);

		this.set({
			total: total,
			liveLoans: liveLoans,
			rollovers: liveRollovers,
			loan: loan,
			amount: customer.get('TotalEarlyPayment'),
			currentRollover: currentRollover,
			hasLateLoans: customer.get('hasLateLoans'),
			totalLatePayment: customer.get('TotalLatePayment'),
			paymentType: liveRollovers.length > 0 ? 'rollover' : 'loan',
			isEarly: customer.get('IsEarly')
		});
	}, // recalculate

	calcCurrentRollover: function(loan) {
		var rollovers = this.get('customer').get('ActiveRollovers').toJSON();

		var currentRollover = _.where(rollovers, { LoanId: loan && loan.get('Id') })[0] || null;

		return currentRollover;
	}, // calcCurrentRollover

	paymentTypeChanged: function(e) {
		var loan;
		var type = this.get('paymentType');

		switch (type) {
		case 'total':
			this.set('amount', this.get('customer').get('TotalEarlyPayment'));
			break;

		case 'totalLate':
			this.set('amount', this.get('customer').get('TotalLatePayment'));
			break;

		case 'loan':
			loan = this.get('loan');

			if (loan && this.get('liveLoans').length > 1)
				this.set('loanPaymentType', 'full');

			break;

		case 'rollover':
			this.set('rolloverPaymentType', 'minimum');
			break;
		} // switch
	}, // paymentTypeChanged

	loanChanged: function() {
		var currentRollover = (this.calcCurrentRollover(this.get('loan'))) || null;
		var status = this.get('loan') && this.get('loan').get('Status');

		this.set({
			currentRollover: currentRollover,
			loanPaymentType: status === 'Late' ? 'late' : 'full'
		});

		if (currentRollover) {
			this.set({
				amount: currentRollover && currentRollover.RolloverPayValue
			});
		} // if
	}, // loanChanged

	rolloverPaymentTypeChanged: function() {
		this.set('amount', this.get('currentRollover') && this.get('currentRollover').RolloverPayValue);
	}, // rolloverPaymentTypeChanged

	loanPaymentTypeChanged: function() {
		var loan = this.get('loan');

		if (!loan)
			return;

		var amount = 0;
		var type = this.get('loanPaymentType');

		switch (type) {
		case 'full':
			amount = loan.get('TotalEarlyPayment');
			break;

		case 'next':
			amount = loan.get('NextEarlyPayment');
			break;

		case 'late':
			amount = loan.get('AmountDue');
			break;

		case 'nextInterest':
			amount = loan.get('NextInterestPayment');
			break;

		case 'other':
			amount = loan.get('TotalEarlyPayment');
			break;
		} // switch

		this.set('amount', amount);
	}, // loanPaymentTypeChanged

	changed: function() {
		var loan = this.get('loan');

		if (!loan)
			return;

		var url = window.gRootPath + 'Customer/Paypoint/Pay?amount=' + this.get('amount');
		url += '&type=' + this.get('paymentType');
		url += '&paymentType=' + this.getPaymentType();
		url += '&loanId=' + loan.id;
		url += '&rolloverId=' + (this.get('currentRollover') ? this.get('currentRollover').Id : -1);

		this.set({ url: url });

		var paymentType = this.get('paymentType');

		this.set({
			'isPayTotal': paymentType === 'total',
			'isPayRollover': paymentType === 'rollover',
			'isPayLoan': paymentType === 'loan',
			'isPayTotalLate': paymentType === 'totalLate',
			'isNextInterest': paymentType === 'nextInterest'
		});
	}, // changed

	getPaymentType: function () {
	    if (this.get('loanPaymentType') === 'rollover')
			return this.get('rolloverPaymentType');
		else
			return this.get('loanPaymentType');
	}, // getPaymentType
});

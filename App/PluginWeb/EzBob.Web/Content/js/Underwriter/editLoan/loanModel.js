var EzBob = EzBob || {};

EzBob.Installment = Backbone.Model.extend({
	defaults: {
		IsAdding: false,
		skipRecalculations: false,
	}, // defaults

	initialize: function() {
		this.on('change:Balance', this.balanceChanged, this);
		this.on('change:Principal', this.principalChanged, this);
		this.on('change:Total', this.totalChanged, this);
		this.on('change:Date', this.dateChanged, this);
	}, // initialize

	balanceChanged: function() {
		this.safeRecalculate(function() {
			if (this.get('Balance') === this.previous('Balance'))
				return;

			var principal = this.round(this.get('BalanceBeforeRepayment') - this.get('Balance'));
			this.set('Principal', principal);
			this.recalculate();
		});
	}, // balanceChanged

	totalChanged: function() {
		this.safeRecalculate(function() {
			var diff = this.get('Total') - this.previous('Total');
			if (diff === 0)
				return;

			this.set('Balance', this.get('Balance') - diff);
			this.set('Principal', this.get('Principal') + diff);
		});
	}, // totalChanged

	principalChanged: function() {
		this.safeRecalculate(function() {
			var diff = this.get('Principal') - this.previous('Principal');
			if (diff === 0)
				return;

			this.set('Balance', this.get('Balance') - diff);
			this.recalculate();
		});
	}, // principalChanged

	safeRecalculate: function() {
		var func = arguments[0];
		var params = 2 <= arguments.length ? [].slice.call(arguments, 1) : [];

		if (this.skipRecalculations)
			return;

		this.skipRecalculations = true;
		func.call(this, params);
		this.skipRecalculations = false;
	}, // safeRecalculate

	recalculate: function() {
		this.set({
			'Total': this.get('Principal') + this.get('Interest') + this.get('Fees')
		});
	}, // recalculate

	dateChanged: function() {
		this.set('Date', new Date(moment.utc(this.get('Date'))));
	}, // dateChanged

	round: function(number) {
		number = Math.round(number * 100);
		return number = number / 100;
	}, // round
}); // EzBob.Installment

EzBob.Installments = Backbone.Collection.extend({
	model: EzBob.Installment,

	comparator: function(m1, m2) {
		var r;

		var d1 = moment.utc(m1.get('Date')).startOf('day');
		var d2 = moment.utc(m2.get('Date')).startOf('day');
		var d = d1.diff(d2, 'days');

		if (d < 0)
			r = -1;
		else if (d === 0)
			r = 0;
		else
			r = 1;

		if (r === 0 && m1.get('Type') !== m2.get('Type')) {
			if (m1.get('Type') === 'Installment')
				r = 1;
			else
				r = -1;
		} // if

		return r;
	}, // comparator
}); // EzBob.Installments

EzBob.LoanModel = Backbone.Model.extend({
	url: function() {
		return '' + window.gRootPath + 'Underwriter/LoanEditor/Loan/' + (this.get('Id'));
	}, // url

	initialize: function() {
		var items = new EzBob.Installments();
		items.on('change', this.itemsChanged, this);
		this.set('Items', items);
	}, // initialize

	itemsChanged: function() {
		return this.trigger('change');
	}, // itemsChanged

	shiftDate: function(installment, newDate, oldDate) {
		var diff = moment.utc(newDate) - moment.utc(oldDate);

		var index = this.get('Items').indexOf(installment);

		var oItems = this.get('Items').models;
		var nItemCount = oItems.length;

		for (var i = 0; i < nItemCount; i++) {
			var item = oItems[i];

			if (i <= index)
				continue;

			item.set('Date', moment.utc(item.get('Date')).add(diff).toDate());
		} // for

		return false;
	}, // shiftDate

	shiftInterestRate: function(installment, rate) {
		var index = this.get('Items').indexOf(installment);

		var oItems = this.get('Items').models;
		var nItemCount = oItems.length;

		for (var i = 0; i < nItemCount; i++) {
			var item = oItems[i];

			if (i > index)
				item.set('InterestRate', rate);
		} // for

		return false;
	}, // shiftInterestRate

	parse: function(r) {
		_.each(r.Items, function(item) {
			item.Date = new Date(moment.utc(item.Date));
		});

		this.get('Items').reset(r.Items);

		delete r.Items;

		r.Date = new Date(moment.utc(r.Date));

		return r;
	}, // parse

	toJSON: function() {
		var r = EzBob.LoanModel.__super__.toJSON.call(this);
		//r.Items = r.Items.toJSON();
		return r;
	}, // toJSON

	removeItem: function(index) {
		var items = this.get('Items');
		items.remove(items.at(index));
		this.recalculate();
	}, // removeItem

	addInstallment: function(installment) {
		this.get('Items').add(installment);
		this.recalculate();
	}, // addInstallment

	addFee: function(fee) {
		this.get('Items').add(fee);
		this.recalculate();
	}, // addFee

	recalculate: function() {
		this.save({}, {
			url: '' + window.gRootPath + 'Underwriter/LoanEditor/Recalculate/' + (this.get('Id'))
		});
	}, // recalculate

	saveAutoChargeOptions: function (schedultItemId) {
        this.save({}, {
            url: '' + window.gRootPath + 'Underwriter/LoanEditor/SaveAutoChargesOption/' + (this.get('Id')) + '?schedultItemId=' + schedultItemId
        }).always(function () { BlockUi('off'); });
	},

	removeAutoChargeOptions: function () {
	    this.save({}, {
	        url: '' + window.gRootPath + 'Underwriter/LoanEditor/RemoveAutoChargesOption/' + (this.get('Id'))
	    }).always(function () { BlockUi('off'); });
	},

	SaveLateFeeOption: function (lateFeeStartDate, lateFeeEndDate) {
	    this.save({}, {
	        url: '' + window.gRootPath + 'Underwriter/LoanEditor/SaveLateFeeOption/' + (this.get('Id')) + '?lateFeeStartDate=' + lateFeeStartDate + '&lateFeeEndDate=' + lateFeeEndDate
	    }).always(function () { BlockUi('off'); });
	},
	RemoveLateFeeOption: function () {
	    this.save({}, {
	        url: '' + window.gRootPath + 'Underwriter/LoanEditor/RemoveLateFeeOption/' + (this.get('Id'))
	    }).always(function () { BlockUi('off'); });
	},
	saveFreezeInterval: function (sStartDate, sEndDate) {
	    this.save({}, {
	        url: '' + window.gRootPath + 'Underwriter/LoanEditor/SaveFreezeInterval/' + (this.get('Id')) + '?startdate=' + sStartDate + '&enddate=' + sEndDate
	    }).always(function () { BlockUi('off'); });
	}, // addFreezeInterval

	removeFreezeInterval: function (intervalId) {
	    this.save({}, {
	        url: '' + window.gRootPath + 'Underwriter/LoanEditor/RemoveFreezeInterval/' + (this.get('Id')) + '?intervalid=' + intervalId
	    }).always(function () { BlockUi('off'); });
	}, // removeFreezeInterval

	getInstallmentBefore: function(date) {
		date = moment.utc(date).toDate();

		var installment = null;

		var oItems = this.get('Items').models;
		var nItemCount = oItems.length;

		for (var i = 0; i < nItemCount; i++) {
			var item = oItems[i];

			if (item.get('Type') === 'Installment' && moment.utc(item.get('Date')).toDate() < date)
				installment = item;
		} // for

		return installment;
	}, // getInstallmentBefore

	getInstallmentAfter: function(date) {
		date = moment.utc(date).toDate();

		var oItems = this.get("Items").models;
		var nItemCount = oItems.length;

		for (var i = 0; i < nItemCount; i++) {
			var item = oItems[i];

			if (item.get('Type') === 'Installment' && moment.utc(item.get('Date')).toDate() > date)
				return item;
		} // for

		return null;
	}, // getInstallmentAfter
}); // EzBob.LoanModel

EzBob.LoanModelTemplate = EzBob.LoanModel.extend({
	url: function() {
		return '' + window.gRootPath + 'Underwriter/LoanEditor/LoanCR/' + (this.get('CashRequestId'));
	}, // url

	recalculate: function() {
		this.save({}, { url: '' + window.gRootPath + 'Underwriter/LoanEditor/RecalculateCR' });
	}, // recalculate
}); // EzBob.LoanModelTemplate

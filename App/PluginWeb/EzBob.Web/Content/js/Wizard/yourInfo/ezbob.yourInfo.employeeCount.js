var EzBob = EzBob || {};

EzBob.EmployeeCountView = Backbone.View.extend({
	initialize: function(options) {
		this.template = _.template($('#employee-count-template').html());
		this.model = options.model;
		this.onChangeCallback = options.onchange;
		this.prefix = options.prefix;
	}, // initialize

	events: {
		"change   #EmployeeCount": "countChanged",
		"keyup    #EmployeeCount": "countChanged",
		"focusout #EmployeeCount": "countChanged",
		"click    #EmployeeCount": "countChanged",

		"change   #TopEarningEmployeeCount": "topCountChanged",
		"keyup    #TopEarningEmployeeCount": "topCountChanged",
		"focusout #TopEarningEmployeeCount": "topCountChanged",
		"click    #TopEarningEmployeeCount": "topCountChanged",

		"change   #BottomEarningEmployeeCount": "bottomCountChanged",
		"keyup    #BottomEarningEmployeeCount": "bottomCountChanged",
		"focusout #BottomEarningEmployeeCount": "bottomCountChanged",
		"click    #BottomEarningEmployeeCount": "bottomCountChanged",

		"change   #EmployeeCountChange": "deltaChanged",
		"keyup    #EmployeeCountChange": "deltaChanged",
		"focusout #EmployeeCountChange": "deltaChanged",
		"click    #EmployeeCountChange": "deltaChanged",

		"change   #MonthlySalary": "deltaChanged",
		"keyup    #MonthlySalary": "deltaChanged",
		"focusout #MonthlySalary": "deltaChanged",
		"click    #MonthlySalary": "deltaChanged",
	}, // events

	deltaChanged: function(el) {
		this.onChangeCallback.call();
	}, // deltaChanged

	countChanged: function() {
		var oEmployeeCount = this.$el.find('#EmployeeCount');
		var oTopCount = this.$el.find('#TopEarningEmployeeCount');
		var oBottomCount = this.$el.find('#BottomEarningEmployeeCount');

		oEmployeeCount.tooltip('destroy');

		var self = this;

		var oIcons = {
			Count: self.$el.find('#EmployeeCountImage'),
			Top: self.$el.find('#TopEarningEmployeeCountImage'),
			Bottom: self.$el.find('#BottomEarningEmployeeCountImage')
		};

		var nEmployeeCount = oEmployeeCount.val();

		if (nEmployeeCount === '') {
			this.changeValue(oTopCount);
			this.changeValue(oBottomCount);

			for (var o in oIcons)
				oIcons[o].field_status('clear');

			this.onChangeCallback.call();
			return;
		} // if

		nEmployeeCount = parseInt(nEmployeeCount, 10);

		var maxCount = parseInt(oEmployeeCount.attr('max'), 10);
		var minCount = parseInt(oEmployeeCount.attr('min'), 10);

		if ((nEmployeeCount < minCount) || (nEmployeeCount > maxCount)) {
			var msg = (nEmployeeCount < 0)
				? 'This field cannot contain value less than ' + minCount + '.'
				: 'This field cannot contain value greater than ' + maxCount + '.';

			oIcons.Count.field_status('set', 'fail');
			oEmployeeCount.tooltip({ title: msg }).tooltip('enable').tooltip('fixTitle');

			this.changeValue(oTopCount);
			this.changeValue(oBottomCount);

			this.onChangeCallback.call();
			return;
		} // if

		oIcons.Count.field_status('set', 'ok');
		this.onChangeCallback.call();
	}, // countChanged

	changeValue: function(oControl, sNewValue) {
		sNewValue = sNewValue || '';

		var sCurValue = oControl.val();

		if (sNewValue === sCurValue)
			return;

		oControl.val(sNewValue).change();
	}, // changeValue

	topCountChanged: function() {
		this.someCountChanged(
			this.$el.find('#TopEarningEmployeeCount'),
			this.$el.find('#TopEarningEmployeeCountImage'),
			this.$el.find('#BottomEarningEmployeeCount')
		);

		this.onChangeCallback.call();
	}, // topCountChanged

	bottomCountChanged: function() {
		this.someCountChanged(
			this.$el.find('#BottomEarningEmployeeCount'),
			this.$el.find('#BottomEarningEmployeeCountImage'),
			this.$el.find('#TopEarningEmployeeCount')
		);

		this.onChangeCallback.call();
	}, // bottomCountChanged

	someCountChanged: function(oMe, oMyIcon, oOther) {
		oMe.tooltip('destroy');

		if (oMe.val() === '') {
			oMyIcon.field_status('clear');
			return;
		} // if

		var nMyCount = parseInt(oMe.val(), 10);

		if (nMyCount < 0) {
			oMyIcon.field_status('set', 'fail');
			oMe.tooltip({ title: 'This field cannot contain negative value.' }).tooltip('enable').tooltip('fixTitle');
			return;
		} // if

		var oEmployeeCount = this.$el.find('#EmployeeCount');
		var nTotalCount = oEmployeeCount.val();

		if (nTotalCount === '') {
			oMyIcon.field_status('set', 'fail');
			return;
		} // if

		nTotalCount = parseInt(nTotalCount, 10);

		if (nTotalCount < 0) {
			oMyIcon.field_status('set', 'fail');
			oEmployeeCount.tooltip({ title: 'This field cannot contain negative value.' }).tooltip('enable').tooltip('fixTitle');
			oMe.tooltip({ title: 'Please fix employee count first.' }).tooltip('enable').tooltip('fixTitle');
			return;
		} // if

		var nOtherCount = oOther.val();

		nOtherCount = (nOtherCount === '') ? 0 : parseInt(nOtherCount, 10);

		if (nOtherCount < 0)
			nOtherCount = 0;

		if (nMyCount + nOtherCount <= nTotalCount)
			oMyIcon.field_status('set', 'ok');
		else {
			oMyIcon.field_status('set', 'fail');
			oMe.tooltip({ title: 'Sum of top earning and bottom earning employees cannot be greater than employee count.' }).tooltip('enable').tooltip('fixTitle');
		} // if
	}, // someCountChanged

	isValid: function() {
	    if (this.$el.find('#EmployeeCountChange').val() === '' && this.$el.find('#EmployeeCountChange').is(":visible"))
			return false;

		var nTotalCount = this.getVal('#EmployeeCount');

		if (nTotalCount < 0)
			return false;

		var nTopCount = this.getVal('#TopEarningEmployeeCount');
	    var bTopCountVisible = this.$el.find('#TopEarningEmployeeCount').is(":visible");
		if (nTopCount < 0 && bTopCountVisible)
			return false;

		var nBottomCount = this.getVal('#BottomEarningEmployeeCount');
		var bBottomCountVisible = this.$el.find('#BottomEarningEmployeeCount').is(":visible");
		if (nBottomCount < 0 && bBottomCountVisible)
			return false;

		if (bBottomCountVisible && bTopCountVisible)
		    return nTopCount + nBottomCount <= nTotalCount;

	    return true;

	}, // isValid

	getVal: function(sEl) {
		var sValue = this.$el.find(sEl).val();

		return (sValue === '') ? -1 : parseInt(sValue, 10);
	}, // getVal

	render: function() {
		this.$el.html(this.template({ prefix: this.prefix }));

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.$el.find('.numeric').each(function() {
			var ctrl = $(this);

			var maxValue = parseInt(ctrl.attr('max'), 10);

			if (maxValue === 0)
				ctrl.numericOnly();
			else {
				var maxLen = parseInt(Math.log(maxValue) / Math.LN10 + 0.5, 10) + 1;
				ctrl.attr('maxlength', maxLen).numericOnly(maxLen);
			} // if
		});

		this.$el.find('.cashInput').moneyFormat();

		EzBob.UiAction.registerView(this);

		return this;
	} // render
}); // EzBob.EmployeeCountView

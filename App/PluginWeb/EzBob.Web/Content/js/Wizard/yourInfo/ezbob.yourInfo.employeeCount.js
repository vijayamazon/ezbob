var EzBob = EzBob || {};

EzBob.EmployeeCountView = Backbone.View.extend({
	initialize: function (options) {
		this.template = _.template($('#employee-count-template').html());
		this.model = options.model;
		this.parentView = options.parentView;
		this.onChangeCallback = options.onchange;
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
	}, // events

	deltaChanged: function() {
		this.onChangeCallback.call(this.parentView);
	}, // deltaChanged

	countChanged: function() {
		var oEmployeeCount = this.$el.find('#EmployeeCount');
		var oTopCount = this.$el.find('#TopEarningEmployeeCount');
		var oBottomCount = this.$el.find('#BottomEarningEmployeeCount');

		var self = this;

		var oIcons = {
			Count: self.$el.find('#EmployeeCountImage'),
			Top: self.$el.find('#TopEarningEmployeeCountImage'),
			Bottom: self.$el.find('#BottomEarningEmployeeCountImage')
		};

		var nEmployeeCount = oEmployeeCount.val();

		if (nEmployeeCount == '') {
			oTopCount.val('');
			oBottomCount.val('');

			for (var o in oIcons)
				oIcons[o].field_status('clear');

			this.onChangeCallback.call(this.parentView);
			return;
		} // if

		nEmployeeCount = parseInt(nEmployeeCount);

		if (nEmployeeCount < 0) {
			oIcons.Count.field_status('set', 'fail');

			oTopCount.val('');
			oBottomCount.val('');

			oIcons.Top.field_status('clear');
			oIcons.Bottom.field_status('clear');

			this.onChangeCallback.call(this.parentView);
			return;
		} // if

		oIcons.Count.field_status('set', 'ok');
		this.onChangeCallback.call(this.parentView);
	}, // countChanged

	topCountChanged: function() {
		this.someCountChanged(
			this.$el.find('#TopEarningEmployeeCount'),
			this.$el.find('#TopEarningEmployeeCountImage'),
			this.$el.find('#BottomEarningEmployeeCount')
		);

		this.onChangeCallback.call(this.parentView);
	}, // topCountChanged

	bottomCountChanged: function() {
		this.someCountChanged(
			this.$el.find('#BottomEarningEmployeeCount'),
			this.$el.find('#BottomEarningEmployeeCountImage'),
			this.$el.find('#TopEarningEmployeeCount')
		);

		this.onChangeCallback.call(this.parentView);
	}, // bottomCountChanged

	someCountChanged: function (oMe, oMyIcon, oOther) {
		if (oMe.val() == '') {
			oMyIcon.field_status('clear');
			return;
		} // if

		var nMyCount = parseInt(oMe.val());

		if (nMyCount < 0) {
			oMyIcon.field_status('set', 'fail');
			return;
		} // if

		var nTotalCount = this.$el.find('#EmployeeCount').val();

		if (nTotalCount == '') {
			oMyIcon.field_status('set', 'fail');
			return;
		} // if

		nTotalCount = parseInt(nTotalCount);
		
		if (nTotalCount < 0) {
			oMyIcon.field_status('set', 'fail');
			return;
		} // if

		var nOtherCount = oOther.val();

		nOtherCount = (nOtherCount == '') ? 0 : parseInt(nOtherCount);

		if (nOtherCount < 0)
			nOtherCount = 0;

		oMyIcon.field_status('set', (nMyCount + nOtherCount <= nTotalCount) ? 'ok' : 'fail');
	}, // someCountChanged

	isValid: function() {
		if (this.$el.find('#EmployeeCountChange').val() == '')
			return false;

		var nTotalCount = this.getVal('#EmployeeCount');

		if (nTotalCount < 0)
			return false;

		var nTopCount = this.getVal('#TopEarningEmployeeCount');

		if (nTopCount < 0)
			return false;

		var nBottomCount = this.getVal('#BottomEarningEmployeeCount');

		if (nBottomCount < 0)
			return false;

		return nTopCount + nBottomCount <= nTotalCount;
	}, // isValid

	getVal: function(sEl) {
		var sValue = this.$el.find(sEl).val();

		return (sValue == '') ? -1 : parseInt(sValue);
	}, // getVal

	render: function () {
		this.$el.html(this.template());

		var oFieldStatusIcons = this.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		this.$el.find('.numeric').numericOnly();

		return this;
	} // render
}); // EzBob.EmployeeCountView

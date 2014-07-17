EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SubmitView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.SubmitView.__super__.initialize.apply(this, arguments);

		this.SubmitButtons = {};
	}, // initialize

	initSubmitBtn: function() {
		var self = this;

		$.each(arguments, function(idx, val) {
			self.SubmitButtons[val] = 1;
		});
	}, // initSubmitBtn
	
	events: function() {
		var evt = {};

		$.each(this.SubmitButtons, function(idx, val) {
			evt['click ' + idx] = 'doSubmit';
		});

		evt['cut input'] = 'inputChanged';
		evt['paste input'] = 'inputChanged';

		evt['change input'] = 'inputChanged';
		evt['keyup  input'] = 'inputChanged';
		evt['change select'] = 'inputChanged'; 

		return evt;
	}, // events

	submitSelector: function() {
		var sSelector = '';

		$.each(this.SubmitButtons, function(idx, val) {
			if (sSelector !== '')
				sSelector += ', ';

			sSelector += idx;
		});

		return sSelector;
	}, // submitSelector

	doSubmit: function(event) {
		event.preventDefault();
		event.stopPropagation();

		if (!this.isSubmitEnabled())
			return;

		this.setSubmitEnabled(false);
		BlockUi();

		this.onSubmit(event);
	}, // doSubmit

	onSubmit: function(event) {}, // onSubmit

	isSubmitEnabled: function() {
		return this.isSomethingEnabled(this.submitSelector());
	}, // isSubmitEnabled

	setSubmitEnabled: function(bEnabled) {
		return this.setSomethingEnabled(this.submitSelector(), bEnabled).toggleClass('btn-green', bEnabled);
	}, // setSubmitEnabled

	inputChanged: function(evt) {
		this.setSubmitEnabled(EzBob.Validation.checkForm(this.validator) && this.customValidationResult());
	}, // inputChanged

	customValidationResult: function() {
		return true;
	}, // customerValidationResult

	setSidebar: function(oSidebar) {
		this.$el.find('.customer-sidebar').append(oSidebar);
		return this;
	}, // setSidebar

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			this.setSubmitEnabled(false);
			return this;
		} // if

		if (this.setAuthOnRender())
			this.router.setAuth();

		this.onRender();

		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	onFocus: function() {
		this.inputChanged();
	}, // onFocus

	setAuthOnRender: function() {
		return true;
	}, // setAuthOnRender

	onRender: function() {}, // onRender
}); // EzBob.Broker.SubmitView

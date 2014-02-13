EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.SubmitView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.SubmitView.__super__.initialize.apply(this, arguments);
	}, // initialize

	initSubmitBtnID: function(sID) {
		this.SubmitBtnID = sID;
	}, // initSubmitBtnID
	
	events: function() {
		var evt = {};

		evt['click #' + this.SubmitBtnID] = 'doSubmit';

		evt['change input'] = 'inputChanged';
		evt['keyup  input'] = 'inputChanged';
		evt['change select'] = 'inputChanged'; 

		return evt;
	}, // events

	doSubmit: function() {
		event.preventDefault();
		event.stopPropagation();

		if (!this.isSubmitEnabled())
			return;

		this.setSubmitEnabled(false);
		BlockUi();

		this.onSubmit();
	}, // doSubmit

	onSubmit: function() {}, // onSubmit

	isSubmitEnabled: function() {
		var oBtn = this.$el.find('#' + this.SubmitBtnID);

		if (oBtn.hasClass('disabled') || oBtn.attr('disabled') || oBtn.prop('disabled'))
			return false;

		return true;
	}, // isSubmitEnabled

	setSubmitEnabled: function(bEnabled) {
		return this.setSomethingEnabled('#' + this.SubmitBtnID, bEnabled);
	}, // setSubmitEnabled

	inputChanged: function(evt) {
		this.setSubmitEnabled(EzBob.Validation.checkForm(this.validator));
	}, // inputChanged

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

		this.router.setAuth();

		this.onRender();

		this.inputChanged();

		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	onRender: function() {}, // onRender
}); // EzBob.Broker.SubmitView

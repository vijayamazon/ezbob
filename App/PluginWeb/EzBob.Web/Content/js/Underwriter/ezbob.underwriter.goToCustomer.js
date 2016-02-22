var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.RecentCustomersModel = Backbone.Model.extend({
	url: window.gRootPath + 'Underwriter/CustomerNavigator/GetRecentCustomers',
}); // EzBob.Underwriter.RecentCustomersModel

EzBob.Underwriter.goToCustomerId = Backbone.Marionette.ItemView.extend({
	initialize: function() {
		var self = this;

		Mousetrap.bind('ctrl+g', function() {
			self.render();
			return false;
		});

		this.on('NotFound', this.notFound);
	}, // initialize

	template: function() {
		var recentCustomers = JSON.parse(localStorage.getItem('RecentCustomers'));

		var allOptions = '';

		for (var _i = 0, _len = recentCustomers.length; _i < _len; _i++) {
			var customer = recentCustomers[_i];
			allOptions += '<option value="' + customer.Item1 + '">' + customer.Item2 + '</option>';
		} // for

		var el = $("<div id='go-to-template'></div>").html(
			"<input type=text class='goto-customerId form-control' autocomplete='off'/><br/>" +
			"<label>Recent customers:</label>" +
				"<select id='recentCustomers'class='selectheight form-control'>" + allOptions + "</select><br/>" +
				"<div class='error-place' style='color:red'></div>"
	   );

		$('body').append(el);
		return el;
	}, // template

	ui: {
		'input': '.goto-customerId',
		'select': '#recentCustomers',
		'template': '#go-to-template',
		'errorPlace': '.error-place'
	}, // ui

	onRender: function() {
		var self = this;

		this.dialog = EzBob.ShowMessageEx({
			message: this.ui.template,
			title: 'Customer ID?',
			timeout: 0,
			onOk: function () { return self.okTrigger(); },
			okText: 'OK',
			onCancel: null,
			cancelText: 'Cancel',
			closeOnEscape: true,
			dialogWidth: 555,
		});

		this.ui.input.on('keydown', function(e) {
			return self.keydowned(e);
		});

		this.okBtn = $('.ok-button');

		this.ui.input.autocomplete({
			source: '' + window.gRootPath + 'Underwriter/CustomerNavigator/FindCustomer',
			autoFocus: false,
			minLength: 3,
			delay: 500
		});
	}, // onRender

	okTrigger: function() {
		var val = this.ui.input.val();

		if (!IsInt(val, true))
			val = val.substring(0, val.indexOf(':'));

		if (!IsInt(val, true)) {
			var selectVal = this.ui.select.val();

			if (!IsInt(selectVal, true)) {
				this.addError("Incorrect input");
				return false;
			} // if
			val = selectVal;
		} // if

		this.checkCustomer(val);
		return false;
	}, // okTrigger

	keydowned: function(e) {
		this.addError('');

		if (this.okBtn.attr('disabled') === 'disabled')
			return;

		if ($('.ui-autocomplete:visible').length !== 0)
			return;

		if (e.keyCode === 13)
			this.okTrigger();
	}, // keydowned

	addError: function(val) {
		this.ui.errorPlace.text(val);
	}, // addError

	checkCustomer: function(id) {
		this.okBtn.attr('disabled', 'disabled');

		var self = this;

		var xhr = $.get('' + window.gRootPath + 'Underwriter/CustomerNavigator/CheckCustomer?customerId=' + id);

		xhr.done(function(res) {
			switch (res.State) {
				case 'NotFound':
					self.addError('Customer id. #' + id + ' was not found');
					break;

				case 'NotSuccesfullyRegistred':
				case 'Ok':
					self.trigger('ok', id);
					self.dialog.dialog('close');
					break;
			} // switch
		});

		xhr.complete(function() {
			self.okBtn.removeAttr('disabled');
		});
	}, // checkCustomer
}); // EzBob.Underwriter.goToCustomerId

var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.FraudModel = Backbone.Model.extend({
	defaults: function() {
		return {
			FirstName: '',
			LastName: '',
			Addresses: [],
			Phones: [],
			Emails: [],
			EmailDomains: [],
			BankAccounts: [],
			Companies: [],
			Shops: []
		};
	}, // defaults

	sendToServer: function() {
		var xhr = Backbone.sync('create', this, {
			url: '' + window.gRootPath + 'Underwriter/Fraud/AddNewUser'
		});

		var self = this;

		xhr.complete(function() {
			self.trigger('saved');
		});
	}, // sendToServer
}); // EzBob.Underwriter.FraudModel

EzBob.Underwriter.FraudModels = Backbone.Collection.extend({
	url: '' + gRootPath + 'Underwriter/Fraud/GetAll',
	model: EzBob.Underwriter.FraudModel,
}); // EzBob.Underwriter.FraudModels

EzBob.Underwriter.simpleValueAddView = Backbone.Marionette.ItemView.extend({
	initialize: function(options) {
		this.template = options.template;
		this.type = options.type || '';
	}, // initialize

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Add fraud ' + this.type.toLowerCase(),
			position: 'center',
			draggable: false,
			width: 530,
			dialogClass: 'value-add-popup'
		};
	}, // jqoptions

	events: {
		'click .ok': 'okClicked'
	}, // events

	ui: {
		form: 'form'
	}, // ui

	onRender: function() {
		if (this.type !== 'Addresses')
			this.ui.form.find('input, textarea').addClass('required');

		this.validator = this.ui.form.validate({
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
		});

		return this;
	}, // onRender

	okClicked: function() {
		if (!this.validator.form())
			return false;

		var model = new Backbone.Model(SerializeArrayToEasyObject(this.ui.form.serializeArray()));

		this.trigger('added', {
			model: model,
			type: this.type
		});

		this.close();
		return false;
	}, // okClicked
}); // EzBob.Underwriter.simpleValueAddView

EzBob.Underwriter.SimpleValueView = Backbone.Marionette.ItemView.extend({
	initialize: function(options) {
		this.template = options.template;
	}, // initialize

	serializeData: function() {
		return { models: this.model };
	}, // serializeData
}); // EzBob.Underwriter.SimpleValueView

EzBob.Underwriter.AddEditFraudView = Backbone.Marionette.ItemView.extend({
	template: '#fraud-add-edit-template',

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Add new fraud user',
			position: 'center',
			draggable: false,
			dialogClass: 'fraud-popup',
			width: 600
		};
	}, // jqoptions

	ui: {
		form: 'form'
	}, // ui

	onRender: function() {
		this.ui.form.find('input, textarea').addClass('required');

		this.validator = this.ui.form.validate({
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
		});
	}, // onRender

	events: {
		'click .save': 'saveButtonClicked',
		'click .add': 'addClicked',
		'click .remove': 'removeClicked'
	}, // events

	removeClicked: function(e) {
		var $el = $(e.currentTarget);
		var index = $el.data("index");
		var type = $el.data("type");

		this.model.get(type).splice(index, 1);

		this.reRenderArea(type);
		return false;
	}, // removeClicked

	saveButtonClicked: function() {
		var isValid = this.validator.form();

		if (!isValid) {
			this.ui.form.closest('.modal-body').animate({ scrollTop: 0 }, 500);
			return;
		} // if

		var formData = SerializeArrayToEasyObject(this.ui.form.serializeArray());

		this.model.set({
			FirstName: formData.FirstName,
			LastName: formData.LastName,
		});

		this.model.sendToServer();
		this.close();
	}, // saveButtonClicked

	addClicked: function(e) {
		var $el = $(e.currentTarget);
		var type = $el.data('type');
		var template = '#add-' + type + '-template';

		var view = new EzBob.Underwriter.simpleValueAddView({
			template: template,
			type: type,
		});

		EzBob.App.jqmodal.show(view);
		view.on('added', this.simpleValueAdded, this);
		return false;
	}, // addClicked

	simpleValueAdded: function(data) {
		this.model.get(data.type).push(data.model.toJSON());
		this.reRenderArea(data.type);
		return false;
	}, // simpleValueAdded

	reRenderArea: function(type) {
		var $el = this.$el.find('.' + type);

		(new EzBob.Underwriter.SimpleValueView({
			template: '#' + type + '-template',
			el: $el,
			model: this.model.get(type)
		})).render();
	}, // reRenderArea
}); // EzBob.Underwriter.AddEditFraudView

EzBob.Underwriter.FraudView = Backbone.Marionette.ItemView.extend({
	template: '#fraud-template',

	initialize: function() {
		this.model = new EzBob.Underwriter.FraudModels();
		this.model.on('change reset', this.render, this);
		this.model.fetch();
	}, // initialize

	events: {
		'click .add': 'addButtonClicked'
	}, // events

	serializeData: function() {
		return {
			data: this.model.toJSON(),
		};
	}, // serializeData

	addButtonClicked: function() {
		var model = new EzBob.Underwriter.FraudModel();
		var view = new EzBob.Underwriter.AddEditFraudView({ model: model });

		view.modalOptions = {
			show: true,
			keyboard: false,
			width: 600,
			height: 600,
		};

		EzBob.App.jqmodal.show(view);

		var self = this;
		model.on('saved', function() { self.model.fetch(); });

		return false;
	}, // addButtonClicked

	show: function() {
		this.$el.show();
	}, // show

	hide: function() {
		this.$el.hide();
	}, // hide
}); // EzBob.Underwriter.FraudView

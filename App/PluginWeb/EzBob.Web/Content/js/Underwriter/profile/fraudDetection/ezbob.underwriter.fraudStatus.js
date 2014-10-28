var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.FraudStatusModel = Backbone.Model.extend({
	urlRoot: function() {
		return '' + window.gRootPath + 'Underwriter/FraudStatus/Index?Id=' + (this.get('customerId'));
	}, // urlRoot
}); // EzBob.Underwriter.FraudStatusModel

EzBob.FraudStatusItemsView = Backbone.Marionette.ItemView.extend({
	template: '#fraud-status-items-template'
}); // EzBob.FraudStatusItemsView

EzBob.Underwriter.FraudStatusLayout = Backbone.Marionette.Layout.extend({
	template: '#fraud-status-layout-template',

	initialize: function() {
		this.modelBinder = new Backbone.ModelBinder();
	}, // initialize

	bindings: {
		currentStatus: { selector: "input[name='currentStatus']" },
		customerId: { selector: "input[name='customerId']" }
	}, // bindings

	regions: {
		list: '#list-fraud-items',
		content: '#fraud-view'
	}, // regions

	events: {
		'change #fraud-status-items': 'changeStatus'
	}, // events

	changeStatus: function() {
		var currentStatusId = $('#fraud-status-items option:selected').val();
		var currentStatus = $('#fraud-status-items option:selected').text();

		this.model.set({
			'currentStatus': parseInt(currentStatusId)
		});

		this.model.set({
			'currentStatusText': currentStatus
		});

		this.renderStatusValue();

		return this;
	}, // changeStatus

	renderStatusValue: function() {
		var self = this;

		this.model.fetch().done(function() {
			self.$el.find('#fraud-view').show();
		});

		return false;
	},

	save: function() {
		BlockUi("on");

		var form = this.$el.find('form');
		var postData = form.serialize();
		var action = '' + window.gRootPath + 'Underwriter/FraudStatus/Save/';

		var self = this;

		$.post(action, postData).done(function() {
			self.trigger('saved');
			self.close();
		}).complete(function() {
			BlockUi('off');
		});

		return false;
	}, // save

	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);

		var common = new EzBob.FraudStatusItemsView;

		this.list.show(common);

		this.$el.find("#fraud-status-items [value='" + (this.model.get('CurrentStatusId')) + "']").attr("selected", "selected");

		this.renderStatusValue();

		return this;
	}, // onRender

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Fraud Status',
			draggable: true,
			width: '400',
			buttons: {
			    'OK': _.bind(this.onSave, this),
			    'Cancel': _.bind(this.onCancel, this)
			},
		};
	}, // jqoptions

	onCancel: function() {
		this.close();
	}, // onCancel

	onSave: function() {
		this.save();
	}, // onSave
}); // EzBob.Underwriter.FraudStatusLayout

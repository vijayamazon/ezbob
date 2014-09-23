var EzBob = EzBob || {};

EzBob.BoundItemView = Backbone.Marionette.ItemView.extend({
	events: {},

	initialize: function() {
		this.events['click .btn-primary:not(.not-save)'] = 'save';
		this.modelBinder = new Backbone.ModelBinder();
		return this;
	}, // initialize

	onRender: function() {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		return this;
	}, // onRender

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Bug Reporter',
			position: 'center',
			draggable: false,
			dialogClass: 'bugs-popup',
			width: 700,
		};
	}, // jqoptions

	save: function() {
		this.trigger('save');

		if (this.onSave != null)
			this.onSave();
	}, // save

	onClose: function() {
		this.modelBinder.unbind();
	}, // onClose
}); // EzBob.BoundItemView

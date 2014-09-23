var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ControlButtonsView = Backbone.Marionette.ItemView.extend({
	template: _.template(($("#controls-button-template").length > 0 ? $("#controls-button-template") : $("<div/>")).html())
	//fix for error on Reports
});

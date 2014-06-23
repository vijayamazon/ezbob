﻿var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
	template: "#company-directors-template",

	initialize: function() {

	}, // initialize

	onRender: function() {
		console.log('model is', this.model);
	}, // onRender

	serializeData: function() {
		var company = this.model.get("CompanyInfo") || {};
		return {
			data: company.Directors || []
		};
	}, // serializeData
}); // EzBob.Profile.CompanyDirectorsView

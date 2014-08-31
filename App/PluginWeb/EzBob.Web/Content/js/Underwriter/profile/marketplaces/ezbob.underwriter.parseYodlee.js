var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ParseYodleeView = Backbone.Marionette.ItemView.extend({
	template: '#parse-yodlee-template',

	initialize: function(options) {
		this.customerId = options.customerId;
	}, // initialize
	events: {
		"click .back": "back",
		"click .parseYodlee": "parseYodlee",
	},
	serializeData: function () {
		var companyFiles = _.find(this.model.models, function (model) {
			return model.get('CompanyFiles') != null;
		}) || {};

		console.log('companyFiles.Files', companyFiles, companyFiles.get('CompanyFiles').Files);
		return { files: companyFiles.get('CompanyFiles').Files };
	},
	onRender: function () {
		return this;
	}, // onRender
	
	parseYodlee: function () {
		console.log('parse', this.$el.find("[name='YodleeBankFile']:checked").val());
	},
	
	back: function () {
		EzBob.App.vent.trigger('ct:marketplaces.parseYodleeBack');
	}
	
}); // EzBob.Underwriter.UploadHmrcView

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
		return { files: companyFiles.get('CompanyFiles').Files };
	},
	onRender: function () {
		return this;
	}, // onRender
	
	parseYodlee: function () {
		var fileId = this.$el.find("[name='YodleeBankFile']:checked").val();
		if (fileId) {
			var xhr = $.post(window.gRootPath + "Underwriter/MarketPlaces/ParseYodlee", { fileId: fileId, customerId: this.customerId });
			xhr.done(function() {
				EzBob.ShowMessageTimeout("Parsing of file began, refresh in a while", "Parsing began", 3);
			});
		}
	},
	
	back: function () {
		EzBob.App.vent.trigger('ct:marketplaces.parseYodleeBack');
	}
	
}); // EzBob.Underwriter.UploadHmrcView

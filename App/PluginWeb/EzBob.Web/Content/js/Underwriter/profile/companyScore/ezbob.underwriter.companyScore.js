var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CompanyScoreModel = Backbone.Model.extend({
	url: function () {
		return window.gRootPath + "Underwriter/CompanyScore/Index/" + this.customerId;
	}
});

EzBob.Underwriter.CompanyScoreView = Backbone.View.extend({
	initialize: function () {
		this.template = _.template($('#company-score-template').html());
		this.model.on('change sync', this.render, this);
	},
	render: function () {
		var onAfterRender = [];

		this.$el.html(this.template({ companyScoreData: this.model.toJSON(), onAfterRender: onAfterRender }));

		for (var i = 0; i < onAfterRender.length; i++)
			onAfterRender[i].call(undefined);
	}
});

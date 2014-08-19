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
		this.list = null;
		this.activePanel = 0;
	}, // initialize

	render: function () {
		var onAfterRender = [];

		var sHtml = this.template({
			companyScoreData: this.model.toJSON(),
			isOwner: false,
			onAfterRender: onAfterRender,
			caption: 'Company'
		});

		this.activePanel = 0;

		var oOwners = this.model.get('Owners');

		var i;

		if (oOwners) {
			for (i = 0; i < oOwners.length; i++) {
				sHtml += this.template({
					companyScoreData: oOwners[i],
					isOwner: true,
					onAfterRender: onAfterRender,
					caption: 'Company Owner'
				});

				this.activePanel++;
			} // for each owner
		} // if has owners

		this.$el.html(sHtml);

		for (i = 0; i < onAfterRender.length; i++)
			onAfterRender[i].call(undefined);

		this.redisplayAccordion();
	}, // render

	redisplayAccordion: function () {
		if (this.list) {
			this.list.accordion('destroy');
			this.list = null;
		} // if

		this.list = this.$el.find('.company-score-data').accordion({
			heightStyle: 'content',
			collapsible: true,
			active: this.activePanel
		});

		this.list.addClass('box');
		this.list.find('.ui-state-default').addClass('box-title');
	}, // redisplayAccordion
});

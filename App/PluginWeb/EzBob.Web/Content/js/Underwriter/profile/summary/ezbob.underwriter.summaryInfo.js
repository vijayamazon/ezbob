var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SummaryInfoView = Backbone.Marionette.ItemView.extend({
	template: "#profile-summary-template",

	initialize: function() {
		this.bindTo(this.model, "change sync", this.render, this);
	},

	events: {
		"keydown #CommentArea": "CommentAreaChanged",
		"click #commentSave": "saveComment"
	},

	CommentAreaChanged: function() {
		this.CommentSave.removeClass("disabled");
		this.CommentArea.css("border", "1px solid yellow");
	},

	saveComment: function() {
		var that = this;

		$.post(window.gRootPath + "Underwriter/Profile/SaveComment", {
			Id: this.model.get("Id"),
			comment: this.CommentArea.val()
		}).done(function() {
			that.CommentArea.css("border", "1px solid #ccc");
			that.CommentSave.addClass("disabled");
		}).fail(function(data) {
			that.CommentArea.css("border", "1px solid red");
			console.error(data.responseText);
		});

		return false;
	},

	serializeData: function() {
		return {
			m: this.model.toJSON()
		};
	},

	onRender: function() {
		this.CommentSave = this.$el.find("#commentSave");

		this.CommentArea = this.$el.find("#CommentArea");

		this.$el.find('a[data-bug-type]').tooltip({
			title: 'Report bug'
		});

		var isNew = this.model.get("MarketPlaces") && this.model.get("MarketPlaces").IsNew;

		$("#new-ribbon-marketplaces").toggle(Convert.toBool(isNew));

		if (!this.model.get('IsOffline'))
			this.$el.find('.offline').remove();
	},
});

EzBob.Underwriter.SummaryInfoModel = Backbone.Model.extend({
	idAttribute: "Id",
	urlRoot: window.gRootPath + "Underwriter/Profile/Index",
});

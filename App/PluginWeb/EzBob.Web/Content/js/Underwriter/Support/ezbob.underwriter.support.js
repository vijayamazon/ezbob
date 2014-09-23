var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SupportView = Backbone.Marionette.ItemView.extend({
	initialize: function() {
		this.model = new EzBob.Underwriter.SupportModel();
		this.model.on("change reset", this.render, this);
		this.model.fetch();
		this.preHeight = 20;
	},

	template: "#support-template",

	events: {
		"click pre": "preClicked",
		"click .reCheckMP": "recheckClicked",
		'click [data-sort-type]': 'sortClicked'
	},

	onRender: function() {
		this.$el.find("[data-sort-type]").css('cursor', 'pointer');

		this.$el.find("pre").tooltip({
			title: 'Click to see detail info'
		}).tooltip('fixTitle');

		this.$el.find("pre").height(this.preHeight).css("overflow", "hidden").css('cursor', 'pointer');

		this.$el.find('.arrow').hide();

		var arrow = this.$el.find("[data-sort-type=" + (this.model.get('sortField')) + "] .arrow");

		arrow.show();
		arrow.removeClass().addClass(this.model.get('sortType') === 'asc' ? 'arrow icon-arrow-up' : 'arrow icon-arrow-down');

		BlockUi("off");

		EzBob.handleUserLayoutSetting();
	},

	sortClicked: function(e) {
		BlockUi("on");

		var $el = $(e.currentTarget);
		var field = $el.data('sort-type');
		var currentField = this.model.get('sortField');
		var currentSortType = this.model.get('sortType');

		this.model.set({
			'sortField': field,
			'sortType': field !== currentField || currentSortType === 'desc' ? 'asc' : 'desc'
		}, {
			silent: true
		});

		this.model.fetch();
	},

	recheckClicked: function(e) {
		var $el = $(e.currentTarget);

		if ($el.hasClass('disabled'))
			return false;

		var self = this;
		var umi = $el.attr("umi");
		var mpType = $el.attr("marketplaceType");
		var customerId = this.model.customerId;

		var okFn = function() {
			$el.addClass('disabled');

			var xhr = $.get("" + window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", {
				customerId: customerId,
				umi: umi,
				marketplaceType: mpType
			});

			xhr.done(function(response) {
				if (response && response.error !== void 0)
					EzBob.ShowMessage(response.error, "Error occured");
				else
					EzBob.ShowMessage("Wait a few minutes", "The marketplace recheck is running. ", null, "OK");

				self.trigger("rechecked", {
					umi: umi,
					el: $el
				});
			});

			xhr.fail(function(data) {
				console.error(data.responseText);
			});
		};

		EzBob.ShowMessage("", "Are you sure?", okFn, "Yes", null, "No");
		return false;
	},

	preClicked: function(e) {
		var $el = $(e.currentTarget);
		var elHeight = $el.height();

		$el.height(elHeight !== this.preHeight ? this.preHeight : "auto");

		$el.tooltip('destroy');

		$el.tooltip({
			title: elHeight !== this.preHeight ? 'Click to see detail info' : 'Click to hide detail info'
		});

		$el.tooltip("enable").tooltip('fixTitle');
	},

	hide: function() {
		this.$el.hide();
		clearInterval(this.modelUpdater);
		BlockUi('off');
	},

	show: function() {
		var self = this;
		this.$el.show();

		this.modelUpdater = setInterval(function() {
			self.model.fetch();
		}, 2000);
	},

	serializeData: function() {
		return {
			model: this.model.get('models')
		};
	}
});

EzBob.Underwriter.SupportModel = Backbone.Model.extend({
	initialize: function() {
		this.set({
			sortField: 4,
			sortType: 'desc',
			models: []
		}, {
			silent: true
		});
	},

	urlRoot: function() {
		return "" + window.gRootPath + "Underwriter/Support/Index?sortField=" + (this.get('sortField')) + "&sortType=" + (this.get('sortType'));
	},
});

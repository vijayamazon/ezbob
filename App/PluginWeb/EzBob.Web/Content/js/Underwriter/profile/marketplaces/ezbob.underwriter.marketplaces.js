var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlaceModel = Backbone.Model.extend({
	idAttribute: "Id",

	initialize: function() {
		this.on('change reset', this.recalculate, this);
		this.recalculate();
	},

	recalculate: function() {
		var ai = this.get('AnalysisDataInfo');
		var accountAge = this.get('AccountAge');
		var monthSales = ai ? (ai.TotalSumofOrders1M || 0) * 1 : 0;
		var monthAnnualizedSales = ai ? (ai.TotalSumofOrdersAnnualized1M || 0) * 1 : 0;
		var anualSales = ai ? (ai.TotalSumofOrders12M || ai.TotalSumofOrders6M || ai.TotalSumofOrders3M || ai.TotalSumofOrders1M || 0) * 1 : 0;
		var inventory = ai && !isNaN(ai.TotalValueofInventoryLifetime * 1) ? ai.TotalValueofInventoryLifetime * 1 : "-";
		var pp = this.get("PayPal");

		if (pp) {
			monthSales = pp.GeneralInfo.MonthInPayments;
			monthAnnualizedSales = pp.GeneralInfo.MonthInPaymentsAnnualized;
			anualSales = pp.GeneralInfo.TotalNetInPayments;
		}

		var age = accountAge !== "-" && accountAge !== 'undefined' ? EzBob.SeniorityFormat(accountAge, 0) : "-";

		this.set({
			age: age,
			monthSales: monthSales,
			monthAnnualizedSales: monthAnnualizedSales,
			anualSales: anualSales,
			inventory: inventory
		}, {
			silent: true
		});
	}
});

EzBob.Underwriter.MarketPlaces = Backbone.Collection.extend({
	model: EzBob.Underwriter.MarketPlaceModel,

	url: function() {
		return "" + window.gRootPath + "Underwriter/MarketPlaces/Index/?id=" + this.customerId + "&history=" + this.history;
	}
});

EzBob.Underwriter.Affordability = Backbone.Model.extend({
	url: function() {
		return "" + window.gRootPath + "Underwriter/MarketPlaces/GetAffordabilityData/?id=" + this.customerId;
	}
});

EzBob.Underwriter.MarketPlacesView = Backbone.Marionette.ItemView.extend({
	template: "#marketplace-template",

	initialize: function() {
		var self = this;

		this.model.on("reset change sync", this.render, this);

		this.rendered = false;

		window.YodleeTryRecheck = function(result) {
			if (result.error) {
				return EzBob.ShowMessage(result.error, "Yodlee Recheck Error", "OK");
			} else {
				return EzBob.ShowMessage('Yodlee recheked successfully, refresh the page', null, "OK");
			}
		};

		EzBob.App.vent.on('ct:marketplaces.history', function() {
			self.$el.find('#hmrc-upload-container').hide().empty();
		});

		EzBob.App.vent.on('ct:marketplaces.uploadHmrc', function() {
			var oUploader = $('<div class="box-content"></div>');
			self.$el.find('#hmrc-upload-container').empty().append(oUploader);
			var uploadHmrcView = new EzBob.Underwriter.UploadHmrcView({
				el: oUploader,
				customerId: self.model.customerId,
				companyRefNum: self.options.personalInfoModel.get('CompanyExperianRefNum')
			});
			uploadHmrcView.render();
			self.$el.find('#hmrc-upload-container').show();
			$(".mps-tables").hide();
		});

		EzBob.App.vent.on('ct:marketplaces.uploadHmrcBack', function() {
			$(".mps-tables").show();
			self.$el.find('#hmrc-upload-container').hide().empty();
		});

		EzBob.App.vent.on('ct:marketplaces.enterHmrc', function() {
			EzBob.Underwriter.EnterHmrcView.execute(self.model.customerId, self.model);
		});

		EzBob.App.vent.on('ct:marketplaces.parseYodlee', function() {
			self.parseYodlee();
		});

		EzBob.App.vent.on('ct:marketplaces.parseYodleeBack', function() {
			self.$el.find(".mps-tables").show();
			self.$el.find('#parse-yodlee-container').hide().empty();
		});

		EzBob.App.vent.on('ct:marketplaces.addedFile', function() {
			self.model.fetch().done(function() {
				self.parseYodlee();
			});
		});

		return this;
	},

	onRender: function() {
		this.$el.find('.mp-error-description').tooltip({ placement: "bottom" });

		this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });

		_.each(this.$el.find('[data-original-title]'), function(elem) {
			$(elem).tooltip({ title: elem.getAttribute('data-original-title') });
		});

		if (this.detailView)
			this.detailView.render();

		var marketplacesHistoryDiv = this.$el.find("#marketplaces-history");

		this.marketPlacesHistory = new EzBob.Underwriter.MarketPlacesHistory();
		this.marketPlacesHistory.customerId = this.model.customerId;
		this.marketPlacesHistory.silent = true;

		this.marketPlaceHistoryView = new EzBob.Underwriter.MarketPlacesHistoryView({
			model: this.marketPlacesHistory,
			el: marketplacesHistoryDiv,
			customerId: this.model.customerId
		});

		return this;
	},

	events: {
		"click .tryRecheckYodlee": "tryRecheckYodlee",
		"click .reCheckMP": "reCheckmarketplaces",
		"click tbody tr": "rowClick",
		"click .mp-error-description": "showMPError",
		"click .renew-token": "renewTokenClicked",
		"click .disable-shop": "disableShop",
		"click .enable-shop": "enableShop"
	},

	rowClick: function(e) {
		if (e.target.getAttribute('href'))
			return;

		if (e.target.tagName === 'I')
			return;

		var id = e.currentTarget.getAttribute("data-id");
		if (!id)
			return;

		this.detailView = new EzBob.Underwriter.MarketPlaceDetailsView({
			model: this.model,
			currentId: id,
			customerId: this.model.customerId,
			personalInfoModel: this.options.personalInfoModel
		});

		EzBob.App.jqmodal.show(this.detailView);

		this.detailView.on("reCheck", this.reCheckmarketplaces, this);
		this.detailView.on("disable-shop", this.disableShop, this);
		this.detailView.on("enable-shop", this.enableShop, this);
		this.detailView.on("recheck-token", this.renewToken);
		this.detailView.customerId = this.model.customerId;
		this.detailView.render();
	},

	showMPError: function() {
		return false;
	},

	serializeData: function() {
		var isMarketplace = function(x) {
			var cg;
			if (!(EzBob.CgVendors.all()[x.get('Name')])) {
				return !x.get('IsPaymentAccount');
			}
			cg = EzBob.CgVendors.all()[x.get('Name')];
			return (cg.Behaviour === 0) && !cg.HasExpenses;
		};

		var data = {
			customerId: this.model.customerId,
			marketplaces: _.sortBy(_.pluck(_.filter(this.model.models, function(x) {
				return x && isMarketplace(x);
			}), "attributes"), "UWPriority"),
			accounts: _.sortBy(_.pluck(_.filter(this.model.models, function(x) {
				return x && !isMarketplace(x);
			}), "attributes"), "UWPriority"),
			hideAccounts: false,
			hideMarketplaces: false,
			summary: {
				monthSales: 0,
				anualSales: 0,
				inventory: 0,
				positive: 0,
				negative: 0,
				neutral: 0,
				monthAnnualizedSales: 0
			}
		};

		for (var i = 0; i < data.marketplaces.length; i++) {
			var m = data.marketplaces[i];

			if (m.Disabled === false)
				data.summary.monthSales += m.monthSales;

			if (m.Disabled === false)
				data.summary.anualSales += m.anualSales;

			if (m.Disabled === false)
				data.summary.monthAnnualizedSales += m.monthAnnualizedSales;

			if (m.Disabled === false)
				data.summary.inventory += m.inventory;

			data.summary.positive += m.PositiveFeedbacks;
			data.summary.negative += m.NegativeFeedbacks;
			data.summary.neutral += m.NeutralFeedbacks;
		}

		var total = data.summary.positive + data.summary.negative + data.summary.neutral;

		data.summary.rating = total > 0 ? data.summary.positive / total : 0;

		return data;
	},

	disableShop: function(e) {
		var self = this;
		var $el = $(e.currentTarget);
		var umi = $el.attr("umi");

		EzBob.ShowMessage("Disable shop", "Are you sure?", (function() {
			self.doEnableShop(umi, false);
		}), "Yes", null, "No");

		return false;
	},

	doEnableShop: function(umi, enabled) {
		var self = this;

		var url = enabled ? "" + window.gRootPath + "Underwriter/MarketPlaces/Enable" : "" + window.gRootPath + "Underwriter/MarketPlaces/Disable";

		var xhr = $.post(url, { umi: umi });

		xhr.done(function() {
			self.model.fetch();
		});
	},

	enableShop: function(e) {
		var self = this;
		var $el = $(e.currentTarget);
		var umi = $el.attr("umi");

		EzBob.ShowMessage("Enable shop", "Are you sure?", (function() {
			self.doEnableShop(umi, true);
		}), "Yes", null, "No");

		return false;
	},

	tryRecheckYodlee: function() { },

	reCheckmarketplaces: function(e) {
		var self = this;

		var $el = $(e.currentTarget);

		var umi = $el.attr("umi");

		var mpType = $el.attr("marketplaceType");

		var customerId = this.model.customerId;

		var okFn = function() {
			var xhr = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", {
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

	renewTokenClicked: function(e) {
		var umi = $(e.currentTarget).data("umi");
		this.renewToken(umi);
		return false;
	},

	renewToken: function(umi) {
		var xhr = $.post("" + window.gRootPath + "Underwriter/MarketPlaces/RenewEbayToken", {
			umi: umi
		});

		xhr.done(function() {
			EzBob.ShowMessage("Renew started successfully", "Successfully");
		});
	},

	parseYodlee: function() {
		var parseYodleeView = new EzBob.Underwriter.ParseYodleeView({
			el: this.$el.find('#parse-yodlee-container'),
			customerId: this.model.customerId,
			model: this.model
		});

		parseYodleeView.render();

		this.$el.find('#parse-yodlee-container').show();

		$(".mps-tables").hide();

		return this;
	}
});

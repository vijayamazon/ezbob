var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlaceModel = Backbone.Model.extend({
	idAttribute: "Id",

	initialize: function() {
		this.on('change reset', this.recalculate, this);
		this.recalculate();
	},

	recalculate: function() {
		var age = EzBob.SeniorityFormat(this.get('AccountAge'), 0);
		this.set({age: age}, {silent: true});
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

EzBob.Underwriter.MarketPlacesView = EzBob.ItemView.extend({
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
		this.reloadRevenue();

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

	reloadRevenue: function() {
		this.startReloadRevenue();

		this.cancelUpdateRevenue();

		var self = this;

		var oRequest = $.getJSON(
			window.gRootPath + 'Underwriter/MarketPlaces/GetCustomerManualAnnualRevenue',
			{ nCustomerID: this.model.customerId, }
		);

		oRequest.done(function(oResponse) {
			self.drawRevenue(oResponse, 'load');
		}); // on success

		oRequest.fail(function() {
			self.$el.find('.revenue-value').text('failed to load');
		}); // on fail

		oRequest.always(_.bind(this.finishReloadRevenue, this));
	}, // reloadRevenue

	finishReloadRevenue: function() {
		this.$el.find('.manual-annualized-revenue-loading').addClass('hide');
		this.$el.find('.manual-annualized-revenue').removeClass('hide');
	}, // finishReloadRevenue

	startReloadRevenue: function() {
		this.$el.find('.manual-annualized-revenue-loading').removeClass('hide');
		this.$el.find('.manual-annualized-revenue').addClass('hide');

		this.$el.find('.alibaba-no-revenue').addClass('hide');
	}, // startReloadRevenue

	drawRevenue: function(oResponse, sAction) {
		this.cancelUpdateRevenue();

		if (!oResponse.success) {
			this.$el.find('.revenue-value').text('failed to ' + sAction);
			return;
		} // if

		if (!oResponse.has_value) {
			this.$el.find('.revenue-value').text('none');
			this.$el.find('.alibaba-no-revenue').toggleClass('hide', !oResponse.is_alibaba);
			return;
		} // if

		this.$el.find('.revenue-value').text(
			oResponse.value.Revenue + ' since ' +
			EzBob.formatDate(oResponse.value.EntryTime) + ' (' +
			oResponse.value.Comment + ')'
		);
	}, // drawRevenue

	showUpdateRevenueValue: function() {
		this.$el.find('.show-update-revenue-value').addClass('hide');
		this.$el.find('.update-revenue-value').removeClass('hide');

		this.$el.find('.new-revenue-comment').val('');

		this.toggleUpdateRevenueEnabled();

		this.$el.find('.new-revenue-value').val('').focus();
	}, // showUpdateRevenueValue

	doUpdateRevenue: function() {
		this.toggleUpdateRevenueEnabled();

		if (!this.isSomethingEnabled('.do-update-revenue'))
			return;

		var nRevenue = parseFloat(this.$el.find('.new-revenue-value').val());

		var sComment = $.trim(this.$el.find('.new-revenue-comment').val());

		this.startReloadRevenue();

		var oRequest = $.post( window.gRootPath + 'Underwriter/MarketPlaces/SetCustomerManualAnnualRevenue', {
			nCustomerID: this.model.customerId,
			nRevenue: nRevenue,
			sComment: sComment,
		});

		var self = this;

		oRequest.done(function(oResponse) {
			self.drawRevenue(oResponse, 'update');
		}); // on success

		oRequest.fail(function() {
			EzBob.ShowMessage('Failed to update customer annualized revenue.', 'Error');
		}); // on fail

		oRequest.always(_.bind(this.finishReloadRevenue, this));
	}, // doUpdateRevenue

	cancelUpdateRevenue: function() {
		this.$el.find('.show-update-revenue-value').removeClass('hide');
		this.$el.find('.update-revenue-value').addClass('hide');

		this.$el.find('.new-revenue-value, .new-revenue-comment').val('');
		this.toggleUpdateRevenueEnabled();
	}, // cancelUpdateRevenue

	toggleUpdateRevenueEnabled: function() {
		var bRevenue = !isNaN(parseFloat(this.$el.find('.new-revenue-value').val()));

		var bComment = !!$.trim(this.$el.find('.new-revenue-comment').val());

		this.setSomethingEnabled('.do-update-revenue', bRevenue && bComment);
	}, // toggleUpdateRevenueEnabled

	events: {
		"click .tryRecheckYodlee": "tryRecheckYodlee",
		"click .reCheckMP": "reCheckmarketplaces",
		"click tbody tr": "rowClick",
		"click .mp-error-description": "showMPError",
		"click .renew-token": "renewTokenClicked",
		"click .disable-shop": "disableShop",
		"click .enable-shop": "enableShop",
		'click .show-update-revenue-value': 'showUpdateRevenueValue',
		'click .do-update-revenue': 'doUpdateRevenue',
		'click .cancel-update-revenue': 'cancelUpdateRevenue',

		'click .new-revenue-comment': 'toggleUpdateRevenueEnabled',
		'change .new-revenue-comment': 'toggleUpdateRevenueEnabled',
		'paste .new-revenue-comment': 'toggleUpdateRevenueEnabled',
		'cut .new-revenue-comment': 'toggleUpdateRevenueEnabled',
		'keyup .new-revenue-comment': 'toggleUpdateRevenueEnabled',
		'blur .new-revenue-comment': 'toggleUpdateRevenueEnabled',

		'click .new-revenue-value': 'toggleUpdateRevenueEnabled',
		'change .new-revenue-value': 'toggleUpdateRevenueEnabled',
		'paste .new-revenue-value': 'toggleUpdateRevenueEnabled',
		'cut .new-revenue-value': 'toggleUpdateRevenueEnabled',
		'keyup .new-revenue-value': 'toggleUpdateRevenueEnabled',
		'blur .new-revenue-value': 'toggleUpdateRevenueEnabled',
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
		var data = {
			customerId: this.model.customerId,
			marketplaces: _.sortBy(_.pluck(_.filter(this.model.models, function(mp) {
				return mp && !mp.get('IsPaymentAccount');
			}), "attributes"), "UWPriority"),
			accounts: _.sortBy(_.pluck(_.filter(this.model.models, function(mp) {
				return mp && mp.get('IsPaymentAccount');
			}), "attributes"), "UWPriority"),
			hideAccounts: false,
			hideMarketplaces: false,
			summary: {
				monthSales: 0,
				anualSales: 0,
				positive: 0,
				negative: 0,
				neutral: 0,
				monthAnnualizedSales: 0
			}
		};

		for (var i = 0; i < data.marketplaces.length; i++) {
			var m = data.marketplaces[i];

			if (m.Disabled === false) {
				data.summary.monthSales += m.MonthSales;
				data.summary.anualSales += m.AnnualSales;
				data.summary.monthAnnualizedSales += m.MonthSalesAnnualized;
				
				data.summary.positive += m.PositiveFeedbacks;
				data.summary.negative += m.NegativeFeedbacks;
				data.summary.neutral += m.NeutralFeedbacks;
			}
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

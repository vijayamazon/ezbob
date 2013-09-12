var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MarketPlaceDetailModel = Backbone.Model.extend({
});

EzBob.Underwriter.MarketPlaceDetails = Backbone.Collection.extend({
	model: EzBob.Underwriter.MarketPlaceDetailModel,
	url: function () {
		return window.gRootPath + "Underwriter/MarketPlaces/Details/" + this.makertplaceId;
	}
});

EzBob.Underwriter.MarketPlaceDetailsView = Backbone.Marionette.View.extend({
	initialize: function () {
		this.template = _.template($('#marketplace-values-template').html());
	},
	render: function () {
		var aryCGAccounts = $.parseJSON($('div#cg-account-list').text());

		var shop = this.model.get(this.options.currentId);
		// drawChart(shop.get("Id"));
		var data = { marketplaces: [], accounts: [], summary: null, customerId: this.options.customerId };

		var sTargetList = '';

		var cg = aryCGAccounts[shop.get('Name')];
		if (cg)
			sTargetList = ((cg.Behaviour == 0) && !cg.HasExpenses) ? 'marketplaces' : 'accounts';
		else
			sTargetList = shop.get('IsPaymentAccount') ? 'accounts' : 'marketplaces';

		data[sTargetList].push(shop.toJSON());

		data.hideAccounts = data.accounts.length == 0;
		data.hideMarketplaces = data.marketplaces == 0;

		this.$el.html(this.template(data));
		this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });
	    
		var oDataTableArgs = {
		    aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
		    iDisplayLength: 100,
		    asSorting: [],
		    aoColumns:[{ sType: "string" }, { sType: "date" }, { sType: "formatted-num" }, { sType: "string" }, { sType: "string" }, { sType: "string" }]
		};
		this.$el.find('.YodleeTransactionsTable').dataTable(oDataTableArgs);


		return this;
	},
	events: {
		"click .reCheckMP": "reCheck",
		"click .reCheck-paypal": "reCheckPayPal",
		"click .renew-token": "renewTokenClicked",
		"click .disable-shop": "diShop",
		"click .enable-shop": "enShop"
	},
	renewTokenClicked: function (e) {
		var umi = $(e.currentTarget).data("umi");
		this.trigger("recheck-token", umi);
	},
	reCheck: function (e) {
		this.trigger("reCheck", e);
		return false;
	},
	reCheckPayPal: function (e) {
		this.trigger("reCheck-PayPal", e);
		return false;
	},
	enShop: function (e) {
		this.trigger("enable-shop", e);
		return false;
	},
	diShop: function (e) {
		this.trigger("disable-shop", e);
		return false;
	},
	recheckAskville: function (e) {
		var el = $(e.currentTarget);
		var guid = el.attr("data-guid");
		var marketplaceId = el.attr("data-marketplaceId");
		EzBob.ShowMessage(
			"",
			"Are you sure?",
			function () {
				BlockUi('on');
				$.post(window.gRootPath + "Customer/AmazonMarketPlaces/Askville", { askvilleGuid: guid, customerMarketPlaceId: marketplaceId })
					.done(function (askvilleStatus) {
						$("#recheck-askville").closest("tr").find('.askvilleStatus').text(askvilleStatus);
						EzBob.ShowMessage("Successfully", "The askville recheck was starting. ", null, "OK");
					})
					.done(function () {
						BlockUi('off');
					});
			},
			"Yes", null, "No");
		return false;
	},
	jqoptions: function () {
		return {
			modal: true,
			resizable: false,
			title: this.model.get('Name'),
			position: "center",
			draggable: false,
			width: "73%",
			height: Math.max(window.innerHeight * 0.9, 600),
			dialogClass: "marketplaceDetail"
		};
	}
});
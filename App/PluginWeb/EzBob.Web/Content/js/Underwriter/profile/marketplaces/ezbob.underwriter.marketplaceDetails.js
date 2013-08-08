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
        var shop = this.model.get(this.options.currentId);
        drawChart(shop.get("Id"));
        var data = { marketplaces: [], accounts: [], summary: null, customerId: this.options.customerId };
        data[shop.get("IsPaymentAccount") ? "accounts" : "marketplaces"].push(shop.toJSON());

        data.hideAccounts = data.accounts.length == 0;
        data.hideMarketplaces = data.marketplaces == 0;

        this.$el.html(this.template(data));
        this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });

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
    recheckAskville: function(e) {
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
                    .done(function() {
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
            height: "600",
            dialogClass: "marketplaceDetail"
        };
    }
});
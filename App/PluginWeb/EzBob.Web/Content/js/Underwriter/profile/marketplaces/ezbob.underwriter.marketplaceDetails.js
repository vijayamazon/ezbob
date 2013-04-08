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

EzBob.Underwriter.MarketPlaceDetailsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#marketplace-values-template').html());
    },
    render: function () {
        var that = this;
        var shop = this.model.at(this.options.currentId);
        var content = (this.template({ marketplaces: [shop.toJSON()], summary: null, customerId: this.options.customerId }));
        if (this.container) {
            this.container.html(content);
        } else {
            this.container = $('<div/>');
            this.container.dialog({
                modal: true,
                resizable: false,
                title: that.model.get('Name'),
                position: "center",
                draggable: false,
                width: "73%",
                height: "600",
                dialogClass: "marketplaceDetail",
                close: function () { $(this).remove(); },
                open: function () {
                    $(this).html(content);
                }
            });
        }
        
        this.container.find("#recheck-askville").click(function (e) {
            that.recheckAskville.call(that, e);
        });
        
        this.container.find(".reCheck-amazon, .reCheck-ebay").click(function (e) {
            that.reCheck.call(that, e);
        });
        
        this.container.find(".renew-token").click(function (e) {
            that.renewTokenClicked.call(that, e);
        });
        this.container.find('a[data-bug-type]').tooltip({ title: 'Report bug' });
        return this;
    },
    events: {
        "click .reCheck-amazon": "reCheck",
        "click .reCheck-ebay": "reCheck",
        "click .renew-token": "renewTokenClicked"
    },
    
    renewTokenClicked: function (e) {
        var umi = $(e.currentTarget).data("umi");
        this.trigger("recheck-token", umi);
    },
    reCheck: function (e) {
        this.trigger("reCheck", e);
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
    }
});
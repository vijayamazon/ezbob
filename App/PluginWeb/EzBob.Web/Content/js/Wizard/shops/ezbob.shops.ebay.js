///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
var EzBob = EzBob || {};

EzBob.EbayStoreModel = Backbone.Model.extend({
    defaults: {
        displayName: null
    }
});

EzBob.EbayStoreModels = Backbone.Collection.extend({
    model: EzBob.EbayStoreModel,
    url: window.gRootPath + 'Customer/EbayMarketPlaces'
});

EzBob.EbayStoreInfoView = Backbone.View.extend({
    events: {
        "click .back": "back"
    },
    back: function () {
        this.trigger('back');
        return false;
    },
    initialize: function () {
        var that = this;
        
        EzBob.CT.bindShopToCT(this, 'ebay');
        
        if (!window.AlertToken){
            window.AlertToken = function(name) {
                $.getJSON(window.gRootPath + "Customer/EbayMarketplaces/FetchToken?callback=?", { username: name })
                    .success(function (result) {
                        if (result.error) {
                            EzBob.App.trigger('error', result.error);
                            $.colorbox.close();
                            that.trigger('back');
                            return;
                        }
                        EzBob.App.trigger('info', result.msg);
                        that.trigger('back');
                        that.trigger('completed');
                        
                    })
                    .error(function() {
                        EzBob.App.trigger('error', 'Ebay Account Failed to Add');
                        $.colorbox.close();
                        that.trigger('back');
                        
                    });
            };
        };
    },
    getDocumentTitle: function() {
        return "Ebay: Link Ebay Account";
    }
});

EzBob.EbayButtonView = EzBob.StoreButtonView.extend({
    initialize: function () {
        this.constructor.__super__.initialize.apply(this, [{ name: "eBay", logoText: "", shops: this.model }]);
    },
    update: function () {
        var that = this;
        var xhr = this.model.fetch();
        xhr.done(function() {
            that.model.trigger("sync");
        });
    }
});

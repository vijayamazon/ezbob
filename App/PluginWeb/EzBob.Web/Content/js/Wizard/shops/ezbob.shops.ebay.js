///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/Wizard/ezbob.storebutton.js" />
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
                    .success(function(result) {
                        if (result.error) {
                            EzBob.App.trigger('error', result.error);
                            that.trigger('back');
                            return;
                        }
                        EzBob.App.trigger('info', result.msg);
                        that.trigger('completed');
                        that.trigger('back');
                    })
                    .error(function() {
                        EzBob.App.trigger('error', 'Ebay account failed to add');
                    });
            };
        };
    },
    render: function () {
        this.$el.html($('#ebay-store-info').html());
        return this;
    }
});

EzBob.EbayButtonView = EzBob.StoreButtonWithListView.extend({
    initialize: function () {
        this.listView = new EzBob.StoreListView({ model: this.model });
        this.constructor.__super__.initialize.apply(this, [{ name: "ebay", logoText:"" }]);
    },
    update: function () {
        this.model.fetch();
    }
});

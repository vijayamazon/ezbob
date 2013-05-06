var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.StoresView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#stores-template').html());

        this.storeInfoStepModel = new EzBob.StoreInfoStepModel({
            ebayMarketPlaces: this.model.get('ebayMarketPlaces'),
            amazonMarketPlaces: this.model.get('amazonMarketPlaces'),
            ekmShops: this.model.get('ekmShops'),
            volusionShops: this.model.get('volusionShops'),
            payPointAccounts: this.model.get('payPointAccounts'),
            paypalAccounts: this.model.get('paypalAccounts')
        });

        this.storeInfoView = new EzBob.StoreInfoView({ model: this.storeInfoStepModel });

        this.model.on('change:ebayMarketPlaces change:amazonMarketPlaces change:ekmShops change:volusionShops change:payPointAccounts change:paypalAccounts', this.render, this);
        this.storeInfoView.on('previous', this.render, this);
        this.storeInfoView.on('completed', this.completed, this);

    },
    events: {
        'click .add-store': 'addStore'
    },
    render: function () {
        this.storeInfoView.render();
        
        if (!this.content) {
            this.content = $('<div/>');
            this.content.appendTo(this.$el);
            this.storeInfoView.$el.appendTo(this.$el);
        }

        this.renderTable();
        
        this.content.show();
        this.storeInfoView.$el.hide();
        this.storeInfoView.$el.find(".next ").hide();
        return this;
    },
    
    renderTable: function() {
        this.content.html(this.template({ stores: this.storeInfoStepModel.getStores() }));
    },

    addStore: function () {
        this.content.hide();
        this.storeInfoView.$el.show();
        return false;
    },
    completed: function() {
        var that = this;
        var xhr = this.model.fetch();
        xhr.done(function () {
            that.storeInfoStepModel.set({
                ebayMarketPlaces: that.model.get('ebayMarketPlaces'),
                amazonMarketPlaces: that.model.get('amazonMarketPlaces'),
                ekmShops: that.model.get('ekmShops'),
                volusionShops: that.model.get('volusionShops'),
                payPointAccounts: that.model.get('payPointAccounts'),
                paypalAccounts: that.model.get('paypalAccounts')
            });
            that.renderTable();
        });
    }
});
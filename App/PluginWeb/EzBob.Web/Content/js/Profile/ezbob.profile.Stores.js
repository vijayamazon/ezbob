var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.StoresView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#stores-template').html());

		var modelArgs = {
			ebayMarketPlaces: this.model.get('ebayMarketPlaces'),
			amazonMarketPlaces: this.model.get('amazonMarketPlaces'),
			ekmShops: this.model.get('ekmShops'),
			freeAgentAccounts: this.model.get('freeAgentAccounts'),
			sageAccounts: this.model.get('sageAccounts'),
			payPointAccounts: this.model.get('payPointAccounts'),
			yodleeAccounts: this.model.get('yodleeAccounts'),
			paypalAccounts: this.model.get('paypalAccounts')
		};

		var oShops = {};
		var cgShops = this.model.get('cgShops');

		for (var i in cgShops) {
			if (!cgShops.hasOwnProperty(i))
				continue;

			var o = cgShops[i];

			if (!oShops[o.storeInfoStepModelShops])
				oShops[o.storeInfoStepModelShops] = [];

			oShops[o.storeInfoStepModelShops].push(o);
		} // for each cg shop

		var sCgOnChange = '';
		for (var i in oShops) {
			modelArgs[i] = oShops[i];
			sCgOnChange += ' change:' + i;
		} // for

		modelArgs.isOffline = this.model.get('IsOffline');
		modelArgs.isProfile = this.model.get('IsProfile');
		this.storeInfoStepModel = new EzBob.StoreInfoStepModel(modelArgs);

        this.storeInfoView = new EzBob.StoreInfoView({ model: this.storeInfoStepModel });

        this.model.on('change:ebayMarketPlaces change:amazonMarketPlaces change:ekmShops change:freeAgentAccounts change:sageAccounts change:payPointAccounts change:paypalAccounts change:yodleeAccounts' + sCgOnChange, this.render, this);

        this.storeInfoView.on('previous', this.render, this);
        this.storeInfoView.on('completed', this.completed, this);
        
        var that = this;
        window.YodleeRefreshAccountRetry = function () {
            that.attemptsLeft = (that.attemptsLeft || 5) - 1;
            return {
                url: that.$el.find('#refreshYodleeBtn1').attr('href'),
                attemptsLeft: that.attemptsLeft
            };
        };
        window.YodleeAccountUpdateError = function (msg) {
            $.colorbox.close();
            EzBob.App.trigger('error', msg);
        };
    },
    events: {
        'click .add-store': 'addStore',
        'click .updateYodlee': 'updateYodleeClicked'
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

    updateYodleeClicked: function (el) {
        var displayName = $(el.currentTarget).attr('data-bank');
        var url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/RefreshYodlee/?displayName=" + displayName;
        
        this.$el.find("#refreshYodleeBtn1").attr("href", url);
        this.$el.find('.refresh_yodlee_help1').colorbox({ href: "#refresh_yodlee_help1", inline: true, transition: 'none', open: true });
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
                freeAgentAccounts: that.model.get('freeAgentAccounts'),
                sageAccounts: that.model.get('sageAccounts'),
                cgShops: that.model.get('cgShops'),
                yodleeAccounts: that.model.get('yodleeAccounts'),
                payPointAccounts: that.model.get('payPointAccounts'),
                paypalAccounts: that.model.get('paypalAccounts')
            });
            that.renderTable();
        });
    }
});
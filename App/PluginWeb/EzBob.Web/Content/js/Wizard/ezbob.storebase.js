///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
var EzBob = EzBob || {};

/* View represent store selector */
EzBob.StoreInfoBaseView = Backbone.View.extend({
    initialize: function () {

        var that = this;

        _.each(this.stores, function (store) {
            store.button.on('selected', that.connect, that);
            store.view.on('completed', _.bind(that.completed, that, store.button.name));
            store.view.on('back', that.back, that);
            store.button.on('ready', that.ready, that);
        });

        this.storeList = $($('#store-info').html());

        EzBob.App.on('ct:storebase.' + this.name + '.connect', this.connect, this);

        this.isReady = false;

    },

    completed: function (name) {
        this.stores[name].button.update();
        this.$el.find('>div').hide();
        this.storeList.show();
        this.trigger('completed');
    },
    back: function () {
        this.$el.find('>div').hide();
        this.storeList.show();
        $(document).attr("title", this.oldTitle);
        return false;
    },
    next: function () {
        this.trigger('next');
        EzBob.App.trigger('clear');
        return false;
    },
    ready: function (name) {

        this.trigger('ready', name);
        if (!this.isReady) {
            this.isReady = true;
            this.$el.find('.next').show();
        }
    },
    render: function () {
        var that = this,
            buttonList = this.storeList.find('.buttons-list'),
            row = null,
            i = 0;

        _.each(this.stores, function (store) {
        	if (!store.active)
        		return;

            if ((i % 2) == 0) {
                row = $("<div class='row-fluid'/>");
                row.appendTo(buttonList);
            }
            i++;
            store.button.render().$el.appendTo(row);
            store.view.render().$el.hide().appendTo(that.$el);
        });

        this.storeList.appendTo(this.$el);

        if (this.stores["bank-account"] != null) {
            if (this.stores['bank-account'].button.model.get('bankAccountAdded')) {
                that.ready();
            }
        }

        return this;
    },
    events: {
        "click a.connect-store": "close",
        "click a.next": "next",
        "click a.back-step": "previousClick"
    },

    previousClick: function () {
        this.trigger('previous');
        return false;
    },

    connect: function (storeName) {
        EzBob.CT.recordEvent('ct:storebase.' + this.name + '.connect', storeName);
        this.$el.find('>div').hide();
        this.stores[storeName].view.$el.show();
        this.oldTitle = $(document).attr("title");
        this.setDocumentTitle(storeName);
        this.setFocus(storeName);
        return false;
    },
    
    setFocus: function (storeName) {
        switch (storeName) {
            case "ekm":
                this.$el.find('#ekm_login').focus();
                break;
            case 'volusion':
                this.$el.find('#volusion_login').focus();
                break;
            case "payPoint":
                this.$el.find('#payPoint_login').focus();
                break;
            case 'bank-account':
                this.$el.find('#AccountNumber').focus();
                break;
            default:
        }
    },
    
    setDocumentTitle: function (storeName) {
        switch (storeName) {
            case "amazon":
                $(document).attr("title", "Wizard 2 Amazon: Link Your Amazon Shop | EZBOB");
                break;
            case 'ebay':
                $(document).attr("title", "Wizard 2 Ebay: Link Your Ebay Shop | EZBOB");
                break;
            case 'bank-account':
                $(document).attr("title", "Wizard 3 Bank: Bank Account Details | EZBOB");
                break;
            case 'paypal':
                $(document).attr("title", "Wizard 3 PayPal: Link Your PayPal Account | EZBOB");
                break;
            default:
        }
    },
    close: function () {
    }
});
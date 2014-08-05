var EzBob = EzBob || {};

EzBob.StoreButtonView = Backbone.Marionette.ItemView.extend({
    template: "#store-button-template",
    initialize: function (options) {
        this.name = options.name;
        this.description = options.description;
        this.mpAccounts = options.mpAccounts.get('customer').get('mpAccounts');
        this.shops = this.mpAccounts ? _.where(this.mpAccounts, { MpName: this.name }) : [];
        this.shopClass = options.name.replace(' ', '');
    },
    serializeData: function () {
        var data;
        data = {
            name: this.name,
            shopClass: this.shopClass,
            shopDescription: this.description
        };
        return data;
    },
    onRender: function () {
        var btn, oHelpWindowTemplate, oLinks, sTitle, sTop;
        btn = this.$el.find('.marketplace-button-account-' + this.shopClass);
        this.$el.removeClass('marketplace-button-full marketplace-button-empty');
        sTitle = (this.shops.length ? 'Some' : 'No') + ' accounts linked. Click to link ';
        if (this.shops.length) {
            this.$el.addClass('marketplace-button-full');
            sTitle += 'more.';
        } else {
            this.$el.addClass('marketplace-button-empty');
            sTitle += 'one.';
        }
        this.$el.attr('title', sTitle);
        switch (this.shopClass) {
            case 'eBay':
            case 'paypal':
            case 'FreeAgent':
            case 'Sage':
                oHelpWindowTemplate = _.template($('#store-button-help-window-template').html());
                this.$el.append(oHelpWindowTemplate(this.serializeData()));
                oLinks = JSON.parse($('#store-button-help-window-links').html());
                this.$el.find('.help-window-continue-link').attr('href', oLinks[this.shopClass]);
                btn.attr('href', '#' + this.shopClass + '_help');
                btn.colorbox({
                    inline: true,
                    transition: 'elastic',
                    onClosed: function () {
                        var oBackLink;
                        oBackLink = $('#link_account_implicit_back');
                        if (oBackLink.length) {
                            EzBob.UiAction.saveOne('click', oBackLink);
                        }
                    }
                });
                break;
            default:
                btn.click((function (_this) {
                    return function (evt) {
                        EzBob.App.trigger('ct:storebase.shops.connect', _this.shopClass);
                        evt.preventDefault();
                    };
                })(this));
        }
        if ($.browser.name.toLowerCase() === 'firefox') {
            sTop = '1px';
        } else if (document.all) {
            sTop = '2px';
        } else {
            sTop = 0;
        }
        btn.hoverIntent((function (evt) {
            $('.onhover', this).animate({
                top: sTop,
                opacity: 1
            });
        }), (function (evt) {
            $('.onhover', this).animate({
                top: '60px',
                opacity: 0
            });
        }));
    },
    isAddingAllowed: function () {
        return true;
    },
    update: function (data) {
        this.shops = data ? this.shops = _.where(data, {
            MpName: this.name
        }) : [];
    }
});
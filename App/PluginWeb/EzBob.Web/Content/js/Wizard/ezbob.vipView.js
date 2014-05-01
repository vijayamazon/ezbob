var EzBob = EzBob || {};

EzBob.VipView = Backbone.Marionette.ItemView.extend({
    template: "#vip-template",
    initialize: function (options) {
        this.model = options.model;
        EzBob.App.on('wizard:vipRequest', this.requestVipBtnClicked, this);
    }, // initialize

    events: {
        "click #vipRequestBtn": "vipClicked"
    }, // events

    vipClicked: function () {
        var that = this;
        this.model.fetch().done(function() {
            if (that.model.get('VipFullName') && that.model.get('VipPhone') && that.model.get('VipEmail')) {
                that.submitRequest();
            } else {
                $.colorbox({ href: '#vip_help', inline: true, open: true, returnFocus: true, trapFocus: true, });
                $('input[name="VipPhone"]').numericOnly(11);
                if (that.model.get('VipPhone')) {
                    $('input[name="VipPhone"]').val(that.model.get('VipPhone')).change().attr('readonly', 'readonly');
                }

                if (that.model.get('VipEmail')) {
                    $('input[name="VipEmail"]').val(that.model.get('VipEmail')).change().attr('readonly','readonly');
                }
            }
        });
    },

    onRender: function() {
        EzBob.UiAction.registerView(this);
    },

    requestVipBtnClicked: function (data) {
        var that = this;
        _.each(data, function (obj) { that.model.set(obj.name, obj.value); });
        this.submitRequest();
    },

    submitRequest: function() {
        this.model.save();
        this.$el.hide();
        EzBob.App.trigger('info', 'Your VIP request submited. We will contact you asap.');
    }
});

EzBob.VipModel = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Customer/Wizard/Vip";
    },
});
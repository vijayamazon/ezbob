var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ChangeAddressModel = Backbone.Model.extend({});

EzBob.Underwriter.ChangeAddressView = Backbone.Marionette.ItemView.extend({
    template: "#change-address-template",
    initialize: function() {
        this.model.on("sync", this.render, this);
        this.model.set('customerAddress', new EzBob.AddressModels());
    },//initialize
    events: {
        'click .btn-update-address': 'updateAddressClicked'
    },//events
    onRender: function() {
        var oAddressContainer = this.$el.find('#customerAddress');

        this.addressView = new EzBob.AddressView({
            model: this.model.get('customerAddress'),
            name: 'customerAddress',
            max: 1,
            uiEventControlIdPrefix: oAddressContainer.attr('data-ui-event-control-id-prefix')
        });

        var that = this;
        this.model.get('customerAddress').on('all', function() {
            that.inputAddressChanged();
        });

        this.addressView.render().$el.appendTo(oAddressContainer);
        this.ui.addressCaption.hide();

        this.ui.customerId.val(this.model.get('customerId'));
        return this;
    },//onRender
    ui: {
        btnUpdateAddress: '.btn-update-address',
        addressCaption: '.addressCaption',
        form: 'form.change-address-form',
        customerId: '#customerId'
    },//ui
    updateAddressClicked: function() {
        BlockUi('on');

        if (this.ui.btnUpdateAddress.hasClass('disabled')) {
            EzBob.ShowMessage("You must enter the new address first");
            return false;
        }
        var data = this.ui.form.serializeArray();
        
        var xhr = $.post(window.gRootPath + "Underwriter/CrossCheck/ChangeAddress", data);
        var that = this;
        xhr.success(function () {
            that.trigger('addressChanged', that.model.get('customerId'));
            that.close();
        });

        xhr.always(function() {
            BlockUi('off');
        });

        return false;
    },//updateAddressClicked
    inputAddressChanged: function () {
        if (this.model.get('customerAddress').length === 1) {
            this.ui.btnUpdateAddress.removeClass('disabled');
        } else {
            this.ui.btnUpdateAddress.addClass('disabled');
        }

    },//inputAddressChanged
    jqoptions: function () {
        return {
            modal: true,
            title: "Change address",
            position: "center",
            width: "600",
            dialogClass: "change-address"
        };
    }//jqoptions
});//EzBob.Underwriter.ChangeAddressView
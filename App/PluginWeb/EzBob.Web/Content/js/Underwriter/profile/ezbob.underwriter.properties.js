var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Properties = Backbone.Model.extend({
    url: function () {
        return window.gRootPath + "Underwriter/Properties/Index/" + this.customerId;
    }
});

EzBob.Underwriter.PropertiesView = Backbone.Marionette.ItemView.extend({
    template: '#propertiesTemplate',
    initialize: function () {
        this.model.on("reset sync", this.render, this);
    },
    onRender: function() {
        $(document).ready(function () {
            $('#ownedPropertiesTable').dataTable();
        });
    },
    serializeData: function () {
        return { model: this.model.toJSON() };
    },
    events: {
        "click .zooplaRecheck": "recheckZoopla",
        "click #landregistry": "showLandRegistry",
    },
    recheckZoopla: function () {
        BlockUi("On");
        var that = this;
        var xhr = $.get(window.gRootPath + "Underwriter/Properties/Zoopla/?customerId=" + this.customerId + "&recheck=true");
        xhr.done(function () {
            that.render(that.model);
        });
        xhr.always(function () {
            BlockUi("Off");
        });
    },
    showLandRegistry: function (el) {
        var address = $(el.currentTarget).attr('data-address');
        var postcode = $(el.currentTarget).attr('data-postcode');
        var that = this;
        BlockUi("On");
        $.post(window.gRootPath + "Underwriter/Properties/LandRegistryEnquiries/?customerId=" + this.customerId, function (data) {
            BlockUi("Off");
            that.lrEnqView = new EzBob.LandRegistryEnquiryView({ model: { postcode: postcode, address: address, customerId: that.customerId, titles: data.titles } });
            EzBob.App.vent.on('landregistry:retrieved', that.landRegistryRetrieved, that);
            EzBob.App.jqmodal.show(that.lrEnqView);
        });
    },
    landRegistryRetrieved: function () {
        BlockUi("Off");
        Backbone.history.loadUrl();
        return false;
    },
});


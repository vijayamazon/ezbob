var EzBob = EzBob || {};

EzBob.LandRegistryEnquiryView = Backbone.Marionette.ItemView.extend({
    template: '#landregistry-enquiry-template',
    initialize: function (options) {
        this.model = options.model;
        return this;
    },
    onRender: function () {
        this.$el.find('input[name="postCode"]').val(this.model.postcode);
        return this;
    },
    events: {
        "click .btnOk": "okClicked",
    },
    serializeData: function() {
        return { data: this.model };
    },
    okClicked: function () {
        var that = this;
        var data = this.$el.find('#landregistry-enquiry-form').serializeArray();
        data.push({ name: "customerId", value: this.model.customerId });
        BlockUi("On");
        $.post(window.gRootPath + "Underwriter/CrossCheck/LandRegistryEnquiry", data, function (response) {
            if (response && response.titles && response.titles.length == 1) {
                $.post(window.gRootPath + "Underwriter/CrossCheck/LandRegistry/?customerId=" + that.model.customerId + "&titleNumber=" + response.titles[0].TitleNumber, function (data) {
                    this.landRegistryRetrieved();
                    BlockUi("Off");
                });
            } else {
                that.enqRes = new EzBob.LandRegistryEnquiryResultsView({ model:new Backbone.Model(response), customerId: that.model.customerId });
                EzBob.App.jqmodal.show(that.enqRes);
                BlockUi("Off");
            }
        });
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: "Land Registry Enquiry",
            position: "center",
            draggable: true,
            dialogClass: "landregistryEnquiry",
            width: 900
        };
    },
    landRegistryRetrieved: function () {
        EzBob.App.vent.trigger('LandRegistry');
    },
});

EzBob.LandRegistryEnquiryResultsView = Backbone.Marionette.ItemView.extend({
    template: '#landregistry-enquiry-results-template',
    initialize: function (options) {
        this.customerId = options.customerId;
    },
    events: {
        "click tr": "trClicked",
        "click .btnOk": "btnOkClicked"
    },
    trClicked: function(el) {
        this.$el.find("tbody tr").css("background", "#fff");
        $(el.currentTarget).css("background", "#ddd");
        this.titleNumber = $(el.currentTarget).attr("data-titlenumber");
    },
    btnOkClicked: function() {
        if (!this.titleNumber) {
            return false;
        }
        var that = this;
        BlockUi("On");
        
        var xhr = $.post(window.gRootPath + "Underwriter/CrossCheck/LandRegistry/?customerId=" + this.customerId + "&titleNumber=" + this.titleNumber);
        xhr.done(function () {
            that.close();
            EzBob.App.vent.trigger('landregistry:retrieved');
        });

        xhr.always(function () {
            BlockUi("Off");
        });
        return false;
    },
    serializeData: function () {
        return { data: this.model.toJSON() };
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: "Select customer's address",
            position: "center",
            draggable: true,
            dialogClass: "landregistryEnquiryResults",
            width: 900
        };
    },
});

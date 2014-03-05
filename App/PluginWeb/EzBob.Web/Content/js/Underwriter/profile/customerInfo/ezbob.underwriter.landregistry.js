var EzBob = EzBob || {};

EzBob.LandRegistryView = Backbone.Marionette.View.extend({
    initialize: function (options) {
        this.template = _.template($('#landregistry-template').html());
        this.model = options.model;
        this.address = options.address;
        this.postcode = options.postcode;
        this.customerId = options.customerId;
    },
    events: {
        "click .anotherEnquiry": "anotherEnquiryClicked",
        "click .anotherTitle": "anotherTitleClicked",
    },
    render: function () {
        this.$el.html(this.template());
        return this;
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: "Land Registry",
            position: "top",
            draggable: true,
            dialogClass: "landregistry",
            width: "60%"
        };
    },
    anotherEnquiryClicked: function() {
        var lrEnqView = new EzBob.LandRegistryEnquiryView({ model: { postcode: this.postcode, address: this.address, customerId: this.customerId } });
        EzBob.App.jqmodal.show(lrEnqView);
    },
    anotherTitleClicked: function () {
        var anotherTitleView = new EzBob.LandRegistryAnotherTitleView({ customerId: this.customerId });
        EzBob.App.jqmodal.show(anotherTitleView);
    },
});

EzBob.LandRegistryEnquiryView = Backbone.Marionette.View.extend({
    initialize: function (options) {
        this.template = _.template($('#landregistry-enquiry-template').html());
        this.model = options.model;
    },
    render: function () {
        this.$el.html(this.template());
        this.$el.find('input[name="postCode"]').val(this.model.postcode);
        return this;
    },
    events: {
        "click .btnOk": "okClicked",
    },
    okClicked: function () {
        var that = this;
        var data = this.$el.find('#landregistry-enquiry-form').serializeArray();
        data.push({ name: "customerId", value: this.model.customerId });
        BlockUi("On");
        $.get(window.gRootPath + "Underwriter/CrossCheck/LandRegistryEnquiry", data, function (response) {
            if (response && response.titles && response.titles.length == 1) {
                $.get(window.gRootPath + "Underwriter/CrossCheck/LandRegistry/?customerId=" + that.model.customerId + "&titleNumber=" + response.titles[0].TitleNumber, function (data) {
                    var lrView = new EzBob.LandRegistryView({ model: data });
                    scrollTop();
                    EzBob.App.jqmodal.show(lrView);
                    BlockUi("Off");
                });
            } else {
                var enqRes = new EzBob.LandRegistryEnquiryResultsView({ model: response, customerId: that.model.customerId });
                EzBob.App.jqmodal.show(enqRes);
                BlockUi("Off");
            }
            
        });
    },
    jqoptions: function () {
        return {
            modal: true,
            resizable: true,
            title: "Land Registry",
            position: "center",
            draggable: true,
            dialogClass: "landregistryEnquiry",
            width: 900
        };
    },
});

EzBob.LandRegistryEnquiryResultsView = Backbone.Marionette.View.extend({
    initialize: function (options) {
        this.template = _.template($('#landregistry-enquiry-results-template').html());
        this.model = options.model;
        this.customerId = options.customerId;
        
    },
    render: function () {
        this.$el.html(this.template());
        return this;
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

        BlockUi("On");
        return $.get(window.gRootPath + "Underwriter/CrossCheck/LandRegistry/?customerId=" + this.customerId + "&titleNumber=" + this.titleNumber, function (data) {
            var lrView = new EzBob.LandRegistryView({ model: data });
            scrollTop();
            EzBob.App.jqmodal.show(lrView);
            BlockUi("Off");
        });
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

EzBob.LandRegistryAnotherTitleView = Backbone.Marionette.ItemView.extend({
    template: "#another-title-template",
    initialize: function (options) {
        this.customerId = options.customerId;
    },
    events: {
        'click .anotherTitleBtn': 'okClicked',
    },
    jqoptions: function() {
        return {
            modal: true,
            resizable: false,
            title: "Another title",
            position: "center",
            draggable: false,
            width: 530,
            dialogClass: "another-title-popup",
        };
    },
    okClicked: function () {
        BlockUi("On");
        var xhr = $.get(window.gRootPath + "Underwriter/CrossCheck/LandRegistry?customerId=" + this.customerId + "&titleNumber=" + this.$el.find('#titleNumber').val());
        xhr.done(function(data) {
            var lrView = new EzBob.LandRegistryView({ model: data });
            scrollTop();
            EzBob.App.jqmodal.show(lrView);
        });
        xhr.always(function() {
            BlockUi("Off");
        });
    }
});
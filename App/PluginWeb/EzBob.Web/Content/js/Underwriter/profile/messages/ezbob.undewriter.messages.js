var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MessageModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Messages/Index"
});

EzBob.Underwriter.Message = Backbone.View.extend({
    initialize: function () {
        this.model.on("change sync", this.render, this);
        this.template = _.template($('#message-template').html());
    },
    
    render: function() {
        this.$el.html(this.template({ model: this.model.toJSON() }));
        this.customerId = this.model.get("Id");
    },
    
    events: {
        "click #btnAMLInformation": "btnAMLInformationClicked",
        "click #btnAMLandBWAInformation": "btnAMLandBWAClicked",
        "click #btnBWAInformation": "btnBWAInformationClicked"
    },    
    
    btnAMLInformationClicked: function () {
        var that = this;
        EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
                $.post(window.gRootPath + "Underwriter/Messages/MoreAMLInformation",
                {
                    id: that.customerId
                }).done(function () {
                    EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                    that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                    EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                    BlockUi("off");
                });
                BlockUi("on");
                return true;
            },
            "Yes", null, "No");         
         },

    btnAMLandBWAClicked: function () {
        var that = this;
        EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
                $.post(window.gRootPath + "Underwriter/Messages/MoreAMLandBWAInformation",
                {
                    id: that.customerId
                }).done(function () {
                    EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                    that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                    EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                    BlockUi("off");
                });
                BlockUi("on");
                return true;
            },
            "Yes", null, "No"); 
         },

    btnBWAInformationClicked: function () {
        var that = this;
        EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
                $.post(window.gRootPath + "Underwriter/Messages/MoreBWAInformation",
                {
                    id: that.customerId
                }).done(function () {
                    EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                    that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                    EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                    BlockUi("off");
                });
                BlockUi("on");
                return true;
            },
            "Yes", null, "No");
         }

});
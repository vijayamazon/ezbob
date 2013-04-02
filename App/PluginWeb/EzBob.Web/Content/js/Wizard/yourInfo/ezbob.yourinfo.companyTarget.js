var EzBob = EzBob || {};

EzBob.companyTargets = Backbone.View.extend({
    initialize: function (options) {
        this.template = _.template($('#CompanyTargets').html());
        this.jsonModel = options.model;
    },
    events: {
        "click .btnTargetOk": "btnOkClick",
        "click .btnTargetCancel": "btnCancelClick"
    },
    render: function () {
        this.$el.html(this.template());
        var that = this;

        this.$el.dialog({
            autoOpen: true,
            title: "Please choose your company",
            modal: true,
            resizable: false,
            width: 390
        });

        this.targetsList = this.$el.find(".targets");

        $.each(this.jsonModel, function (i, val) {
            that.targetsList.append($('<li></li>')
                    .attr("data", val.BusRefNum)
                    .html(
                            val.BusName + ", " +
                            val.BusRefNum + ", " +
                            val.PostCode + ", " +
                            val.AddrLine1 + ", " +
                            val.AddrLine2 + ", " +
                            val.AddrLine3 + ", " +
                            val.AddrLine4
                    ));
        });

        this.targetsList.beautifullList();

        return this;
    },
    btnOkClick: function () {
        if (this.targetsList.attr('data') == null || this.targetsList.attr('data') == 0) {
            this.targetsList.css("border", "1px solid red");
        } else {
            this.trigger("BusRefNumGetted", this.targetsList.attr('data'));
            this.btnCancelClick();
        }
    },
    btnCancelClick: function () {
        this.$el.dialog("close");
    }
});
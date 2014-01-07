var EzBob = EzBob || {};

EzBob.companyTargets = Backbone.View.extend({
    initialize: function (options) {
        this.template = _.template($('#CompanyTargets').html());
        this.jsonModel = options.model;
    },
    events: {
        "click .btnTargetOk": "btnOkClick",
        "click .btnTargetNotFound": "btnNotFoundClick",
        "click .btnTargetCancel": "btnCancelClick",
        'dblclick .targets': 'targetsDoubleClicked',
        'click .targets': 'targetsClicked',
    },
    render: function () {
        this.$el.html(this.template());
        var that = this;

        this.$el.dialog({
            autoOpen: true,
            title: "Please Choose Company",
            modal: true,
            resizable: true,
            width: 400
        });

        this.targetsList = this.$el.find(".targets");

        $.each(this.jsonModel, function (i, val) {
            that.targetsList.append($('<li></li>')
                    .attr("data", val.BusRefNum)
                    .html(
                            val.BusName + ", " + val.BusRefNum +
                            (val.PostCode  ? (", " + val.PostCode)  : "") +
                            (val.AddrLine1 ? (", " + val.AddrLine1) : "") +
                            (val.AddrLine2 ? (", " + val.AddrLine2) : "") +
                            (val.AddrLine3 ? (", " + val.AddrLine3) : "") +
                            (val.AddrLine4 ? (", " + val.AddrLine4) : "")
                    ));
        });

        this.targetsList.beautifullList();

        return this;
    },
    targetsClicked: function (evt) {
        EzBob.UiAction.saveOne(EzBob.UiAction.evtClick(), evt.target);
        $('.btnTargetOk').removeAttr('disabled');
    }, 
    targetsDoubleClicked: function(evt) {
        this.targetsClicked(evt);
        EzBob.UiAction.saveOne(EzBob.UiAction.evtLinked(), evt.target);
        this.btnOkClick();
    },
    btnOkClick: function () {
        if (this.targetsList.attr('data') == null || this.targetsList.attr('data') == 0) {
            this.targetsList.css("border", "1px solid red");
        } else {
            this.trigger("BusRefNumGetted", this.targetsList.attr('data'));
            this.btnCancelClick();
        }
    },
    btnNotFoundClick: function () {
        this.trigger("BusRefNumGetted", "NotFound");
        console.log("not found");
        this.btnCancelClick();
    },
    btnCancelClick: function () {
        this.$el.dialog("close");
    }
});
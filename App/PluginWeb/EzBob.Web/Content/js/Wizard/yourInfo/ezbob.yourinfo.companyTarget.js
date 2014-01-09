﻿var EzBob = EzBob || {};

EzBob.companyTargets = Backbone.View.extend({
    initialize: function (options) {
        this.template = _.template($('#CompanyTargets').html());
        this.jsonModel = options.model;
    },
    events: {
        'dblclick .targets': 'targetsDoubleClicked',
        'click .targets': 'targetsClicked',
    },
  
    render: function () {
        this.$el.html(this.template());
        var that = this;

        this.$el.dialog({
            autoOpen: true,
            title: "Select company",
            modal: true,
            resizable: true,
            width: 920,
            minWidth: 500,
            height: 500,
            minHeight:200,
            buttons: [
                {
                    text: 'Skip',
                    'class': 'addr-button green',
                    click: function () { that.btnNotFoundClick(); },
                    'ui-event-control-id': 'company-target:not-found',
                },
                {
                    text: 'Cancel',
                    'class': 'addr-button green',
                    click: function () { that.btnCancelClick(); },
                    'ui-event-control-id': 'company-target:cancel',
                },
                {
                    text: 'OK',
                    'class': 'addr-button green btnTargetOk disabled',
                    click: function () { that.btnOkClick(); },
                    'ui-event-control-id': 'company-target:ok',
                }
            ]
        });


        $('body, .ui-widget-overlay').addClass('address-dialog-no-overflow');

        var oWidget = this.$el.dialog('widget');

        oWidget.find('.ui-dialog-title').addClass('address-dialog-title');
        oWidget.find('.ui-dialog-titlebar').addClass('address-dialog-titlebar');
        oWidget.find('.ui-dialog-buttonpane').addClass('address-dialog-buttonpane');
        EzBob.UiAction.registerView(this);

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
        $('.btnTargetOk').removeClass('disabled');
    }, 
    targetsDoubleClicked: function(evt) {
        this.targetsClicked(evt);
        EzBob.UiAction.saveOne(EzBob.UiAction.evtLinked(), evt.target);
        this.btnOkClick();
    },
    btnOkClick: function () {
        //if (this.$el.find('.btnTargetOk').hasClass('disabled')) return false;
        
        if (this.targetsList.attr('data') == null || this.targetsList.attr('data') == 0) {
            this.targetsList.css("border", "1px solid red");
        } else {
            if (this.targetsList.attr('data') == 'skip') {
                this.trigger("BusRefNumGetted", "NotFound");
            } else {
                this.trigger("BusRefNumGetted", this.targetsList.attr('data'));
            }
            this.btnCancelClick();
        }
    },
    btnNotFoundClick: function () {
        this.trigger("BusRefNumGetted", "NotFound");
        this.btnCancelClick();
    },
    btnCancelClick: function () {
        this.$el.dialog("close");
        $('body').removeClass('address-dialog-no-overflow');
    }
});
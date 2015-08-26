﻿var EzBob = EzBob || {};

EzBob.SlidersModel = Backbone.Model.extend({
    initialize: function () {
        this.set('amount', 20000);
        this.set('term', 15);
    },
});

EzBob.SlidersView = Backbone.Marionette.ItemView.extend({
    template: '#sliders-template',

    initialize: function () {
        this.model.on('change', this.render, this);
        return this;
    },
    events: {
       
    },

    onRender: function () {
        console.log('this.model', this.model.get('amount'), this.model.get('term'));
        var self = this;
        InitAmountPeriodSliders({
            container: this.$('#calc-slider'),
            amount: {
                min: 1000,
                max: 120000,
                start: 1000,
                step: 1000,
                caption: 'How much do you need?',
                hasbutton : true
            },

            period: {
                min: 3,
                max: 24,
                start: 3,
                step: 1,
                hide: false,
                caption: 'How long do you want it for?',
                hasbutton: true
            },
            callback: function(ignored, sEvent) {
                if (sEvent === 'change')
                    self.loanSelectionChanged();
            }
        });
        return this;
    },

    loanSelectionChanged: function() {
        var currentRepaymentPeriod = this.$('#loan-sliders .period-slider').slider('value');
        console.log(currentRepaymentPeriod);
    },

    jqoptions: function () {
        return {
            autoOpen: true,
            title: 'Change requested loan amount',
            modal: true,
            resizable: true,
            width: 940,
            maxWidth: '100%',
            height: 515,
            closeOnEscape: true,
        }
    },
});



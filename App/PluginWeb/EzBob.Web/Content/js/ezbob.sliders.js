var EzBob = EzBob || {};

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
        return this;
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





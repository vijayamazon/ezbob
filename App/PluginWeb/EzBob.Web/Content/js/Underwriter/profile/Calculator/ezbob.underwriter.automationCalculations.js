var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AutomationCalculationModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Automation/Index/"
});

EzBob.Underwriter.AutomationCalculationView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#automation-calculation-template').html());
        this.model.on('change', this.render, this);
        this.currentId = 0;
    },

    render: function() {
        var automations = this.model.toJSON();
        var current = (automations.trails && automations.trails.length > 0) ? automations.trails[this.currentId] : {};

        this.$el.html(this.template({
            automations: automations,
            current: current,
            currentId: this.currentId
        }));

        return this;
    },

    events: {
        'change #AutomationDetailsHistory': 'changeHistory',
        'click #automation-explanation-btn': 'clickedExplanation'
    },

    changeHistory: function(el) {
        this.currentId = $(el.currentTarget).val();
        this.render();
    },
    
    clickedExplanation: function() {
        EzBob.ShowMessageEx({
            message: 'Shows history of all automatic decisions that where executed list of all decisions sorted by date descending.<br>' +
            '<ul class="alert-list"><li>Negative - system decision for this specific automation decision wasn\'t reached - so Approve Negative = No automatic approve, Reject Negative = No automatic reject.</li>' +
            '<li>Affirmative - system decision for this specific automation decision reached - so Approve Affirmative = automatic approve, Reject affirmative = automatic reject.</li></ul><br><br>' +
            'In the table the rows colored by status of each rule the following way: <br>' +
            '<ul class="alert-list"><li>White row - no decision was reached for this specific row</li>' +
            '<li>Yellow row - the decision was locked Negative - no automation, affirmative there is an automatic decision</li>' +
            '<li>Red row - negative decision</li>' +
            '<li>Green row - affirmative decision</li></ul><br><br>' +
            'Tags shows what triggered the automation to run (if no tag then regular main strategy invocation) <br>Tag types:<br>' +
            '<ul class="alert-list"><li>#Consumer - experian consumer check</li>' +
            '<li>#Company - experian company check</li>' +
            '<li>#Aml - aml check</li>' +
            '<li>#UpdateMarketplace - update market place</li>' +
            '<li>#LandRegistry - update land registry</li>',
            title: 'Automation explanation',
            timeout: null,
            onOk: function () { },
            okText: 'Got it',
            closeOnEscape: true,
            dialogWidth: '80%'
        });
    },
});



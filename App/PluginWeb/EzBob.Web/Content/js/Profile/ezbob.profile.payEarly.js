var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.PayEarlyView = Backbone.View.extend({
    className: "d-widget",
    initialize: function () {
        this.template = _.template($('#d-payEarly-template').html());
        this.lateTemplate = _.template($('#d-lateLoan-template').html());
        this.model.on('change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans change:HasRollovers', this.render, this);
    },
    render: function () {
        var hasLateLoans = this.model.get('LateLoans') > 0;
        var hasRollover = this.model.get('HasRollovers');
        var state = this.model.get('state');
        
        if (hasLateLoans) {
            this.$el.html(this.lateTemplate(this.model.toJSON()));
            if (hasRollover)
                this.$el.find('button').text('Pay Roll Over');
            return this;
        }

        var activeLoans = this.model.get('ActiveLoans');

        this.$el.html(this.template(this.model.toJSON()));
        this.$el.find('input .money').moneyFormat();
        this.$el.toggle(activeLoans.length > 0);
        
        if (hasRollover)
            this.$el.find('button').text('Pay Roll Over');
        return this;
    },
    events: {
        'click button': 'payEarly'
    },
    payEarly: function () {
        if (!this.model.get('TotalBalance')) {
            $('#payEarly-modal').modal();
            return;
        }
        window.location.href = "#PayEarly";
    }
});
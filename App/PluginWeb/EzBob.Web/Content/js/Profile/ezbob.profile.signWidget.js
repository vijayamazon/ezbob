var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SignModel = Backbone.Model.extend({
    defaults: {
        color: 'green',
        text: 'Welcome back',
        signTemplate: "standard"
    }
});

EzBob.Profile.SignWidget = Backbone.View.extend({
    initialize: function (options) {
        this.templates = {
            standard: _.template($('#sign-template').html()),
            welcome: _.template($('#sign-welcome-template').html())
        };
        this.customerModel = options.customerModel;
        this.model = new EzBob.Profile.SignModel();
        this.model.on('change', this.render, this);
        this.customerModel.on('change:TotalBalance', this.processBalanceChanged, this);
        this.customerModel.on('change:state', this.processBalanceChanged, this);
        this.customerModel.on('change:HasRollovers', this.processBalanceChanged, this);
        this.balanceChanged();
    }, // initialize

    render: function () {
        this.$el.html(this.templates[this.model.get("signTemplate")](this.model.toJSON()));
        EzBob.UiAction.registerView(this);
        return this;
    }, // render

    events: {
        "click a.pay-early": "click"
    }, // events

    click: function () {
        this.trigger('payEarly');
        window.location.href = "#PayEarly";
        return false;
    }, // click

    processBalanceChanged: function() {
        this.balanceChanged();
        EzBob.UiAction.registerView(this);
    }, // processBalanceChanged

    balanceChanged: function () {
        var balance = this.customerModel.get('TotalBalance'),
            state = this.customerModel.get('state'),
            hasLoans = this.customerModel.get('hasLoans'),
            isNew = hasLoans == 0,
            hasRollOver = this.customerModel.get('HasRollovers'),
            name = this.customerModel.get('CustomerPersonalInfo') != null ? this.customerModel.get('CustomerPersonalInfo').FirstName : "",
            isEarly = this.customerModel.get('IsEarly');

        if (!hasRollOver && state == "late") {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Payment is Required',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (hasRollOver) {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, You have a Rollover payment pending</span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (balance > 0) {
            var valueOfText;
            if (isEarly) {
                valueOfText = '<span><span class="client-name">' + name + '</span>, Pay Early &amp; Save';
            } else {
                valueOfText = '<span><span class="client-name">' + name + '</span>, Payment is Required';
            }
            this.model.set({
                color: 'green',
                text: valueOfText,
                signTemplate: "welcome"
            });
            return;
        }

        if (state == "get" && !isNew) {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Congratulations, your credit is ready to be taken</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "get" && isNew) {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Congratulations, credit is approved and can be taken</span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (state == "bad" && hasLoans) {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Welcome back</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "bad" && !hasLoans) {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Add more accounts below to Get Cash</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "apply") {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Request Cash to get funding today!</span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (!isNew && state == "wait") {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Your application is under review, we will revert as soon as possible</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (isNew && state == "wait") {
            this.model.set({
                color: 'green',
                text: '<span><span class="client-name">' + name + '</span>, Welcome to the ezbob Family</span>',
                signTemplate: "welcome"
            });
            return;
        }

        this.model.set({
            color: 'green',
            text: '<span><span class="client-name">' + name + '</span>, Welcome back</span>',
            signTemplate: "welcome"
        });
    }, // balanceChanged
});
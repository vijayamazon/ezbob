var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SignModel = Backbone.Model.extend({
    defaults: {
        color: 'green',
        text: 'Pay early &amp; Save',
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
        this.customerModel.on('change:TotalBalance', this.balanceChanged, this);
        this.customerModel.on('change:state', this.balanceChanged, this);
        this.customerModel.on('change:HasRollovers', this.balanceChanged, this);
        this.balanceChanged();
    },
    render: function () {
        this.$el.html(this.templates[this.model.get("signTemplate")](this.model.toJSON()));
        return this;
    },
    events: {
        "click a.pay-early": "click"
    },
    click: function () {
        this.trigger('payEarly');
        window.location.href = "#PayEarly";
        return false;
    },
    balanceChanged: function () {
        var balance = this.customerModel.get('TotalBalance'),
            state = this.customerModel.get('state'),
            hasLoans = this.customerModel.get('hasLoans'),
            isNew = hasLoans == 0,
            hasRollOver = this.customerModel.get('HasRollovers'),
            name = this.customerModel.get('CustomerPersonalInfo') != null ? this.customerModel.get('CustomerPersonalInfo').FirstName : "";

        if (!hasRollOver && state == "late") {
            this.model.set({
                color: 'green',
                text: '<span>'+ name + ', <br/>Payment is Required</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (hasRollOver) {
            this.model.set({
                color: 'green',
                text: "<span>Roll over, and reduce late charges</span>",
                signTemplate: "welcome"
            });
            return;
        }

        if (balance > 0) {
            this.model.set({
                color: 'green',
                text: '<span><a href="#" class="pay-early">Pay Early &amp; Save</a></span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (state == "get" && !isNew) {
            this.model.set({
                color: 'green',
                text: '<span>Congrats.  Use the money wisely</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "get" && isNew) {
            this.model.set({
                color: 'green',
                text: '<span>Congratulations</span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (state == "bad" && hasLoans) {
            this.model.set({
                color: 'green',
                text: '<span>Please revisit us soon</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "bad" && !hasLoans) {
            this.model.set({
                color: 'green',
                text: '<span>Add Stores to Get Cash</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (state == "apply") {
            this.model.set({
                color: 'green',
                text: '<span>Request Cash<br/>&<br/>Grow</span>',
                signTemplate: "welcome"
            });
            return;
        }

        if (!isNew && state == "wait") {
            this.model.set({
                color: 'green',
                text: '<span>EZBOB Will Revert Back to You Shortly</span>',
                signTemplate: "welcome"
            });
            return;
        }
        
        if (isNew && state == "wait") {
            this.model.set({
                color: 'green',
                text: "<span>" + name + ", Welcome to the EZBOB Family</span>",
                signTemplate: "welcome"
            });
            return;
        }

        this.model.set({
            color: 'green',
            text: '<span>Apply &amp; get funds</span>',
            signTemplate: "welcome"
        });
    }
});
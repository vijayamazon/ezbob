var EzBob = EzBob || {};

EzBob.CalculatorModel = Backbone.Model.extend({

    initialize: function () {
        this.on("error", function () {
            this.set({ "Credit": 0.00 });
        });
    },

    youCanGet: function (amount, sales) {

        var credit = parseFloat(sales / 5);

        if (credit > amount)
            credit = amount.toFixed(3);

        if (credit > 30000) {
            credit = 30000;
        }

        credit = Math.round(credit);
        this.set({
            "Amount": amount,
            "Sales": sales,
            "Credit": credit
        });
    },

    validate: function (atts) {
        if ("Amount" in atts & !atts.Amount) {
            return "amount is required ";
        }

        if ("Amount" in atts & isNaN(atts.Amount)) {
            return "please enter correct  required amount";
        }

        if ("Amount" in atts & atts.Amount < 0) {
            return "value should not be negative";
        }

        if ("Sales" in atts & !atts.Sales) {
            return "Sales not required ";
        }

        if ("Sales" in atts & isNaN(atts.Sales)) {
            return "please enter correct  required total amount of yearly sales";
        }

        if ("Sales" in atts & atts.Sales < 0) {
            return "value should not be negative";
        }
    }
});


EzBob.HowMuchView = Backbone.View.extend({
    events: {
        "change input:text": "callYouCanGet",
        "keypress input:text": "callYouCanGet",
        "keyup input:text": "callYouCanGet",
        "keydown input:text": "callYouCanGet"
    },


    initialize: function () {
        this.model.on('change', this.render, this);
    },

    callYouCanGet: function (e) {

        if (e) $(e.target).val($(e.target).val().replace(".", ""));
        var amount = parseFloat(this.$el.find('input#RequiredAmount').autoNumeric('get'));
        var sales = parseFloat(this.$el.find('input#TotalAmount').autoNumeric('get'));
        this.model.youCanGet(amount, sales);

    },

    render: function () {
        var options = { 'aSep': ',', 'aDec': '.', 'aPad': false, 'mNum': 16, 'mRound': 'F' };

        this.$el.find('#Credit').text(EzBob.formatPoundsFormat(this.model.get('Credit'), options));
        return this;
    }
});





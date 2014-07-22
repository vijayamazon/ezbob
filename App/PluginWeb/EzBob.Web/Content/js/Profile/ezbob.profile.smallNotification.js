﻿var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ProccessingMessageView = Backbone.View.extend({
    className: "processing-header",
    initialize: function (options) {
        this.template = _.template($('#processing-message-template').html());
        this.model.on('change:state change:ActiveLoans change:hasLoans change:hasLateLoans change:HasRollovers', this.render, this);
    },
    events: {
        'click .get-cash': 'getCash',
        'click .pay-now': 'payNow',
        'click .apply-for-loan': 'applyForLoan',
        'click .request-cash': 'applyForLoan'
    },
    getCash: function () {
        this.trigger('getCash');
    },
    payNow: function () {
        this.trigger('payEarly');
    },
    applyForLoan: function () {
        var that = this;
        $.post(window.gRootPath + "Customer/Profile/ApplyForALoan")
            .done(function () {
                that.model.set('state', 'wait');
            });
        return false;
    },
    render: function () {
        var message = "",
            cls = "red",
            state = this.model.get('state'),
            hasLoans = this.model.get('hasLoans'),
            hasRollOver = this.model.get('HasRollovers'),
            name = this.model.get('CustomerPersonalInfo').FirstName,
            sum = this.model.get('CreditSum'),
            save = this.model.get('TotalPayEarlySavings');
      
        if (state == 'get' && hasLoans) {
            message = "<em>Congratulations!</em> Click Choose Amount and choose the exact amount you need.";
            cls = "green";
        }

        var wasSetAccordingToApplyState = false;
        if (save && hasLoans && (state == 'get' || state == 'wait' || state == 'apply')) {
            if (state == 'apply') {
                wasSetAccordingToApplyState = true;
            }
            if (this.model.get('IsEarly')) {
                message = "Pay today, and save up to " + EzBob.formatPounds(save);
            } else {
                message = "Please pay now";
            }
            cls = "green";
        }

        if (state == 'apply' && hasLoans && !wasSetAccordingToApplyState) {
            message = "<em>Note:</em> Our previous offer to you has expired. Click Request Cash.  ";
            cls = "green";
        }

        if (state == 'apply' && !hasLoans) {
            message = "<em>Note:</em> Our previous offer to you expired.  Click Request Cash.  ";
            cls = "green";
        }

        if (state == 'get' && !hasLoans) {
            message = "<em>Congratulations!</em> Click Choose Amount and choose the exact amount you need.";
            cls = "green";
        }

        //        if (state == 'apply') {
        //            message = name + ", the previous offer has expired. To receive another offer, click <a class='blue apply-for-loan' href='#'>REQUEST CASH</a>.";
        //            cls = "green";
        //        }

        if (state == 'wait') {
            message = "We are currently processing your request.";
            cls = "green";
        }

        if (state == 'wait' && !hasLoans) {
            message = "We are currently processing your application.";
            cls = "green";
        }

        if (state == 'wait' && hasLoans) {
            message = "<em>Note:</em> We are currently processing your request.";
            cls = "green";
        }
        
        if (state == 'disabled' || (state == 'bad' && hasLoans)) {
            message = "Unfortunately, we cannot extend you more cash at this time.";
            cls = 'blue';
        }
        
        if (state == 'bad' && !hasLoans) {
            message = "Min. requirements not met. Try adding more stores!";
            cls = 'blue';
        }
        
        if (state == 'late') {
            message = "Your account is late. Please pay now.";
            cls = 'red';
        }
        
        if (hasRollOver) {
            message = "Please pay your roll over";
            cls = 'red';
        }

        cls = "hm_" + cls;

        this.$el.html(this.template({ Message: message, cls: cls }));
	    EzBob.UiAction.registerView(this);
        return this;
    }
});
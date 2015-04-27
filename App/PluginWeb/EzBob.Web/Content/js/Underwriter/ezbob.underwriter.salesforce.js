﻿var EzBob = EzBob || {};

(function () {
    EzBob.SalesForce = {
        returnToWaitingForDecision: "button.btn-return-waiting-for-decision",
        init: function (customerID) {
            this.customerID = customerID;
            var self = this;
            
            $(this.returnToWaitingForDecision).click(
                function() {
                    var model = new Backbone.Model({ CustomerId: self.customerID, Reason: '' });
                    var functionPopupView = new EzBob.Underwriter.Returned({ model: model });
                    functionPopupView.render();
                    functionPopupView.on('changedSystemDecision', self.changedSystemDecision, self);
                    return false;
                }
            );

            EzBob.SalesForceRouter = Backbone.Router.extend({
                routes: {
                    '*actions': 'defaultRoute'
                },
                defaultRoute: function (action) {}
            });

            this.salesForceRouter = new EzBob.SalesForceRouter;
        }, // init

        changedSystemDecision: function () {
            window.location.reload();
        }//changedSystemDecision

    }; // EzBob.SalesForce
})();
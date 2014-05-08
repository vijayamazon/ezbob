var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Properties = Backbone.Model.extend({
    url: function () {
        debugger;
        return window.gRootPath + "Underwriter/Properties/Index/" + this.customerId;
    }
});

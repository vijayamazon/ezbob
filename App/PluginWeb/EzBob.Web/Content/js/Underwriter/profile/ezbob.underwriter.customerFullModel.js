(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter.CustomerFullModel = (function(_super) {

    __extends(CustomerFullModel, _super);

    function CustomerFullModel() {
      return CustomerFullModel.__super__.constructor.apply(this, arguments);
    }

    CustomerFullModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/FullCustomer/Index/?id=" + (this.get("customerId")) + "&history=" + (this.get("history"));
    };

    return CustomerFullModel;

  })(Backbone.Model);

}).call(this);

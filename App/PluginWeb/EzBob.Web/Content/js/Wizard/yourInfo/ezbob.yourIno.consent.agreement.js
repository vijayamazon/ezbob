(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.ConsentAgreementModel = (function(_super) {

    __extends(ConsentAgreementModel, _super);

    function ConsentAgreementModel() {
      return ConsentAgreementModel.__super__.constructor.apply(this, arguments);
    }

    ConsentAgreementModel.prototype.defaults = {
      fullName: 'Dimka Ivanish',
      date: 'sssssss'
    };

    return ConsentAgreementModel;

  })(Backbone.Model);

  EzBob.ConsentAgreement = (function(_super) {

    __extends(ConsentAgreement, _super);

    function ConsentAgreement() {
      return ConsentAgreement.__super__.constructor.apply(this, arguments);
    }

    ConsentAgreement.prototype.template = "#consent-agreement-temlate";

    ConsentAgreement.prototype.events = {
      'click .print': 'onPrint'
    };

    ConsentAgreement.prototype.onPrint = function() {
      return printElement("consent");
    };

    return ConsentAgreement;

  })(Backbone.Marionette.ItemView);

}).call(this);

(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.PayPointCardModel = (function(_super) {

    __extends(PayPointCardModel, _super);

    function PayPointCardModel() {
      return PayPointCardModel.__super__.constructor.apply(this, arguments);
    }

    return PayPointCardModel;

  })(Backbone.Model);

  EzBob.Underwriter.AddPayPointCardView = (function(_super) {

    __extends(AddPayPointCardView, _super);

    function AddPayPointCardView() {
      return AddPayPointCardView.__super__.constructor.apply(this, arguments);
    }

    AddPayPointCardView.prototype.template = '#add-paypoint-card-template';

    AddPayPointCardView.prototype.events = {
      'click .btn-primary': 'save'
    };

    AddPayPointCardView.prototype.ui = {
      'transactionid': 'input[name="transactionid"]',
      'cardno': 'input[name="cardno"]',
      'expiredate': 'input[name="expiredate"]'
    };

    AddPayPointCardView.prototype.onRender = function() {
      return this.ui.expiredate.datepicker({
        format: 'dd/mm/yyyy'
      });
    };

    AddPayPointCardView.prototype.save = function() {
      this.model.set({
        'transactionid': this.ui.transactionid.val(),
        'cardno': this.ui.cardno.val(),
        'expiredate': moment(this.ui.expiredate.val(), "DD/MM/YYYY").toDate()
      });
      this.trigger('save');
      return this.close();
    };

    return AddPayPointCardView;

  })(Backbone.Marionette.ItemView);

}).call(this);

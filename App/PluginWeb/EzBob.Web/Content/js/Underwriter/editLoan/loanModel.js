(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Installment = (function(_super) {

    __extends(Installment, _super);

    function Installment() {
      return Installment.__super__.constructor.apply(this, arguments);
    }

    Installment.prototype.initialize = function() {
      this.on("change:Balance", this.balanceChanged, this);
      this.on("change:Principal", this.principalChanged, this);
      return this.on("change:Date", this.dateChanged, this);
    };

    Installment.prototype.balanceChanged = function() {
      var principal;
      principal = Math.round((this.get('BalanceBeforeRepayment') - this.get('Balance')) * 100);
      principal = principal / 100;
      return this.set('Principal', principal);
    };

    Installment.prototype.principalChanged = function() {
      return this.set('Total', this.get('Principal') + this.get('Interest') + this.get('Fees'));
    };

    Installment.prototype.dateChanged = function() {};

    return Installment;

  })(Backbone.Model);

  EzBob.Installments = (function(_super) {

    __extends(Installments, _super);

    function Installments() {
      return Installments.__super__.constructor.apply(this, arguments);
    }

    Installments.prototype.model = EzBob.Installment;

    return Installments;

  })(Backbone.Collection);

  EzBob.LoanModel = (function(_super) {

    __extends(LoanModel, _super);

    function LoanModel() {
      return LoanModel.__super__.constructor.apply(this, arguments);
    }

    LoanModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/LoanEditor/Loan/" + (this.get('Id'));
    };

    LoanModel.prototype.initialize = function() {
      var items;
      items = new EzBob.Installments();
      items.on("change", this.itemsChanged, this);
      this.set("Items", items);
      return this.on("shiftDate", this.dateShifted, this);
    };

    LoanModel.prototype.itemsChanged = function() {
      return this.trigger("change");
    };

    LoanModel.prototype.dateShifted = function(installment, newDate, oldDate) {
      var diff, item, _i, _len, _ref;
      newDate = moment.utc(newDate);
      oldDate = moment.utc(oldDate);
      diff = newDate - oldDate;
      _ref = this.get("Items");
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        item = _ref[_i];
        if (item.get("Date") < oldDate && item !== installment) {
          item.set("Date", item.get("Date").add(diff));
        }
      }
      return false;
    };

    LoanModel.prototype.parse = function(r, o) {
      this.get("Items").reset(r.Items);
      delete r.Items;
      return r;
    };

    LoanModel.prototype.toJSON = function() {
      var r;
      r = LoanModel.__super__.toJSON.call(this);
      r.Items = r.Items.toJSON();
      return r;
    };

    LoanModel.prototype.removeItem = function(index) {
      var items;
      items = this.get("Items");
      items.remove(items.at(index));
      return this.recalculate();
    };

    LoanModel.prototype.addInstallment = function(installment) {
      this.get("Items").add(installment);
      return this.recalculate();
    };

    LoanModel.prototype.recalculate = function() {
      return this.save({}, {
        url: "" + window.gRootPath + "Underwriter/LoanEditor/Recalculate/" + (this.get('Id'))
      });
    };

    return LoanModel;

  })(Backbone.Model);

  EzBob.LoanModelTemplate = (function(_super) {

    __extends(LoanModelTemplate, _super);

    function LoanModelTemplate() {
      return LoanModelTemplate.__super__.constructor.apply(this, arguments);
    }

    LoanModelTemplate.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/LoanEditor/LoanCR/" + (this.get('CashRequestId'));
    };

    LoanModelTemplate.prototype.recalculate = function() {
      return this.save({}, {
        url: "" + window.gRootPath + "Underwriter/LoanEditor/RecalculateCR/" + (this.get('CashRequestId'))
      });
    };

    return LoanModelTemplate;

  })(EzBob.LoanModel);

}).call(this);

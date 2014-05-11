(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __slice = [].slice;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Installment = (function(_super) {

    __extends(Installment, _super);

    function Installment() {
      return Installment.__super__.constructor.apply(this, arguments);
    }

    Installment.prototype.defaults = {
      IsAdding: false,
      skipRecalculations: false
    };

    Installment.prototype.initialize = function() {
      this.on("change:Balance", this.balanceChanged, this);
      this.on("change:Principal", this.principalChanged, this);
      this.on("change:Total", this.totalChanged, this);
      return this.on("change:Date", this.dateChanged, this);
    };

    Installment.prototype.balanceChanged = function() {
      return this.safeRecalculate(function() {
        var principal;
        if (this.get('Balance') === this.previous('Balance')) {
          return;
        }
        principal = this.round(this.get('BalanceBeforeRepayment') - this.get('Balance'));
        this.set('Principal', principal);
        return this.recalculate();
      });
    };

    Installment.prototype.totalChanged = function() {
      return this.safeRecalculate(function() {
        var diff;
        diff = this.get("Total") - this.previous("Total");
        if (diff === 0) {
          return;
        }
        this.set('Balance', this.get("Balance") - diff);
        return this.set('Principal', this.get("Principal") + diff);
      });
    };

    Installment.prototype.principalChanged = function() {
      return this.safeRecalculate(function() {
        var diff;
        diff = this.get("Principal") - this.previous("Principal");
        if (diff === 0) {
          return;
        }
        this.set('Balance', this.get("Balance") - diff);
        return this.recalculate();
      });
    };

    Installment.prototype.safeRecalculate = function() {
      var func, params;
      func = arguments[0], params = 2 <= arguments.length ? __slice.call(arguments, 1) : [];
      if (this.skipRecalculations) {
        return;
      }
      this.skipRecalculations = true;
      func.call(this, params);
      return this.skipRecalculations = false;
    };

    Installment.prototype.recalculate = function() {
      return this.set({
        'Total': this.get('Principal') + this.get('Interest') + this.get('Fees')
      });
    };

    Installment.prototype.dateChanged = function() {
      return this.set("Date", new Date(moment.utc(this.get("Date"))));
    };

    Installment.prototype.round = function(number) {
      number = Math.round(number * 100);
      return number = number / 100;
    };

    return Installment;

  })(Backbone.Model);

  EzBob.Installments = (function(_super) {

    __extends(Installments, _super);

    function Installments() {
      return Installments.__super__.constructor.apply(this, arguments);
    }

    Installments.prototype.model = EzBob.Installment;

    Installments.prototype.comparator = function(m1, m2) {
      var d, d1, d2, r;
      d1 = moment.utc(m1.get('Date')).startOf('day');
      d2 = moment.utc(m2.get('Date')).startOf('day');
      d = d1.diff(d2, "days");
      if (d < 0) {
        r = -1;
      } else if (d === 0) {
        r = 0;
      } else {
        r = 1;
      }
      if (r === 0 && m1.get("Type") !== m2.get("Type")) {
        if (m1.get("Type") === "Installment") {
          r = 1;
        } else {
          r = -1;
        }
      }
      return r;
    };

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
      return this.set("Items", items);
    };

    LoanModel.prototype.itemsChanged = function() {
      return this.trigger("change");
    };

    LoanModel.prototype.shiftDate = function(installment, newDate, oldDate) {
      var d1, d2, diff, i, index, item, _i, _len, _ref;
      newDate = moment.utc(newDate);
      oldDate = moment.utc(oldDate);
      diff = newDate - oldDate;
      index = this.get("Items").indexOf(installment);
      _ref = this.get("Items").models;
      for (i = _i = 0, _len = _ref.length; _i < _len; i = ++_i) {
        item = _ref[i];
        if (!(i > index)) {
          continue;
        }
        d1 = moment.utc(item.get("Date"));
        d2 = moment.utc(item.get("Date")).add(diff);
        item.set("Date", moment.utc(item.get("Date")).add(diff).toDate());
      }
      return false;
    };

    LoanModel.prototype.shiftInterestRate = function(installment, rate) {
      var i, index, item, _i, _len, _ref;
      index = this.get("Items").indexOf(installment);
      _ref = this.get("Items").models;
      for (i = _i = 0, _len = _ref.length; _i < _len; i = ++_i) {
        item = _ref[i];
        if (i > index) {
          item.set("InterestRate", rate);
        }
      }
      return false;
    };

    LoanModel.prototype.parse = function(r, o) {
      _.each(r.Items, function(item) {
        return item.Date = new Date(moment.utc(item.Date));
      });
      this.get("Items").reset(r.Items);
      delete r.Items;
      r.Date = new Date(moment.utc(r.Date));
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

    LoanModel.prototype.addFee = function(fee) {
      this.get("Items").add(fee);
      this.recalculate();
    };

    LoanModel.prototype.recalculate = function() {
      return this.save({}, {
        url: "" + window.gRootPath + "Underwriter/LoanEditor/Recalculate/" + (this.get('Id'))
      });
    };

    LoanModel.prototype.addFreezeInterval = function(sStartDate, sEndDate, nRate) {
      return this.save({}, {
        url: "" + window.gRootPath + "Underwriter/LoanEditor/AddFreezeInterval/" + (this.get('Id')) + "?startdate=" + sStartDate + "&enddate=" + sEndDate + "&rate=" + nRate
      });
    };

    LoanModel.prototype.removeFreezeInterval = function(intervalId) {
      return this.save({}, {
        url: "" + window.gRootPath + "Underwriter/LoanEditor/RemoveFreezeInterval/" + (this.get('Id')) + "?intervalid=" + intervalId
      });
    };

    LoanModel.prototype.getInstallmentBefore = function(date) {
      var installment, item, _i, _len, _ref;
      date = moment.utc(date).toDate();
      installment = null;
      _ref = this.get("Items").models;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        item = _ref[_i];
        if (item.get("Type") === "Installment" && moment.utc(item.get("Date")).toDate() < date) {
          installment = item;
        }
      }
      return installment;
    };

    LoanModel.prototype.getInstallmentAfter = function(date) {
      var item, _i, _len, _ref;
      date = moment.utc(date).toDate();
      _ref = this.get("Items").models;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        item = _ref[_i];
        if (item.get("Type") === "Installment" && moment.utc(item.get("Date")).toDate() > date) {
          return item;
        }
      }
      return null;
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
        url: "" + window.gRootPath + "Underwriter/LoanEditor/RecalculateCR"
      });
    };

    return LoanModelTemplate;

  })(EzBob.LoanModel);

}).call(this);

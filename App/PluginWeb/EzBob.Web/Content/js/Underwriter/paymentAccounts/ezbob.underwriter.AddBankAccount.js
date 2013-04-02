(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.AddBankAccount = (function(_super) {

    __extends(AddBankAccount, _super);

    function AddBankAccount() {
      return AddBankAccount.__super__.constructor.apply(this, arguments);
    }

    AddBankAccount.prototype.template = '#add-bank-account-template';

    AddBankAccount.prototype.events = {
      "click .check-account": "check"
    };

    AddBankAccount.prototype.bindings = {
      BankAccount: {
        selector: "input[name='number']"
      },
      SortCode: {
        selector: "input[name='sortcode']"
      }
    };

    AddBankAccount.prototype.initialize = function(options) {
      this.model = new Backbone.Model({
        customerId: options.customerId,
        SortCode: "",
        BankAccount: ""
      });
      return AddBankAccount.__super__.initialize.call(this);
    };

    AddBankAccount.prototype.onRender = function() {
      return this.$el.find('form').validate({
        number: {
          number: true,
          minlength: 8,
          maxlength: 8
        },
        sortcode: {
          number: true,
          minlength: 6,
          maxlength: 8
        }
      });
    };

    AddBankAccount.prototype.onSave = function() {
      var xhr,
        _this = this;
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/TryAddBankAccount", this.model.toJSON());
      xhr.done(function(r) {
        _this.trigger('saved');
        return _this.close();
      });
      return false;
    };

    AddBankAccount.prototype.check = function() {
      var xhr;
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/CheckBankAccount", this.model.toJSON());
      return xhr.done(function(r) {
        var view;
        if (r.error != null) {
          return;
        }
        view = new EzBob.Underwriter.BankAccountDetailsView({
          model: new Backbone.Model(r)
        });
        return EzBob.App.modal2.show(view);
      });
    };

    return AddBankAccount;

  })(EzBob.BoundItemView);

}).call(this);

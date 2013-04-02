(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.LoanOptionsModel = (function(_super) {

    __extends(LoanOptionsModel, _super);

    function LoanOptionsModel() {
      return LoanOptionsModel.__super__.constructor.apply(this, arguments);
    }

    LoanOptionsModel.prototype.IdAttribute = "Id";

    LoanOptionsModel.prototype.urlRoot = function() {
      return "" + window.gRootPath + "Underwriter/LoanOptions/Index?loanId=" + this.loanId;
    };

    return LoanOptionsModel;

  })(Backbone.Model);

  EzBob.Underwriter.LoanOptionsView = (function(_super) {

    __extends(LoanOptionsView, _super);

    function LoanOptionsView() {
      this.onCancel = __bind(this.onCancel, this);

      this.save = __bind(this.save, this);
      return LoanOptionsView.__super__.constructor.apply(this, arguments);
    }

    LoanOptionsView.prototype.template = '#loan-options-template';

    LoanOptionsView.prototype.save = function() {
      var action, data, form, request;
      form = this.$el.find('form');
      data = form.serialize();
      action = form.attr('action');
      request = $.post(action, data);
      this.close();
      return false;
    };

    LoanOptionsView.prototype.onCancel = function() {
      return this.close();
    };

    LoanOptionsView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: 'Loan Options',
        draggable: false,
        width: "500",
        buttons: {
          "OK": this.save,
          "Cancel": this.onCancel
        }
      };
    };

    return LoanOptionsView;

  })(Backbone.Marionette.ItemView);

}).call(this);

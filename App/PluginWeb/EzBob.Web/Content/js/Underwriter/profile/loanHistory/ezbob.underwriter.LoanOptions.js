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

    LoanOptionsModel.prototype.urlRoot = function() {
      return "" + window.gRootPath + "Underwriter/LoanOptions/Index?loanId=" + this.loanId;
    };

    return LoanOptionsModel;

  })(Backbone.Model);

  EzBob.Underwriter.LoanOptionsView = (function(_super) {

    __extends(LoanOptionsView, _super);

    function LoanOptionsView() {
      this.onSave = __bind(this.onSave, this);

      this.onCancel = __bind(this.onCancel, this);

      this.changeAccountStatus = __bind(this.changeAccountStatus, this);
      return LoanOptionsView.__super__.constructor.apply(this, arguments);
    }

    LoanOptionsView.prototype.template = '#loan-options-template';

    LoanOptionsView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Loan Options",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "loan-options-popup"
      };
    };

    LoanOptionsView.prototype.initialize = function() {
      this.loanOptions = new Backbone.Model(this.model.get('Options'));
      this.modelBinder = new Backbone.ModelBinder();
      return this;
    };

    LoanOptionsView.prototype.bindings = {
      Id: {
        selector: "input[name='Id']"
      },
      LoanId: {
        selector: "input[name='LoanId']"
      },
      AutoPayment: {
        selector: "input[name='AutoPayment']"
      },
      ReductionFee: {
        selector: "input[name='ReductionFee']"
      },
      LatePaymentNotification: {
        selector: "input[name='LatePaymentNotification']"
      },
      StopSendingEmails: {
        selector: "input[name='StopSendingEmails']"
      }
    };

    LoanOptionsView.prototype.events = {
      'change #cais-flags': 'changeFlags',
      'change #CaisAccountStatus': 'changeAccountStatus',
      "click .btnOk": "onSave"
    };

    LoanOptionsView.prototype.changeFlags = function() {
      var curentFlag, index;
      this.loanOptions.set('ManulCaisFlag', this.$el.find("#cais-flags option:selected").val());
      index = this.$el.find("#cais-flags option:selected").attr('data-id');
      curentFlag = this.model.get('ManualCaisFlags')[index];
      return this.$el.find('.cais-comment').html('<h5>' + curentFlag.ValidForRecordType + '</h5>' + curentFlag.Comment);
    };

    LoanOptionsView.prototype.changeAccountStatus = function() {
      var tmp;
      tmp = $("#CaisAccountStatus option:selected").val();
      if (tmp === '8') {
        $("#defaultExplanation").show();
      } else {
        $("#defaultExplanation").hide();
      }
      return this.loanOptions.set('CaisAccountStatus', $("#CaisAccountStatus option:selected").val());
    };

    LoanOptionsView.prototype.save = function() {
      var action, postData, request;
      postData = this.loanOptions.toJSON();
      action = "" + window.gRootPath + "Underwriter/LoanOptions/Save";
      request = $.post(action, postData);
      return false;
    };

    LoanOptionsView.prototype.onCancel = function() {
      return this.close();
    };

    LoanOptionsView.prototype.onSave = function() {
      this.save();
      return this.close();
    };

    LoanOptionsView.prototype.onRender = function() {
      this.modalOptions = {
        show: true,
        keyboard: false,
        width: 700
      };
      this.modelBinder.bind(this.loanOptions, this.el, this.bindings);
      this.$el.find("#CaisAccountStatus option[value='" + (this.loanOptions.get('CaisAccountStatus')) + "']").attr('selected', 'selected');
      this.$el.find("#cais-flags option[value='" + (this.loanOptions.get('ManulCaisFlag')) + "']").attr('selected', 'selected');
      return this.changeFlags();
    };

    return LoanOptionsView;

  })(Backbone.Marionette.ItemView);

}).call(this);

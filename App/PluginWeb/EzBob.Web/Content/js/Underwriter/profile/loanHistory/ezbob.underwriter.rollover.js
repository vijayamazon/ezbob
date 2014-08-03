(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.RolloverView = (function(_super) {

    __extends(RolloverView, _super);

    function RolloverView() {
      return RolloverView.__super__.constructor.apply(this, arguments);
    }

    RolloverView.prototype.initialize = function() {
      var val, _i, _len, _ref;
      this.template = _.template($('#payment-rollover-template').html());
      this.model.hasActive = false;
      _ref = this.model.rollover;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        val = _ref[_i];
        if (val.Status === 0) {
          this.model.hasActive = true;
          this.model.rollover = val;
          return null;
        }
      }
      return this;
    };

    RolloverView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Rollover",
        position: "center",
        draggable: false,
        dialogClass: "rollover-popup",
        width: 600
      };
    };

    RolloverView.prototype.events = {
      "click .confirm": "addRollover",
      "click .remove": "removeRollover",
      "change [name='ExperiedDate']": "updatePaymentData"
    };

    RolloverView.prototype.render = function() {
      this.model.title = !this.model.hasActive ? "Add roll over" : "Edit roll over";
      this.$el.html(this.template({
        model: this.model
      }));
      this.$el.find('.ezDateTime').splittedDateTime();
      this.form = this.$el.find("#rollover-dialog");
      this.validator = EzBob.validateRollover(this.form);
      if (this.model.hasActive) {
        SetDefaultDate(this.$el.find('#ExperiedDate'), this.model.rollover.ExpiryDate);
      }
      this.$el.find('select[name=\"ScheduleId\"]').change();
      this.updatePaymentData();
      return this;
    };

    RolloverView.prototype.addRollover = function() {
      var disabled;
      if (!this.validator.form()) {
        return false;
      }
      disabled = this.form.find(':input:disabled').removeAttr('disabled');
      this.trigger("addRollover", this.form.serializeArray());
      disabled.attr('disabled', 'disabled');
      return true;
    };

    RolloverView.prototype.removeRollover = function() {
      var rolloverId;
      rolloverId = this.$el.find('input[name=\"rolloverId\"]');
      return this.trigger("removeRollover", rolloverId);
    };

    RolloverView.prototype.updatePaymentData = function() {
      var data, request,
        _this = this;
      data = {
        loanId: this.model.loanId,
        isEdit: this.model.hasActive
      };
      request = $.get(window.gRootPath + "Underwriter/LoanHistory/GetRolloverInfo", data);
      return request.done(function(r) {
        if (r.error) {
          return;
        }
        _this.$el.find("#Payment").val(r.rolloverAmount);
        _this.$el.find("#interest").val(r.interest);
        _this.$el.find("#lateFees").val(r.lateCharge);
        return _this.$el.find("#MounthCount").val(r.mounthAmount);
      });
    };

    return RolloverView;

  })(Backbone.View);

}).call(this);

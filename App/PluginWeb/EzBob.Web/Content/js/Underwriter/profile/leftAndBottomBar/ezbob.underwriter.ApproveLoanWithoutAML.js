(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ApproveLoanWithoutAML = (function(_super) {

    __extends(ApproveLoanWithoutAML, _super);

    function ApproveLoanWithoutAML() {
      return ApproveLoanWithoutAML.__super__.constructor.apply(this, arguments);
    }

    ApproveLoanWithoutAML.prototype.template = '#approve-loan-without-aml-template';

    ApproveLoanWithoutAML.prototype.initialize = function(options) {
      this.model = options.model;
      this.parent = options.parent;
      this.skipPopupForApprovalWithoutAML = options.skipPopupForApprovalWithoutAML;
      return ApproveLoanWithoutAML.__super__.initialize.call(this);
    };

    ApproveLoanWithoutAML.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Warning",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "warning-aml-status-popup"
      };
    };

    ApproveLoanWithoutAML.prototype.render = function() {
      ApproveLoanWithoutAML.__super__.render.call(this);
      return this;
    };

    ApproveLoanWithoutAML.prototype.onSave = function() {
      var isChecked, that, xhr;
      isChecked = $('#isDoNotShowAgain').is(':checked');
      this.model.set('SkipPopupForApprovalWithoutAML', isChecked);
      BlockUi("on");
      that = this;
      xhr = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/SaveApproveWithoutAML/", {
        customerId: this.model.get('CustomerId'),
        doNotShowAgain: isChecked
      });
      return xhr.complete(function() {
        BlockUi("off");
        that.close();
        that.parent.CheckCustomerStatusAndCreateApproveDialog();
        return false;
      });
    };

    return ApproveLoanWithoutAML;

  })(EzBob.BoundItemView);

}).call(this);

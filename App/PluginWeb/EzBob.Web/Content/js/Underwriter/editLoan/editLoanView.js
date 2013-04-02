(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EditLoanView = (function(_super) {

    __extends(EditLoanView, _super);

    function EditLoanView() {
      return EditLoanView.__super__.constructor.apply(this, arguments);
    }

    EditLoanView.prototype.template = "#loan_editor_template";

    EditLoanView.prototype.editors = {
      "Installment": EzBob.InstallmentEditor
    };

    EditLoanView.prototype.events = {
      "click .edit-schedule-item": "editScheduleItem"
    };

    EditLoanView.prototype.editScheduleItem = function(e) {
      var editor, id, item;
      id = e.currentTarget.getAttribute("data-id");
      item = this.model.get("Items")[id];
      console.log(item);
      editor = this.editors[item.Editor];
      console.log("using");
      return console.log(editor);
    };

    EditLoanView.prototype.onOk = function() {
      return console.log("ok");
    };

    EditLoanView.prototype.jqoptions = function() {
      return {
        buttons: {
          'Close': this.onOk
        },
        width: 800,
        modal: true,
        title: 'Edit Loan Details'
      };
    };

    return EditLoanView;

  })(Backbone.Marionette.ItemView);

}).call(this);

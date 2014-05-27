(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.AddCustomerRelationsEntry = (function(_super) {

    __extends(AddCustomerRelationsEntry, _super);

    function AddCustomerRelationsEntry() {
      return AddCustomerRelationsEntry.__super__.constructor.apply(this, arguments);
    }

    AddCustomerRelationsEntry.prototype.template = '#add-customer-relations-entry-template';

    AddCustomerRelationsEntry.prototype.events = {
      'keyup #Comment': 'commentKeyup'
    };

    AddCustomerRelationsEntry.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: true,
        title: 'CRM - add entry',
        position: 'center',
        draggable: true,
        dialogClass: 'customer-relations-popup',
        width: 600
      };
    };

    AddCustomerRelationsEntry.prototype.initialize = function(options) {
      this.onsave = options.onsave;
      this.onbeforesave = options.onbeforesave;
      this.customerId = this.model.customerId;
      this.url = window.gRootPath + 'Underwriter/CustomerRelations/SaveEntry/';
      return AddCustomerRelationsEntry.__super__.initialize.call(this);
    };

    AddCustomerRelationsEntry.prototype.onRender = function() {
      return this.ui.Action.prop('selectedIndex', 1);
    };

    AddCustomerRelationsEntry.prototype.serializeData = function() {
      var data;
      data = {
        actions: EzBob.CrmActions,
        statuses: EzBob.CrmStatuses,
        ranks: EzBob.CrmRanks
      };
      return data;
    };

    AddCustomerRelationsEntry.prototype.commentKeyup = function(el) {
      return this.ui.Comment.val(this.ui.Comment.val().replace(/\r\n|\r|\n/g, '\r\n').slice(0, 1000));
    };

    AddCustomerRelationsEntry.prototype.ui = {
      "Incoming": "#Incoming_I",
      "Status": "#Status",
      "Action": "#Action",
      "Rank": "#Rank",
      "Comment": "#Comment"
    };

    AddCustomerRelationsEntry.prototype.onSave = function() {
      var opts, xhr,
        _this = this;
      if (this.ui.Status[0].selectedIndex === 0) {
        return false;
      }
      if (this.ui.Action[0].selectedIndex === 0) {
        return false;
      }
      if (this.ui.Rank[0].selectedIndex === 0) {
        return false;
      }
      BlockUi();
      opts = {
        isIncoming: this.ui.Incoming[0].checked,
        action: this.ui.Action[0].value,
        status: this.ui.Status[0].value,
        rank: this.ui.Rank[0].value,
        comment: this.ui.Comment.val(),
        customerId: this.customerId
      };
      if (this.onbeforesave) {
        this.onbeforesave(opts);
      }
      xhr = $.post(this.url, opts);
      xhr.done(function(r) {
        if (r.success) {
          _this.model.fetch();
        } else {
          if (r.error) {
            EzBob.ShowMessage(r.error, 'Error');
          }
        }
        return _this.close();
      });
      xhr.always(function() {
        return UnBlockUi();
      });
      return false;
    };

    return AddCustomerRelationsEntry;

  })(EzBob.BoundItemView);

}).call(this);

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
      "keyup textarea": "commentKeyup"
    };

    AddCustomerRelationsEntry.prototype.initialize = function(options) {
      this.model = new Backbone.Model({
        actions: options.actions,
        statuses: options.statuses
      });
      this.mainTab = options.mainTab;
      return AddCustomerRelationsEntry.__super__.initialize.call(this);
    };

    AddCustomerRelationsEntry.prototype.render = function() {
      AddCustomerRelationsEntry.__super__.render.call(this);
      this.$el.find('#Action').prop('selectedIndex', 1);
      return this;
    };

    AddCustomerRelationsEntry.prototype.commentKeyup = function(el) {
      return $(el.target).val($(el.target).val().replace(/\r\n|\r|\n/g, "\r\n").slice(0, 1000));
    };

    AddCustomerRelationsEntry.prototype.onSave = function() {
      var that, xhr,
        _this = this;
      if (!$('#Incoming_I')[0].checked && !$('#Incoming_O')[0].checked) {
        return false;
      }
      if ($('#Status')[0].selectedIndex === 0) {
        return false;
      }
      if ($('#Action')[0].selectedIndex === 0) {
        return false;
      }
      BlockUi("on");
      that = this;
      xhr = $.post("" + window.gRootPath + "Underwriter/CustomerRelations/SaveEntry/", {
        isIncoming: $('#Incoming_I')[0].checked,
        action: $('#Action')[0].value,
        status: $('#Status')[0].value,
        comment: $('#Comment').val(),
        customerId: this.mainTab.model.customerId
      });
      xhr.done(function(r) {
        if (r.error != null) {
          EzBob.ShowMessage(r.error, "Error");
          return;
        }
        that.mainTab.model.fetch();
        return _this.close();
      });
      xhr.complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    return AddCustomerRelationsEntry;

  })(EzBob.BoundItemView);

}).call(this);

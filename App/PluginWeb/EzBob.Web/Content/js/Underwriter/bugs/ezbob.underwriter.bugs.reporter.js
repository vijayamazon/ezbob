(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter.BugModel = (function(_super) {

    __extends(BugModel, _super);

    function BugModel() {
      return BugModel.__super__.constructor.apply(this, arguments);
    }

    BugModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/Bugs/Report";
    };

    BugModel.prototype.idAttribute = 'Id';

    BugModel.prototype.defaults = {
      TextOpened: "",
      Type: "Other",
      DateOpened: new Date(),
      State: 'Opened',
      MarketPlaceId: null
    };

    return BugModel;

  })(Backbone.Model);

  EzBob.Underwriter.Bugs = (function(_super) {

    __extends(Bugs, _super);

    function Bugs() {
      return Bugs.__super__.constructor.apply(this, arguments);
    }

    Bugs.prototype.model = EzBob.Underwriter.BugModel;

    return Bugs;

  })(Backbone.Collection);

  EzBob.Underwriter.ReportBugView = (function(_super) {

    __extends(ReportBugView, _super);

    function ReportBugView() {
      return ReportBugView.__super__.constructor.apply(this, arguments);
    }

    ReportBugView.prototype.template = '#bug-report-template';

    ReportBugView.prototype.bindings = {
      TextOpened: {
        selector: "textarea[name='description']"
      }
    };

    return ReportBugView;

  })(EzBob.BoundItemView);

  EzBob.Underwriter.EditBugView = (function(_super) {

    __extends(EditBugView, _super);

    function EditBugView() {
      return EditBugView.__super__.constructor.apply(this, arguments);
    }

    EditBugView.prototype.events = {
      'click .btn-danger': 'closeBug'
    };

    EditBugView.prototype.template = '#bug-edit-template';

    EditBugView.prototype.eqConverter = function(v) {
      return function(direction, value) {
        return value === v;
      };
    };

    EditBugView.prototype.notEqConverter = function(v) {
      return function(direction, value) {
        return value !== v;
      };
    };

    EditBugView.prototype.bindings = {
      TextOpened: {
        selector: "textarea[name='description']"
      },
      TextClosed: {
        selector: "textarea[name='closeDescription']"
      },
      State: [
        {
          selector: "textarea",
          converter: EditBugView.prototype.notEqConverter('Closed'),
          elAttribute: 'enabled'
        }, {
          selector: ".btn-danger",
          converter: EditBugView.prototype.notEqConverter('Closed'),
          elAttribute: 'displayed'
        }, {
          selector: ".underwriter-closed",
          converter: EditBugView.prototype.eqConverter('Closed'),
          elAttribute: 'displayed'
        }
      ]
    };

    EditBugView.prototype.closeBug = function() {
      this.trigger('closeBug');
      return this.close();
    };

    return EditBugView;

  })(EzBob.BoundItemView);

  $('body').on('click', 'a[data-bug-type]', function(e) {
    var $e, bugCustomer, bugMP, bugType, xhr,
      _this = this;
    $e = $(e.currentTarget);
    bugType = $e.data('bug-type');
    bugMP = $e.data('bug-mp');
    bugCustomer = $e.data('bug-customer');
    if (!((bugType != null) && (bugCustomer != null))) {
      return false;
    }
    xhr = $.getJSON("" + window.gRootPath + "Underwriter/Bugs/TryGet", {
      MP: bugMP,
      CustomerId: bugCustomer,
      BugType: bugType
    });
    xhr.done(function(data) {
      var model, view;
      if ((data != null ? data.error : void 0)) {
        return;
      }
      view = null;
      model = null;
      if (data != null ? data.Id : void 0) {
        model = new EzBob.Underwriter.BugModel(data);
        view = new EzBob.Underwriter.EditBugView({
          model: model
        });
        view.on('closeBug', function() {
          return $.post("" + window.gRootPath + "Underwriter/Bugs/Close", model.toJSON());
        });
      } else {
        model = new EzBob.Underwriter.BugModel({
          Type: bugType,
          CustomerId: bugCustomer,
          MarketPlaceId: bugMP
        });
        view = new EzBob.Underwriter.ReportBugView({
          model: model
        });
      }
      view.on('save', function() {
        return model.save({}, {
          url: "" + window.gRootPath + "Underwriter/Bugs/Report"
        });
      });
      return EzBob.App.modal.show(view);
    });
    return false;
  });

}).call(this);

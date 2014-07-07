(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

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

    BugModel.prototype.save = function() {
      if (this.isNew()) {
        return BugModel.__super__.save.call(this, {}, {
          url: "" + window.gRootPath + "Underwriter/Bugs/CreateBug"
        });
      } else {
        this.set("DateOpened", new Date(moment.utc(this.get("DateOpened"))));
        return BugModel.__super__.save.call(this, {}, {
          url: "" + window.gRootPath + "Underwriter/Bugs/UpdateBug"
        });
      }
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
      'click .closeBug': 'closeBug',
      'click .reopenBug': 'reopenBug'
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
          selector: ".closeBug",
          converter: EditBugView.prototype.notEqConverter('Closed'),
          elAttribute: 'displayed'
        }, {
          selector: ".underwriter-closed",
          converter: EditBugView.prototype.eqConverter('Closed'),
          elAttribute: 'displayed'
        }, {
          selector: ".reopenBug",
          converter: EditBugView.prototype.eqConverter('Closed'),
          elAttribute: 'displayed'
        }, {
          selector: ".saveBug",
          converter: EditBugView.prototype.notEqConverter('Closed'),
          elAttribute: 'enabled'
        }
      ]
    };

    EditBugView.prototype.closeBug = function() {
      this.trigger('closeBug');
      return this.close();
    };

    EditBugView.prototype.reopenBug = function() {
      return this.trigger("reopenBug");
    };

    return EditBugView;

  })(EzBob.BoundItemView);

  EzBob.InitBugs = function() {
    return $('body').unbind('click').on('click', 'a[data-bug-type]', function(e) {
      var $e, bugCustomer, bugMP, bugType, director, xhr,
        _this = this;
      $e = $(e.currentTarget);
      bugType = $e.attr('data-bug-type');
      bugMP = $e.attr('data-bug-mp');
      bugCustomer = $e.attr('data-bug-customer');
      director = $e.attr('data-credit-bureau-director-id');
      console.log(e, $e.attr('data-bug-customer'));
      if (!((bugType != null) && (bugCustomer != null))) {
        return false;
      }
      xhr = $.getJSON("" + window.gRootPath + "Underwriter/Bugs/TryGet", {
        MP: bugMP,
        CustomerId: bugCustomer,
        BugType: bugType,
        Director: director
      });
      xhr.done(function(data) {
        var model, view;
        console.log(bugCustomer, bugType, data);
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
            var req;
            req = $.post("" + window.gRootPath + "Underwriter/Bugs/Close", model.toJSON());
            return req.done(function(response) {
              EzBob.UpdateBugsIcon($e, response.State);
              return view.close();
            });
          });
          view.on('reopenBug', function() {
            var req;
            req = $.post("" + window.gRootPath + "Underwriter/Bugs/Reopen", model.toJSON());
            return req.done(function(response) {
              model = new EzBob.Underwriter.BugModel(response);
              view.model = model;
              view.render();
              return EzBob.UpdateBugsIcon($e, response.State);
            });
          });
          view.on("closed", function() {
            return EzBob.UpdateBugsIcon($e, model.get('State'));
          });
        } else {
          model = new EzBob.Underwriter.BugModel({
            Type: bugType,
            CustomerId: bugCustomer,
            MarketPlaceId: bugMP,
            DirectorId: director
          });
          view = new EzBob.Underwriter.ReportBugView({
            model: model
          });
          EzBob.UpdateBugsIcon($e, model.get('State'));
          view.on("closed", function() {
            return EzBob.UpdateBugsIcon($e, "New");
          });
        }
        view.on('save', function() {
          if (model.get("State") !== "Closed") {
            model.save();
          }
          return view.close();
        });
        view.options = {
          show: true,
          keyboard: false,
          focusOn: "textarea:first"
        };
        return EzBob.App.jqmodal.show(view);
      });
      return false;
    });
  };

}).call(this);

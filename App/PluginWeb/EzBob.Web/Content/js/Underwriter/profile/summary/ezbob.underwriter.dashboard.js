(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.DashboardView = (function(_super) {

    __extends(DashboardView, _super);

    function DashboardView() {
      return DashboardView.__super__.constructor.apply(this, arguments);
    }

    DashboardView.prototype.template = "#dashboard-template";

    DashboardView.prototype.initialize = function(options) {
      this.crmModel = options.crmModel;
      this.personalModel = options.personalModel;
      this.experianModel = options.experianModel;
      this.bindTo(this.model, "change sync", this.render, this);
      this.bindTo(this.crmModel, "change sync", this.render, this);
      this.bindTo(this.personalModel, "change sync", this.render, this);
      return this.bindTo(this.experianModel, "change sync", this.render, this);
    };

    DashboardView.prototype.serializeData = function() {
      return {
        m: this.model.toJSON(),
        crm: this.crmModel.toJSON(),
        experian: this.experianModel.toJSON()
      };
    };

    DashboardView.prototype.events = {
      'click a[data-action="collapse"]': "boxToolClick",
      'click a[data-action="close"]': "boxToolClick"
    };

    DashboardView.prototype.boxToolClick = function(e) {
      var action, btn, obj;
      obj = e.currentTarget;
      if ($(obj).data("action") === undefined) {
        false;
      }
      action = $(obj).data("action");
      btn = $(obj);
      switch (action) {
        case "collapse":
          $(btn).children("i").addClass("anim-turn180");
          $(obj).parents(".box").children(".box-content").slideToggle(500, function() {
            if ($(this).is(":hidden")) {
              $(btn).children("i").attr("class", "fa fa-chevron-down");
            } else {
              $(btn).children("i").attr("class", "fa fa-chevron-up");
            }
            return false;
          });
          break;
        case "close":
          $(obj).parents(".box").fadeOut(500, function() {
            $(this).parent().remove();
            return false;
          });
          break;
        case "config":
          $("#" + $(obj).data("modal")).modal("show");
      }
      return false;
    };

    DashboardView.prototype.onRender = function() {};

    return DashboardView;

  })(Backbone.Marionette.ItemView);

}).call(this);

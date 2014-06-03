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
      this.propertiesModel = options.propertiesModel;
      this.mpsModel = options.mpsModel;
      this.loanModel = options.loanModel;
      this.bindTo(this.model, "change sync", this.render, this);
      this.bindTo(this.crmModel, "change sync", this.render, this);
      this.bindTo(this.personalModel, "change sync", this.personalModelChanged, this);
      this.bindTo(this.experianModel, "change sync", this.render, this);
      this.bindTo(this.propertiesModel, "change sync", this.render, this);
      this.bindTo(this.mpsModel, "change sync", this.render, this);
      return this.bindTo(this.loanModel, "change sync", this.render, this);
    };

    DashboardView.prototype.serializeData = function() {
      return {
        m: this.model.toJSON(),
        crm: _.first(_.filter(this.crmModel.get('CustomerRelations'), function(crm) {
          return crm.User !== 'System';
        }), 5),
        experian: this.experianModel.toJSON(),
        properties: this.propertiesModel.toJSON(),
        mps: this.mpsModel.toJSON(),
        loan: this.loanModel.toJSON(),
        affordability: _.first(_.filter(this.mpsModel.toJSON(), function(mp) {
          return mp.Name === 'HMRC';
        }), 1)
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

    DashboardView.prototype.personalModelChanged = function(e, a) {
      if (e && a && this.model) {
        return this.model.fetch();
      }
    };

    DashboardView.prototype.onRender = function() {
      var companyHistoryScores, consumerHistoryCIIs, consumerHistoryScores, historyScoresSorted;
      if (this.model.get('Alerts') !== void 0) {
        if (this.model.get('Alerts').length === 0) {
          $('#customer-label-span').removeClass('label-warning').removeClass('label-important').addClass('label-success');
        } else {
          if (_.some(this.model.get('Alerts'), function(alert) {
            return alert.AlertType === 'danger';
          })) {
            $('#customer-label-span').removeClass('label-success').removeClass('label-warning').addClass('label-important');
          } else {
            $('#customer-label-span').removeClass('label-success').removeClass('label-important').addClass('label-warning');
          }
        }
      }
      if (this.experianModel && this.experianModel.get('ConsumerHistory')) {
        historyScoresSorted = _.sortBy(this.experianModel.get('ConsumerHistory'), function(history) {
          return history.Date;
        });
        consumerHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',');
        this.$el.find(".consumerScoreGraph").attr('values', consumerHistoryScores);
        consumerHistoryCIIs = _.pluck(historyScoresSorted, 'CII').join(',');
        this.$el.find(".consumerCIIGraph").attr('values', consumerHistoryCIIs);
      }
      if (this.experianModel && this.experianModel.get('CompanyHistory')) {
        historyScoresSorted = _.sortBy(this.experianModel.get('CompanyHistory'), function(history) {
          return history.Date;
        });
        companyHistoryScores = _.pluck(historyScoresSorted, 'Score').join(',');
        this.$el.find(".companyScoreGraph").attr('values', companyHistoryScores);
      }
      $(".inline-sparkline").sparkline("html", {
        width: "100%",
        height: "100%",
        lineWidth: 2,
        spotRadius: 3,
        lineColor: "#88bbc8",
        fillColor: "#f2f7f9",
        spotColor: "green",
        maxSpotColor: "#00AEEF",
        minSpotColor: "red",
        chartRangeMin: -1,
        valueSpots: {
          ':': 'green'
        }
      });
      return this.$el.find('[data-toggle="tooltip"]').tooltip({
        'placement': 'bottom'
      });
    };

    return DashboardView;

  })(Backbone.Marionette.ItemView);

}).call(this);

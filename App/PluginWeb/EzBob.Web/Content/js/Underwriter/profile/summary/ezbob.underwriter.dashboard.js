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
      this.companyModel = options.companyModel;
      this.bindTo(this.model, "change sync", this.render, this);
      this.bindTo(this.crmModel, "change sync", this.render, this);
      this.bindTo(this.personalModel, "change sync", this.personalModelChanged, this);
      this.bindTo(this.experianModel, "change sync", this.render, this);
      this.bindTo(this.companyModel, "change sync", this.render, this);
      this.bindTo(this.propertiesModel, "change sync", this.render, this);
      this.bindTo(this.mpsModel, "change sync", this.render, this);
      this.bindTo(this.loanModel, "change sync", this.render, this);
      return this.expCompany = [];
    };

    DashboardView.prototype.serializeData = function() {
      var _this = this;
      this.expCompany = [];
      if (this.companyModel.get('DashboardModel')) {
        this.expCompany.push(this.companyModel.get('DashboardModel'));
      }
      if (this.companyModel.get('Owners')) {
        _.each(this.companyModel.get('Owners'), function(owner) {
          return _this.expCompany.push(owner.DashboardModel);
        });
      }
      return {
        m: this.model.toJSON(),
        crm: _.first(_.filter(this.crmModel.get('CustomerRelations'), function(crm) {
          return crm.User !== 'System';
        }), 5),
        experian: this.experianModel.toJSON(),
        company: this.expCompany,
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
      'click a[data-action="close"]': "boxToolClick",
      'click a[href^="#companyExperian"]': "companyChanged"
    };

    DashboardView.prototype.companyChanged = function(e) {
      var obj;
      obj = e.currentTarget;
      return this.$el.find('.company-name').text($(obj).data('companyname') + ' ' + $(obj).data('companyref'));
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
      var cc, cii, companyHistoryScores, consumerHistoryCIIs, consumerHistoryScores, directors, historyScoresSorted, i, properties, _results,
        _this = this;
      this.$el.find('a[data-bug-type]').tooltip({
        title: 'Report bug'
      });
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
        this.$el.find(".companyScoreGraph0").attr('values', companyHistoryScores);
      }
      this.$el.find(".inline-sparkline").sparkline("html", {
        width: "100%",
        height: "100%",
        lineWidth: 2,
        spotRadius: 3.5,
        lineColor: "#cfcfcf",
        fillColor: "transparent",
        spotColor: "#cfcfcf",
        maxSpotColor: "#cfcfcf",
        minSpotColor: "#cfcfcf",
        valueSpots: {
          ':': '#cfcfcf'
        }
      });
      properties = this.propertiesModel.toJSON();
      if (properties && properties.NetWorth) {
        this.drawDonut(this.$el.find("#assets-donut"), "#00ab5d", properties.NetWorth / (properties.NetWorth + properties.SumOfMortgages));
      }
      this.$el.find('[data-toggle="tooltip"]').tooltip({
        'placement': 'bottom'
      });
      cc = this.$el.find("#consumerScoreCanvas");
      this.halfDonut(cc, cc.data('color'), cc.data('percent'));
      cii = this.$el.find("#consumerCIICanvas");
      this.halfDonut(cii, cii.data('color'), cii.data('percent'));
      if (this.expCompany && this.expCompany.length > 0 && !this.expCompany[0].Error) {
        _.each(this.expCompany, function(c, i) {
          var compC, equity, profit;
          compC = _this.$el.find("#companyScoreCanvas" + i);
          _this.halfDonut(compC, compC.data('color'), compC.data('percent'));
          if (c.IsLimited) {
            profit = _.pluck(c.FinDataHistories, 'AdjustedProfit').reverse().join(',');
            _this.$el.find("#companyProfit" + i).attr('values', profit);
            equity = _.pluck(c.FinDataHistories, 'TangibleEquity').reverse().join(',');
            return _this.$el.find("#companyEquity" + i).attr('values', equity);
          }
        });
      }
      this.$el.find('.bar-sparkline').sparkline("html", {
        type: 'bar',
        barColor: '#cfcfcf',
        height: "50px"
      });
      if (this.experianModel && this.experianModel.get('directorsModels')) {
        directors = this.experianModel.get('directorsModels').length;
        i = 0;
        _results = [];
        while (i < directors) {
          cc = this.$el.find("#directorScoreCanvas" + i);
          this.halfDonut(cc, cc.data('color'), cc.data('percent'));
          cii = this.$el.find("#directorCIICanvas" + i);
          this.halfDonut(cii, cii.data('color'), cii.data('percent'));
          _results.push(i++);
        }
        return _results;
      }
    };

    DashboardView.prototype.halfDonut = function(el, fillColor, fillPercent) {
      var canvas, context, counterClockwise, endAngle, lineWidth, radius, startAngle, x, y;
      canvas = el[0];
      context = canvas.getContext('2d');
      x = canvas.width / 2;
      y = canvas.height / 2;
      radius = 40;
      startAngle = 1 * Math.PI;
      endAngle = 2 * Math.PI;
      counterClockwise = false;
      lineWidth = 15;
      context.beginPath();
      context.arc(x, y, radius, startAngle, endAngle, counterClockwise);
      context.lineWidth = lineWidth;
      context.strokeStyle = '#ebebeb';
      context.stroke();
      context.beginPath();
      context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent), counterClockwise);
      context.strokeStyle = fillColor;
      return context.stroke();
    };

    DashboardView.prototype.drawDonut = function(el, fillColor, fillPercent) {
      var canvas, context, endAngle, lineWidth, radius, startAngle, x, y;
      canvas = el[0];
      context = canvas.getContext("2d");
      x = canvas.width / 2;
      y = canvas.height / 2;
      radius = 70;
      startAngle = 1 * Math.PI;
      endAngle = 4 * Math.PI;
      lineWidth = 25;
      context.beginPath();
      context.arc(x, y, radius, startAngle, endAngle, false);
      context.lineWidth = lineWidth;
      context.strokeStyle = "#ebebeb";
      context.stroke();
      context.beginPath();
      context.arc(x, y, radius, startAngle, Math.PI * (1 + fillPercent * 2), false);
      context.strokeStyle = fillColor;
      context.stroke();
    };

    return DashboardView;

  })(Backbone.Marionette.ItemView);

}).call(this);

(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.AgreementViewBase = (function(_super) {

    __extends(AgreementViewBase, _super);

    function AgreementViewBase() {
      return AgreementViewBase.__super__.constructor.apply(this, arguments);
    }

    AgreementViewBase.prototype.initialize = function() {
      return this.template = Handlebars.compile($(this.getTemplate()).html());
    };

    AgreementViewBase.prototype.render = function(data) {
      var _this = this;
      this.$el.html(this.template(data));
      this.addScroll();
      this.$el.find("a[data-toggle=\"tab\"]").on("shown", function(e) {
        return _this.addScroll();
      });
      return this;
    };

    AgreementViewBase.prototype.addScroll = function() {
      return this.$el.find(".overview").jScrollPane();
    };

    return AgreementViewBase;

  })(Backbone.View);

  EzBob.Profile.CompaniesAgreementView = (function(_super) {

    __extends(CompaniesAgreementView, _super);

    function CompaniesAgreementView() {
      return CompaniesAgreementView.__super__.constructor.apply(this, arguments);
    }

    CompaniesAgreementView.prototype.getTemplate = function() {
      return "#companies-agreement-template";
    };

    return CompaniesAgreementView;

  })(EzBob.Profile.AgreementViewBase);

  EzBob.Profile.ConsumersAgreementView = (function(_super) {

    __extends(ConsumersAgreementView, _super);

    function ConsumersAgreementView() {
      return ConsumersAgreementView.__super__.constructor.apply(this, arguments);
    }

    ConsumersAgreementView.prototype.getTemplate = function() {
      return "#consumers-agreement-template";
    };

    ConsumersAgreementView.prototype.events = {
      "change .preAgreementTermsRead": "preAgreementTermsReadChange"
    };

    ConsumersAgreementView.prototype.preAgreementTermsReadChange = function() {
      var readPreAgreement;
      readPreAgreement = $(".preAgreementTermsRead").is(":checked");
      this.$el.find(".agreementTermsRead").attr("disabled", !readPreAgreement);
      if (readPreAgreement) {
        return this.$el.find("a[href=\"#tab4\"]").tab("show");
      } else {
        this.$el.find("a[href=\"#tab3\"]").tab("show");
        return this.$el.find(".agreementTermsRead").attr("checked", false);
      }
    };

    ConsumersAgreementView.prototype.render = function(data) {
      this.$el.find(".agreementTermsRead").attr("checked", false).attr("disabled", true);
      return ConsumersAgreementView.__super__.render.call(this, data);
    };

    return ConsumersAgreementView;

  })(EzBob.Profile.AgreementViewBase);

}).call(this);

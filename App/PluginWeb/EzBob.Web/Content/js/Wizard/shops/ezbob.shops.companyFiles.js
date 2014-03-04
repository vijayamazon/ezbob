(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.CompanyFilesAccountModel = (function(_super) {

    __extends(CompanyFilesAccountModel, _super);

    function CompanyFilesAccountModel() {
      return CompanyFilesAccountModel.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccountModel.prototype.urlRoot = "" + window.gRootPath + "Customer/CompanyFilesMarketPlaces/Accounts";

    return CompanyFilesAccountModel;

  })(Backbone.Model);

  EzBob.CompanyFilesAccounts = (function(_super) {

    __extends(CompanyFilesAccounts, _super);

    function CompanyFilesAccounts() {
      return CompanyFilesAccounts.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccounts.prototype.model = EzBob.CompanyFilesAccountModel;

    CompanyFilesAccounts.prototype.url = "" + window.gRootPath + "Customer/CompanyFilesMarketPlaces/Accounts";

    return CompanyFilesAccounts;

  })(Backbone.Collection);

  EzBob.CompanyFilesAccountInfoView = (function(_super) {

    __extends(CompanyFilesAccountInfoView, _super);

    function CompanyFilesAccountInfoView() {
      return CompanyFilesAccountInfoView.__super__.constructor.apply(this, arguments);
    }

    CompanyFilesAccountInfoView.prototype.events = {
      'change input': 'inputChanged',
      'keyup input': 'inputChanged',
      'click a.hmrcBack': 'back',
      'click a.connect-account': 'connect'
    };

    CompanyFilesAccountInfoView.prototype.initialize = function(options) {
      this.accountType = 'CompanyFiles';
      this.template = '#' + this.accountType + 'AccountInfoTemplate';
      return this.isOldInternetExplorer = 'Microsoft Internet Explorer' === navigator.appName && navigator.appVersion.indexOf("MSIE 1") === -1;
    };

    CompanyFilesAccountInfoView.prototype.render = function() {
      var that;
      CompanyFilesAccountInfoView.__super__.render.call(this);
      that = this;
      return this;
    };

    CompanyFilesAccountInfoView.prototype.inputChanged = function() {};

    CompanyFilesAccountInfoView.prototype.uploadFiles = function() {};

    CompanyFilesAccountInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    CompanyFilesAccountInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Upload Company Files';
    };

    CompanyFilesAccountInfoView.prototype.connect = function() {
      this.inputChanged();
      this.trigger('completed');
      this.trigger('back');
      return false;
    };

    return CompanyFilesAccountInfoView;

  })(Backbone.Marionette.ItemView);

}).call(this);

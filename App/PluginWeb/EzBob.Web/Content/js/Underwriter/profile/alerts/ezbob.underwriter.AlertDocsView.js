(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter.DocModel = (function(_super) {

    __extends(DocModel, _super);

    function DocModel() {
      return DocModel.__super__.constructor.apply(this, arguments);
    }

    return DocModel;

  })(Backbone.Model);

  EzBob.Underwriter.Docs = (function(_super) {

    __extends(Docs, _super);

    function Docs() {
      return Docs.__super__.constructor.apply(this, arguments);
    }

    Docs.prototype.model = EzBob.Underwriter.DocModel;

    Docs.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/AlertDocs/List/" + this.customerId;
    };

    return Docs;

  })(Backbone.Collection);

  EzBob.Underwriter.UploadDocView = (function(_super) {

    __extends(UploadDocView, _super);

    function UploadDocView() {
      return UploadDocView.__super__.constructor.apply(this, arguments);
    }

    UploadDocView.prototype.template = '#uploadAlertDocDialog';

    UploadDocView.prototype.initialize = function(options) {
      return this.customerId = options.customerId;
    };

    UploadDocView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Upload Doc",
        position: "center",
        draggable: false,
        width: 530,
        dialogClass: "upload-doc-popup"
      };
    };

    UploadDocView.prototype.events = {
      'click .button-upload': 'upload'
    };

    UploadDocView.prototype.upload = function(e) {
      var f,
        _this = this;
      if ($(e.currentTarget).hasClass("disabled")) {
        return;
      }
      $(e.currentTarget).addClass("disabled");
      f = this.$el.find('input[type="file"]')[0];
      if (typeof f.files !== "undefined" && f.files.length === 0 || !f.value) {
        EzBob.ShowMessage("Please select a file!", "Warning");
        $(e.currentTarget).removeClass("disabled");
        return;
      }
      this.$el.find("#fileForm").find('input[name=CustomerId]').val(this.customerId);
      return this.$el.find("#fileForm").ajaxSubmit({
        cache: false,
        success: function() {
          _this.trigger('upload:ok');
          return _this.close();
        },
        error: function() {
          EzBob.ShowMessage("Upload failed, possible cause may be big file size (use smaller file) or session time out.", "Error");
          return $(e.currentTarget).removeClass("disabled");
        }
      });
    };

    return UploadDocView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.AlertDocsView = (function(_super) {

    __extends(AlertDocsView, _super);

    function AlertDocsView() {
      return AlertDocsView.__super__.constructor.apply(this, arguments);
    }

    AlertDocsView.prototype.root = window.gRootPath + "Underwriter/AlertDocs/";

    AlertDocsView.prototype.template = '#docs-template';

    AlertDocsView.prototype.customerId = 0;

    AlertDocsView.prototype.dialogId = "null";

    AlertDocsView.prototype.events = {
      "click #addNewDoc": "addClick",
      "click #deleteDocs": "deleteClick"
    };

    AlertDocsView.prototype.initialize = function(options) {
      return this.bindTo(this.model, 'change reset fetch sync', this.render, this);
    };

    AlertDocsView.prototype.create = function(customerId) {
      this.customerId = customerId;
      this.dialogId = "#uploadAlertDocDialog" + customerId;
      return this.model.customerId = customerId;
    };

    AlertDocsView.prototype.serializeData = function() {
      return {
        docs: this.model.toJSON()
      };
    };

    AlertDocsView.prototype.addClick = function() {
      var cb, view;
      view = new EzBob.Underwriter.UploadDocView({
        customerId: this.customerId
      });
      cb = function() {
        this.model.fetch();
        return EzBob.ShowMessage("File successfully downloaded to \"Messages\" tab", "Successful");
      };
      view.on('upload:ok', cb, this);
      return EzBob.App.jqmodal.show(view);
    };

    AlertDocsView.prototype.deleteClick = function() {
      var e, ids,
        _this = this;
      ids = (function() {
        var _i, _len, _ref, _results;
        _ref = this.$el.find('input[type="checkbox"]:checked');
        _results = [];
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          e = _ref[_i];
          _results.push($(e).data('id'));
        }
        return _results;
      }).call(this);
      if (ids.length === 0) {
        EzBob.ShowMessage("Please select at least one document to delete.", "Warning");
        return;
      }
      EzBob.ShowMessage("Are you sure you want to delete selected docs?", "", function() {
        var xhr;
        xhr = $.ajax({
          type: "POST",
          traditional: true,
          url: "" + _this.root + "DeleteDocs",
          data: {
            docIds: ids
          },
          dataType: "json"
        });
        return xhr.done(function() {
          _this.model.fetch();
          return EzBob.ShowMessage("File successfully deleted", "Successful");
        });
      }, "OK", null, "Cancel");
      return false;
    };

    return AlertDocsView;

  })(Backbone.Marionette.ItemView);

}).call(this);

(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.CAIS = EzBob.Underwriter.CAIS || {};

  EzBob.Underwriter.CAIS.ListOfFilesModel = (function(_super) {

    __extends(ListOfFilesModel, _super);

    function ListOfFilesModel() {
      return ListOfFilesModel.__super__.constructor.apply(this, arguments);
    }

    ListOfFilesModel.prototype.url = "" + gRootPath + "Underwriter/CAIS/ListOfFiles";

    ListOfFilesModel.prototype["default"] = {
      cais: {}
    };

    ListOfFilesModel.prototype.initialize = function() {
      var interval,
        _this = this;
      interval = setInterval((function() {
        return _this.fetch();
      }), 2000);
      return this.set("interval", interval);
    };

    return ListOfFilesModel;

  })(Backbone.Model);

  EzBob.Underwriter.CAIS.SelectedFile = (function(_super) {

    __extends(SelectedFile, _super);

    function SelectedFile() {
      return SelectedFile.__super__.constructor.apply(this, arguments);
    }

    SelectedFile.prototype["default"] = {
      id: ""
    };

    return SelectedFile;

  })(Backbone.Model);

  EzBob.Underwriter.CAIS.SelectedFiles = (function(_super) {

    __extends(SelectedFiles, _super);

    function SelectedFiles() {
      return SelectedFiles.__super__.constructor.apply(this, arguments);
    }

    SelectedFiles.prototype.model = EzBob.Underwriter.CAIS.SelectedFile;

    SelectedFiles.prototype.getModelById = function(id) {
      return this.filter(function(val) {
        return val.get("id") === id;
      });
    };

    return SelectedFiles;

  })(Backbone.Collection);

  EzBob.Underwriter.CAIS.CaisManageView = (function(_super) {

    __extends(CaisManageView, _super);

    function CaisManageView() {
      return CaisManageView.__super__.constructor.apply(this, arguments);
    }

    CaisManageView.prototype.template = _.template($("#cais-template").length > 0 ? $("#cais-template").html() : "");

    CaisManageView.prototype.initialize = function() {
      var _this = this;
      this.model = new EzBob.Underwriter.CAIS.ListOfFilesModel();
      this.bindTo(this.model, "change reset", this.render, this);
      BlockUi("on");
      this.model.fetch().done(function() {
        return BlockUi("off");
      });
      this.checkedModel = new EzBob.Underwriter.CAIS.SelectedFiles();
      return this.bindTo(this.checkedModel, "add remove reset", this.checkedFileModelChanged, this);
    };

    CaisManageView.prototype.ui = {
      count: ".reports-count",
      download: ".download"
    };

    CaisManageView.prototype.onRender = function() {
      return this.checkedFileModelChanged();
    };

    CaisManageView.prototype.serializeData = function() {
      return {
        model: this.model.get("cais"),
        checkedModel: this.checkedModel.toJSON()
      };
    };

    CaisManageView.prototype.checkedFileModelChanged = function() {
      if (this.checkedModel.length === 0) {
        return this.ui.download.hide();
      } else {
        this.ui.download.show();
        return this.ui.count.text(this.checkedModel.length);
      }
    };

    CaisManageView.prototype.events = {
      "click .generate": "generateClicked",
      "click .download ": "downloadFile",
      "dblclick [data-id]": "fileSelected",
      "click [data-id]": "fileChecked"
    };

    CaisManageView.prototype.downloadFile = function() {
      return _.each(this.checkedModel.toJSON(), function(val) {
        return window.open("" + gRootPath + "Underwriter/CAIS/DownloadFile?Id=" + val.id, "_blank");
      });
    };

    CaisManageView.prototype.fileViewChanged = function(e) {
      var $el;
      $el = $(e.currentTarget);
      ($(".save-change")).removeClass("disabled");
      return $el.css("border", "1px solid lightgreen");
    };

    CaisManageView.prototype.resetFileView = function() {
      ($("textarea")).css("border", "1px solid #cccccc");
      return ($(".save-change")).addClass("disabled");
    };

    CaisManageView.prototype.fileChecked = function(e) {
      var $el, checked, id;
      if (_.keys($(e.target).data()).length > 0) {
        return;
      }
      $el = $(e.currentTarget);
      checked = $el.hasClass("checked");
      id = $el.data("id");
      $el.toggleClass("checked", !checked);
      if (!checked) {
        return this.checkedModel.add(new EzBob.Underwriter.CAIS.SelectedFile({
          id: id
        }));
      } else {
        return this.checkedModel.remove(this.checkedModel.getModelById(id));
      }
    };

    CaisManageView.prototype.generateClicked = function(e) {
      var $el;
      $el = $(e.currentTarget);
      if ($el.hasClass("disabled")) {
        return;
      }
      $el.addClass("disabled");
      return $.post(gRootPath + 'Underwriter/CAIS/Generate').done(function(response) {
        if (response.error !== void 0) {
          EzBob.ShowMessage("Something went wrong", "Error occured");
          return;
        }
        return EzBob.ShowMessage("Generating current CAIS reports. Please wait few minutes.", "Successful");
      }).always(function() {
        return $el.removeClass("disabled");
      });
    };

    CaisManageView.prototype.fileSelected = function(e) {
      var $el, id, self,
        _this = this;
      self = this;
      $el = $(e.currentTarget);
      id = $el.data("id");
      BlockUi("on");
      return ($.get("" + gRootPath + "Underwriter/CAIS/GetOneFile", {
        id: id
      })).done(function(response) {
        var dialog;
        if (response.error) {
          EzBob.ShowMessage(response.error, "Error");
          return;
        }
        dialog = $('<div/>').html("<textarea wrap='off' class='cais-file-view'>" + response + "</textarea>");
        dialog.dialog({
          title: id,
          width: '75%',
          height: 600,
          modal: true,
          draggable: false,
          resizable: false,
          buttons: [
            {
              text: "Save file changes",
              click: function(e) {
                return self.saveFileChange(e);
              },
              "class": 'btn btn-primary save-change disabled',
              'data-id': id
            }, {
              html: "<i class='fa fa-refresh'></i>Set Status Uploaded",
              click: function(e) {
                return self.fileUploaded(e);
              },
              "class": 'btn btn-primary save-change',
              'data-id': id
            }, {
              text: "Close",
              click: function() {
                return dialog.dialog('destroy');
              },
              "class": 'btn btn-primary'
            }
          ]
        });
        return (dialog.find(".cais-file-view")).on("keypress keyup keydown", _this.fileViewChanged);
      }).always(function() {
        return BlockUi("off");
      });
    };

    CaisManageView.prototype.fileUploaded = function(e) {
      var $el, id, self, xhr;
      self = this;
      $el = $(e.currentTarget);
      id = $el.data("id");
      BlockUi("on");
      xhr = $.post("" + gRootPath + "CAIS/UpdateStatus", {
        id: id
      });
      xhr.done(function(response) {
        if (response.error) {
          EzBob.ShowMessage(response.error, "Something went wrong");
          return false;
        }
        EzBob.ShowMessage("Status Updated ", "Successful");
        return self.resetFileView();
      });
      xhr.fail(function() {
        return EzBob.ShowMessage("Error occured", "Something went wrong");
      });
      return xhr.always(function() {
        return BlockUi("off");
      });
    };

    CaisManageView.prototype.saveFileChange = function(e) {
      var $caisTextarea, $el, caisContent, id, saveFn, self;
      self = this;
      $el = $(e.currentTarget);
      if ($el.hasClass("disabled")) {
        return;
      }
      $caisTextarea = $("textarea:visible");
      caisContent = $caisTextarea.val();
      id = $el.data("id");
      saveFn = function() {
        var xhr;
        BlockUi("on");
        xhr = $.post("" + gRootPath + "CAIS/SaveFileChange", {
          fileContent: caisContent,
          id: id
        });
        xhr.done(function(response) {
          if (response.error) {
            EzBob.ShowMessage(response.error, "Something went wrong");
            return false;
          }
          EzBob.ShowMessage("File #" + id + " successfully saved ", "Successful");
          return self.resetFileView();
        });
        xhr.fail(function() {
          return EzBob.ShowMessage("Error occured", "Something went wrong");
        });
        return xhr.always(function() {
          return BlockUi("off");
        });
      };
      return EzBob.ShowMessage("Are you sure you want to save the change?", "Confirmation", saveFn, "Save", null, "Cancel");
    };

    return CaisManageView;

  })(Backbone.Marionette.ItemView);

}).call(this);

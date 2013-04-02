var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AlertsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#alerts-template').html());
        this.model.on('change reset', this.render, this);
        
        this.dialog = new EzBob.Underwriter.ShowEditDialog();
        this.dialog.on("dataSaved", this.dialogDataSaved, this);
    },
    render: function () {
        this.$el.html(this.template({ m: this.model.toJSON() }));
        $("#alertDialog").data('model', this.model);
        this.$el.find('.underwriter-tooltip').tooltip();
    },
    events: {
        "click .alert-link": "showDialog"
    },
    showDialog: function (e) {
        var id = $(e.currentTarget).attr('data-id');
        this.dialog.model.set("Id", id, {silent: true});
        this.dialog.model.fetch();
        this.dialog.$el.dialog("open");
        return false;
    },
    dialogDataSaved: function () {
        this.model.fetch();
    }
});

EzBob.Underwriter.AlertsModel = Backbone.Model.extend({
    idAttribute: "Id",
    url: function () {
        return window.gRootPath + "Underwriter/Alerts/Index?id=" + this.get("Id") + (this.showPassed != undefined ? "&showPassed=true" : "");
    }
});

EzBob.Underwriter.AlertModel = Backbone.Model.extend({
    idAttribute: "Id",
    urlRoot: window.gRootPath + "Underwriter/Alerts/AlertOperation"
});

EzBob.Underwriter.ShowEditDialog = Backbone.View.extend({
    initialize: function () {
        this.$el.html($('.alertDialog').html());
        this.model = new EzBob.Underwriter.AlertModel();
        this.model.on("change", this.render, this);
    },
    render: function () {
        this.$el.find("[name='status']").val(this.model.get("Status"));
        this.$el.find("[name='details']").text(this.model.get("Details"));

        this.$el.dialog({ title: "Alert", resizable: false, modal: true, draggable: false });
    },
    events: {
        "click [name='save']" : "saveData"
    },
    saveData: function (e) {
        var that = this;

        this.model.set({
            "Status": this.$el.find("[name='status']").val(),
            "Details": this.$el.find("[name='details']").val()
        }, { silent: true });
        
        $.post(window.gRootPath + "Underwriter/Alerts/SaveAlert", this.model.toJSON())
                .done(function () {
                    that.dataSaved();
                    that.$el.dialog('destroy');
                });
        return false;
    },
    dataSaved: function () {
        this.trigger("dataSaved");
    }
});

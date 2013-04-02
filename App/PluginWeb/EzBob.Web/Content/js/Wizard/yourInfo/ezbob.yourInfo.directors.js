var EzBob = EzBob || {};

EzBob.DirectorMainView = Backbone.View.extend({
    initialize: function (options) {
        this.preffix = options.name || 'directors';
        this.template = _.template($('#director-edit-template').html());
        this.directorTemplate = _.template($('#oneDirector').html());
        this.model = new EzBob.DirectorsModels();
        this.name = 'DirectorAddress';
    },
    events: {
        "click #addDirector": "addDirector",
        "click .removeDirector": "removeDirector"
    },
    render: function () {
        this.$el.html(this.template());
        this.directorArea = this.$el.find('.directorArea');
        return this;
    },
    renderDirector: function () {
        var that = this;

        this.directorArea.html(this.directorTemplate({ preffix: this.preffix, directors: this.model.toJSON() }));
        this.directorArea.find('.ezDateTime').splittedDateTime();

        $.each(this.model.models, function (i, val) {
            var addressElem = that.preffix + 'Address' + i,
                name = that.preffix + "[" + i + "]." + that.name,
                addressView = new EzBob.AddressView({ model: val.get('Address'), name: name, max: 1 }),
                dateOfBirthValName = that.preffix + "[" + i + "]." + 'DateOfBirth';

            addressView.render().$el.appendTo(that.directorArea.find("#" + addressElem));
            SetDefaultDate(dateOfBirthValName, val.get("DateOfBirth"));
        });
    },
    addDirector: function () {
        this.model.add(new EzBob.DirectorsModel({ Address: new EzBob.AddressModels() }));
        this.updateModel();
        this.renderDirector();

        return false;
    },
    removeDirector: function (el) {
        this.updateModel();
        var num = $(el.currentTarget).attr('number') - 1;
        this.model.remove(this.model.at(num));
        this.renderDirector();
        return false;
    },
    updateModel: function () {
        var model = SerializeArrayToEasyObject(this.directorArea.parents('form').serializeArray()),
            that = this;
        $.each(this.model.models, function (i, oneModel) {
            $.each(oneModel.toJSON(), function (key) {
                $.each(model, function (k, formModelValue) {
                    var valueName = that.preffix + "[" + i + "]." + key;
                    if (k == valueName) {
                        oneModel.set(key, formModelValue);
                    }
                });
            });
        });
    }
});

EzBob.DirectorsModel = Backbone.Model.extend({
    defaults: {
        Name: "",
        Middle:"",
        Surname: "",
        Gender: "",
        DateOfBirth: "",
        Address: new EzBob.AddressModels()
    }
});

EzBob.DirectorsModels = Backbone.Collection.extend({
    model: EzBob.DirectorModel
});
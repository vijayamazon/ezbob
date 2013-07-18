var EzBob = EzBob || {};

EzBob.DirectorMainView = Backbone.View.extend({
    initialize: function(options) {
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
    render: function() {
        this.$el.html(this.template());
        this.directorArea = this.$el.find('.directorArea');
        return this;
    },
    renderDirector: function() {
        var that = this;

        this.directorArea.html(this.directorTemplate({ preffix: this.preffix, directors: this.model.toJSON() }));
        this.directorArea.find('.ezDateTime').splittedDateTime();

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

        $.each(this.model.models, function(i, val) {
            var addressElem = that.preffix + 'Address' + i,
                name = that.preffix + "[" + i + "]." + that.name,
                addressView = new EzBob.AddressView({ model: val.get('Address'), name: name, max: 1 }),
                dateOfBirthValName = that.preffix + "[" + i + "]." + 'DateOfBirth';

            val.get('Address').on("all", function() {
                that.trigger("director:addressChanged");
            });

            addressView.render().$el.appendTo(that.directorArea.find("#" + addressElem));
            SetDefaultDate(dateOfBirthValName, val.get("DateOfBirth"));

            that.addressErrorPlacement(addressView.$el, addressView.model);
        });
        this.$el.attardi_labels('toggle_all');
        this.trigger("director:change");
    },

    validateAddresses: function() {
        var result = true;
        $.each(this.model.models, function(i, val) {
            if (val.get('Address').length == 0) {
                result = false;
            }
        });
        return result;
    },

    addressErrorPlacement: function (el, model) {
        var $el = $(el);
        $el.on("focusout", function () {
            if (model.length == 0) {
                $el.tooltip({
                    title: "This field is required"
                }).tooltip("enable").tooltip('fixTitle');
            }
        });

        model.on("change", function () {
            if (model.length > 0) {
                $el.tooltip('destroy');
            }
        });
    },

    addDirector: function() {
        this.model.add(new EzBob.DirectorsModel({ Address: new EzBob.AddressModels() }));
        this.updateModel();
        this.renderDirector();
        return false;
    },
    removeDirector: function(el) {
        this.updateModel();
        var num = $(el.currentTarget).attr('number') - 1;
        this.model.remove(this.model.at(num));
        this.renderDirector();
        return false;
    },
    updateModel: function() {
        var model = SerializeArrayToEasyObject(this.directorArea.parents('form').serializeArray()),
            that = this;
        $.each(this.model.models, function(i, oneModel) {
            $.each(oneModel.toJSON(), function(key) {
                $.each(model, function(k, formModelValue) {
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
        Middle: "",
        Surname: "",
        Gender: "",
        DateOfBirth: "",
        Address: new EzBob.AddressModels()
    }
});

EzBob.DirectorsModels = Backbone.Collection.extend({
    model: EzBob.DirectorModel
});
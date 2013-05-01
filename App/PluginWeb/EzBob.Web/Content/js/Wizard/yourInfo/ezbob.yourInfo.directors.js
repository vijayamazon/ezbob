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
        "click .removeDirector": "removeDirector",
        "change .director_name_part": "directorNamePartChanged",
        "change .director_gender": "directorGenderChanged"
    },
	directorNamePartChanged: function(jqEvent) {
		var oTarget = $(jqEvent.target);
		var oIcon = oTarget.closest('div').find('img.field_status').first();

		var sStatusName = '';
		if (oIcon.hasClass('required'))
			sStatusName = oTarget.val() ? 'ok' : 'fail';
		else
			sStatusName = oTarget.val() ? 'ok' : 'empty';

		oIcon.field_status('set', sStatusName);
	},
	directorGenderChanged: function(jqEvent) {
		var oTarget = $(jqEvent.target);
		var oParent = oTarget.closest('div');

		var nCheckedCount = 0;

		oParent.find('.director_gender').each(function(idx, oChk) {
			if ($(oChk).attr('checked')) {
				nCheckedCount++;
				return false;
			} // if

			return true;
		});

		sStatusName = nCheckedCount ? 'ok' : 'fail';

		oParent.find('img.field_status').field_status('set', sStatusName);
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

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });

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
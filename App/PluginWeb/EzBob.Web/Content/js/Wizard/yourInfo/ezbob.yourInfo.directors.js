var EzBob = EzBob || {};

EzBob.DirectorMainView = Backbone.View.extend({
    initialize: function(options) {
        this.preffix = options.name || 'directors';
        this.template = _.template($('#director-edit-template').html());
        this.directorTemplate = _.template($('#oneDirector').html());
        this.model = new EzBob.DirectorsModels();
        this.name = 'DirectorAddress';
        this.validator = options.validator;
    },
    events: {
        "click #addDirector": "addDirector",
        "click .removeDirector": "removeDirector",

        "change input": "inputChanged",
        "keyup input": "inputChanged",
        "focusout input": "inputChanged",
        "input click": "inputChanged",
        "click select": "inputChanged",
        "focusout select": "inputChanged",
        "keyup select": "inputChanged"
    },
    inputChanged: function(event) {
        var status = this.validator.check($(event.currentTarget));
        var val = $(event.currentTarget).val();
        var name = $(event.currentTarget).attr('name');
        var hidden = $('[name="' + name + 'Image"]');
        
        if (hidden) {
            if (status) {
                hidden.val('ok');
            }
            if (!status && val) {
                hidden.val('fail');
            }
            if (!status && !val && $(event.currentTarget).hasClass('required')) {
                hidden.val('required');
            }
            if (!val && $(event.currentTarget).hasClass('nonrequired')) {
                hidden.val('empty');
                var el = event ? $(event.currentTarget) : null;
                el.closest('div').find('.field_status').field_status('set', 'empty', 2);
            }
        }
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

        this.$el.find('.alphaOnly').alphaOnly();
        this.$el.find('.phonenumber').numericOnly(11);
        this.$el.find(".addressCaption").hide();
        $.each(this.model.models, function(i, val) {
            var addressElem = that.preffix + 'Address' + i,
                name = that.preffix + "[" + i + "]." + that.name,
                addressView = new EzBob.AddressView({ model: val.get('Address'), name: name, max: 1 }),
                dateOfBirthValName = that.preffix + "[" + i + "]." + 'DateOfBirth',
                phoneName = that.preffix + "[" + i + "]." + 'Phone';

            val.get('Address').on("all", function() {
                that.trigger("director:addressChanged");
            });

            addressView.render().$el.appendTo(that.directorArea.find("#" + addressElem));
            SetDefaultDate(dateOfBirthValName, val.get("DateOfBirth"));
            that.updateStatuses(val, i);
            that.addressErrorPlacement(addressView.$el, addressView.model);

            that.validator.settings.rules[dateOfBirthValName] = { yearLimit: 18 };
            that.validator.settings.messages[dateOfBirthValName] = { yearLimit: "The number of full year should be more then 18 year" };

            that.validator.settings.rules[phoneName] = { required: true, regex: "^0[0-9]{10}$" };
            that.validator.settings.messages[phoneName] = { regex: "Please enter a valid UK number" };
        });
        this.$el.attardi_labels('toggle_all');
        this.trigger("director:change");
    },

	updateStatuses: function(val, i) {
		var dateStatus = val.get('DateOfBirthImage'),
			genderStatus = val.get('GenderImage'),
			nameStatus = val.get('NameImage'),
			middleStatus = val.get('MiddleImage'),
			surenameStasus = val.get('SurnameImage'),
			emailStatus = val.get('EmailImage'),
			phoneStatus = val.get('PhoneImage');

		$("[name='" + this.preffix + "[" + i + "].DateOfBirthImage']").val(dateStatus);
		$("[name='" + this.preffix + "[" + i + "].GenderImage']").val(genderStatus);
		$("[name='" + this.preffix + "[" + i + "].NameImage']").val(nameStatus);
		$("[name='" + this.preffix + "[" + i + "].MiddleImage']").val(middleStatus);
		$("[name='" + this.preffix + "[" + i + "].SurnameImage']").val(surenameStasus);
		$("[name='" + this.preffix + "[" + i + "].EmailImage']").val(emailStatus);
		$("[name='" + this.preffix + "[" + i + "].PhoneImage']").val(phoneStatus);

		if (dateStatus) $("[id='" + this.preffix + "[" + i + "].DateOfBirthImage']").field_status('set', dateStatus, 2);
		if (genderStatus) $("[id='" + this.preffix + "[" + i + "].GenderImage']").field_status('set', genderStatus, 2);
		if (nameStatus) $("[id='" + this.preffix + "[" + i + "].NameImage']").field_status('set', nameStatus, 2);
		if (middleStatus) $("[id='" + this.preffix + "[" + i + "].MiddleImage']").field_status('set', middleStatus, 2);
		if (surenameStasus) $("[id='" + this.preffix + "[" + i + "].SurnameImage']").field_status('set', surenameStasus, 2);
		if (emailStatus) $("[id='" + this.preffix + "[" + i + "].EmailImage']").field_status('set', emailStatus, 2);
		if (phoneStatus) $("[id='" + this.preffix + "[" + i + "].PhoneImage']").field_status('set', phoneStatus, 2);
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
        Address: new EzBob.AddressModels(),
        NameImage: "",
        MiddleImage: "",
        SurnameImage: "",
        GenderImage: "",
        DateOfBirthImage: "",
    }
});

EzBob.DirectorsModels = Backbone.Collection.extend({
    model: EzBob.DirectorModel
});
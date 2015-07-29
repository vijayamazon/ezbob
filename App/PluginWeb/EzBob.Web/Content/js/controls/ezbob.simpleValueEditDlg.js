var EzBob = EzBob || {};
EzBob.Dialogs = EzBob.Dialogs || {};

EzBob.Dialogs.SimpleValueEdit = Backbone.View.extend({
    initialize: function () {
        this.template = $($('#simpleEditDlg').html());
    },
    events: {
        "keydown" : "onEnterKeydown"
    },
    onEnterKeydown: function (event) {
        if(event.keyCode === 13 ) {
            this.onSave();
            return false;
        }
        return true;
    },
    render:function () {
    	var dialog = this.template;
	    
        dialog.dialog({
            autoOpen: false,
            title: this.options.title,
            modal: true,
            resizable: true,
            draggable: true,
            width: this.options.width ? this.options.width : 350,
            open: _.bind(this.dlgOpened, this),
            close: _.bind(this.dlgClosed, this),
            buttons: {
				Save: {
		            text: this.options.buttonName || 'Save',
		            click: _.bind(this.onSave, this)
	            }
            }
        });

    	this.setElement(dialog);
        var val = this.model.get(this.options.propertyName);
        val = this.onBeforeValuePassedToEditor(val);

        this.editField = this.createEditField();
        this.setEditValue(val);
        dialog.append(this.editField);
        
        dialog.dialog('open');
    },

    dlgOpened: function (event, ui) {
        this.$el.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn btn-primary');
        this.onDialogOpened(event, ui, this.options.required);
    },
    dlgClosed: function () {
        this.$el.empty();
    },
    onDialogOpened: function(event, ui, required){
    },
    onSave: function () {
        var f = this.$el,
            that = this;
        
        if (!f.valid())
            return;

        var val = this.getEditValue(),
            data = this.options.data;

        data[this.options.postValueName] = val;

        BlockUi("on");
        $.post(window.gRootPath + this.options.url, data, function (res) {
            if (res.error) {
                console.log(res.error);
                return;
            }
            
            if (!isNaN(res.setupFee)) {
                that.model.set("SetupFee", res.setupFee);
            }
            
            val = that.onBeforeUpdateModel(val);
            that.model.set(that.options.propertyName, val);
            that.$el.dialog('close');
            that.$el.empty();
        }).done(function() {
            that.trigger('done');
        }).always(function () {
            BlockUi("off");
        });
    },
    onBeforeUpdateModel: function (val) {
        return val;
    },
    onBeforeValuePassedToEditor: function (val) {
        return val;
    },
    createEditField: function () {
        return $('<input name="simpleValueEdit" id="simpleValueEdit" type="text" class="form-control"/>');
    },
    getEditValue: function () {
        return this.editField.val();
    },
    setEditValue: function (val) {
        this.editField.val(val);
    }
});

EzBob.Dialogs.DateTimeEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function(event, ui){
        var d = $(event.target);
        d.find("[name='simpleValueEdit']").mask("99/99/9999 99:99");
        d.validator = d.validate({
            rules: {
                simpleValueEdit: {
                    dateCheck: true,
                    required: true
                }
            }
        });
    }
});

EzBob.Dialogs.DateEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function(event, ui){
        var d = $(event.target);
        d.find("[name='simpleValueEdit']").datepicker({ autoclose: true, format: 'dd/mm/yyyy' }).datepicker('show').mask("99/99/9999");
        d.validator = d.validate({
            rules: {
                simpleValueEdit: {
                    dateCheck: true,
                    required: true
                }
            }
        });
    }
});

EzBob.Dialogs.IntegerEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function (event, ui, required) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: {
                simpleValueEdit: {
                    required: required,
                    digits: true,
                    //min: 1
                }
            }
        });
    }
});

EzBob.Dialogs.AutonumericEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function (event, ui, required) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: this.rules || {
                simpleValueEdit: {
                    required: required
                }
            }
        });
    },
    createEditField: function () {
        var el = EzBob.Dialogs.SimpleValueEdit.prototype.createEditField.call(this);
        el.autoNumeric(this.autonumericFormat);
        return el;
    },
    setEditValue: function (val) {
        this.editField.autoNumericSet(EzBob.roundNumber(val, 2));
    },
    getEditValue: function () {
        return this.editField.autoNumericGet();
    }
});

EzBob.Dialogs.PercentsEdit = EzBob.Dialogs.AutonumericEdit.extend({
    autonumericFormat: EzBob.percentFormat,
    onBeforeUpdateModel: function (val) {
        return val/100;
    },
    onBeforeValuePassedToEditor: function (val) {
        return val*100;
    },
    onDialogOpened: function (event, ui, required) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: {
                simpleValueEdit: {
                    required: required,
                }
            }
        });
    }
});

EzBob.Dialogs.PoundsNoDecimalsEdit = EzBob.Dialogs.AutonumericEdit.extend({
    autonumericFormat: EzBob.moneyFormatNoDecimals
});

EzBob.Dialogs.PacentManual = EzBob.Dialogs.PoundsNoDecimalsEdit.extend({
    rules: { simpleValueEdit: { required: true } },
    initialize: function (options) {
        this.rules.simpleValueEdit.autonumericMin = options.min;
        this.rules.simpleValueEdit.autonumericMax = options.max;
        EzBob.Dialogs.PoundsNoDecimalsEdit.prototype.initialize.call(this, arguments);
    }
});

EzBob.Dialogs.TextEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function (event, ui) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: {}
        });
    },
    createEditField: function () {
        return $('<textarea id="simpleValueEdit" name="simpleValueEdit" style="height: 120px" maxlength="1000"></textarea>');
    }
});

EzBob.Dialogs.CheckBoxEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function (event, ui) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: {}
        });
    },
    createEditField: function () {
        return $('<label class="checkbox"><input id="simpleValueEdit" name="simpleValueEdit" type="checkbox" value="">' + this.options.checkboxName + '</label>');
    },
    getEditValue: function () {
        return this.editField.find(':checkbox').is(':checked');
    },
    setEditValue: function (val) {
        this.editField.find(':checkbox').prop("checked", val);
    }
});

EzBob.Dialogs.ComboEdit = EzBob.Dialogs.SimpleValueEdit.extend({
    onDialogOpened: function (event, ui) {
        var d = $(event.target);
        d.validator = d.validate({
            rules: {}
        });
    },
    createEditField: function () {
        var template = _.template($('#simple-value-editor-cb-template').html());
        return $(template({ options: this.options.comboValues }));
    }
});

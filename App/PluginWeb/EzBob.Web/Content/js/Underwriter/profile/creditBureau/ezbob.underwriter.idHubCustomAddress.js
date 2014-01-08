var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.IdHubCustomAddressModel = Backbone.Model.extend({
    idAttribute: 'Id',
    url: function () {
        return window.gRootPath + "Underwriter/CreditBureau/IdHubCustomAddress/" + this.id;
    }
});

EzBob.Underwriter.IdHubCustomAddressView = Backbone.View.extend({
    initialize: function (options) {
        this.template = _.template($('#idhub-customaddress-template').html());
        this.content = $('<div/>');
        this.model.on('change init', this.rerender, this);
        this.dialogTitle = options.dialogTitle;
        this.checkType = options.checkType;
    },
    rerender: function () {
        this.content.html(this.template({ customAddress: this.model.toJSON() }));
        this.content.dialog('option', 'disabled', false);
    },
    render: function () {
        var that = this;
        this.content.dialog({
            modal: true,
            resizable: true,
            title: that.dialogTitle,
            position: "right",
            draggable: true,
            width: "73%",
            height: "640",
            dialogClass: "idHubCustomAddress",
            open: _.bind(this.dlgOpened, this),
            close: function () {
                $(this).remove();
                that.model.off();
            },
            disabled: true,
            buttons: {
                'Run Check': _.bind(this.onRunCheck, this),
                'Cancel': _.bind(this.onCancel, this)
            }
        });

        this.tbHouseNumber = this.content.find("#idhub-addr-housenumber");
        this.tbHouseName = this.content.find("#idhub-addr-housename");
        this.tbStreet = this.content.find("#idhub-addr-street");
        this.tbDistrict = this.content.find("#idhub-addr-district");
        this.tbTown = this.content.find("#idhub-addr-town");
        this.tbCounty = this.content.find("#idhub-addr-county");
        this.tbPostcode = this.content.find("#idhub-addr-postcode");

        this.tbBankAccount = this.content.find("#idhub-bankaccount");
        this.tbSortCode = this.content.find("#idhub-sortcode");

        if (this.checkType == 1) {
            this.tbBankAccount.attr('readonly', 'readonly');
            this.tbSortCode.attr('readonly', 'readonly');
        }

	    if (this.model.get('CurAddressIsManual'))
		    this.content.find('.current-address').addClass('manually-entered-address').attr('title', 'Customer has entered this address manually.');

	    if (this.model.get('PrevAddressIsManual'))
		    this.content.find('.previous-address').addClass('manually-entered-address').attr('title', 'Customer has entered this address manually.');

        return this;
    },
    dlgOpened: function (event, ui) {
        this.content.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn btn-primary');
    },
    onRunCheck: function () {


        $.post(window.gRootPath + "Underwriter/CreditBureau/RunAMLBWACheck",
            {
                id: this.model.get("Id"),
                checkType: this.checkType,
                houseNumber: this.tbHouseNumber.val(),
                houseName: this.tbHouseName.val(),
                street: this.tbStreet.val(),
                district: this.tbDistrict.val(),
                town: this.tbTown.val(),
                county: this.tbCounty.val(),
                postcode: this.tbPostcode.val(),
                bankAccount: this.tbBankAccount.val(),
                sortCode: this.tbSortCode.val()
            }).
            done(function (response) {
                EzBob.ShowMessage(response.Message, "Information");
            }).
            fail(function (data) {
                console.error(data.responseText);
            });

        this.content.dialog('close');
        return false;
    },
    onCancel: function () {
        this.content.dialog('close');
        return false;
    }
});
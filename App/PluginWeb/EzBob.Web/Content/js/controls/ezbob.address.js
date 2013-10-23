var EzBob = EzBob || {};

EzBob.Popup = Backbone.View.extend({
	initialize: function (options) {
		this.template = $('#address-popup-template').html();
		this.defaultPostcode = options.postcode;
		if (window.gRootPath == undefined) {
			this.rootPath = "/";
			console.warn("window.gRootPath is not initialized!");
		} else {
			this.rootPath = window.gRootPath;
		}

	},

	render: function () {
		this.$el.html(this.template);

		this.$el.dialog({
			autoOpen: true,
			title: "Select Address",
			modal: true,
			resizable: false,
			width: 390
		});

		this.textArea = this.$el.find('.postCodeTextArea');

		this.$el.find('.postCode').val(this.defaultPostcode);

		return this;
	},

	events: {
		"click input.postCodeBtnOk": "PostCodeBtnOk",
		"click input.postCodeBtnCancel": "PostCodeBtnCancel",
		"change .postCode": "SearchByPostcode",
		'click .postCodeTextArea': 'AddressesListClicked',
		'dblclick .postCodeTextArea': 'AddressesListDoubleClicked'
	},

	AddressesListClicked: function () {
		$(".postCodeBtnOk").removeAttr("disabled");
	},
	AddressesListDoubleClicked: function () {
		this.AddressesListClicked();
		this.PostCodeBtnOk();
	},
	PostCodeBtnOk: function () {
		var id = this.textArea.attr("data");
		if (!id || id == 0) {
			this.textArea.css("border", "1px solid red");
			return;
		}

		var addressModel = null;

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			addressModel = $.parseJSON($('.by_id', oDummyResults.first()).html());
			addressModel.Line3 = 'Found by postcode ' + this.$el.find(".postCode").val();
		} else {
			addressModel = new EzBob.AddressModel({ id: id });
			addressModel.fetch();
		} // if dummy

		this.model.add(addressModel);

		this.$el.dialog("close");
		this.remove();
		this.unbind();
	},

	PostCodeBtnCancel: function () {
		this.$el.dialog("close");
	},
	SearchByPostcode: function () {
		var postCode = this.$el.find(".postCode").val(),
			that = this;

		this.textArea.html("");
		this.$el.find('.postCodeBtn').attr("disabled", "disabled");

		var oDoAlways = function () {
			that.textArea.trigger("liszt:updated");
			that.$el.find('.postCodeBtn').removeAttr("disabled");

			//fix for chosen select and JQuery dialog 
			$('.ui-dialog-content').css("overflow", "visible");
			$('.ui-dialog ').css("overflow", "visible");
		};

		var oOnSuccess = function (oRecords) {
			$.each(oRecords, function (i, val) {
				that.textArea.append($('<li></li>').attr("data", val.Id).html(val.L));
			});

			that.textArea.beautifullList();

			that.textArea.removeAttr("disabled");
		};

		var oDummyResults = $('.dummy_address_search_result');

		if (oDummyResults.length > 0) {
			var oRecords = $.parseJSON($('.by_postcode', oDummyResults.first()).html());
			oOnSuccess(oRecords);
			oDoAlways();
			return;
		} // if dummy

		var request = $.getJSON(this.rootPath + "Postcode/GetAddressFromPostCode", { postCode: postCode });

		request.done(function (data) {
			if (data.Success != undefined && (!data.Success || data.Recordcount == 0)) {
				that.textArea.append($('<li></li>').val(0).html("Not found"));
				return;
			}

			oOnSuccess(data.Records);
		});

		request.fail(function () {
			that.textArea.append($('<li></li>').val(0).html("Not found"));

			that.textArea.attr("disabled", "disabled");
			that.$el.find('.postCodeBtnOk').attr("disabled", "disabled");
		});

		request.always(oDoAlways);
	}
});

EzBob.AddressView = Backbone.View.extend({
	initialize: function (options) {
		this.template = _.template($('#address-template').html());
		this.model.on("all", this.render, this);
		this.name = options.name;
		this.max = options.max || 5;
		this.isShowClear = options.isShowClear;
		this.directorId = options.directorId || 0;
		this.customerId = options.customerId || 0;
	},

	render: function () {
		var self = this;

		this.$el.html(this.template({ addresses: this.model.toJSON(), name: this.name }));
		this.$el.find('.btn').toggle(this.max > this.model.length);
		this.$el.find('.addAddressContainer').toggle(this.max > this.model.length);
		this.postcodeInput = this.$el.find(".addAddressInput");
		this.showClear(this.isShowClear);

		var sInitialStatus = (this.model && this.model.length) ? 'ok' : '';

		this.$el.find('img.field_status').each(function () {
			var bRequired = $(this).hasClass('required');
			$(this).field_status({ required: bRequired, initial_status: sInitialStatus });
		});

		_.each(this.model.models, function (val) {
			val.set(
				{
					"director": self.directorId,
					"customer": self.customerId
				},
				{
					silent: true
				}
			);
		});

		return this;
	},

	events: {
		"click .removeAddress": "removeAddress",
		"click .addAddress": "addAddress",
		"keyup .addAddressInput": "addAddressInputChanged",
		"change .addAddressInput": "addAddressInputChanged"
	},

	showClear: function (isShow) {
		if (isShow || isShow == undefined) {
			this.$el.find('.removeAddress').show();
		} else {
			this.$el.find('.removeAddress').hide();
		}
	},

	removeAddress: function (e) {
		var i = e.currentTarget.getAttribute("element-number");
		this.model.remove(this.model.at(i));
		return false;
	},
	addAddressInputChanged: function () {
		if (this.postcodeInput.val().length > 0)
			this.$el.find(".addAddress").removeAttr("disabled");
		else
			this.$el.find(".addAddress").attr("disabled", 'disabled');
	},

	addAddress: function () {
		var popUp = new EzBob.Popup({ model: this.model, postcode: $.trim(this.postcodeInput.val()) });
		popUp.render();
		popUp.SearchByPostcode();
	}
});
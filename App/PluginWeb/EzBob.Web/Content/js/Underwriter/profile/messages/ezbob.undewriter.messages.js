var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.MessageModel = Backbone.Model.extend({
	idAttribute: "Id",
	urlRoot: window.gRootPath + "Underwriter/Messages/Index"
});

EzBob.Underwriter.Message = Backbone.View.extend({
	initialize: function () {
		this.model.on("change sync", this.render, this);
		this.template = _.template($('#message-template').html());
	},

	render: function () {
		// this.$el.html(this.template({ model: this.model.toJSON() }));
		this.$el.html(this.template());
		this.customerId = this.model.get("Id");

		var template = this.$el.find('.body-row-template'); 

		var target = this.$el.find('.messages-list').empty();

		var messagesList = this.model.toJSON();

		for (var i in messagesList) {
			if (!messagesList.hasOwnProperty(i))
				continue;

			var msg = messagesList[i];

			if (typeof (msg) === 'function')
				continue;

			if (!msg.Id)
				continue;

			var row = template.clone();

			target.append(row);

			row.removeClass('.body-row-template');

			row.find('.' + (msg.IsOwn ? 'own' : 'not-own')).removeClass('hide');

			this.setHref(row, '.open', msg.Id, msg.FileName);

			this.setHref(row, '.download', msg.Id);

			row.find('.creation-date').text(msg.CreationDate);
		} // for

		this.$el.find('.body-template').remove();
	}, // render

	setHref: function(row, className, id, text) {
		var elm = row.find(className);

		var attr = elm.attr('href');

		elm.attr('href', attr + id);

		if (text)
			elm.text(text);
	}, // setHref

	events: {
		"click #btnAMLInformation": "btnAMLInformationClicked",
		"click #btnAMLandBWAInformation": "btnAMLandBWAClicked",
		"click #btnBWAInformation": "btnBWAInformationClicked"
	},

	btnAMLInformationClicked: function () {
		var that = this;
		EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
            	$.post(window.gRootPath + "Underwriter/Messages/MoreAMLInformation",
                {
                	id: that.customerId
                }).done(function () {
                	EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                	that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                	EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                	BlockUi("off");
                });
            	BlockUi("on");
            	return true;
            },
            "Yes", null, "No");
	},

	btnAMLandBWAClicked: function () {
		var that = this;
		EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
            	$.post(window.gRootPath + "Underwriter/Messages/MoreAMLandBWAInformation",
                {
                	id: that.customerId
                }).done(function () {
                	EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                	that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                	EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                	BlockUi("off");
                });
            	BlockUi("on");
            	return true;
            },
            "Yes", null, "No");
	},

	btnBWAInformationClicked: function () {
		var that = this;
		EzBob.ShowMessage(
            "",
            "Are you sure?",
            function () {
            	$.post(window.gRootPath + "Underwriter/Messages/MoreBWAInformation",
                {
                	id: that.customerId
                }).done(function () {
                	EzBob.ShowMessage("Message has been sent to user email address", "Application incomplete - More information needed", null, "OK");
                	that.trigger("creditResultChanged");
                })
                .fail(function (data) {
                	EzBob.ShowMessage(data, "The marketplace recheck error. ", null, "OK");

                })
                .complete(function () {
                	BlockUi("off");
                });
            	BlockUi("on");
            	return true;
            },
            "Yes", null, "No");
	}

});
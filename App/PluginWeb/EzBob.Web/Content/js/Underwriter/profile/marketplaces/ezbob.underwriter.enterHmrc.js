var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

(function() {
	var oMemberDef = {
		initialize: function(opts) {
			this.CustomerID = opts.CustomerID;
		}, // initialize

		events: {
			'click .add-one-period': 'addOnePeriod',
			'click .remove-one-period': 'removeOnePeriod',
		}, // events

		render: function() {
			this.$el.append($('#hmrc-enter-template').html());

			this.$el.find('.RegNo').numericOnly(15);

			return this;
		}, // render

		addOnePeriod: function() {
			this.setSomethingEnabled(this.$el.find('.add-one-period'), false);

			var sPeriodID = EzBob.guid();

			var oDateFrom = this.createDateField(sPeriodID, 'DateFrom', 'From');

			this.$el.find('.period-holder').append(
				this.createCell(sPeriodID)
					.append(oDateFrom)
					.append(this.createDateField(sPeriodID, 'DateTo', 'to'))
			);

			var self = this;

			this.$el.find('.box-holder').each(function() { self.createBoxField($(this), sPeriodID); });

			this.$el.find('.remove-holder').append(this.createCell(sPeriodID));

			this.tabReindex();

			oDateFrom.focus();

			this.setSomethingEnabled(this.$el.find('.add-one-period'), true);

			return this;
		}, // addOnePeriod

		removeOnePeriod: function(event) {
			console.log('remove one', event.currentTarget);

			var sPeriodID = $(event.currentTarget).closest('td').attr('data-period-id');

			this.$el.find('.td' + sPeriodID).remove();
			this.tabReindex();
		}, // removeOnePeriod

		tabReindex: function() {
			var aryPeriods = [];
			var aryBoxes = [];

			this.$el.find('.period-holder TD').each(function() { aryPeriods.push($(this).attr('data-period-id')); });

			this.$el.find('.box-holder').each(function() { aryBoxes.push($(this).attr('data-box-num')); });

			if (aryPeriods.length < 2)
				this.$el.find('.remove-holder').hide();
			else {
				this.$el.find('.remove-holder').show().find('TD').each(function() {
					$(this).empty().append($('<button type=button title="Remove this period" class=remove-one-period>Remove</button>'));
				});
			} // if

			var nTabIndex = parseInt(this.$el.find('.BusinessAddress').attr('tabIndex'), 10);

			for (var i = 0; i < aryPeriods.length; i++) {
				var sPeriodID = aryPeriods[i];

				this.$el.find('.' + this.dateId('DateFrom', sPeriodID)).attr('tabIndex', ++nTabIndex);
				this.$el.find('.' + this.dateId('DateTo', sPeriodID)).attr('tabIndex', ++nTabIndex);

				for (var j = 0; j < aryBoxes.length; j++)
					this.$el.find('.' + this.boxId(aryBoxes[j], sPeriodID)).attr('tabIndex', ++nTabIndex);
			} // for

			console.log('periods', aryPeriods, 'boxes', aryBoxes);
		}, // tabReindex

		createCell: function(sPeriodID) {
			var oTD = $('<td />')
				.addClass('td' + sPeriodID)
				.attr('data-period-id', sPeriodID);

			return oTD;
		}, // createCell

		createBoxField: function(oTR, sPeriodID) {
			var oTD = this.createCell(sPeriodID);

			var sBoxNum = oTR.attr('data-box-num');

			var oFld = $('<input type=text />')
				.addClass('data' + sPeriodID + ' ' + this.boxId(sBoxNum, sPeriodID))
				.attr({
					'data-period-id': sPeriodID,
					'data-field-name': 'box' + sBoxNum,
				})
				.moneyFormat();

			oTR.append(oTD.append(oFld));
		}, // createBoxField

		boxId: function(sBoxNum, sPeriodID) { return 'box-' + sBoxNum + '-' + sPeriodID; }, // boxId

		dateId: function(sFieldName, sPeriodID) { return 'date-' + sFieldName + '-' + sPeriodID; }, // dateId

		createDateField: function(sPeriodID, sFieldName, sCaption) {
			var oContainer = $('<div class=date-holder>' + sCaption + ': </div>');

			var oFld = $('<input type=date />')
				.addClass('data' + sPeriodID + ' ' + this.dateId(sFieldName, sPeriodID))
				.attr({
					'data-period-id': sPeriodID,
					'data-field-name': sFieldName,
				});

			return oContainer.append(oFld);
		}, // createDateField
	}; // member def

	var oStaticDef = {
		execute: function(nCustomerID) {
			console.log('enter hmrc view for customer', nCustomerID);

			var oView = new EzBob.Underwriter.EnterHmrcView({
				CustomerID: nCustomerID,
				el: $('<div title="Enter VAT manually"></div>'),
			});

			oView.render().addOnePeriod();

			oView.$el.dialog({
				width: Math.min($(window).width() - 10, 1300),
				height: Math.min($(window).height() - 10, 800),
				modal: true,
				buttons: {
					'Cancel': function() {
						$(this).dialog('close');
					}, // cancel

					'Save': function() {
						console.log('Save');

						$(this).dialog('close');
					}, // Save
				}, // buttons
			});
		}, // execute
	}; // static def

	EzBob.Underwriter.EnterHmrcView = EzBob.View.extend(oMemberDef, oStaticDef);
})();

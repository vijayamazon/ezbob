var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

(function() {
	var oMemberDef = {
		initialize: function(opts) {
			this.CustomerID = opts.CustomerID;
			this.Model = opts.Model;
		}, // initialize

		events: {
			'click .add-one-period': 'addOnePeriod',
			'click .remove-one-period': 'removeOnePeriod',
			'click .remove-all-periods': 'removeAllPeriods',
			'click .fill-test-data': 'fillTestData',
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

			oDateFrom.find('input').focus();

			this.setSomethingEnabled(this.$el.find('.add-one-period'), true);

			return this;
		}, // addOnePeriod

		removeOnePeriod: function(event) {
			var sPeriodID = $(event.currentTarget).closest('td').attr('data-period-id');

			this.$el.find('.td' + sPeriodID).remove();

			this.tabReindex();

			return this;
		}, // removeOnePeriod

		removeAllPeriods: function() {
			var oIds = this.getAllIds();

			var self = this;

			_.each(oIds.periods, function(sPeriodID) {
				self.$el.find('.td' + sPeriodID).remove();
			});

			this.addOnePeriod();

			return oIds;
		}, // removeAllPeriods

		tabReindex: function() {
			var oIds = this.getAllIds();

			if (oIds.periods.length < 2)
				this.$el.find('.remove-holder').hide();
			else {
				this.$el.find('.remove-holder').show().find('TD').each(function() {
					$(this).empty().append($('<button type=button title="Remove this period" class=remove-one-period>Remove</button>'));
				});
			} // if

			var nTabIndex = parseInt(this.$el.find('.BusinessAddress').attr('tabIndex'), 10);

			for (var i = 0; i < oIds.periods.length; i++) {
				var sPeriodID = oIds.periods[i];

				this.$el.find('.' + this.dateId('DateFrom', sPeriodID)).attr('tabIndex', ++nTabIndex);
				this.$el.find('.' + this.dateId('DateTo', sPeriodID)).attr('tabIndex', ++nTabIndex);

				for (var j = 0; j < oIds.boxes.length; j++)
					this.$el.find('.' + this.boxId(oIds.boxes[j], sPeriodID)).attr('tabIndex', ++nTabIndex);
			} // for

			var oScrollPane = this.$el.find('.hmrc-enter-data-holder');
			var nVisibleWidth = oScrollPane.width();
			var nFullWidth = oScrollPane[0].scrollWidth;

			oScrollPane.scrollLeft((nFullWidth > nVisibleWidth) ? (nFullWidth - nVisibleWidth) : 0);

			return oIds;
		}, // tabReindex

		getAllIds: function() {
			var oResult = { periods: [], boxes: [], };

			this.$el.find('.period-holder TD').each(function() { oResult.periods.push($(this).attr('data-period-id')); });

			this.$el.find('.box-holder').each(function() { oResult.boxes.push($(this).attr('data-box-num')); });

			return oResult;
		}, // getAllIds

		createCell: function(sPeriodID) {
			var oTD = $('<td />')
				.addClass('td' + sPeriodID)
				.attr('data-period-id', sPeriodID);

			return oTD;
		}, // createCell

		createBoxField: function(oTR, sPeriodID) {
			var oTD = this.createCell(sPeriodID);

			var sBoxNum = oTR.attr('data-box-num');

			var oFld = $($('#hmrc-enter-one-value-template').html());

			var sBoxID = this.boxId(sBoxNum, sPeriodID);

			oFld.find('.value')
				.addClass('validatable data' + sPeriodID + ' ' + sBoxID)
				.attr({
					'data-period-id': sPeriodID,
					'data-field-name': 'box' + sBoxNum,
					'ui-event-control-id': 'enter-hmrc:one-value',
					'id': sBoxID,
				})
				.moneyFormat();

			oFld.find('.value-label').text('Box ' + sBoxNum);

			oTR.append(oTD.append(oFld));
		}, // createBoxField

		boxId: function(sBoxNum, sPeriodID) { return 'box-' + sBoxNum + '-' + sPeriodID; }, // boxId

		dateId: function(sFieldName, sPeriodID) { return 'date-' + sFieldName + '-' + sPeriodID; }, // dateId

		createDateField: function(sPeriodID, sFieldName, sCaption) {
			var oContainer = $('<div class=date-holder>' + sCaption + ': </div>');

			var oFld = $('<input type=date />')
				.addClass('form-control validatable date data' + sPeriodID + ' ' + this.dateId(sFieldName, sPeriodID))
				.attr({
					'data-period-id': sPeriodID,
					'data-field-name': sFieldName,
				});

			return oContainer.append(oFld);
		}, // createDateField

		validate: function() {
			this.$el.find('.validation-result').removeClass('good bad').empty();

			this.$el.find('.validatable').filter('.invalid').removeClass('invalid');

			var aryErrors = [];

			var nErrorCount = 0;

			this.$el.find('.RegNo').each(function() {
				var oCtrl = $(this);

				if (oCtrl.val() === '') {
					aryErrors.push('Business registration number not specified.');
					oCtrl.addClass('invalid');
				} // if
			});

			this.$el.find('.BusinessName, .BusinessAddress').each(function() {
				var oCtrl = $(this);

				if ($.trim(oCtrl.val()) === '') {
					aryErrors.push('Business ' + (this.tagName.toLowerCase() === 'input' ? 'name' : 'address') + ' not specified.');
					oCtrl.addClass('invalid');
				} // if
			});

			this.$el.find('.date').each(function() {
				var oDateCtrl = $(this);

				if (oDateCtrl.val() === '') {
					oDateCtrl.addClass('invalid');
					nErrorCount++;
				} // if
			});

			if (nErrorCount > 0)
				aryErrors.push('Some date fields are not filled.');

			nErrorCount = 0;

			this.$el.find('.value').each(function() {
				var oCtrl = $(this);

				if (oCtrl.val() === '')
					oCtrl.autoNumeric('set', 0).blur();
			});

			if (nErrorCount > 0)
				aryErrors.push('Some box fields are not filled.');

			if (aryErrors.length < 1) {
				var oSet = new EzBob.DateIntervalSet();

				var oIds = this.getAllIds();

				for (var i = 0; i < oIds.periods.length; i++) {
					var sPeriodID = oIds.periods[i];

					var sFrom = this.$el.find('.' + this.dateId('DateFrom', sPeriodID)).val();
					var sTo = this.$el.find('.' + this.dateId('DateTo', sPeriodID)).val();

					if (!oSet.add(sFrom, sTo)) {
						aryErrors.push('Inconsistent date intervals detected.');
						break;
					} // if
				} // for

				if (!oSet.isConsequentNoCheck())
					aryErrors.push('In-consequent date intervals detected.');
			} // if

			if (aryErrors.length > 0)
				this.$el.find('.validation-result').addClass('bad').text(aryErrors.join(' '));
			else
				this.$el.find('.validation-result').addClass('good').text('Valid.');

			return aryErrors.length === 0;
		}, // validate

		fillTestData: function(event) {
			if (!event.ctrlKey)
				return;

			this.$el.find('.RegNo').val('112233445566').blur();

			this.$el.find('.BusinessName').val('The Horns And The Hooves Inc').blur();

			this.$el.find('.BusinessAddress').val('Horny Hooves House\n13 The Holy Cow Blvd\nJust around the corner\nLondon\nXY76 85E').blur();

			this.removeAllPeriods();

			this.addOnePeriod().addOnePeriod().addOnePeriod();

			var oIds = this.getAllIds();

			var oCurDate = moment(moment.utc().add('years', -2)).date(1);

			for (var i = 0; i < oIds.periods.length; i++) {
				var sPeriodID = oIds.periods[i];

				this.$el.find('.' + this.dateId('DateFrom', sPeriodID)).val(oCurDate.format('YYYY-MM-DD')).blur();
				oCurDate = oCurDate.add('months', 3);
				oCurDate = oCurDate.add('days', -1);

				this.$el.find('.' + this.dateId('DateTo', sPeriodID)).val(oCurDate.format('YYYY-MM-DD')).blur();
				oCurDate = oCurDate.add('days', 1);

				for (var j = 0; j < oIds.boxes.length; j++)
					this.$el.find('.' + this.boxId(oIds.boxes[j], sPeriodID)).autoNumeric('set', 100 * i + 10 * j + 10).blur();
			} // for
		}, // fillTestData

		show: function() {
			var self = this;

			this.$el.dialog({
				width: Math.min($(window).width() - 10, 1300),
				height: Math.min($(window).height() - 10, 900),
				modal: true,
				buttons: [
					{
						text: 'Cancel',
						'class': 'hmrc-enter-data-btn',
						click: function() { self.close(); },
					}, // cancel
					{
						text: 'Validate',
						'class': 'hmrc-enter-data-btn',
						click: function() { self.validate(); },
					}, // Validate
					{
						text: 'Save',
						'class': 'hmrc-enter-data-btn',
						click: function() { self.save(); },
					}, // Save
				], // buttons
			});
		}, // show

		close: function() {
			this.$el.dialog('close');
		}, // close

		save: function() {
			this.disableButtons();

			if (!this.validate()) {
				this.enableButtons();
				return;
			} // if

			var oPackage = this.packData();

			console.log('package to save', oPackage);

			var self = this;

			var oXhr = $.ajax({
				url: window.gRootPath + 'Underwriter/UploadHmrc/SaveNewManuallyEntered',
				type: 'POST',
				data: { sData: JSON.stringify(oPackage) },
				dataType: 'json',
			});

			oXhr.done(function(oResponse) {
				self.enableButtons();

				if (oResponse.success) {
					self.$el.find('.RegNo').val('').blur();
					self.$el.find('.BusinessName').val('').blur();
					self.$el.find('.BusinessAddress').val('').blur();

					self.removeAllPeriods();
					self.close();

					self.Model.fetch();

					return;
				} // if success

				if (oResponse.error) {
					EzBob.ShowMessage(oResponse.error, 'Oops!');
					console.error('Failed to save VAT account.', oResponse.error);
				}
				else
					EzBob.ShowMessage('Error saving VAT data.', 'Oops!');
			}); // on success

			oXhr.fail(function() {
				EzBob.ShowMessage('Failed to save VAT data.', 'Oops!');
				self.enableButtons();
			}); // on fail
		}, // save

		packData: function() {
			var oPackage = {
				CustomerID: this.CustomerID,
				RegNo: this.$el.find('.RegNo').val(),
				BusinessName: $.trim(this.$el.find('.BusinessName').val()),
				BusinessAddress: $.trim(this.$el.find('.BusinessAddress').val()),

				BoxNames: {},

				VatPeriods: [],
			};

			this.$el.find('.box-holder').each(function() {
				var oTR = $(this);

				var sBoxNum = oTR.attr('data-box-num');
				var sBoxName = oTR.find('th').text();

				oPackage.BoxNames[sBoxNum] = sBoxName;
			});

			var oIds = this.getAllIds();

			var self = this;

			_.each(oIds.periods, function(sPeriodID) {
				var oVat = {
					FromDate: moment.utc(self.$el.find('.' + self.dateId('DateFrom', sPeriodID)).val(), 'YYYY-MM-DD').toDate(),
					ToDate: null,
					Period: '',
					DueDate: null,
					BoxData: {},
				};

				var oToDate = moment.utc(self.$el.find('.' + self.dateId('DateTo', sPeriodID)).val(), 'YYYY-MM-DD');

				oVat.ToDate = oToDate.toDate();

				oVat.Period = oToDate.format("MM YY");

				oVat.DueDate = moment(oToDate).add('month', 1).add('week', 1).toDate();

				_.each(oPackage.BoxNames, function(sNameIgnored, sBoxNum) {
					oVat.BoxData[sBoxNum] = parseFloat(self.$el.find('.' + self.boxId(sBoxNum, sPeriodID)).autoNumeric('get'));
				}); // for each box

				oPackage.VatPeriods.push(oVat);
			}); // for each period

			return oPackage;
		}, // packData

		enableButtons: function() {
			this.setSomethingEnabled(this.$el.dialog('widget').find('.hmrc-enter-data-btn'), true);
		}, // enableButtons

		disableButtons: function() {
			this.setSomethingEnabled(this.$el.dialog('widget').find('.hmrc-enter-data-btn'), false);
		}, // disableButtons
	}; // member def

	var oStaticDef = {
		execute: function(nCustomerID, oModel) {
			var oView = new EzBob.Underwriter.EnterHmrcView({
				CustomerID: nCustomerID,
				Model: oModel,
				el: $('<div title="Enter VAT manually"></div>'),
			});

			oView.render().addOnePeriod().show();
		}, // execute
	}; // static def

	EzBob.Underwriter.EnterHmrcView = EzBob.View.extend(oMemberDef, oStaticDef);
})();

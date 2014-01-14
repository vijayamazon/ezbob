var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.GridTools = {
	CGAccounts: null,

	showMedalIcon: function(cellval) {
		var text;
		text = cellval.text || cellval;
		return '<i data-toggle=tooltip title="' + text + '" class="' + (text.toLowerCase().replace(/\s/g, '')) + '"></i>';
	}, // showMedalIcon

	showMPIcon: function(cellval) {
		if (!EzBob.Underwriter.GridTools.CGAccounts)
			EzBob.Underwriter.GridTools.CGAccounts = $.parseJSON($('div#cg-account-list').text());

		var className, text;
		text = cellval || '';
		className = text.replace(/\s|\d/g, '');
		className = EzBob.Underwriter.GridTools.CGAccounts[className] ? 'cgaccount' : className.toLowerCase();
		return '<i data-toggle=tooltip title="' + text + '" class="' + className + '"></i>';
	}, // showMPIcon

	showMPsIcon: function(cellval) {
		var mps, retVal;

		mps = cellval || '';
		mps = mps.split(',').clean('');
		retVal = '';

		_.each(mps, function(val) {
			return retVal += EzBob.Underwriter.GridTools.showMPIcon(val);
		});

		return '<div style="overflow: auto; width: 102%;">' + (retVal + ' ') + '</div>';
	}, // showMPsIcon

	withScrollbar: function(sContent) {
		return '<div style="overflow: auto; width: auto;">' + sContent + '</div>';
	}, // withScrollbar

	profileLink: function(nCustomerID, sLinkText) {
		return EzBob.Underwriter.GridTools.withScrollbar('<a class=profileLink title="Open customer profile" href="#profile/' + nCustomerID + '">' + (sLinkText || nCustomerID) + '</a>');
	}, // profileLink

	profileWithTypeLink: function(nCustomerID, sGridType) {
		return EzBob.Underwriter.GridTools.withScrollbar('<a class=profileLink href="#profile/' + nCustomerID + '/' + sGridType + '">' + nCustomerID + '</a>');
	}, // profileWithTypeLink
}; // EzBob.Underwriter.GridTools

EzBob.Underwriter.GridsView = Backbone.View.extend({
	initialize: function(options) {
		this.router = null;

		this.loadLogbookEntryTypeList();

		function GridProperties(opts) {
			_.extend(this, opts);
		} // GridProperties

		GridProperties.prototype.AddToTitle = function() { return null; };

		GridProperties.prototype.OnAfterRender = function() { };

		GridProperties.prototype.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
			if (oData.hasOwnProperty('Cart'))
				$('.grid-item-Cart', oTR).empty().html(EzBob.Underwriter.GridTools.showMedalIcon(oData.Cart));

			if (oData.hasOwnProperty('MP_List'))
				$('.grid-item-MP_List', oTR).empty().html(EzBob.Underwriter.GridTools.showMPsIcon(oData.MP_List));

			if (oData.hasOwnProperty('Id')) {
				$('.grid-item-Id', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id));

				if (oData.hasOwnProperty('Name')) {
					if (oData.Name)
						$('.grid-item-Name', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id, oData.Name));
				} // if has name

				$(oTR).dblclick(function() {
					location.assign($(oTR).find('.profileLink').first().attr('href'));
				});
			} // if has id

			if (oData.IsWasLate) {
				//$(oTR).addClass(oData.IsWasLate);
				$(oTR).addClass("table-flag-red");
			} // if
		}; // fnRowCallback

		var sWaitingColumns = '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,CurrentStatus,$CalcAmount,$OSBalance,SegmentType';

		function approvedLateColumns(onTime) {
			return '#Id,Cart,MP_List,Name,Email,^ApplyDate,^ApproveDate,^RegDate,$CalcAmount,$ApprovedSum,$AmountTaken' +
				(onTime ? ',^OfferExpireDate' : '') +
				',#ApprovesNum,#RejectsNum,SegmentType' +
				(!onTime ? ',$OSBalance,^LatePaymentDate,$LatePaymentAmount,#Delinquency,CRMstatus,CRMcomment' : '');
		} // approvedLateColumns

		this.gridProperties = {
			waiting: new GridProperties({
				icon: 'envelope-o',
				title: 'Waiting for decision',
				action: 'GridWaiting',
				columns: sWaitingColumns,
			}), // waiting
			escalated: new GridProperties({
				icon: 'arrow-up',
				action: 'GridEscalated',
				columns: sWaitingColumns + ',^EscalationDate,Underwriter,Reason',
			}), // escalated
			pending: new GridProperties({
				icon: 'clock-o',
				action: 'GridPending',
				columns: sWaitingColumns + ',Pending',
			}), // pending
			approved: new GridProperties({
				icon: 'thumbs-o-up',
				action: 'GridApproved',
				columns: approvedLateColumns(true),
			}), // approved
			loans: new GridProperties({
				icon: 'gbp',
				action: 'GridLoans',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^ApplyDate,^FirstLoanDate,^LastLoanDate,$LastLoanAmount,$AmountTaken,$TotalPrincipalRepaid,$OSBalance,^NextRepaymentDate,CustomerStatus,SegmentType',
			}), // loans
			sales: new GridProperties({
				icon: 'phone',
				action: 'GridSales',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$ApprovedSum,$AmountTaken,^OfferDate,$OSBalance,CRMstatus,CRMcomment,#Interactions,SegmentType',
			}), // sales
			collection: new GridProperties({
				icon: 'rocket',
				action: 'GridCollection',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$AmountTaken,$OSBalance,CRMstatus,CRMcomment,CollectionStatus,SegmentType',
			}), // collection
			late: new GridProperties({
				icon: 'flag',
				action: 'GridLate',
				columns: approvedLateColumns(false),
			}), // late
			rejected: new GridProperties({
				icon: 'thumbs-o-down',
				action: 'GridRejected',
				columns: '#Id,Cart,MP_List,Name,Email,^ApplyDate,^RegDate,^DateRejected,Reason,#RejectsNum,#ApprovesNum,$OSBalance,SegmentType',
			}), // rejected
			offline: new GridProperties({
				icon: 'briefcase',
				action: 'GridOffline',
				columns: '#Id,^RegDate,Cart,MP_List,Name,Email,WizardStep'
			}), // offline
			all: new GridProperties({
				icon: 'female',
				action: 'GridAll',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^ApplyDate,CustomerStatus,$CalcAmount,$ApprovedSum,$OSBalance,SegmentType',
			}), // all
			registered: new GridProperties({
				icon: 'bars',
				action: 'GridRegistered',
				columns: '#UserId,Email,UserStatus,^RegDate,MP_Statuses,WizardStep,SegmentType',
				fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					$('.grid-item-UserId', oTR).empty().html(EzBob.Underwriter.GridTools.profileWithTypeLink(oData.UserId, 'registered'));

					$(oTR).dblclick(function() {
						location.assign($(oTR).find('.profileLink').first().attr('href'));
					});

					if (oData.IsWasLate)
						$(oTR).addClass(oData.IsWasLate);
				}, // fnRowCallback
			}), // registered
			logbook: new GridProperties({
				icon: 'file-text-o',
				action: 'GridLogbook',
				columns: '#EntryID,LogbookEntryTypeDescription,FullName,^EntryTime,EntryContent',
				AddToTitle: function() {
					return $('#reload-button-template')
						.clone()
						.text('Add new entry')
						.attr('id', 'logbook-show-new-entry-form')
						.removeClass('hide');
				}, // AddToTitle
				OnAfterRender: function(oView, sTableContainer) {
					oView.$el.find(sTableContainer + ' #logbook-show-new-entry-form')
						.toggleClass(
							'hide',
							!oView.$el.find(sTableContainer + ' #logbook-new-entry-form').hasClass('hide')
						);
				}, // OnAfterRender
			}), // logbook
		}; // gridProperties
	}, // initialize

	events: {
		'click #include-test-customers': 'reloadActive',
		'click #include-all-customers': 'reloadActive',
		'click .reload-button': 'reloadActive',
		'click #logbook-show-new-entry-form': 'showNewLogbookEntryForm',
		'click #logbook-new-entry-cancel': 'hideNewLogbookEntryForm',
		'click #logbook-new-entry-save': 'submitNewLogbookEntryForm',
	}, // events

	submitNewLogbookEntryForm: function() {
		var nEntryType = $('#logbook-new-entry-type').val();

		if (nEntryType === '') {
			$('#logbook-new-entry-type').focus();
			return;
		} // if

		var sMsg = $.trim($('#logbook-new-entry').val());

		if (sMsg === '') {
			$('#logbook-new-entry').focus();
			return;
		} // if

		BlockUi();

		var self = this;

		$.post(window.gRootPath + 'Underwriter/Customers/AddLogbookEntry', { type: nEntryType, content: sMsg }, 'json')
			.done(function(data) {
				self.handleNewLogbookSaved(data.success, data.msg);
			}) // on success
			.fail(function() {
				self.handleNewLogbookSaved(false, 'Failed to save an entry.');
			}); // on fail
	}, // submitNewLogbookEntryForm

	handleNewLogbookSaved: function(bSuccess, sMsg) {
		UnBlockUi();

		if (bSuccess) {
			EzBob.ShowMessageTimeout(sMsg || 'New entry has been logged successfully.', 'Success', 2);

			this.hideNewLogbookEntryForm();
			this.reloadActive();
		}
		else
			EzBob.ShowMessage(sMsg || 'Failed to save an entry.', 'Error');
	}, // handleNewLogbookSaved

	showNewLogbookEntryForm: function() {
		$('#logbook-new-entry-form').slideDown('slow').removeClass('hide');
		$('#logbook-show-new-entry-form').addClass('hide');
		$('#logbook-new-entry-type').focus();
	}, // showNewLogbookEntryForm

	hideNewLogbookEntryForm: function() {
		$('#logbook-new-entry-form').slideUp('slow').addClass('hide');
		$('#logbook-show-new-entry-form').removeClass('hide');

		$('#logbook-new-entry-type').val('');
		$('#logbook-new-entry').val('');
	}, // hideNewLogbookEntryForm

	loadLogbookEntryTypeList: function() {
		$.getJSON(
			window.gRootPath + 'Underwriter/Customers/LoadLogbookEntryTypeList',
			function(data) {
				var oSelect = $('#logbook-new-entry-type');

				$.each(data, function(idx, oEntryType) {
					oSelect.append(
						$('<option></option>').val(oEntryType.ID).text(oEntryType.Description)
					);
				}); // for each
			} // onsuccess
		);
	}, // loadLogbookEntryTypeList

	render: function() {
		var self = this;

		this.tabLinks().on('shown.bs.tab', function (e) { self.handleTabSwitch(e); });

		return this;
	}, // render

	handleTabSwitch: function (evt) {
		this.loadGrid(this.typeFromHref($(evt.target).attr('href')));
	}, // handleTabSwitch

	toggleAllCustomers: function(sCurTabType) {
		this.$el.find('.all-customers').toggleClass('hide', sCurTabType !== 'registered');
	}, // toggleAllCustomers

	reloadActive: function() {
		this.loadGrid(this.activeGridType());
	}, // reloadActive

	activeGridType: function() {
		return this.typeFromHref(
			this.tabLinks().closest('li').filter('.active').find('a[data-toggle]').attr('href')
		);
	}, // activeGridType

	loadGrid: function(sGridName) {
		if (this.router)
			this.router.navigate('#customers/' + sGridName);

		localStorage.setItem('uw_grids_last_shown-' + $('#uw-name-and-icon').attr('data-uw-id'), sGridName);

		this.toggleAllCustomers(sGridName);

		this.tabLinks().closest('li').filter('.active').removeClass('active');

		this.tabLinkTo(sGridName).closest('li').addClass('active');

		this.$el.find('.tab-pane').hide().filter('#' + sGridName + '-grid').show();

		var oGridProperties = this.gridProperties[sGridName];

		if (!oGridProperties)
			return;

		if (!oGridProperties.name)
			oGridProperties.name = sGridName;

		if (!oGridProperties.title)
			oGridProperties.title = sGridName.charAt(0).toUpperCase() + sGridName.slice(1);

		var sTableContainer = '#' + sGridName + '-grid';

		this.$el.find(sTableContainer + ' .grid-data').dataTable({
			bDestroy: true,
			bProcessing: true,
			sAjaxSource: this.gridSrcUrl(oGridProperties),
			aoColumns: this.extractColumns(oGridProperties),

			bDeferRender: true,

			aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
			iDisplayLength: 50,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			fnRowCallback: oGridProperties.fnRowCallback,

			aaSorting: [[ 0, 'desc' ]],

			bAutoWidth: true,
			sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"if><"dataTables_top_left">>>>tr<"bottom"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>><"clear">',

			bStateSave: true,

			fnStateSave: function (oSettings, oData) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				localStorage.setItem(sKey, JSON.stringify(oData));
			}, // fnStateSave

			fnStateLoad: function (oSettings) {
				var sKey = 'uw-grid-state-' + $('#uw-name-and-icon').attr('data-uw-id') + '-' + sGridName;
				var sData = localStorage.getItem(sKey);
				var oData = sData ? JSON.parse(sData) : null;
				return oData;
			}, // fnStateLoad
		}); // create data table

		var oTableTitle = this.$el.find(sTableContainer + ' .dataTables_top_left');

		if ($('h3', oTableTitle).length < 1) {
			this.$el.find(sTableContainer + ' .dataTables_top_right,' + sTableContainer + ' .dataTables_bottom_right').prepend(
				$('#reload-button-template').clone().attr('id', '').removeClass('hide').addClass('reload-button')
			);

			oTableTitle.append('<h3><i class="fa fa-' + oGridProperties.icon + '"></i>' + oGridProperties.title + '</h3>');

			var oAdditional = oGridProperties.AddToTitle();

			if (oAdditional)
				oTableTitle.append(oAdditional);
		} // if

		oGridProperties.OnAfterRender(this, sTableContainer);
	}, // loadGrid

	gridSrcUrl: function(oGridProperties) {
		var bIncludeTest = this.$el.find('#include-test-customers:checked').length > 0;
		var bIncludeAll = this.$el.find('#include-all-customers:visible:checked').length > 0;

		var oTabPane = this.$el.find('#' + oGridProperties.name + '-grid');

		var oShowingTest = oTabPane.find('.showing-test');

		if (bIncludeTest) {
			if (oShowingTest.length !== 1)
				oTabPane.find('h2').append('<span class=showing-test> (including test customers)</span>');
		}
		else
			oShowingTest.remove();

		return window.gRootPath + 'Underwriter/Customers/' + oGridProperties.action +
			'?includeTestCustomers=' + (bIncludeTest ? 'true' : 'false') +
			'&includeAllCustomers=' + (bIncludeAll ? 'true' : 'false');
	}, // gridSrcUrl

	extractColumns: function(oGridProperties) {
		var aryResult = [];

		var aryNames = oGridProperties.columns.split(',');

		function renderMoney(oData, sAction, oFullSource) {
			switch (sAction) {
			case 'display':
				return EzBob.formatPoundsNoDecimals(oData);

			case 'filter':
			case 'type':
			case 'sort':
			default:
				return oData;
			} // switch
		} // renderMoney

		function renderDate(oData, sAction, oFullSource) {
			switch (sAction) {
			case 'display':
				return EzBob.formatDate(oData);

			case 'filter':
				return oData + ' ' + EzBob.formatDate(oData);

			case 'type':
			case 'sort':
			default:
				return oData;
			} // switch
		} // renderDate

		for (var i = 0; i < aryNames.length; i++) {
			var sName = aryNames[i];

			if (!sName)
				continue;

			var oRenderFunc = null;
			var sClass = '';

			if (sName[0] === '#') {
				sClass = 'numeric';
				sName = sName.substr(1);
			}
			else if (sName[0] === '$'){
				sClass = 'numeric';
				sName = sName.substr(1);
				oRenderFunc = renderMoney;
			}
			else if (sName[0] === '^') {
				sName = sName.substr(1);
				oRenderFunc = renderDate;
			}

			aryResult.push({
				mData: sName,
				sClass: sClass + ' grid-item-' + sName,
				mRender: oRenderFunc,
			});
		} // for

		return aryResult;
	}, // extractColumns

	tabLinks: function () {
		return this.$el.find('a[data-toggle="tab"]');
	}, // tabLinks

	tabLinkTo: function (sTarget) {
		return this.tabLinks().filter('[href="#' + (sTarget || '') + '-grid"]');
	}, // tabLinkTo

	/// id argument is not used here. It was added to be consistent with
	/// previously created views.
	show: function (id, type) {
		this.$el.show();
		this.loadGrid(this.getValidType(type));
	}, // show

	typeFromHref: function (sHref) {
		if (!sHref)
			sHref = this.tabLinks().first().attr('href');

		var aryMatch = sHref.match(/^#(.+)-grid$/);

		if (aryMatch && aryMatch[1])
			return aryMatch[1];

		return this.typeFromHref(this.tabLinks().first().attr('href'));
	}, // typeFromHref

	getValidType: function (sType) {
		if (this.isValidType(sType))
			return sType;

		return this.typeFromHref();
	}, // getValidType

	isValidType: function(sType) {
		return this.tabLinkTo(sType).length === 1;
	}, // isValidType

	hide: function() {
		this.$el.hide();
	}, // hide
}); // EzBob.Underwriter.GridsView

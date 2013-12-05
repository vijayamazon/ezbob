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
}; // EzBob.Underwriter.GridTools

EzBob.Underwriter.GridsView = Backbone.View.extend({
	initialize: function(options) {
		this.router = null;

		function GridProperties(opts) {
			_.extend(this, opts);
		} // GridProperties

		GridProperties.prototype.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
			console.log('row call', oTR, oData, iDisplayIndex, iDisplayIndexFull);

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

			if (oData.IsWasLate)
				$(oTR).addClass(oData.IsWasLate);
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
				action: 'GridWaiting',
				columns: sWaitingColumns,
			}), // waiting
			escalated: new GridProperties({
				action: 'GridEscalated',
				columns: sWaitingColumns + ',^EscalationDate,Underwriter,Reason',
			}), // escalated
			pending: new GridProperties({
				action: 'GridPending',
				columns: sWaitingColumns + ',Pending',
			}), // pending
			approved: new GridProperties({
				action: 'GridApproved',
				columns: approvedLateColumns(true),
			}), // approved
			loans: new GridProperties({
				action: 'GridLoans',
				columns: '#Id,Cart,MP_List,Name,Email,^RegDate,^ApplyDate,^FirstLoanDate,^LastLoanDate,$LastLoanAmount,$AmountTaken,$TotalPrincipalRepaid,$OSBalance,^NextRepaymentDate,CustomerStatus,SegmentType',
			}), // loans
			sales: new GridProperties({
				action: 'GridSales',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$ApprovedSum,$AmountTaken,^OfferDate,$OSBalance,CRMstatus,CRMcomment,#Interactions,SegmentType',
			}), // sales
			collection: new GridProperties({
				action: 'GridCollection',
				columns: '#Id,Email,Name,MobilePhone,DaytimePhone,$AmountTaken,$OSBalance,CRMstatus,CRMcomment,CollectionStatus,SegmentType',
			}), // collection
			late: new GridProperties({
				action: 'GridLate',
				columns: approvedLateColumns(false),
			}), // late
		}; // gridProperties
	}, // initialize

	events: {
		'click .show-old-grids': 'showOldGrids',
		'click #include-test-customers': 'reloadActive',
		'click #include-all-customers': 'reloadActive',
	}, // events

	showOldGrids: function() {
		this.hide();
		$('#customers-view').show();
	}, // showOldGrids

	render: function() {
		var self = this;

		this.tabLinks().on('shown', function(e) { self.handleTabSwitch(e); });

		return this;
	}, // render

	handleTabSwitch: function(evt) {
		var sType = this.typeFromHref($(evt.target).attr('href'));

		this.$el.find('.all-customers').toggleClass('hide', sType !== 'registered');

		this.loadGrid(sType);
	}, // handleTabSwitch

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

		this.tabLinks().closest('li').filter('.active').removeClass('active');

		this.tabLinkTo(sGridName).closest('li').addClass('active');

		this.$el.find('.tab-pane').hide().filter('#' + sGridName + '-grid').show();

		var oGridProperties = this.gridProperties[sGridName];

		if (!oGridProperties)
			return;

		if (!oGridProperties.name)
			oGridProperties.name = sGridName;

		$.ajax(this.gridSrcUrl(oGridProperties)).success(function(data, status, jqXHR) {
			console.log('data:', data);
		});

		this.$el.find('#' + sGridName + '-grid .grid-data').dataTable({
			bDestroy: true,
			bProcessing: true,
			sAjaxSource: this.gridSrcUrl(oGridProperties),
			aoColumns: this.extractColumns(oGridProperties),

			aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
			iDisplayLength: -1,

			sPaginationType: 'full_numbers',
			bJQueryUI: true,

			fnRowCallback: oGridProperties.fnRowCallback,

			bAutoWidth: true,
		}); // create data table
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

		console.log('columns:', aryResult);
		return aryResult;
	}, // extractColumns

	tabLinks: function() {
		return this.$el.find('a[data-toggle="tab"]');
	}, // tabLinks

	tabLinkTo: function(sTarget) {
		return this.tabLinks().filter('[href="#' + (sTarget || '') + '-grid"]');
	}, // tabLinkTo

	/// id argument is not used here. It was added to be consistent with
	/// previously created views.
	show: function(id, type) {
		this.$el.show();
		this.loadGrid(this.getValidType(type));
	}, // show

	typeFromHref: function(sHref) {
		if (!sHref)
			sHref = this.tabLinks().first().attr('href');

		var aryMatch = sHref.match(/^#(.+)-grid$/);

		if (aryMatch && aryMatch[1])
			return aryMatch[1];

		return this.typeFromHref(this.tabLinks().first().attr('href'));
	}, // typeFromHref

	getValidType: function(sType) {
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

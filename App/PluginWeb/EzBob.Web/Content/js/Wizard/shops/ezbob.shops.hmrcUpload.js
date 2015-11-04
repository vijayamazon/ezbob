var EzBob = EzBob || {};
EzBob.HmrcUploadAccountInfoView = Backbone.Marionette.ItemView.extend({
	template: '#HMRCUploadAccountInfoTemplate',

	initialize: function () {
		this.uploadUi = new EzBob.HmrcUploadUi({
			chartMonths: this.options.chartMonths,
			formID: 'hmrcAccountUpload',
			uploadUrl: '/Customer/Hmrc/SaveFile',
			loadPeriodsUrl: '/Customer/Hmrc/LoadPeriods',
			isUnderwriter: false,
			uiEventControlIDs: {
				form: 'hmrc:dropzone',
			},
			customButtons: true
		});

		EzBob.App.on("showButton", this.showButton, this);
		EzBob.App.on("hideButton", this.hideButton, this);
	},

	onRender: function() {
		this.uploadUi.$el = this.$el.find('.hmrc-upload-ui');
		this.uploadUi.render();
		$('body').scrollTop(0);
	},

	events: {
		'click #uploadInfoButton': 'getUploadHelp',
		'click #hmrc_upload_back_button': 'back',
		'click #hmrc_upload_done_button': 'doUploadFiles',
		'click #uploadHelpButton': 'getUploadHelp',
	},

	back: function () {
		this.trigger('back');
		return false;
	}, // back

	doUploadFiles: function () {
		this.trigger('completed');
		this.trigger('back');
		return false;
	}, // doUploadFiles

	getUploadHelp: function () {
		var oDialog = $('#hmrcUploadHelpPopup');

		if (oDialog.length > 0) {
			$.colorbox({
				inline: true,
				open: true,
				href: oDialog,
				width: '35%',
				maxWidth: '100%',
				maxHeight: '100%',
				close: '<i class="pe-7s-close"></i>',
				onOpen: function() {
					 $('body').addClass('stop-scroll');
				},
				onClosed: function() {
					$('body').removeClass('stop-scroll');
				}
			});
		} // if
	}, // getUploadHelp

	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return 'Upload VAT reports';
	}, // getDocumentTitle

	showButton: function (name) {
		if (name === 'Done') {
			this.$el.find('#hmrc_upload_done_button').show();
		}

		if (name === 'Back') {
			this.$el.find('#hmrc_upload_back_button').show();
		}
	}, // showButton

	hideButton: function (name) {
		if (name === 'Done') {
			this.$el.find('#hmrc_upload_done_button').hide();
		}

		if (name === 'Back') {
			this.$el.find('#hmrc_upload_back_button').hide();
		}
	}, // hideButton
});//EzBob.HmrcUploadAccountInfoView


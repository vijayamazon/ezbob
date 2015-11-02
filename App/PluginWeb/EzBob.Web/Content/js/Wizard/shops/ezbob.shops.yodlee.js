var EzBob = EzBob || {};

EzBob.YodleeAccountInfoView = Backbone.Marionette.ItemView.extend({
	template: '#YodleeAccoutInfoTemplate',
	events: {
		'click a.back': 'back',
		'change input[name="Bank"]': 'bankChanged',
		'click .radio-fx': 'parentBankSelected',
		'change .SubBank': 'subBankSelectionChanged',
		'click #yodleeLinkAccountBtn': 'linkAccountClicked',
		'click #OtherYodleeBanks': 'OtherYodleeBanksClicked',
		'change #OtherYodleeBanks': 'OtherYodleeBanksClicked',
	},
	
	initialize: function (options) {
		var that = this;
		this.isProfile = options.isProfile;
		window.YodleeAccountAdded = function (result) {
			if (result.error) {
				EzBob.App.trigger('error', result.error);
			} else {
				EzBob.App.trigger('info', 'Congratulations. Bank account was added successfully.');
			}
			$.colorbox.close();
			that.trigger('completed');
			that.trigger('ready');
			return that.trigger('back');
		};
		window.YodleeAccountAddingError = function (msg) {
			$.colorbox.close();
			EzBob.App.trigger('error', msg);
			return that.trigger('back');
		};

		window.YodleeAccountRetry = function () {
			that.attemptsLeft = (that.attemptsLeft || 5) - 1;
			return {
				url: that.$el.find('#yodleeContinueBtn').attr('href'),
				attemptsLeft: that.attemptsLeft
			};
		};
	},
	
	onRender: function () {
		EzBob.UiAction.registerView(this);
		if (this.isProfile) {
			this.$el.find('.marketplace-button').addClass('marketplace-button-profile');
		}
		$('body').scrollTop(0);
		return this;
	},
	
	OtherYodleeBanksClicked: function (el) {
		var url;
		var selectedId = $(el.currentTarget).find(':selected').val();
		var selectedName = $(el.currentTarget).find(':selected').text();
		if (selectedId) {
			this.$el.find("input[type='radio'][name='Bank']:checked").removeAttr('checked');
			this.$el.find(".SubBank:not(.hide)").addClass('hide');
			this.$el.find('a.selected-bank').parent().toggleClass('marketplace-button-full marketplace-button-empty');
			this.$el.find("a.selected-bank").removeClass('selected-bank');
			url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + selectedId + "&bankName=" + selectedName;
			this.$el.find("#yodleeContinueBtn").attr("href", url);
			return this.$el.find("#yodleeLinkAccountBtn").removeClass('disabled');
		} else {
			return this.$el.find("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled');
		}
	},
	
	subBankSelectionChanged: function () {
		if (this.$el.find(".SubBank option:selected").length === 0) {
			return false;
		}
		var url = "" + window.gRootPath + "Customer/YodleeMarketPlaces/AttachYodlee?csId=" + (this.$el.find("option:selected").val()) + "&bankName=" + (this.$el.find("input[type='radio'][name='Bank']:checked").attr('value'));
		this.$el.find("#yodleeContinueBtn").attr("href", url);
		return this.$el.find("#yodleeLinkAccountBtn").removeClass('disabled');
	},
	
	bankChanged: function () {
		this.$el.find("input[type='radio']:not([name='Bank']):checked").removeAttr('checked');
		var currentSubBanks = this.$el.find(".SubBank:not([class*='hide'])");
		currentSubBanks.addClass('hide');
		this.$el.find("#subTypeHeader[class*='hide']").removeClass('hide');
		currentSubBanks.find('option').removeAttr('selected');
		var bank = this.$el.find("input[type='radio'][name='Bank']:checked").val();
		this.$el.find("." + bank + "Container").removeClass('hide');
		return $("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled');
	},
	
	linkAccountClicked: function () {
		if (this.$el.find('#yodleeLinkAccountBtn').hasClass('disabled')) {
			return false;
		}

		return $.colorbox({
			href:"#yodlee_help",
			inline: true,
			open: true,
			close: '<i class="pe-7s-close"></i>',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$('body').removeClass('stop-scroll');
			}
		});
	},
	
	parentBankSelected: function (evt) {
		evt.preventDefault();
		this.$el.find('#Bank_' + evt.currentTarget.id).click();
		this.$el.find('a.selected-bank').parent().toggleClass('marketplace-button-full marketplace-button-empty');
		this.$el.find('a.selected-bank').removeClass('selected-bank');
		
		$(evt.currentTarget).addClass('selected-bank');
		$(evt.currentTarget).parent().toggleClass('marketplace-button-full marketplace-button-empty');

		this.$el.find(".SubBank:not(.hide) option:selected").prop('selected', false);
		this.$el.find("#OtherYodleeBanks option").eq(0).prop('selected', true);
		this.$el.find("#OtherYodleeBanks").change();
	},
	
	back: function () {
		this.trigger('back');
		return false;
	},
	
	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return "Link Bank Account";
	}
});

EzBob.YodleeAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/YodleeMarketPlaces/Accounts"
});

EzBob.YodleeAccounts = Backbone.Collection.extend({
	model: EzBob.YodleeAccountModel,
	url: "" + window.gRootPath + "Customer/YodleeMarketPlaces/Accounts"
});

EzBob.YodleeUploadAccountModel = Backbone.Model.extend({
	urlRoot: "" + window.gRootPath + "Customer/YodleeMarketPlaces/UploadAccounts"
});

EzBob.YodleeUploadAccounts = Backbone.Collection.extend({
	model: EzBob.YodleeAccountModel,
	url: "" + window.gRootPath + "Customer/YodleeMarketPlaces/UploadAccounts"
});

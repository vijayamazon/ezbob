var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.StrategyAutomationView = Backbone.View.extend({
	initialize: function () {
		this.template = _.template($("#automation-detail-settings").html());
		return this;
	},
	render: function () {
		var approval, automation, rejection;
		this.$el.html(this.template());
		automation = this.$el.find("#automation-settings");
		approval = this.$el.find("#approvals-settings");
		rejection = this.$el.find("#rejections-settings");
		this.automationModel = new EzBob.Underwriter.SettingsAutomationModel();
		this.automationView = new EzBob.Underwriter.SettingsAutomationView({
			el: automation,
			model: this.automationModel
		});
		this.approvalModel = new EzBob.Underwriter.SettingsApprovalModel();
		this.approvalView = new EzBob.Underwriter.SettingsApprovalView({
			el: approval,
			model: this.approvalModel
		});
		this.rejectionModel = new EzBob.Underwriter.SettingsRejectionModel();
		this.rejectionView = new EzBob.Underwriter.SettingsRejectionView({
			el: rejection,
			model: this.rejectionModel
		});
	},
	show: function (type) {
		this.$el.show();
	},
	hide: function () {
		this.$el.hide();
	},
	onClose: function () {
		this.modelBinder.unbind();
	}
});

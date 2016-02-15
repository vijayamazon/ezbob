namespace EzBob.Web.Infrastructure {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Models.Repository;
	using Newtonsoft.Json;
	using SquishIt.Framework;
	using SquishIt.Mvc;
	using StructureMap;

	public class BundleHelper {
		public static MvcHtmlString RenderPaypointTemplateCss() {
			return Bundle.Css()
			.Add("~/Content/css/paypoint-template.css")				
			.MvcRender("~/Content/css/min/paypoint_#.css");
		} // RenderPaypointTemplateCss

        public static MvcHtmlString RenderEverlineCustomerCss() {
            return Bundle.Css()
                //libs
                .Add("~/Content/css/lib/normalize.css")
				.Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
				.Add("~/Content/css/lib/jquery.ui.1.8.16.ie.css")
                .Add("~/Content/css/lib/dropzone.css")
                .Add("~/Content/css/lib/font-awesome.min.css")
                .Add("~/Content/css/lib/notifications.css")
                .Add("~/Content/css/lib/coin-slider-styles.css")
                .Add("~/Content/css/lib/chosen.css")
				.Add("~/Content/css/lib/pe-icon-7-stroke.css")
				.Add("~/Content/css/lib/pe-icon-7-helper.css")
               
                //common
                .Add("~/Content/css/profile-ny2015scratch.css")
                .Add("~/Content/css/valentine2015scratch.css")
                .Add("~/Content/css/easter2015scratch.css")
                
				//custom css
				.Add("~/Content/css/everline.css")
				.Add("~/Content/css/Everlinecustomer.css")
                .Add("~/Content/css/Everlineslider.css")
				.Add("~/Content/css/hmrc-upload-ui.css")
                .MvcRender("~/Content/css/min/customereverline_#.css");
        }// RenderEverlineCustomerCss

		public static MvcHtmlString RenderEzbobCustomerCss() {
			return Bundle.Css()
                //libs
                .Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
                .Add("~/Content/css/lib/jquery.ui.1.8.16.ie.css")
                .Add("~/Content/css/lib/bootstrap2.css")
                .Add("~/Content/css/lib/dropzone.css")
                .Add("~/Content/css/lib/font-awesome.min.css")
                .Add("~/Content/css/lib/notifications.css")
                .Add("~/Content/css/lib/coin-slider-styles.css")
                .Add("~/Content/css/lib/chosen.css")
				.Add("~/Content/css/lib/pe-icon-7-stroke.css")
				.Add("~/Content/css/lib/pe-icon-7-helper.css")

				//custom css
				.Add("~/Content/css/common.css")
				.Add("~/Content/css/customer.css")
				.Add("~/Content/css/profile-ny2015scratch.css")
				.Add("~/Content/css/valentine2015scratch.css")
				.Add("~/Content/css/easter2015scratch.css")
				.Add("~/Content/css/ezbob.css")
				.Add("~/Content/css/hmrc-upload-ui.css")
				.MvcRender("~/Content/css/min/customerezbob_#.css");
		} // RenderEzbobCustomerCss

		public static MvcHtmlString RenderAlibabaCss() {
			return Bundle.Css()
			.Add("~/Content/css/alibaba.css")				
			.MvcRender("~/Content/css/min/alibaba_#.css");
		} // RenderAlibabaCss

		public static MvcHtmlString RenderEzbobProfileCss() {
			return Bundle.Css()
				//libs
				.Add("~/Content/css/lib/jquery.jscrollpane.css")
				
				//custom css
				.Add("~/Content/css/profile.css")
				.Add("~/Content/css/EzbobCustomerProfile.css")
				.Add("~/Content/css/edit-experian-director-data.css")
				.MvcRender("~/Content/css/min/profile_combined_#.css");
		} // RenderEzbobProfileCss

		public static MvcHtmlString RenderEverlineProfileCss() {
			return Bundle.Css()
				//libs
				.Add("~/Content/css/lib/bootstrap2.css")
				.Add("~/Content/css/lib/jquery.jscrollpane.css")
				
				//custom css
				.Add("~/Content/css/profile.css")
				.Add("~/Content/css/EverlineCustomerProfile.css")
				.Add("~/Content/css/EverlineCustomerProfileDirectors.css")
				.Add("~/Content/css/EverlineCustomerProfileLoanDetails.css")
				.Add("~/Content/css/EverlineCustomerProfileMyInformation.css")
				.Add("~/Content/css/EverlineCustomerProfileMainMenu.css")
				.Add("~/Content/css/edit-experian-director-data.css")
				.MvcRender("~/Content/css/min/profile_combined_#.css");
		} // RenderEverlineProfileCss

		public static MvcHtmlString RenderWizardCss() {
			return Bundle.Css()
				//custom css
				.Add("~/Content/css/wizard.css")
				.Add("~/Content/css/mobile.css")
				.MvcRender("~/Content/css/min/wizard_combined_#.css");
		} // RenderWizardCss

		public static MvcHtmlString RenderEverlineWizardCss() {
			return Bundle.Css()
				//custom css
				.Add("~/Content/css/everline-mobile.css")
				.MvcRender("~/Content/css/min/wizard_combined_#.css");
		} // RenderEverlineWizardCss
		
		public static MvcHtmlString RenderLoginCss() {
			return Bundle.Css()
				//custom css
				.Add("~/Content/css/mobile.css")
				.MvcRender("~/Content/css/min/login_combined_#.css");
		} // RenderProfileCss

		public static MvcHtmlString RenderUnderwriterCss() {
			return Bundle.Css()
				//libs
				.Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
				.Add("~/Content/css/lib/jquery.ui.1.8.16.ie.css")
				.Add("~/Content/css/lib/jquery.jscrollpane.css")
				.Add("~/Content/css/lib/font-awesome.min.css")
				.Add("~/Content/css/lib/bootstrap2.css")
				.Add("~/Content/css/lib/bootstrap3.css")
				.Add("~/Content/css/lib/datepicker.css")
				.Add("~/Content/css/lib/daterangepicker.css")
				.Add("~/Content/css/lib/dataTables.bootstrap.css")
				.Add("~/Content/css/lib/bootstrap-switch.css")
				.Add("~/Content/css/lib/DT_bootstrap.css")
				.Add("~/Content/css/lib/bootstrap3-modal-patch.css")
				.Add("~/Content/css/lib/bootstrap-modal.css")
				.Add("~/Content/css/lib/flaty.css")
				.Add("~/Content/css/lib/jquery.jqplot.css")
				.Add("~/Content/css/lib/notifications.css")
				.Add("~/Content/css/lib/chosen.css")
				.Add("~/Content/css/lib/alertify.css")
				.Add("~/Content/css/lib/dropzone.css")

				//custom css
                .Add("~/Content/css/common.css")
				.Add("~/Content/css/underwriter.css")
				.Add("~/Content/css/hmrc-upload-ui.css")
				.Add("~/Content/css/hmrc-manual-ui.css")
				.Add("~/Content/css/Permission.css")
				.Add("~/Content/css/edit-experian-director-data.css")

				.MvcRender("~/Content/css/min/combined_#.css");
		} // RenderUnderwriterCss

		public static MvcHtmlString RenderSalesForceCss() {
			return Bundle.Css()
				.Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
				.Add("~/Content/css/common.css")
				//libs
				.Add("~/Content/css/lib/font-awesome.min.css")
				.Add("~/Content/css/lib/bootstrap3.css")
				.Add("~/Content/css/lib/dataTables.bootstrap.css")
				//custom
				.Add("~/Content/css/edit-experian-director-data.css")
				.Add("~/Content/css/salesforce.css")
				.MvcRender("~/Content/css/min/salesforce_#.css");
		} // RenderUnderwriterCss

		public static MvcHtmlString RenderAdminLoginCss() {
			return Bundle.Css()
				//libs
				.Add("~/Content/css/lib/bootstrap3.css")
				.Add("~/Content/css/lib/notifications.css")
				.Add("~/Content/css/lib/flaty.css")
				//custom
				.Add("~/Content/css/adminlogin.css")
				.MvcRender("~/Content/css/min/adminlogin_#.css");
		}

		public static MvcHtmlString RenderPrintCss() {
			return Bundle.Css()
				.Add("~/Content/css/print.css")
				.WithAttribute("media", "print")
				.MvcRender("~/Content/css/min/print_combined_#.css");
		} // RenderPrintCss

		public static MvcHtmlString RenderEzbobBrokerCss() {
			return Bundle.Css()
			// libs
				.Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
				.Add("~/Content/css/lib/jquery.ui.1.8.16.ie.css")
				.Add("~/Content/css/lib/bootstrap2.css")
				.Add("~/Content/css/lib/bootstrap3.css")
				.Add("~/Content/css/lib/bootstrap3-modal-patch.css")
				.Add("~/Content/css/lib/dataTables.bootstrap.css")
				.Add("~/Content/css/lib/DT_bootstrap.css")
				.Add("~/Content/css/lib/font-awesome.min.css")
				.Add("~/Content/css/lib/dropzone.css")
				.Add("~/Content/css/lib/flaty.css")
				.Add("~/Content/css/lib/flaty-responsive.css")
				.Add("~/Content/css/lib/chosen.css")
				.Add("~/Content/css/lib/notifications.css")

			// custom css
				.Add("~/Content/css/common.css")
				.Add("~/Content/css/ezbob.css")
				.Add("~/Content/css/broker.css")
				.Add("~/Content/css/edit-experian-director-data.css")
				.Add("~/Content/css/mobile.css")
				.Add("~/Content/css/profile-ny2015scratch.css")
				.Add("~/Content/css/valentine2015scratch.css")
				.Add("~/Content/css/easter2015scratch.css")

			.MvcRender("~/Content/css/min/broker_#.css");
		} // RenderEzbobBrokerCss

		public static MvcHtmlString RenderEverlineBrokerCss() {
			return Bundle.Css()
				// libs
				.Add("~/Content/css/lib/normalize.css")
				.Add("~/Content/css/lib/jquery-ui-1.8.16.custom.css")
				.Add("~/Content/css/lib/jquery.ui.1.8.16.ie.css")
				.Add("~/Content/css/lib/bootstrap2.css")
				.Add("~/Content/css/lib/bootstrap3.css")
				.Add("~/Content/css/lib/bootstrap3-modal-patch.css")
				.Add("~/Content/css/lib/dataTables.bootstrap.css")
				.Add("~/Content/css/lib/DT_bootstrap.css")
				.Add("~/Content/css/lib/dropzone.css")
				.Add("~/Content/css/lib/font-awesome.min.css")
				.Add("~/Content/css/lib/chosen.css")
				.Add("~/Content/css/lib/notifications.css")
				.Add("~/Content/css/lib/pe-icon-7-stroke.css")
				.Add("~/Content/css/lib/pe-icon-7-helper.css")

				// custom css
				.Add("~/Content/css/Everlinecustomer.css")
				.Add("~/Content/css/everline.css")
               // .Add("~/Content/css/everline-mobile.css")
				.Add("~/Content/css/EverlineBroker.css")
				.Add("~/Content/css/edit-experian-director-data.css")
				.Add("~/Content/css/profile-ny2015scratch.css")
				.Add("~/Content/css/valentine2015scratch.css")
				.Add("~/Content/css/easter2015scratch.css")

			.MvcRender("~/Content/css/min/broker_#.css");
		} // RenderEverlineBrokerCss

		public static MvcHtmlString RenderCommonJs() {
			return Bundle.JavaScript()
				//libs
				.Add("~/Content/js/lib/jquery-1.8.3.js")
				.Add("~/Content/js/lib/jquery.browser.min.js")
				.Add("~/Content/js/lib/dropzone.js")
				.Add("~/Content/js/lib/jquery.hoverIntent.min.js")
				.Add("~/Content/js/lib/jquery.scrollTo.js")
				.Add("~/Content/js/lib/jquery.blockUI.js")
				.Add("~/Content/js/lib/jquery.mousewheel.js")
				.Add("~/Content/js/lib/jquery.jscrollpane.js")
				.Add("~/Content/js/lib/jquery.validate.js")
				.Add("~/Content/js/lib/jquery-ui-1.8.24.custom.js")
				.Add("~/Content/js/lib/jquery.ui.touch-punch.min.js")
				.Add("~/Content/js/lib/jsuri-1.1.1.js")
				.Add("~/Content/js/lib/jquery.cookie.js")
				.Add("~/Content/js/lib/underscore.js")
				.Add("~/Content/js/lib/backbone.js")
				.Add("~/Content/js/lib/Backbone.EzBob.js")
				.Add("~/Content/js/lib/Backbone.ModelBinder.js")
				.Add("~/Content/js/lib/handlebars.js")
				.Add("~/Content/js/lib/backbone.marionette.js")
				.Add("~/Content/js/lib/moment.js")
				.Add("~/Content/js/lib/jquery.numeric.js")
				.Add("~/Content/js/lib/recaptcha_ajax.js")
				.Add("~/Content/js/lib/autoNumeric.js")
				.Add("~/Content/js/lib/chosen.jquery.js")
				.Add("~/Content/js/lib/jquery.maskedinput-1.2.2.js")
				.Add("~/Content/js/lib/notifications.js")
				.Add("~/Content/js/lib/coin-slider.js")
				.Add("~/Content/js/lib/log.js")
				.Add("~/Content/js/lib/jquery.placeholder.js")
				.Add("~/Content/js/lib/attardi.labels.js")
				.Add("~/Content/js/lib/amount-period-sliders.js")
				.Add("~/Content/js/lib/jquery.field_status.js")
				.Add("~/Content/js/lib/jquery.set_display_value.js")
				.Add("~/Content/js/lib/jquery.load_display_value.js")
				.Add("~/Content/js/lib/mousetrap_1.4.0.js")
				.Add("~/Content/js/lib/jquery.colorbox-min.js")
				.Add("~/Content/js/lib/jquery.dataTables.min.js")
				.Add("~/Content/js/lib/footer.min.js")

				//controls
				.Add("~/Content/js/controls/ezbob.modal.js")
				.Add("~/Content/js/controls/ezbob.jqmodal.js")
				.Add("~/Content/js/controls/ezbob.BoundItemView.js")
				.Add("~/Content/js/controls/captcha.js")

				//login
				.Add("~/Content/js/login/ezbob.restorePassword.js")

				//App
				.Add("~/Content/js/App/ezbob.GA.js")
                .Add("~/Content/js/App/ezbob.iovation.js")
				.Add("~/Content/js/App/ezbob.app.js")
				.Add("~/Content/js/App/ezbob.clicktale.js")
				.Add("~/Content/js/App/ezbob.bindings.js")
				.Add("~/Content/js/App/ezbob.validation.js")

				.Add("~/Content/js/controls/ezbob.notifications.js")

				//custom 
				.Add("~/Content/js/ezbob.csrf.js")
				.Add("~/Content/js/ezbob.view.js")
				.Add("~/Content/js/ezbob.design.js")
				.Add("~/Content/js/ezbob.internal.debug.js")
				.Add("~/Content/js/ezbob.uiaction.js")
				.Add("~/Content/js/ezbob.cgvendors.js")
				.Add("~/Content/js/ezbob.serverlog.js")
				.Add("~/Content/js/ezbob.strengthPassword.js")
                .Add("~/Content/js/ezbob.sliders.js")

				.AddString(GetDbStrings())
				//.ForceRelease()
				.MvcRender("~/Content/js/min/jslib_#.js");
		} // RenderCommonJs

		public static string GetDbStrings() {
			var strings = ObjectFactory.GetInstance<IDbStringRepository>();
			Dictionary<string, string> dict = strings.GetAllStrings().ToDictionary(s => s.Key, s => s.Value);
			string json = JsonConvert.SerializeObject(dict);
			return "var EzBob = EzBob || {};" + "EzBob.dbStrings = " + json + ";";
		} // GetDbStrings

		public static MvcHtmlString RenderUnderwriterJs() {
			return Bundle.JavaScript()
				//3rd party libs
				.Add("~/Content/js/lib/alertify.js")
				.Add("~/Content/js/lib/notifications.js")
				.Add("~/Content/js/lib/jquery.zclip.js")
				.Add("~/Content/js/lib/jquery.form.js")
				.Add("~/Content/js/lib/splitter.js")
				.Add("~/Content/js/lib/jquery.cookie.js")
				.Add("~/Content/js/lib/jquery.slimscroll.min.js")
				.Add("~/Content/js/lib/jquery.sparkline.min.js")
				.Add("~/Content/js/lib/jqplot/jquery.jqplot.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.dateAxisRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasTextRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasAxisLabelRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.trendline.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.categoryAxisRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.logAxisRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasAxisTickRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.highlighter.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.enhancedLegendRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.cursor.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.barRenderer.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasOverlay.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.pointLabels.js")
				.Add("~/Content/js/lib/jqplot/plugins/jqplot.donutRenderer.js")
				.Add("~/Content/js/lib/date.interval.js")

				//Flaty
				.Add("~/Content/js/lib/flaty/bootstrap3.js")
				.Add("~/Content/js/lib/date.js")
				.Add("~/Content/js/lib/daterangepicker.js")
				.Add("~/Content/js/lib/bootstrap-datepicker.js")
				.Add("~/Content/js/lib/bootstrap-modal.js")
				.Add("~/Content/js/lib/bootstrap-modalmanager.js")
				.Add("~/Content/js/lib/flaty/dataTables.bootstrap.js")
				.Add("~/Content/js/lib/flaty/bootstrap-switch.js")
				.Add("~/Content/js/lib/flaty/DT_Bootstrap.js")
				.Add("~/Content/js/lib/flaty/flaty.js")

				// HighCharts
				.Add("~/Content/js/lib/highcharts/highcharts.js")
				.Add("~/Content/js/lib/highcharts/exporting.js")
				.Add("~/Content/js/lib/highcharts/highcharts-more.js")
				.Add("~/Content/js/lib/highcharts/makeChart.js")

				//Underwriter
				.Add("~/Content/js/underwriter/ezbob.underwriter.app.js")
				.Add("~/Content/js/underwriter/bugs/ezbob.underwriter.bugs.reporter.js")
				.Add("~/Content/js/underwriter/ezbob.underwriter.goToCustomer.js")

				//Support
				.Add("~/Content/js/underwriter/support/ezbob.underwriter.support.js")
				.Add("~/Content/js/underwriter/support/ezbob.underwriter.funding.js")

				//Fraud
				.Add("~/Content/js/Underwriter/Fraud/ezbob.underwriter.fraud.js")
				.Add("~/Content/js/Underwriter/profile/fraudDetection/ezbob.underwriter.fraudDetectionLog.js")
				.Add("~/Content/js/Underwriter/profile/fraudDetection/ezbob.underwriter.fraudStatus.js")

				//report
				.Add("~/Content/js/Underwriter/report/ezbob.underwriter.report.js")

				// Strategy Settings
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.charges.js")
				.Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.Experian.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.general.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.approval.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.general.js")
				.Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.rejection.js")
				.Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.Campaign.js")
				.Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.ConfigTables.js")
				.Add("~/Content/js/underwriter/StrategySettings/Ezbob.underwriter.Settings.PricingModel.js")
				.Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.Product.js")
				.Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.OpenPlatform.js")

				//Customer grid
				.Add("~/Content/js/EzBob.DataTables.Helper.js")
				.Add("~/Content/js/underwriter/customersGrid/ezbob.underwriter.grids.js")

				//broker
				.Add("~/Content/js/underwriter/broker/ezbob.underwriter.broker.profile.js")
				.Add("~/Content/js/underwriter/broker/ezbob.underwriter.broker.whitelabel.js")

				//profile
				.Add("~/Content/js/underwriter/profile/ezbob.underwriter.customerFullModel.js")
				.Add("~/Content/js/underwriter/profile/ezbob.underwriter.profile.js")

				//leftAndBottomBar
				.Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.profileHead.js")
				.Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.emaileditview.js")
                .Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.phoneEditView.js")
				.Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.personInfo.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.functionsDialog.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.creditLineDialog.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.ApproveLoanWithoutAML.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.ApproveLoanForWarningStatusCustomer.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.controlButtons.js")

				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.collectionStatus.js")
				//loanHistory
				.Add("~/Content/js/underwriter/profile/loanHistory/ezbob.underwriter.loanInfo.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.loanHistorys.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.loanHistoryDetail.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.rollover.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.manualPayment.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.LoanOptions.js")

				.Add("~/Content/js/underwriter/profile/summary/ezbob.underwriter.summaryInfo.js")
				.Add("~/Content/js/underwriter/profile/summary/ezbob.underwriter.dashboard.js")
				.Add("~/Content/js/underwriter/profile/summary/ezbob.underwriter.affordability.js")
				.Add("~/Content/js/underwriter/profile/summary/ezbob.underwriter.journal.js")

				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.marketplaceDetails.js")
				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.marketplaces.js")
				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.marketplacesHistory.js")

				.Add("~/Content/js/ezbob.hmrc.upload.ui.js")
				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.uploadHmrc.js")
				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.enterHmrc.js")
				.Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.parseYodlee.js")
				.Add("~/Content/js/underwriter/profile/creditBureau/ezbob.underwriter.experianInfo.js")
				.Add("~/Content/js/underwriter/profile/creditBureau/ezbob.underwriter.idHubCustomAddress.js")

				//payment accounts
				.Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.addPayPointCardView.js")
				.Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.AddBankAccount.js")
				.Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.bankAccountDetails.js")
				.Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.paymentAccounts.js")

				.Add("~/Content/js/Underwriter/profile/alerts/ezbob.underwriter.AlertDocsView.js")
				.Add("~/Content/js/Underwriter/profile/Calculator/ezbob.underwriter.medalCalculations.js")
				.Add("~/Content/js/Underwriter/profile/Calculator/ezbob.underwriter.logicalGlueHistory.js")
				.Add("~/Content/js/Underwriter/profile/Calculator/ezbob.underwriter.pricingModelCalculations.js")
                .Add("~/Content/js/Underwriter/profile/Calculator/ezbob.underwriter.automationCalculations.js")
				.Add("~/Content/js/Underwriter/profile/customerInfo/ezbob.underwriter.crosscheck.js")
				.Add("~/Content/js/Underwriter/profile/customerInfo/ezbob.underwriter.zoopla.js")
				.Add("~/Content/js/Underwriter/profile/customerInfo/ezbob.underwriter.landregistry.js")
                .Add("~/Content/js/Underwriter/profile/customerInfo/ezbob.underwriter.changeAddress.js")
				.Add("~/Content/js/Underwriter/profile/messages/ezbob.undewriter.messages.js")
				.Add("~/Content/js/Underwriter/profile/companyScore/ezbob.underwriter.companyScore.js")
				.Add("~/Content/js/Underwriter/profile/APIChecksLog/ezbob.underwriter.apiChecksLog.js")
				.Add("~/Content/js/Underwriter/profile/CustomerRelations/ezbob.underwriter.customerRelations.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.AddCustomerRelationsEntry.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.AddCustomerRelationsFollowUp.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.MarkAsPending.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.SendSms.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.salesForceActivity.js")
				.Add("~/Content/js/Underwriter/profile/ezbob.underwriter.properties.js")
				.Add("~/Content/js/ezbob.edit.experian.director.data.js")
				.Add("~/Content/js/Underwriter/profile/ezbob.underwriter.signature.monitor.js")

				//----
				.Add("~/Content/js/Underwriter/editLoan/loanModel.js")
				.Add("~/Content/js/Underwriter/editLoan/installmentEditor.js")
				.Add("~/Content/js/Underwriter/editLoan/editFee.js")
				.Add("~/Content/js/Underwriter/editLoan/editLoanView.js")
				.Add("~/Content/js/Underwriter/CAIS/ezbob.underwriter.CAIS.caisManage.js")

				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")
				.Add("~/Content/js/controls/ezbob.LoanScheduleView.js")
				.Add("~/Content/js/controls/ezbob.simpleValueEditDlg.js")

				.Add("~/Content/js/controls/ezbob.address.js")
				.Add("~/Content/js/ezbob.models.js")
				.Add("~/Content/js/ezbob.addDirectorInfoView.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourInfo.directors.js")

				//investor
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.addInvestorView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.manageInvestorView.js")
                .Add("~/Content/js/Underwriter/investor/ezbob.underwriter.manageInvestorsView.js")
				
            
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.manageInvestorContactView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.manageInvestorBankView.js")
                .Add("~/Content/js/Underwriter/investor/ezbob.underwriter.manageInvestorDetailsView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.configInvestorView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.accountingInvestorView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.portfolioInvestorView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.statisticsInvestorView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.fundingTransactionsView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.repaymentsTransactionsView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.addTransactionView.js")
				.Add("~/Content/js/Underwriter/investor/ezbob.underwriter.configSchedulerView.js")

				.MvcRender("~/Content/js/min/underwriter_#.js");
		} // RenderUnderwriterJs    

		public static MvcHtmlString RenderWizardJs() {
			return Bundle.JavaScript()
				.Add("~/Content/js/ezbob.customerModel.js")
				.Add("~/Content/js/Wizard/wizardStepSequence.js")
				.Add("~/Content/js/Wizard/ezbob.wizardStepsModel.js")
				.Add("~/Content/js/Wizard/ezbob.wizardView.js")
				.Add("~/Content/js/Wizard/ezbob.vipView.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shopbutton.js")
				.Add("~/Content/js/Wizard/ezbob.ct.bindShopToCT.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.ebay.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.amazon.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.ekm.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.freeagent.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.sage.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.yodlee.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.cg.js")
				.Add("~/Content/js/EzBob.DataTables.Helper.js")
				.Add("~/Content/js/lib/date.interval.js")
				.Add("~/Content/js/ezbob.hmrc.upload.ui.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.hmrc.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.hmrcUpload.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.paypoint.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.companyFiles.js")
				.Add("~/Content/js/Wizard/ezbob.wizard.shops.js")
				.Add("~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js")
				.Add("~/Content/js/Wizard/ezbob.wizard.signupstep.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.personalInfoBase.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.limitedInformation.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.notLimitedInformation.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.personalInfo.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.steps.companydetails.js")

				.Add("~/Content/js/ezbob.models.js")
				.Add("~/Content/js/ezbob.addDirectorInfoView.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourInfo.directors.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourInfo.employeeCount.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")
				.Add("~/Content/js/controls/ezbob.address.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourIno.consent.agreement.js")
				.MvcRender("~/Content/js/min/wizard_#.js");
		} // RenderWizardJs

		public static MvcHtmlString RenderProfileJs() {
			return Bundle.JavaScript()
				//3rd party
				.Add("~/Content/js/lib/bootstrap.js")
				.Add("~/Content/js/lib/bootstrap-datepicker.js")
				.Add("~/Content/js/lib/bootstrap-modal.js")
				.Add("~/Content/js/lib/bootstrap-modalmanager.js")
				.Add("~/Content/js/lib/wScratchPad.min.js")

				//Customer Model
				.Add("~/Content/js/ezbob.customerModel.js")

				//Controls
				.Add("~/Content/js/controls/ezbob.address.js")
				.Add("~/Content/js/controls/ezbob.whatsnew.js")
				.Add("~/Content/js/controls/ezbob.livechat.livechatrouter.js")
				.Add("~/Content/js/controls/ezbob.LoanScheduleView.js")

				//Wizard
				.Add("~/Content/js/Profile/ezbob.profile.payEarly.js")
				.Add("~/Content/js/Profile/ezbob.profile.getCash.js")
				.Add("~/Content/js/Profile/ezbob.profile.reviewSignCreditLine.js")
				.Add("~/Content/js/Profile/ezbob.profile.signWidget.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shopbutton.js")
				.Add("~/Content/js/Wizard/ezbob.ct.bindShopToCT.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.ebay.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.amazon.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.ekm.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.freeagent.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.sage.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.yodlee.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.cg.js")
				.Add("~/Content/js/lib/date.interval.js")
				.Add("~/Content/js/EzBob.DataTables.Helper.js")
				.Add("~/Content/js/ezbob.hmrc.upload.ui.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.hmrc.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.hmrcUpload.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.paypoint.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.companyFiles.js")
				.Add("~/Content/js/Wizard/shops/ezbob.shops.js")
				.Add("~/Content/js/Wizard/ezbob.wizardStepsModel.js")
				.Add("~/Content/js/Wizard/ezbob.wizard.shops.js")
				.Add("~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js")
				.Add("~/Content/js/ezbob.models.js")
				.Add("~/Content/js/ezbob.addDirectorInfoView.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourInfo.directors.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")

				//Profile
				.Add("~/Content/js/Profile/ezbob.profile.smallNotification.js")
				.Add("~/Content/js/Profile/ApplyForLoan/ezbob.profile.PayPointCardSelectView.js")
				.Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanModel.js")
				.Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanView.js")
				.Add("~/Content/js/Profile/ApplyForLoan/ezbob.accounts.bank.js")
				.Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanTopView.js")
                .Add("~/Content/js/Profile/ezbob.Profile.AgreementsDynamicCheckboxes.js")
				.Add("~/Content/js/Profile/ezbob.profile.profileView.js")
				.Add("~/Content/js/Profile/ezbob.profile.profileRouter.js")
				.Add("~/Content/js/Profile/PayEarly/makeEarlyPaymentModel.js")
				.Add("~/Content/js/Profile/PayEarly/makeEarlyPaymentView.js")
				.Add("~/Content/js/Profile/ezbob.profile.accountActivity.js")
				.Add("~/Content/js/Profile/ezbob.profile.loanDetails.js")
				.Add("~/Content/js/Profile/ezbob.profile.paymentAccounts.js")
				.Add("~/Content/js/Profile/ezbob.profile.Stores.js")
				.Add("~/Content/js/Profile/ezbob.profile.yourInfo.js")
				.Add("~/Content/js/Profile/ezbob.profile.paypointSchedule.js")
				.Add("~/Content/js/Profile/ezbob.profile.loanTaken.js")
				.Add("~/Content/js/Profile/ezbob.profile.perks.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.selectcard.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.ny2015.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.valentine2015.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.easter2015.js")
				.Add("~/Content/js/Profile/Settings/ezbob.profile.settingsMain.js")
				.Add("~/Content/js/Profile/Settings/ezbob.profile.settingsMaster.js")
				.Add("~/Content/js/Profile/Settings/ezbob.profile.settingsPassword.js")
				.Add("~/Content/js/Profile/Settings/ezbob.profile.settingsQuestion.js")
				.Add("~/Content/js/Profile/ezbob.profile.Agreements.js")
				.Add("~/Content/js/ezbob.edit.experian.director.data.js")
				.Add("~/Content/js/Profile/ezbob.profile.companyDirectorsView.js")
				.Add("~/Content/js/Profile/ezbob.profile.processingAutomationPopupView.js")
				.MvcRender("~/Content/js/min/profile_#.js");
		} // RenderProfileJs

		public static MvcHtmlString RenderLoginJs() {
			return Bundle.JavaScript()
				.Add("~/Content/js/login/ezbob.login.view.js")
				.MvcRender("~/Content/js/min/profile_#.js");
		} // RenderLoginJs

		public static MvcHtmlString RenderAdminLoginJs() {
			return Bundle.JavaScript()
				//libs
				.Add("~/Content/js/lib/jquery-1.8.3.js")
				.Add("~/Content/js/lib/jquery.validate.js")
				.Add("~/Content/js/lib/notifications.js")
				.Add("~/Content/js/lib/flaty/bootstrap3.js")
				.Add("~/Content/js/lib/underscore.js")
				.Add("~/Content/js/lib/backbone.js")
				.Add("~/Content/js/lib/backbone.marionette.js")
				//custom
				.Add("~/Content/js/ezbob.csrf.js")
				.Add("~/Content/js/controls/ezbob.modal.js")
				.Add("~/Content/js/controls/ezbob.jqmodal.js")
				.Add("~/Content/js/App/ezbob.app.js")
				.Add("~/Content/js/App/ezbob.validation.js")
				.Add("~/Content/js/controls/ezbob.notifications.js")
				.Add("~/Content/js/Underwriter/ezbob.underwriter.adminlogin.js")
				.MvcRender("~/Content/js/min/adminlogin_#.js");
		} // RenderAdminLoginJs

		public static MvcHtmlString RenderCreatePasswordJs() {
			return Bundle.JavaScript()
				.Add("~/Content/js/login/ezbob.create.password.view.js")
				.MvcRender("~/Content/js/min/profile_#.js");
		} // RenderCreatePasswordJs

		public static MvcHtmlString RenderPaypointTemplateJs() {
			return Bundle.JavaScript()
				.Add("~/Content/js/lib/jquery-1.8.3.js")
				.Add("~/Content/js/lib/jquery.browser.min.js")
				.Add("~/Content/js/lib/jquery.scrollTo.js")
				.Add("~/Content/js/lib/jquery.blockUI.js")
				.Add("~/Content/js/lib/jquery.mask.min.js")
				.Add("~/Content/js/lib/jquery.validate.js")
				.Add("~/Content/js/lib/moment.js")
				.Add("~/Content/js/lib/jsuri-1.1.1.js")
				.Add("~/Content/js/lib/bootstrap.js")
				.Add("~/Content/js/lib/jquery.cookie.js")
				.Add("~/Content/js/lib/underscore.js")
				.Add("~/Content/js/lib/backbone.js")
				.Add("~/Content/js/App/ezbob.validation.js")
				.Add("~/Content/js/lib/jquery.field_status.js")
				.Add("~/Content/js/lib/attardi.labels.js")
				.MvcRender("~/Content/js/min/jsPaypojntTemplate_#.js");
		} // RenderPaypointTemplateJs

		public static MvcHtmlString RenderSalesForceJs() {
			return Bundle.JavaScript()
			// libs
				.Add("~/Content/js/lib/jquery-1.8.3.js")
				.Add("~/Content/js/lib/jquery.validate.js")
				.Add("~/Content/js/lib/jquery-ui-1.8.24.custom.js")
				.Add("~/Content/js/lib/jquery.placeholder.js")
				.Add("~/Content/js/lib/jquery.dataTables.js")
				.Add("~/Content/js/lib/flaty/bootstrap3.js")
				.Add("~/Content/js/lib/jquery.blockUI.js")
				.Add("~/Content/js/lib/underscore.js")
				.Add("~/Content/js/lib/backbone.js")
				.Add("~/Content/js/lib/backbone.marionette.js")
				.Add("~/Content/js/lib/moment.js")
				.Add("~/Content/js/lib/attardi.labels.js")
				.Add("~/Content/js/lib/jquery.field_status.js")
			//main
				.Add("~/Content/js/ezbob.design.js")
				.Add("~/Content/js/ezbob.csrf.js")
				.Add("~/Content/js/ezbob.view.js")
				.Add("~/Content/js/ezbob.internal.debug.js")
				.Add("~/Content/js/ezbob.uiaction.js")
				.Add("~/Content/js/ezbob.serverlog.js")
				.Add("~/Content/js/EzBob.DataTables.Helper.js")
				.Add("~/Content/js/ezbob.models.js")
				.Add("~/Content/js/ezbob.addDirectorInfoView.js")
				.Add("~/Content/js/ezbob.edit.experian.director.data.js")

			//controls
				.Add("~/Content/js/controls/ezbob.modal.js")
				.Add("~/Content/js/controls/ezbob.jqmodal.js")
				.Add("~/Content/js/controls/ezbob.address.js")
			//app
				.Add("~/Content/js/App/ezbob.app.js")
				.Add("~/Content/js/App/ezbob.validation.js")
			//uw
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.functionsDialog.js")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.personInfo.js")
				.Add("~/Content/js/Underwriter/profile/ezbob.underwriter.signature.monitor.js")
				.Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.loanInfo.js")
			// custom
				.Add("~/Content/js/Underwriter/ezbob.underwriter.salesforce.js")
			.MvcRender("~/Content/js/min/jsSalesForce_#.js");
		} // RenderSalesForceJs

		public static MvcHtmlString RenderBrokerJs() {
			return Bundle.JavaScript()
			// lib
				.Add("~/Content/js/lib/jquery-1.8.3.js")
				.Add("~/Content/js/ezbob.csrf.js")
				.Add("~/Content/js/lib/jquery.browser.min.js")
				.Add("~/Content/js/lib/dropzone.js")
				.Add("~/Content/js/lib/jquery.hoverIntent.min.js")
				.Add("~/Content/js/lib/jquery.scrollTo.js")
				.Add("~/Content/js/lib/jquery.blockUI.js")
				.Add("~/Content/js/lib/jquery.mousewheel.js")
				.Add("~/Content/js/lib/jquery.jscrollpane.js")
				.Add("~/Content/js/lib/jquery.validate.js")
				.Add("~/Content/js/lib/jquery-ui-1.8.24.custom.js")
				.Add("~/Content/js/lib/jsuri-1.1.1.js")
				.Add("~/Content/js/lib/jquery.cookie.js")
				.Add("~/Content/js/lib/jquery.slimscroll.min.js")
				.Add("~/Content/js/lib/underscore.js")
				.Add("~/Content/js/lib/backbone.js")
				.Add("~/Content/js/lib/backbone.marionette.js")
				.Add("~/Content/js/lib/Backbone.ModelBinder.js")
				.Add("~/Content/js/lib/moment.js")
				.Add("~/Content/js/lib/jquery.numeric.js")
				.Add("~/Content/js/lib/recaptcha_ajax.js")
				.Add("~/Content/js/lib/autoNumeric.js")
				.Add("~/Content/js/lib/chosen.jquery.js")
				.Add("~/Content/js/lib/jquery.maskedinput-1.2.2.js")
				.Add("~/Content/js/lib/notifications.js")
				.Add("~/Content/js/lib/coin-slider.js")
				.Add("~/Content/js/lib/jquery.placeholder.js")
				.Add("~/Content/js/lib/attardi.labels.js")
				.Add("~/Content/js/lib/amount-period-sliders.js")
				.Add("~/Content/js/lib/jquery.field_status.js")
				.Add("~/Content/js/lib/jquery.set_display_value.js")
				.Add("~/Content/js/lib/jquery.load_display_value.js")
				.Add("~/Content/js/lib/mousetrap_1.4.0.js")
				.Add("~/Content/js/lib/jquery.colorbox-min.js")
				.Add("~/Content/js/lib/wScratchPad.min.js")
				.Add("~/Content/js/controls/ezbob.modal.js")
				.Add("~/Content/js/controls/ezbob.jqmodal.js")
				.Add("~/Content/js/App/ezbob.app.js")
				.Add("~/Content/js/App/ezbob.validation.js")
				.Add("~/Content/js/controls/captcha.js")
				.Add("~/Content/js/controls/ezbob.notifications.js")
				.Add("~/Content/js/ezbob.design.js")
				.Add("~/Content/js/ezbob.internal.debug.js")
				.Add("~/Content/js/ezbob.uiaction.js")
				.Add("~/Content/js/ezbob.cgvendors.js")
				.Add("~/Content/js/ezbob.serverlog.js")
				.Add("~/Content/js/ezbob.strengthPassword.js")
				.Add("~/Content/js/lib/jquery.dataTables.js")
				.Add("~/Content/js/lib/flaty/bootstrap3.js")
				.Add("~/Content/js/lib/bootstrap-datepicker.js")
				.Add("~/Content/js/lib/bootstrap-modal.js")
				.Add("~/Content/js/lib/bootstrap-modalmanager.js")
				.Add("~/Content/js/lib/flaty/dataTables.bootstrap.js")
				.Add("~/Content/js/lib/flaty/DT_Bootstrap.js")
				.Add("~/Content/js/lib/flaty/flaty.js")
			// common
				.Add("~/Content/js/EzBob.DataTables.Helper.js")
				.Add("~/Content/js/controls/ezbob.BoundItemView.js")
				.Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.AddCustomerRelationsEntry.js")
				.AddString(GetDbStrings())
				.Add("~/Content/js/ezbob.view.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.selectcard.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.ny2015.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.valentine2015.js")
				.Add("~/Content/js/ScratchCards/ezbob.scratchcard.easter2015.js")
				.Add("~/Content/js/ezbob.edit.experian.director.data.js")
			// custom
				.Add("~/Content/js/Broker/broker.base.view.js")
				.Add("~/Content/js/Broker/broker.submit.view.js")
				.Add("~/Content/js/Broker/broker.mobilephone.view.js")
				.Add("~/Content/js/Broker/broker.forgotten.view.js")
				.Add("~/Content/js/Broker/broker.dashboard.view.js")
				.Add("~/Content/js/Broker/broker.requestAcceptTerms.view.js")
				.Add("~/Content/js/Broker/broker.login.view.js")
				.Add("~/Content/js/Broker/broker.signup.view.js")
				.Add("~/Content/js/Broker/broker.addcustomer.view.js")
				.Add("~/Content/js/Broker/broker.customerdetails.view.js")
				.Add("~/Content/js/Broker/broker.leaddetails.view.js")
				.Add("~/Content/js/Broker/broker.router.js")
				.Add("~/Content/js/Broker/broker.main.js")
				.Add("~/Content/js/Broker/broker.instantoffer.view.js")
				.Add("~/Content/js/Broker/broker.changepassword.view.js")
				.Add("~/Content/js/Broker/broker.addbank.view.js")
				.Add("~/Content/js/Broker/broker.commission.view.js")
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")
				.Add("~/Content/js/controls/ezbob.LoanScheduleView.js")
			.MvcRender("~/Content/js/min/jslib_#.js");
		} // RenderBrokerJs
	} // class BundleHelper
} // namespace

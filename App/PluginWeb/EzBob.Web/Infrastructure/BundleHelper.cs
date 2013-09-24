﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EzBob.Web.Models.Repository;
using Newtonsoft.Json;
using SquishIt.Framework;
using SquishIt.Mvc;
using StructureMap;

namespace EzBob.Web.Infrastructure
{
    public class BundleHelper
    {
        public static MvcHtmlString RenderCss()
        {
            return Bundle.Css()
                .Add("~/Content/css/datepicker.css")
                .Add("~/Content/css/jquery-ui-1.8.16.custom.css")
                .Add("~/Content/css/jquery.ui.1.8.16.ie.css")
                .Add("~/Content/css/ui.jqgrid.css")
                .Add("~/Content/css/jquery.jscrollpane.css")
                .Add("~/Content/css/bootstrap.css")
                .Add("~/Content/css/bootstrap-modal.css")
                .Add("~/Content/css/jquery.jqplot.css")
                .Add("~/Content/css/notifications.css")
                .Add("~/Content/css/wizard.css")
                .Add("~/Content/css/profile.css")
                .Add("~/Content/css/loanDetails.less")
                .Add("~/Content/css/choosen/chosen.css")
                .Add("~/Content/css/underwriter.css")
                .Add("~/Content/css/Permission.css")
                .Add("~/Content/css/alertify.css")
                .Add("~/Content/css/editLoan.less")
                .Add("~/Content/css/captcha.css")
                .Add("~/Content/css/common.css")
                .Add("~/Content/css/popover.css")
                .Add("~/Content/css/shops.css")
                .Add("~/Content/css/app.css")
                .Add("~/Content/css/attardi.labels.css")
                .Add("~/Content/css/loan-type-selector.css")
                .Add("~/Content/css/amount-period-sliders.css")
				.Add("~/Content/css/jquery.dataTables.css")
				.Add("~/Content/css/jquery.dataTables_themeroller.css")
                .AddString(Integration.ChannelGrabberConfig.Configuration.Instance.GetSourceLabelCSS())
                .MvcRender("~/Content/css/min/combined_#.css");
        }

        public static MvcHtmlString CustomerCss()
        {
            return Bundle.Css()
                .Add("~/Content/css/jquery-ui-1.8.16.custom.css")
                .Add("~/Content/css/jquery.ui.1.8.16.ie.css")
                .Add("~/Content/css/bootstrap.css")
                .Add("~/Content/css/style.css")
                .Add("~/Content/css/notifications.css")
                .Add("~/Content/css/wizard.css")
                .Add("~/Content/css/wizard-header.css")
                .Add("~/Content/css/coin-slider-styles.css")
                .Add("~/Content/css/sidebar.css")
                .Add("~/Content/css/customer.css")
                .Add("~/Content/css/captcha.css")
                .Add("~/Content/css/common.css")
                .Add("~/Content/css/postcode.css")
                .Add("~/Content/css/popover.css")
                .Add("~/Content/css/shops.css")
                .Add("~/Content/css/choosen/chosen.css")
                .Add("~/Content/css/app.css")
                .Add("~/Content/css/attardi.labels.css")
                .Add("~/Content/css/loan-type-selector.css")
                .Add("~/Content/css/amount-period-sliders.css")
                .AddString(Integration.ChannelGrabberConfig.Configuration.Instance.GetSourceLabelCSS())
                .MvcRender("~/Content/css/min/customer_#.css");
        }

        public static MvcHtmlString RenderPrintCss()
        {
            return Bundle.Css()
                .Add("~/Content/css/print.css")
                .WithAttribute("media", "print")
                .MvcRender("~/Content/css/min/print_combined_#.css");
        }

        public static MvcHtmlString RenderOverviewCss()
        {
            return Bundle.Css()
                .Add("~/Content/css/overview.css")
                .Add("~/Content/css/addition.css")
                .MvcRender("~/Content/css/min/combined_ov_#.css");
        }

        public static MvcHtmlString RenderProfileCss()
        {
            return Bundle.Css()
                .Add("~/Content/css/bootstrap.css")
                .Add("~/Content/css/jquery-ui-1.8.16.custom.css")
                .Add("~/Content/css/jquery.ui.1.8.16.ie.css")
                .Add("~/Content/css/style.css")
                .Add("~/Content/css/notifications.css")
                .Add("~/Content/css/captcha.css")
                .Add("~/Content/css/wizard-header.css")
                .Add("~/Content/css/sidebar.css")
                .Add("~/Content/css/profile.css")
                .Add("~/Content/css/common.css")
                .Add("~/Content/css/postcode.css")
                .Add("~/Content/css/customer.css")
                .Add("~/Content/css/loanDetails.less")
                .Add("~/Content/css/jquery.jscrollpane.css")
                .Add("~/Content/css/popover.css")
                .Add("~/Content/css/shops.css")
                .Add("~/Content/css/coin-slider-styles.css")
                .Add("~/Content/css/app.css")
                .Add("~/Content/css/attardi.labels.css")
                .Add("~/Content/css/loan-type-selector.css")
                .Add("~/Content/css/amount-period-sliders.css")
                .AddString(Integration.ChannelGrabberConfig.Configuration.Instance.GetSourceLabelCSS())
                .MvcRender("~/Content/css/min/profile_combined_#.css");
        }

        public static MvcHtmlString RenderPaypointTemplateJs()
        {
            return Bundle.JavaScript()
                .AddString(GetDbStrings())
                .Add("~/Content/js/lib/jquery-1.8.3.js")
                .Add("~/Content/js/lib/jquery.blockUI.js")
				.Add("~/Content/js/lib/jquery.mask.min.js")
                .Add("~/Content/js/lib/jquery.validate.js")
                .Add("~/Content/js/lib/moment.js")
                .Add("~/Content/js/lib/jsuri-1.1.1.js")
                .Add("~/Content/js/lib/bootstrap.js")
                .Add("~/Content/js/lib/underscore.js")
                .Add("~/Content/js/lib/backbone.js")
                .Add("~/Content/js/contactUs/ezbo.contactUs.js")
                .Add("~/Content/js/App/ezbob.validation.js")
                .Add("~/Content/js/lib/jquery.field_status.js")
                .Add("~/Content/js/lib/attardi.labels.js")
                .MvcRender("~/Content/js/min/jsPaypojntTemplate_#.js");
        }

        public static MvcHtmlString RenderJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/lib/jquery-1.8.3.js")
                .Add("~/Content/js/lib/jquery.blockUI.js")
                .Add("~/Content/js/lib/jquery.mousewheel.js")
                .Add("~/Content/js/lib/jquery.jscrollpane.js")
                .Add("~/Content/js/lib/jquery.validate.js")
                .Add("~/Content/js/lib/jquery-ui-1.8.24.custom.js")
                .Add("~/Content/js/lib/jsuri-1.1.1.js")
                .Add("~/Content/js/lib/bootstrap.js")
                .Add("~/Content/js/lib/bootstrap-datepicker.js")
                .Add("~/Content/js/lib/bootstrap-modal.js")
                .Add("~/Content/js/lib/bootstrap-modalmanager.js")
                .Add("~/Content/js/lib/underscore.js")
                .Add("~/Content/js/lib/backbone.js")
                .Add("~/Content/js/lib/Backbone.EzBob.js")
                .Add("~/Content/js/lib/Backbone.ModelBinder.js")
                .Add("~/Content/js/lib/handlebars.js")
                .Add("~/Content/js/lib/backbone.marionette.js")
                .Add("~/Content/js/lib/moment.js")
                .Add("~/Content/js/lib/jquery.numeric.js")
                .Add("~/Content/js/lib/recaptcha_ajax.js")
                .Add("~/Content/js/lib/autoNumeric-1.7.4.js")
                .Add("~/Content/js/lib/chosen.jquery.js")
                .Add("~/Content/js/lib/jquery.maskedinput-1.2.2.js")
                .Add("~/Content/js/lib/notifications.js")
                .Add("~/Content/js/controls/ezbob.modal.js")
                .Add("~/Content/js/controls/ezbob.jqmodal.coffee")
                .Add("~/Content/js/controls/ezbob.BoundItemView.coffee")
                .Add("~/Content/js/login/ezbob.login.js")
                .Add("~/Content/js/App/ezbob.GA.coffee")
                .Add("~/Content/js/App/ezbob.app.js")
                .Add("~/Content/js/App/ezbob.clicktale.js")
                .Add("~/Content/js/contactUs/ezbo.contactUs.js")
                .Add("~/Content/js/ezbob.design.js")
                .Add("~/Content/js/App/ezbob.bindings.coffee")
                .Add("~/Content/js/App/ezbob.validation.js")
                .Add("~/Content/js/lib/coin-slider.js")
                .Add("~/Content/js/ezbob.strengthPassword.js")
                .Add("~/Content/js/controls/captcha.js")
                .Add("~/Content/js/lib/log.coffee")
                .Add("~/Content/js/lib/jquery.placeholder.js")
                .Add("~/Content/js/ezbob.csrf.js")
                .Add("~/Content/js/login/ezbob.restorePassword.coffee")
                .Add("~/Content/js/lib/attardi.labels.js")
                .Add("~/Content/js/lib/amount-period-sliders.js")
                .Add("~/Content/js/lib/jquery.field_status.js")
                .Add("~/Content/js/lib/mousetrap_1.4.0.js")
                .Add("~/Content/js/lib/jquery.colorbox-min.js")
                .AddString(GetDbStrings())
                //.ForceRelease()
                .MvcRender("~/Content/js/min/jslib_#.js");
        }

        private static string GetDbStrings()
        {
            var strings = ObjectFactory.GetInstance<IDbStringRepository>();
            Dictionary<string, string> dict = strings.GetAllStrings().ToDictionary(s => s.Key, s => s.Value);
            string json = JsonConvert.SerializeObject(dict);
            return "var EzBob = EzBob || {};" + "EzBob.dbStrings = " + json + ";";
        }

        public static MvcHtmlString RenderUnderwriterJs()
        {
            return Bundle.JavaScript()
                //3rd party libs
                .Add("~/Content/js/lib/alertify.js")
                .Add("~/Content/js/lib/jquery.zclip.js")
                .Add("~/Content/js/lib/grid.locale-en.js")
                .Add("~/Content/js/lib/jquery.jqGrid.src.js")
                .Add("~/Content/js/lib/jqgridHelpers.js")
                .Add("~/Content/js/lib/jquery.form.js")
                .Add("~/Content/js/lib/splitter.js")
                .Add("~/Content/js/lib/attardi.labels.js")
                .Add("~/Content/js/lib/amount-period-sliders.js")
                .Add("~/Content/js/lib/jquery.field_status.js")
                .Add("~/Content/js/lib/jqplot/jquery.jqplot.js")
                .Add("~/Content/js/lib/jqplot/plugins/jqplot.dateAxisRenderer.js")
                .Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasTextRenderer.js")
                .Add("~/Content/js/lib/jqplot/plugins/jqplot.canvasAxisLabelRenderer.js")
                .Add("~/Content/js/lib/jqplot/plugins/jqplot.trendline.js")
				.Add("~/Content/js/lib/jquery.dataTables.min.js")
				.Add("~/Content/js/lib/jqBarGraph.1.1.js")

				// HighCharts
				.Add("~/Content/js/lib/highcharts/highcharts.js")
				.Add("~/Content/js/lib/highcharts/exporting.js")
				.Add("~/Content/js/lib/highcharts/highcharts-more.js")
				.Add("~/Content/js/lib/highcharts/makeChart.js")

				//Underwriter
				.Add("~/Content/js/underwriter/ezbob.underwriter.app.coffee")
                .Add("~/Content/js/underwriter/bugs/ezbob.underwriter.bugs.reporter.coffee")
                .Add("~/Content/js/underwriter/ezbob.underwriter.goToCustomer.coffee")

                //Support
                .Add("~/Content/js/underwriter/support/ezbob.underwriter.support.coffee")

                //Fraud
                .Add("~/Content/js/Underwriter/Fraud/ezbob.underwriter.fraud.coffee")
                .Add("~/Content/js/Underwriter/profile/fraudDetection/ezbob.underwriter.fraudDetectionLog.js")
                .Add("~/Content/js/Underwriter/profile/fraudDetection/ezbob.underwriter.fraudStatus.coffee")

                // Configuration Variables
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.charges.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/EzBob.Underwriter.Settings.Experian.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.settings.general.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.approval.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.general.coffee")
                .Add("~/Content/js/underwriter/StrategySettings/ezbob.underwriter.StrategySettings.automation.rejection.coffee")
				
                //Customer grid
                .Add("~/Content/js/underwriter/customersGrid/ezbob.underwriter.customerGrid.coffee")
                .Add("~/Content/js/underwriter/customersGrid/ezbob.underwriter.customers.js")
                .Add("~/Content/js/Underwriter/customersGrid/ezbob.underwriter.ProfilePopupView.coffee")

                //profile
                .Add("~/Content/js/underwriter/profile/ezbob.underwriter.customerFullModel.coffee")
                .Add("~/Content/js/underwriter/profile/ezbob.underwriter.profile.coffee")
                .Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.emaileditview.coffee")
                .Add("~/Content/js/underwriter/profile/leftAndBottomBar/ezbob.underwriter.personInfo.coffee")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.functionsDialog.coffee")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.creditLineDialog.coffee")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.ApproveLoanWithoutAML.coffee")
				.Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.ApproveLoanForWarningStatusCustomer.coffee")
                .Add("~/Content/js/Underwriter/profile/leftAndBottomBar/ezbob.underwriter.controlButtons.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.collectionStatus.coffee")
                .Add("~/Content/js/underwriter/profile/loanHistory/ezbob.underwriter.loanInfo.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.loanHistorys.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.loanHistoryDetail.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.rollover.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.manualPayment.coffee")
                .Add("~/Content/js/Underwriter/profile/loanHistory/ezbob.underwriter.LoanOptions.coffee")
                .Add("~/Content/js/underwriter/profile/summary/ezbob.underwriter.summaryInfo.coffee")
                .Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.marketplaceDetails.js")
                .Add("~/Content/js/underwriter/profile/marketplaces/ezbob.underwriter.marketplaces.coffee")
                .Add("~/Content/js/underwriter/profile/creditBureau/ezbob.underwriter.experianInfo.js")
                .Add("~/Content/js/underwriter/profile/creditBureau/ezbob.underwriter.idHubCustomAddress.js")
                .Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.addPayPointCardView.coffee")
                .Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.AddBankAccount.coffee")
                .Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.payPalAccountDetails.coffee")
                .Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.bankAccountDetails.coffee")
                .Add("~/Content/js/underwriter/profile/paymentAccounts/ezbob.underwriter.paymentAccounts.coffee")
                .Add("~/Content/js/Underwriter/profile/alerts/ezbob.underwriter.AlertDocsView.coffee")
                .Add("~/Content/js/Underwriter/profile/medalCalculator/ezbob.underwriter.medalCalculations.js")
                .Add("~/Content/js/Underwriter/profile/customerInfo/ezbob.underwriter.crosscheck.js")
                .Add("~/Content/js/Underwriter/profile/messages/ezbob.undewriter.messages.js")
                .Add("~/Content/js/Underwriter/profile/companyScore/ezbob.underwriter.companyScore.js")
                .Add("~/Content/js/Underwriter/profile/APIChecksLog/ezbob.underwriter.apiChecksLog.js")
                .Add("~/Content/js/Underwriter/profile/CustomerRelations/ezbob.underwriter.customerRelations.js")
                .Add("~/Content/js/underwriter/profile/CustomerRelations/ezbob.underwriter.AddCustomerRelationsEntry.coffee")

                //----
                .Add("~/Content/js/Underwriter/editLoan/loanModel.coffee")
                .Add("~/Content/js/Underwriter/editLoan/installmentEditor.coffee")
                .Add("~/Content/js/Underwriter/editLoan/editFee.coffee")
                .Add("~/Content/js/Underwriter/editLoan/editLoanView.coffee")
                .Add("~/Content/js/Underwriter/CAIS/ezbob.underwriter.CAIS.caisManage.coffee")
                
				.Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")
				.Add("~/Content/js/controls/ezbob.LoanScheduleView.coffee")
				.Add("~/Content/js/controls/ezbob.simpleValueEditDlg.js")

                .MvcRender("~/Content/js/min/underwriter_#.js");
        }

        public static MvcHtmlString RenderStoreInfoJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/Wizard/shops/ezbob.shopbutton.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbuttonlist.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopsbase.coffee")
                .Add("~/Content/js/Wizard/ezbob.ct.bindShopToCT.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ebay.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.amazon.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ekm.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.freeagent.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.sage.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.cg.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.paypoint.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.yodlee.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.coffee")
                .MvcRender("~/Content/js/min/storeinfo_#.js");
        }

        public static MvcHtmlString RenderWizardJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/controls/ezbob.notifications.js")
                .Add("~/Content/js/ezbob.customerModel.js")
                .Add("~/Content/js/Wizard/ezbob.wizard.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbutton.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbuttonlist.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopsbase.coffee")
                .Add("~/Content/js/Wizard/ezbob.ct.bindShopToCT.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ebay.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.amazon.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ekm.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.freeagent.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.sage.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.yodlee.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.cg.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.paypoint.coffee")
                .Add("~/Content/js/Wizard/ezbob.wizard.shops.coffee")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.js")
                .Add("~/Content/js/Wizard/ezbob.wizard.shops.coffee")
                .Add("~/Content/js/Wizard/ezbob.wizard.signupstep.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.personalInfoBase.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.limitedInformation.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.notLimitedInformation.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.personalInfo.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.steps.personinfo.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourInfo.directors.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourinfo.companyTarget.js")
                .Add("~/Content/js/controls/ezbob.address.js")
                .Add("~/Content/js/Wizard/ezbob.signupwizard.js")
                .Add("~/Content/js/Wizard/yourInfo/ezbob.yourIno.consent.agreement.coffee")
                .MvcRender("~/Content/js/min/wizard_#.js");
        }

        public static MvcHtmlString RenderAccountsJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/Wizard/shops/ezbob.shopbutton.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbuttonlist.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopsbase.coffee")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.js")
                .MvcRender("~/Content/js/min/account_#.js");
        }

        public static MvcHtmlString RenderProfileJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/controls/ezbob.address.js")
                .Add("~/Content/js/controls/ezbob.notifications.js")
                .Add("~/Content/js/controls/ezbob.livechat.livechatrouter.js")
                .Add("~/Content/js/controls/ezbob.LoanScheduleView.coffee")
                .Add("~/Content/js/Profile/ezbob.profile.payEarly.js")
                .Add("~/Content/js/Profile/ezbob.profile.getCash.js")
                .Add("~/Content/js/Profile/ezbob.profile.signWidget.js")
                .Add("~/Content/js/ezbob.customerModel.js")
                .Add("~/Content/js/Profile/ezbob.profile.smallNotification.js")
                .Add("~/Content/js/Wizard/ezbob.wizard.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbutton.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopbuttonlist.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shopsbase.coffee")
                .Add("~/Content/js/Wizard/ezbob.ct.bindShopToCT.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ebay.js")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.amazon.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.ekm.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.freeagent.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.sage.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.yodlee.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.cg.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.paypoint.coffee")
                .Add("~/Content/js/Wizard/shops/ezbob.shops.coffee")
                .Add("~/Content/js/Wizard/ezbob.wizard.shops.coffee")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js")
                .Add("~/Content/js/Wizard/accounts/ezbob.accounts.js")
                .Add("~/Content/js/Wizard/ezbob.signupwizard.js")
                .Add("~/Content/js/Profile/ApplyForLoan/ezbob.profile.PayPointCardSelectView.coffee")
                .Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanModel.coffee")
                .Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanView.coffee")
                .Add("~/Content/js/Profile/ApplyForLoan/ezbob.accounts.bank.js")
                .Add("~/Content/js/Profile/ApplyForLoan/ApplyForLoanTopView.coffee")
                .Add("~/Content/js/Profile/ezbob.profile.profileView.js")
                .Add("~/Content/js/Profile/PayEarly/makeEarlyPaymentModel.coffee")
                .Add("~/Content/js/Profile/PayEarly/makeEarlyPaymentView.coffee")
                .Add("~/Content/js/Profile/ezbob.profile.accountActivity.js")
                .Add("~/Content/js/Profile/ezbob.profile.loanDetails.js")
                .Add("~/Content/js/Profile/ezbob.profile.paymentAccounts.js")
                .Add("~/Content/js/Profile/ezbob.profile.Stores.js")
                .Add("~/Content/js/Profile/ezbob.profile.AccountSummary.js")
                .Add("~/Content/js/Profile/ezbob.profile.yourInfo.coffee")
                .Add("~/Content/js/Profile/ezbob.profile.paypointSchedule.js")
                .Add("~/Content/js/Profile/Settings/ezbob.profile.settingsMain.js")
                .Add("~/Content/js/Profile/Settings/ezbob.profile.settingsMaster.js")
                .Add("~/Content/js/Profile/Settings/ezbob.profile.settingsPassword.js")
                .Add("~/Content/js/Profile/Settings/ezbob.profile.settingsQuestion.js")
                .Add("~/Content/js/Profile/ezbob.profile.Agreements.coffee")
                .MvcRender("~/Content/js/min/profile_#.js");
        }

        public static MvcHtmlString RenderLoginJs()
        {
            return Bundle.JavaScript()
                .Add("~/Content/js/login/ezbob.login.view.coffee")
                .MvcRender("~/Content/js/min/profile_#.js");
        }
    }
}
namespace ConfigManager {
	using System;

	public partial class CurrentValues {

		public virtual VariableValue CAISPath {
			get { return this[Variables.CAISPath]; } // get
		} // CAISPath

		public virtual VariableValue CAISPath2 {
			get { return this[Variables.CAISPath2]; } // get
		} // CAISPath2

		public virtual VariableValue ChannelGrabberCycleCount {
			get { return this[Variables.ChannelGrabberCycleCount]; } // get
		} // ChannelGrabberCycleCount

		public ChannelGrabberRejectPolicy ChannelGrabberRejectPolicy {
			get {
				ChannelGrabberRejectPolicy cgrp = ChannelGrabberRejectPolicy.Never;
				Enum.TryParse(this[Variables.ChannelGrabberRejectPolicy], true, out cgrp);
				return cgrp;
			} // get
		} // ChannelGrabberRejectPolicy

		public virtual VariableValue ChannelGrabberServiceUrl {
			get { return this[Variables.ChannelGrabberServiceUrl]; } // get
		} // ChannelGrabberServiceUrl

		public virtual VariableValue ChannelGrabberSleepTime {
			get { return this[Variables.ChannelGrabberSleepTime]; } // get
		} // ChannelGrabberSleepTime

		public virtual VariableValue CollectionPeriod1 {
			get { return this[Variables.CollectionPeriod1]; } // get
		} // CollectionPeriod1

		public virtual VariableValue CollectionPeriod2 {
			get { return this[Variables.CollectionPeriod2]; } // get
		} // CollectionPeriod2

		public virtual VariableValue CollectionPeriod3 {
			get { return this[Variables.CollectionPeriod3]; } // get
		} // CollectionPeriod3

		public virtual VariableValue CompanyFilesSavePath {
			get { return this[Variables.CompanyFilesSavePath]; } // get
		} // CompanyFilesSavePath

		public virtual VariableValue CompanyScoreNonLimitedParserConfiguration {
			get { return this[Variables.CompanyScoreNonLimitedParserConfiguration]; } // get
		} // CompanyScoreNonLimitedParserConfiguration

		public virtual VariableValue CompanyScoreParserConfiguration {
			get { return this[Variables.CompanyScoreParserConfiguration]; } // get
		} // CompanyScoreParserConfiguration

		public virtual VariableValue ConnectionPoolMaxSize {
			get { return this[Variables.ConnectionPoolMaxSize]; } // get
		} // ConnectionPoolMaxSize

		public virtual VariableValue ConnectionPoolReuseCount {
			get { return this[Variables.ConnectionPoolReuseCount]; } // get
		} // ConnectionPoolReuseCount

		public virtual VariableValue CustomerSite {
			get { return this[Variables.CustomerSite]; } // get
		} // CustomerSite

		public virtual VariableValue CustomerStateRefreshInterval {
			get { return this[Variables.CustomerStateRefreshInterval]; } // get
		} // CustomerStateRefreshInterval

		public virtual VariableValue CustomerAnalyticsDefaultHistoryYears {
			get { return this[Variables.CustomerAnalyticsDefaultHistoryYears]; } // get
		} // CustomerAnalyticsDefaultHistoryYears

		public virtual VariableValue DefaultFeedbackValue {
			get { return this[Variables.DefaultFeedbackValue]; } // get
		} // DefaultFeedbackValue

		public virtual VariableValue DirectorDetailsNonLimitedParserConfiguration {
			get { return this[Variables.DirectorDetailsNonLimitedParserConfiguration]; } // get
		} // DirectorDetailsNonLimitedParserConfiguration

		public virtual VariableValue DirectorDetailsParserConfiguration {
			get { return this[Variables.DirectorDetailsParserConfiguration]; } // get
		} // DirectorDetailsParserConfiguration

		public virtual VariableValue DirectorInfoNonLimitedParserConfiguration {
			get { return this[Variables.DirectorInfoNonLimitedParserConfiguration]; } // get
		} // DirectorInfoNonLimitedParserConfiguration

		public virtual VariableValue DirectorInfoParserConfiguration {
			get { return this[Variables.DirectorInfoParserConfiguration]; } // get
		} // DirectorInfoParserConfiguration

		public virtual VariableValue DisplayEarnedPoints {
			get { return this[Variables.DisplayEarnedPoints]; } // get
		} // DisplayEarnedPoints

		public virtual VariableValue EchoSignApiKey {
			get { return this[Variables.EchoSignApiKey]; } // get
		} // EchoSignApiKey

		public virtual VariableValue EchoSignDeadline {
			get { return this[Variables.EchoSignDeadline]; } // get
		} // EchoSignDeadline

		public virtual VariableValue EchoSignEnabledCustomer {
			get { return this[Variables.EchoSignEnabledCustomer]; } // get
		} // EchoSignEnabledCustomer

		public virtual VariableValue EchoSignEnabledUnderwriter {
			get { return this[Variables.EchoSignEnabledUnderwriter]; } // get
		} // EchoSignEnabledUnderwriter

		public virtual VariableValue EchoSignReminder {
			get { return this[Variables.EchoSignReminder]; } // get
		} // EchoSignReminder

		public virtual VariableValue EchoSignUrl {
			get { return this[Variables.EchoSignUrl]; } // get
		} // EchoSignUrl

		public virtual VariableValue EnableAutomaticApproval {
			get { return this[Variables.EnableAutomaticApproval]; } // get
		} // EnableAutomaticApproval

		public virtual VariableValue EnableAutomaticReApproval {
			get { return this[Variables.EnableAutomaticReApproval]; } // get
		} // EnableAutomaticReApproval

		public virtual VariableValue EnableAutomaticRejection {
			get { return this[Variables.EnableAutomaticRejection]; } // get
		} // EnableAutomaticRejection

		public virtual VariableValue EnableAutomaticReRejection {
			get { return this[Variables.EnableAutomaticReRejection]; } // get
		} // EnableAutomaticReRejection

		public virtual VariableValue Environment {
			get { return this[Variables.Environment]; } // get
		} // Environment

		public virtual VariableValue ExperianAuthTokenService {
			get { return this[Variables.ExperianAuthTokenService]; } // get
		} // ExperianAuthTokenService

		public virtual VariableValue ExperianAuthTokenServiceIdHub {
			get { return this[Variables.ExperianAuthTokenServiceIdHub]; } // get
		} // ExperianAuthTokenServiceIdHub

		public virtual VariableValue ExperianCertificateThumb {
			get { return this[Variables.ExperianCertificateThumb]; } // get
		} // ExperianCertificateThumb

		public virtual VariableValue ExperianESeriesUrl {
			get { return this[Variables.ExperianESeriesUrl]; } // get
		} // ExperianESeriesUrl

		public virtual VariableValue ExperianIdHubService {
			get { return this[Variables.ExperianIdHubService]; } // get
		} // ExperianIdHubService

		public virtual VariableValue ExperianInteractiveMode {
			get { return this[Variables.ExperianInteractiveMode]; } // get
		} // ExperianInteractiveMode

		public virtual VariableValue ExperianInteractiveService {
			get { return this[Variables.ExperianInteractiveService]; } // get
		} // ExperianInteractiveService

		public virtual VariableValue ExperianSecureFtpHostName {
			get { return this[Variables.ExperianSecureFtpHostName]; } // get
		} // ExperianSecureFtpHostName

		public virtual VariableValue ExperianSecureFtpUserName {
			get { return this[Variables.ExperianSecureFtpUserName]; } // get
		} // ExperianSecureFtpUserName

		public virtual VariableValue ExperianSecureFtpUserPassword {
			get { return this[Variables.ExperianSecureFtpUserPassword]; } // get
		} // ExperianSecureFtpUserPassword

		public virtual VariableValue ExperianUIdCertificateThumb {
			get { return this[Variables.ExperianUIdCertificateThumb]; } // get
		} // ExperianUIdCertificateThumb

		public virtual VariableValue EzbobMailCc {
			get { return this[Variables.EzbobMailCc]; } // get
		} // EzbobMailCc

		public virtual VariableValue EzbobMailTo {
			get { return this[Variables.EzbobMailTo]; } // get
		} // EzbobMailTo

		public virtual VariableValue EzServiceUpdateConfiguration {
			get { return this[Variables.EzServiceUpdateConfiguration]; } // get
		} // EzServiceUpdateConfiguration

		public virtual VariableValue FinancialAccounts_AliasOfJointApplicant {
			get { return this[Variables.FinancialAccounts_AliasOfJointApplicant]; } // get
		} // FinancialAccounts_AliasOfJointApplicant

		public virtual VariableValue FinancialAccounts_AliasOfMainApplicant {
			get { return this[Variables.FinancialAccounts_AliasOfMainApplicant]; } // get
		} // FinancialAccounts_AliasOfMainApplicant

		public virtual VariableValue FinancialAccounts_AssociationOfJointApplicant {
			get { return this[Variables.FinancialAccounts_AssociationOfJointApplicant]; } // get
		} // FinancialAccounts_AssociationOfJointApplicant

		public virtual VariableValue FinancialAccounts_AssociationOfMainApplicant {
			get { return this[Variables.FinancialAccounts_AssociationOfMainApplicant]; } // get
		} // FinancialAccounts_AssociationOfMainApplicant

		public virtual VariableValue FinancialAccounts_JointApplicant {
			get { return this[Variables.FinancialAccounts_JointApplicant]; } // get
		} // FinancialAccounts_JointApplicant

		public virtual VariableValue FinancialAccounts_MainApplicant {
			get { return this[Variables.FinancialAccounts_MainApplicant]; } // get
		} // FinancialAccounts_MainApplicant

		public virtual VariableValue FinancialAccounts_No_Match {
			get { return this[Variables.FinancialAccounts_No_Match]; } // get
		} // FinancialAccounts_No_Match

		public virtual VariableValue FinancialAccounts_Spare {
			get { return this[Variables.FinancialAccounts_Spare]; } // get
		} // FinancialAccounts_Spare

		public virtual VariableValue FinishWizardForApproved {
			get { return this[Variables.FinishWizardForApproved]; } // get
		} // FinishWizardForApproved

		public virtual VariableValue FirstOfMonthEnableCustomerMail {
			get { return this[Variables.FirstOfMonthEnableCustomerMail]; } // get
		} // FirstOfMonthEnableCustomerMail

		public virtual VariableValue FirstOfMonthStatusMailCopyTo {
			get { return this[Variables.FirstOfMonthStatusMailCopyTo]; } // get
		} // FirstOfMonthStatusMailCopyTo

		public virtual VariableValue FirstOfMonthStatusMailEnabled {
			get { return this[Variables.FirstOfMonthStatusMailEnabled]; } // get
		} // FirstOfMonthStatusMailEnabled

		public virtual VariableValue FirstOfMonthStatusMailMandrillTemplateName {
			get { return this[Variables.FirstOfMonthStatusMailMandrillTemplateName]; } // get
		} // FirstOfMonthStatusMailMandrillTemplateName

		public virtual VariableValue FreeAgentCompanyRequest {
			get { return this[Variables.FreeAgentCompanyRequest]; } // get
		} // FreeAgentCompanyRequest

		public virtual VariableValue FreeAgentExpensesRequest {
			get { return this[Variables.FreeAgentExpensesRequest]; } // get
		} // FreeAgentExpensesRequest

		public virtual VariableValue FreeAgentExpensesRequestDatePart {
			get { return this[Variables.FreeAgentExpensesRequestDatePart]; } // get
		} // FreeAgentExpensesRequestDatePart

		public virtual VariableValue FreeAgentInvoicesRequest {
			get { return this[Variables.FreeAgentInvoicesRequest]; } // get
		} // FreeAgentInvoicesRequest

		public virtual VariableValue FreeAgentInvoicesRequestMonthPart {
			get { return this[Variables.FreeAgentInvoicesRequestMonthPart]; } // get
		} // FreeAgentInvoicesRequestMonthPart

		public virtual VariableValue FreeAgentOAuthAuthorizationEndpoint {
			get { return this[Variables.FreeAgentOAuthAuthorizationEndpoint]; } // get
		} // FreeAgentOAuthAuthorizationEndpoint

		public virtual VariableValue FreeAgentOAuthIdentifier {
			get { return this[Variables.FreeAgentOAuthIdentifier]; } // get
		} // FreeAgentOAuthIdentifier

		public virtual VariableValue FreeAgentOAuthSecret {
			get { return this[Variables.FreeAgentOAuthSecret]; } // get
		} // FreeAgentOAuthSecret

		public virtual VariableValue FreeAgentOAuthTokenEndpoint {
			get { return this[Variables.FreeAgentOAuthTokenEndpoint]; } // get
		} // FreeAgentOAuthTokenEndpoint

		public virtual VariableValue FreeAgentUsersRequest {
			get { return this[Variables.FreeAgentUsersRequest]; } // get
		} // FreeAgentUsersRequest

	} // class CurrentValues
} // namespace

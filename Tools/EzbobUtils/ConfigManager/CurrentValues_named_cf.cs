namespace ConfigManager {
	using System;

	public partial class CurrentValues {
		#region property CAISPath

		public virtual VariableValue CAISPath {
			get { return this[Variables.CAISPath]; } // get
		} // CAISPath

		#endregion property CAISPath

		#region property CAISPath2

		public virtual VariableValue CAISPath2 {
			get { return this[Variables.CAISPath2]; } // get
		} // CAISPath2

		#endregion property CAISPath2

		#region property ChannelGrabberCycleCount

		public virtual VariableValue ChannelGrabberCycleCount {
			get { return this[Variables.ChannelGrabberCycleCount]; } // get
		} // ChannelGrabberCycleCount

		#endregion property ChannelGrabberCycleCount

		#region property ChannelGrabberRejectPolicy

		public ChannelGrabberRejectPolicy ChannelGrabberRejectPolicy {
			get {
				ChannelGrabberRejectPolicy cgrp = ChannelGrabberRejectPolicy.Never;
				Enum.TryParse(this[Variables.ChannelGrabberRejectPolicy], true, out cgrp);
				return cgrp;
			} // get
		} // ChannelGrabberRejectPolicy

		#endregion property ChannelGrabberRejectPolicy

		#region property ChannelGrabberServiceUrl

		public virtual VariableValue ChannelGrabberServiceUrl {
			get { return this[Variables.ChannelGrabberServiceUrl]; } // get
		} // ChannelGrabberServiceUrl

		#endregion property ChannelGrabberServiceUrl

		#region property ChannelGrabberSleepTime

		public virtual VariableValue ChannelGrabberSleepTime {
			get { return this[Variables.ChannelGrabberSleepTime]; } // get
		} // ChannelGrabberSleepTime

		#endregion property ChannelGrabberSleepTime

		#region property CollectionPeriod1

		public virtual VariableValue CollectionPeriod1 {
			get { return this[Variables.CollectionPeriod1]; } // get
		} // CollectionPeriod1

		#endregion property CollectionPeriod1

		#region property CollectionPeriod2

		public virtual VariableValue CollectionPeriod2 {
			get { return this[Variables.CollectionPeriod2]; } // get
		} // CollectionPeriod2

		#endregion property CollectionPeriod2

		#region property CollectionPeriod3

		public virtual VariableValue CollectionPeriod3 {
			get { return this[Variables.CollectionPeriod3]; } // get
		} // CollectionPeriod3

		#endregion property CollectionPeriod3

		#region property CompanyFilesSavePath

		public virtual VariableValue CompanyFilesSavePath {
			get { return this[Variables.CompanyFilesSavePath]; } // get
		} // CompanyFilesSavePath

		#endregion property CompanyFilesSavePath

		#region property CompanyScoreNonLimitedParserConfiguration

		public virtual VariableValue CompanyScoreNonLimitedParserConfiguration {
			get { return this[Variables.CompanyScoreNonLimitedParserConfiguration]; } // get
		} // CompanyScoreNonLimitedParserConfiguration

		#endregion property CompanyScoreNonLimitedParserConfiguration

		#region property CompanyScoreParserConfiguration

		public virtual VariableValue CompanyScoreParserConfiguration {
			get { return this[Variables.CompanyScoreParserConfiguration]; } // get
		} // CompanyScoreParserConfiguration

		#endregion property CompanyScoreParserConfiguration

		#region property ConnectionPoolMaxSize

		public virtual VariableValue ConnectionPoolMaxSize {
			get { return this[Variables.ConnectionPoolMaxSize]; } // get
		} // ConnectionPoolMaxSize

		#endregion property ConnectionPoolMaxSize

		#region property ConnectionPoolReuseCount

		public virtual VariableValue ConnectionPoolReuseCount {
			get { return this[Variables.ConnectionPoolReuseCount]; } // get
		} // ConnectionPoolReuseCount

		#endregion property CompanyScoreParserConfiguration

		#region property CustomerSite

		public virtual VariableValue CustomerSite {
			get { return this[Variables.CustomerSite]; } // get
		} // CustomerSite

		#endregion property CustomerSite

		#region property CustomerStateRefreshInterval

		public virtual VariableValue CustomerStateRefreshInterval {
			get { return this[Variables.CustomerStateRefreshInterval]; } // get
		} // CustomerStateRefreshInterval

		#endregion property CustomerStateRefreshInterval

		#region property CustomerAnalyticsDefaultHistoryYears

		public virtual VariableValue CustomerAnalyticsDefaultHistoryYears {
			get { return this[Variables.CustomerAnalyticsDefaultHistoryYears]; } // get
		} // CustomerAnalyticsDefaultHistoryYears

		#endregion property CustomerAnalyticsDefaultHistoryYears

		#region property DefaultFeedbackValue

		public virtual VariableValue DefaultFeedbackValue {
			get { return this[Variables.DefaultFeedbackValue]; } // get
		} // DefaultFeedbackValue

		#endregion property DefaultFeedbackValue

		#region property DirectorDetailsNonLimitedParserConfiguration

		public virtual VariableValue DirectorDetailsNonLimitedParserConfiguration {
			get { return this[Variables.DirectorDetailsNonLimitedParserConfiguration]; } // get
		} // DirectorDetailsNonLimitedParserConfiguration

		#endregion property DirectorDetailsNonLimitedParserConfiguration

		#region property DirectorDetailsParserConfiguration

		public virtual VariableValue DirectorDetailsParserConfiguration {
			get { return this[Variables.DirectorDetailsParserConfiguration]; } // get
		} // DirectorDetailsParserConfiguration

		#endregion property DirectorInfoParserConfiguration

		#region property DirectorInfoNonLimitedParserConfiguration

		public virtual VariableValue DirectorInfoNonLimitedParserConfiguration {
			get { return this[Variables.DirectorInfoNonLimitedParserConfiguration]; } // get
		} // DirectorInfoNonLimitedParserConfiguration

		#endregion property DirectorInfoNonLimitedParserConfiguration

		#region property DirectorInfoParserConfiguration

		public virtual VariableValue DirectorInfoParserConfiguration {
			get { return this[Variables.DirectorInfoParserConfiguration]; } // get
		} // DirectorInfoParserConfiguration

		#endregion property DirectorInfoParserConfiguration

		#region property DisplayEarnedPoints

		public virtual VariableValue DisplayEarnedPoints {
			get { return this[Variables.DisplayEarnedPoints]; } // get
		} // DisplayEarnedPoints

		#endregion property DisplayEarnedPoints

		#region property EchoSignApiKey

		public virtual VariableValue EchoSignApiKey {
			get { return this[Variables.EchoSignApiKey]; } // get
		} // EchoSignApiKey

		#endregion property EchoSignApiKey

		#region property EchoSignDeadline

		public virtual VariableValue EchoSignDeadline {
			get { return this[Variables.EchoSignDeadline]; } // get
		} // EchoSignDeadline

		#endregion property EchoSignDeadline

		#region property EchoSignEnabledCustomer

		public virtual VariableValue EchoSignEnabledCustomer {
			get { return this[Variables.EchoSignEnabledCustomer]; } // get
		} // EchoSignEnabledCustomer

		#endregion property EchoSignEnabledCustomer

		#region property EchoSignEnabledUnderwriter

		public virtual VariableValue EchoSignEnabledUnderwriter {
			get { return this[Variables.EchoSignEnabledUnderwriter]; } // get
		} // EchoSignEnabledUnderwriter

		#endregion property EchoSignEnabledUnderwriter

		#region property EchoSignReminder

		public virtual VariableValue EchoSignReminder {
			get { return this[Variables.EchoSignReminder]; } // get
		} // EchoSignReminder

		#endregion property EchoSignReminder

		#region property EchoSignUrl

		public virtual VariableValue EchoSignUrl {
			get { return this[Variables.EchoSignUrl]; } // get
		} // EchoSignUrl

		#endregion property EchoSignUrl

		#region property EnableAutomaticApproval

		public virtual VariableValue EnableAutomaticApproval {
			get { return this[Variables.EnableAutomaticApproval]; } // get
		} // EnableAutomaticApproval

		#endregion property EnableAutomaticApproval

		#region property EnableAutomaticReApproval

		public virtual VariableValue EnableAutomaticReApproval {
			get { return this[Variables.EnableAutomaticReApproval]; } // get
		} // EnableAutomaticReApproval

		#endregion property EnableAutomaticReApproval

		#region property EnableAutomaticRejection

		public virtual VariableValue EnableAutomaticRejection {
			get { return this[Variables.EnableAutomaticRejection]; } // get
		} // EnableAutomaticRejection

		#endregion property EnableAutomaticRejection

		#region property EnableAutomaticReRejection

		public virtual VariableValue EnableAutomaticReRejection {
			get { return this[Variables.EnableAutomaticReRejection]; } // get
		} // EnableAutomaticReRejection

		#endregion property EnableAutomaticReRejection

		#region property Environment

		public virtual VariableValue Environment {
			get { return this[Variables.Environment]; } // get
		} // Environment

		#endregion property Environment

		#region property ExperianAuthTokenService

		public virtual VariableValue ExperianAuthTokenService {
			get { return this[Variables.ExperianAuthTokenService]; } // get
		} // ExperianAuthTokenService

		#endregion property ExperianAuthTokenService

		#region property ExperianAuthTokenServiceIdHub

		public virtual VariableValue ExperianAuthTokenServiceIdHub {
			get { return this[Variables.ExperianAuthTokenServiceIdHub]; } // get
		} // ExperianAuthTokenServiceIdHub

		#endregion property ExperianAuthTokenServiceIdHub

		#region property ExperianCertificateThumb

		public virtual VariableValue ExperianCertificateThumb {
			get { return this[Variables.ExperianCertificateThumb]; } // get
		} // ExperianCertificateThumb

		#endregion property ExperianCertificateThumb

		#region property ExperianESeriesUrl

		public virtual VariableValue ExperianESeriesUrl {
			get { return this[Variables.ExperianESeriesUrl]; } // get
		} // ExperianESeriesUrl

		#endregion property ExperianESeriesUrl

		#region property ExperianIdHubService

		public virtual VariableValue ExperianIdHubService {
			get { return this[Variables.ExperianIdHubService]; } // get
		} // ExperianIdHubService

		#endregion property ExperianIdHubService

		#region property ExperianInteractiveMode

		public virtual VariableValue ExperianInteractiveMode {
			get { return this[Variables.ExperianInteractiveMode]; } // get
		} // ExperianInteractiveMode

		#endregion property ExperianInteractiveMode

		#region property ExperianInteractiveService

		public virtual VariableValue ExperianInteractiveService {
			get { return this[Variables.ExperianInteractiveService]; } // get
		} // ExperianInteractiveService

		#endregion property ExperianInteractiveService

		#region property ExperianSecureFtpHostName

		public virtual VariableValue ExperianSecureFtpHostName {
			get { return this[Variables.ExperianSecureFtpHostName]; } // get
		} // ExperianSecureFtpHostName

		#endregion property ExperianSecureFtpHostName

		#region property ExperianSecureFtpUserName

		public virtual VariableValue ExperianSecureFtpUserName {
			get { return this[Variables.ExperianSecureFtpUserName]; } // get
		} // ExperianSecureFtpUserName

		#endregion property ExperianSecureFtpUserName

		#region property ExperianSecureFtpUserPassword

		public virtual VariableValue ExperianSecureFtpUserPassword {
			get { return this[Variables.ExperianSecureFtpUserPassword]; } // get
		} // ExperianSecureFtpUserPassword

		#endregion property ExperianSecureFtpUserPassword

		#region property ExperianUIdCertificateThumb

		public virtual VariableValue ExperianUIdCertificateThumb {
			get { return this[Variables.ExperianUIdCertificateThumb]; } // get
		} // ExperianUIdCertificateThumb

		#endregion property ExperianUIdCertificateThumb

		#region property EzbobMailCc

		public virtual VariableValue EzbobMailCc {
			get { return this[Variables.EzbobMailCc]; } // get
		} // EzbobMailCc

		#endregion property EzbobMailCc

		#region property EzbobMailTo

		public virtual VariableValue EzbobMailTo {
			get { return this[Variables.EzbobMailTo]; } // get
		} // EzbobMailTo

		#endregion property EzbobMailTo

		#region property EzServiceUpdateConfiguration

		public virtual VariableValue EzServiceUpdateConfiguration {
			get { return this[Variables.EzServiceUpdateConfiguration]; } // get
		} // EzServiceUpdateConfiguration

		#endregion property EzServiceUpdateConfiguration

		#region property FinancialAccounts_AliasOfJointApplicant

		public virtual VariableValue FinancialAccounts_AliasOfJointApplicant {
			get { return this[Variables.FinancialAccounts_AliasOfJointApplicant]; } // get
		} // FinancialAccounts_AliasOfJointApplicant

		#endregion property FinancialAccounts_AliasOfJointApplicant

		#region property FinancialAccounts_AliasOfMainApplicant

		public virtual VariableValue FinancialAccounts_AliasOfMainApplicant {
			get { return this[Variables.FinancialAccounts_AliasOfMainApplicant]; } // get
		} // FinancialAccounts_AliasOfMainApplicant

		#endregion property FinancialAccounts_AliasOfMainApplicant

		#region property FinancialAccounts_AssociationOfJointApplicant

		public virtual VariableValue FinancialAccounts_AssociationOfJointApplicant {
			get { return this[Variables.FinancialAccounts_AssociationOfJointApplicant]; } // get
		} // FinancialAccounts_AssociationOfJointApplicant

		#endregion property FinancialAccounts_AssociationOfJointApplicant

		#region property FinancialAccounts_AssociationOfMainApplicant

		public virtual VariableValue FinancialAccounts_AssociationOfMainApplicant {
			get { return this[Variables.FinancialAccounts_AssociationOfMainApplicant]; } // get
		} // FinancialAccounts_AssociationOfMainApplicant

		#endregion property FinancialAccounts_AssociationOfMainApplicant

		#region property FinancialAccounts_JointApplicant

		public virtual VariableValue FinancialAccounts_JointApplicant {
			get { return this[Variables.FinancialAccounts_JointApplicant]; } // get
		} // FinancialAccounts_JointApplicant

		#endregion property FinancialAccounts_JointApplicant

		#region property FinancialAccounts_MainApplicant

		public virtual VariableValue FinancialAccounts_MainApplicant {
			get { return this[Variables.FinancialAccounts_MainApplicant]; } // get
		} // FinancialAccounts_MainApplicant

		#endregion property FinancialAccounts_MainApplicant

		#region property FinancialAccounts_No_Match

		public virtual VariableValue FinancialAccounts_No_Match {
			get { return this[Variables.FinancialAccounts_No_Match]; } // get
		} // FinancialAccounts_No_Match

		#endregion property FinancialAccounts_No_Match

		#region property FinancialAccounts_Spare

		public virtual VariableValue FinancialAccounts_Spare {
			get { return this[Variables.FinancialAccounts_Spare]; } // get
		} // FinancialAccounts_Spare

		#endregion property FinancialAccounts_No_Match

		#region property FinishWizardForApproved

		public virtual VariableValue FinishWizardForApproved {
			get { return this[Variables.FinishWizardForApproved]; } // get
		} // FinishWizardForApproved

		#endregion property FinishWizardForApproved

		#region property FirstOfMonthEnableCustomerMail

		public virtual VariableValue FirstOfMonthEnableCustomerMail {
			get { return this[Variables.FirstOfMonthEnableCustomerMail]; } // get
		} // FirstOfMonthEnableCustomerMail

		#endregion property FirstOfMonthEnableCustomerMail

		#region property FirstOfMonthStatusMailCopyTo

		public virtual VariableValue FirstOfMonthStatusMailCopyTo {
			get { return this[Variables.FirstOfMonthStatusMailCopyTo]; } // get
		} // FirstOfMonthStatusMailCopyTo

		#endregion property FirstOfMonthStatusMailCopyTo

		#region property FirstOfMonthStatusMailEnabled

		public virtual VariableValue FirstOfMonthStatusMailEnabled {
			get { return this[Variables.FirstOfMonthStatusMailEnabled]; } // get
		} // FirstOfMonthStatusMailEnabled

		#endregion property FirstOfMonthStatusMailEnabled

		#region property FirstOfMonthStatusMailMandrillTemplateName

		public virtual VariableValue FirstOfMonthStatusMailMandrillTemplateName {
			get { return this[Variables.FirstOfMonthStatusMailMandrillTemplateName]; } // get
		} // FirstOfMonthStatusMailMandrillTemplateName

		#endregion property FirstOfMonthStatusMailMandrillTemplateName

		#region property FreeAgentCompanyRequest

		public virtual VariableValue FreeAgentCompanyRequest {
			get { return this[Variables.FreeAgentCompanyRequest]; } // get
		} // FreeAgentCompanyRequest

		#endregion property FreeAgentCompanyRequest

		#region property FreeAgentExpensesRequest

		public virtual VariableValue FreeAgentExpensesRequest {
			get { return this[Variables.FreeAgentExpensesRequest]; } // get
		} // FreeAgentExpensesRequest

		#endregion property FreeAgentExpensesRequest

		#region property FreeAgentExpensesRequestDatePart

		public virtual VariableValue FreeAgentExpensesRequestDatePart {
			get { return this[Variables.FreeAgentExpensesRequestDatePart]; } // get
		} // FreeAgentExpensesRequestDatePart

		#endregion property FreeAgentExpensesRequestDatePart

		#region property FreeAgentInvoicesRequest

		public virtual VariableValue FreeAgentInvoicesRequest {
			get { return this[Variables.FreeAgentInvoicesRequest]; } // get
		} // FreeAgentInvoicesRequest

		#endregion property FreeAgentInvoicesRequest

		#region property FreeAgentInvoicesRequestMonthPart

		public virtual VariableValue FreeAgentInvoicesRequestMonthPart {
			get { return this[Variables.FreeAgentInvoicesRequestMonthPart]; } // get
		} // FreeAgentInvoicesRequestMonthPart

		#endregion property FreeAgentInvoicesRequestMonthPart

		#region property FreeAgentOAuthAuthorizationEndpoint

		public virtual VariableValue FreeAgentOAuthAuthorizationEndpoint {
			get { return this[Variables.FreeAgentOAuthAuthorizationEndpoint]; } // get
		} // FreeAgentOAuthAuthorizationEndpoint

		#endregion property FreeAgentOAuthAuthorizationEndpoint

		#region property FreeAgentOAuthIdentifier

		public virtual VariableValue FreeAgentOAuthIdentifier {
			get { return this[Variables.FreeAgentOAuthIdentifier]; } // get
		} // FreeAgentOAuthIdentifier

		#endregion property FreeAgentOAuthIdentifier

		#region property FreeAgentOAuthSecret

		public virtual VariableValue FreeAgentOAuthSecret {
			get { return this[Variables.FreeAgentOAuthSecret]; } // get
		} // FreeAgentOAuthSecret

		#endregion property FreeAgentOAuthSecret

		#region property FreeAgentOAuthTokenEndpoint

		public virtual VariableValue FreeAgentOAuthTokenEndpoint {
			get { return this[Variables.FreeAgentOAuthTokenEndpoint]; } // get
		} // FreeAgentOAuthTokenEndpoint

		#endregion property FreeAgentOAuthTokenEndpoint

		#region property FreeAgentUsersRequest

		public virtual VariableValue FreeAgentUsersRequest {
			get { return this[Variables.FreeAgentUsersRequest]; } // get
		} // FreeAgentUsersRequest

		#endregion property FreeAgentUsersRequest
	} // class CurrentValues
} // namespace

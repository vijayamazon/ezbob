﻿namespace ConfigManager {
	using System;

	public partial class CurrentValues {
		public virtual VariableValue CAISPath {
			get { return this[Variables.CAISPath]; }
		}

		public virtual VariableValue CAISPath2 {
			get { return this[Variables.CAISPath2]; }
		}

		public virtual VariableValue CaptchaMode {
			get { return this[Variables.CaptchaMode]; }
		}

		public virtual VariableValue ChannelGrabberCycleCount {
			get { return this[Variables.ChannelGrabberCycleCount]; }
		}

		public ChannelGrabberRejectPolicy ChannelGrabberRejectPolicy {
			get {
				ChannelGrabberRejectPolicy cgrp = ChannelGrabberRejectPolicy.Never;
				Enum.TryParse(this[Variables.ChannelGrabberRejectPolicy], true, out cgrp);
				return cgrp;
			}
		}

		public virtual VariableValue ChannelGrabberServiceUrl {
			get { return this[Variables.ChannelGrabberServiceUrl]; }
		}

		public virtual VariableValue ChannelGrabberSleepTime {
			get { return this[Variables.ChannelGrabberSleepTime]; }
		}

		public virtual VariableValue CheckStoreUniqueness {
			get { return this[Variables.CheckStoreUniqueness]; }
		}

		public virtual VariableValue CollectionPeriod1 {
			get { return this[Variables.CollectionPeriod1]; }
		}

		public virtual VariableValue CollectionPeriod2 {
			get { return this[Variables.CollectionPeriod2]; }
		}

		public virtual VariableValue CollectionPeriod3 {
			get { return this[Variables.CollectionPeriod3]; }
		}

		public virtual VariableValue CompanyCaisLateAlertLongMonths {
			get { return this[Variables.CompanyCaisLateAlertLongMonths]; }
		}

		public virtual VariableValue CompanyCaisLateAlertShortMonths {
			get { return this[Variables.CompanyCaisLateAlertShortMonths]; }
		}

		public virtual VariableValue CompanyCaisLateAlertShortPeriodThreshold {
			get { return this[Variables.CompanyCaisLateAlertShortPeriodThreshold]; }
		}

		public virtual VariableValue CompanyFilesSavePath {
			get { return this[Variables.CompanyFilesSavePath]; }
		}

		public virtual VariableValue CompanyScoreNonLimitedParserConfiguration {
			get { return this[Variables.CompanyScoreNonLimitedParserConfiguration]; }
		}

		public virtual VariableValue CompanyScoreParserConfiguration {
			get { return this[Variables.CompanyScoreParserConfiguration]; }
		}

		public virtual VariableValue ConnectionPoolMaxSize {
			get { return this[Variables.ConnectionPoolMaxSize]; }
		}

		public virtual VariableValue ConnectionPoolReuseCount {
			get { return this[Variables.ConnectionPoolReuseCount]; }
		}

		public virtual VariableValue CustomerAnalyticsDefaultHistoryYears {
			get { return this[Variables.CustomerAnalyticsDefaultHistoryYears]; }
		}

		public virtual VariableValue CustomerSite {
			get { return this[Variables.CustomerSite]; }
		}

		public virtual VariableValue CustomerStateRefreshInterval {
			get { return this[Variables.CustomerStateRefreshInterval]; }
		}

		public virtual VariableValue DefaultFeedbackValue {
			get { return this[Variables.DefaultFeedbackValue]; }
		}

		public virtual VariableValue DirectorDetailsNonLimitedParserConfiguration {
			get { return this[Variables.DirectorDetailsNonLimitedParserConfiguration]; }
		}

		public virtual VariableValue DirectorDetailsParserConfiguration {
			get { return this[Variables.DirectorDetailsParserConfiguration]; }
		}

		public virtual VariableValue DirectorInfoNonLimitedParserConfiguration {
			get { return this[Variables.DirectorInfoNonLimitedParserConfiguration]; }
		}

		public virtual VariableValue DirectorInfoParserConfiguration {
			get { return this[Variables.DirectorInfoParserConfiguration]; }
		}

		public virtual VariableValue DisplayEarnedPoints {
			get { return this[Variables.DisplayEarnedPoints]; }
		}

		public virtual VariableValue DummyAddressSearchResult {
			get { return this[Variables.DummyAddressSearchResult]; }
		}

		public virtual VariableValue DummyPostcodeSearchResult {
			get { return this[Variables.DummyPostcodeSearchResult]; }
		}

		public virtual VariableValue EbayAppId {
			get { return this[Variables.EbayAppId]; }
		}

		public virtual VariableValue EbayCertId {
			get { return this[Variables.EbayCertId]; }
		}

		public virtual VariableValue EbayDevId {
			get { return this[Variables.EbayDevId]; }
		}

		public virtual VariableValue EbayPixelEnabled {
			get { return this[Variables.EbayPixelEnabled]; }
		}

		public virtual VariableValue EbayRuName {
			get { return this[Variables.EbayRuName]; }
		}

		public virtual VariableValue EbayServiceType {
			get { return this[Variables.EbayServiceType]; }
		}

		public virtual VariableValue EchoSignApiKey {
			get { return this[Variables.EchoSignApiKey]; }
		}

		public virtual VariableValue EchoSignDeadline {
			get { return this[Variables.EchoSignDeadline]; }
		}

		public virtual VariableValue EchoSignEnabledCustomer {
			get { return this[Variables.EchoSignEnabledCustomer]; }
		}

		public virtual VariableValue EchoSignEnabledUnderwriter {
			get { return this[Variables.EchoSignEnabledUnderwriter]; }
		}

		public virtual VariableValue EchoSignReminder {
			get { return this[Variables.EchoSignReminder]; }
		}

		public virtual VariableValue EchoSignUrl {
			get { return this[Variables.EchoSignUrl]; }
		}

		public virtual VariableValue EnableAutomaticApproval {
			get { return this[Variables.EnableAutomaticApproval]; }
		}

		public virtual VariableValue EnableAutomaticReApproval {
			get { return this[Variables.EnableAutomaticReApproval]; }
		}

		public virtual VariableValue EnableAutomaticRejection {
			get { return this[Variables.EnableAutomaticRejection]; }
		}

		public virtual VariableValue EnableAutomaticReRejection {
			get { return this[Variables.EnableAutomaticReRejection]; }
		}

		public virtual VariableValue Environment {
			get { return this[Variables.Environment]; }
		}

		public virtual VariableValue EverlineRefinanceEmailReciever {
			get { return this[Variables.EverlineRefinanceEmailReciever]; }
		}
		public virtual VariableValue ExperianAuthTokenService {
			get { return this[Variables.ExperianAuthTokenService]; }
		}

		public virtual VariableValue ExperianAuthTokenServiceIdHub {
			get { return this[Variables.ExperianAuthTokenServiceIdHub]; }
		}

		public virtual VariableValue ExperianCertificateThumb {
			get { return this[Variables.ExperianCertificateThumb]; }
		}

		public virtual VariableValue ExperianESeriesUrl {
			get { return this[Variables.ExperianESeriesUrl]; }
		}

		public virtual VariableValue ExperianIdHubService {
			get { return this[Variables.ExperianIdHubService]; }
		}

		public virtual VariableValue ExperianInteractiveMode {
			get { return this[Variables.ExperianInteractiveMode]; }
		}

		public virtual VariableValue ExperianInteractiveService {
			get { return this[Variables.ExperianInteractiveService]; }
		}

		public virtual VariableValue ExperianSecureFtpHostName {
			get { return this[Variables.ExperianSecureFtpHostName]; }
		}

		public virtual VariableValue ExperianSecureFtpUserName {
			get { return this[Variables.ExperianSecureFtpUserName]; }
		}

		public virtual VariableValue ExperianSecureFtpUserPassword {
			get { return this[Variables.ExperianSecureFtpUserPassword]; }
		}

		public virtual VariableValue ExperianUIdCertificateThumb {
			get { return this[Variables.ExperianUIdCertificateThumb]; }
		}

		public virtual VariableValue EzbobMailCc {
			get { return this[Variables.EzbobMailCc]; }
		}

		public virtual VariableValue EzbobMailTo {
			get { return this[Variables.EzbobMailTo]; }
		}

		public virtual VariableValue EzServiceUpdateConfiguration {
			get { return this[Variables.EzServiceUpdateConfiguration]; }
		}
	}
} // namespace

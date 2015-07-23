namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using API;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Misc;
	using EZBob.DatabaseLib.Repository;
	using Ezbob.Database;
	using Ezbob.Utils;
	using StructureMap;

	public class ApprovedUser : ABrokerMailToo {

		public ApprovedUser(int customerId, decimal loanAmount, int validHours, bool isFirst)
			: base(customerId, true) {
				this.loanAmount = loanAmount;
				this.validHours = validHours;
				this.isFirst = isFirst;
				this.amountInUsd = CalculateLoanAmountInUsd();
		} // constructor

		public override string Name { get { return "Approved User"; } } // Name

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker) {
				SendToCustomer = false;
			}
		}

		public class CashRequestRelevantData : AResultRow {
			public decimal InterestRate { get; set; }
			public int ManualSetupFeeAmount { get; set; }
			public int SystemCalculatedSum { get; set; }
			public int ManagerApprovedSum { get; set; }
		}

		protected override void SetTemplateAndVariables() {
			var cashRequestRelevantData = DB.FillFirst<CashRequestRelevantData>(
				"GetCashRequestData",
				new QueryParameter("@CustomerId", CustomerData.Id));

			
			if (cashRequestRelevantData.ManagerApprovedSum != 0) {
				this.setupFeePercents = (decimal)cashRequestRelevantData.ManualSetupFeeAmount * 100 / cashRequestRelevantData.ManagerApprovedSum;
			}
			else if (cashRequestRelevantData.SystemCalculatedSum != 0) {
				this.setupFeePercents = (decimal)cashRequestRelevantData.ManualSetupFeeAmount * 100 / cashRequestRelevantData.SystemCalculatedSum;
			}
			else {
				this.setupFeePercents = 0;
			}

			this.interestRatePercents = cashRequestRelevantData.InterestRate * 100;
			this.setupFeePercents = MathUtils.Round2DecimalDown(this.setupFeePercents);
			decimal remainingPercentsAfterSetupFee = 100 - this.setupFeePercents;

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LoanAmount", this.loanAmount.ToString("#,#") },
				{ "ValidFor", this.validHours.ToString(CultureInfo.InvariantCulture) },
				{ "AmountInUsd", MathUtils.Round2DecimalDown(this.amountInUsd).ToString("#,#.00") },
				{ "AlibabaId", CustomerData.AlibabaId.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", MathUtils.Round2DecimalDown(this.interestRatePercents).ToString("#,#.00") },
				{ "SetupFee", this.setupFeePercents.ToString("#,#.00") },
				{ "RemainingPercentsAfterSetupFee", remainingPercentsAfterSetupFee.ToString(CultureInfo.InvariantCulture) },
				{ "RefNum", CustomerData.RefNum.ToString(CultureInfo.InvariantCulture) },
				{ "Surname", CustomerData.Surname.ToString(CultureInfo.InvariantCulture) },
				{ "RequestedLoanAmount", CustomerData.RequestedLoanAmount.ToString("#,#") },
				{ "ReportedAnnualTurnover", CustomerData.ReportedAnnualTurnover.ToString("#,#") }
			};

			if (CustomerData.IsAlibaba) {
				TemplateName = "Mandrill - Alibaba - Approval";
			}
			else if (CustomerData.IsCampaign) {
				TemplateName = "Mandrill - Approval Campaign (1st time)";
			} else if (this.isFirst) {
				TemplateName = "Mandrill - Approval (1st time)";
			}
			else {
				TemplateName = "Mandrill - Approval (not 1st time)";
			}
		} // SetTemplateAndVariables

		protected override void ActionAtEnd() {
			if (CustomerData.IsAlibaba) {
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc);
				Log.Info("Sending Alibaba internal approval mail");
				SendCostumeMail("Mandrill - Alibaba - Internal approval email", Variables, new[] { address });
			}

			if (CurrentValues.Instance.SmsApprovedUserEnabled && !CustomerData.IsTest && !ConfigManager.CurrentValues.Instance.SmsTestModeEnabled) {
				string smsTemplate = CurrentValues.Instance.SmsApprovedUserTemplate;

				smsTemplate = smsTemplate
					.Replace("*FirstName*", CustomerData.FirstName)
					.Replace("*LoanAmount*", this.loanAmount.ToString("#,#"))
					.Replace("*ValidFor*", this.validHours.ToString(CultureInfo.InvariantCulture))
					.Replace("*InterestRate*", MathUtils.Round2DecimalDown(this.interestRatePercents).ToString("#,#.00"))
					.Replace("*SetupFee*", this.setupFeePercents.ToString("#,#.00"))
					.Replace("*Origin*", CustomerData.Origin)
					.Replace("*OriginLogin*", CustomerData.OriginSite + "/Customer/Profile")
					.Replace("*OriginPhone*", CustomerData.OriginPhone);
				
				SendSms sendSms = new SendSms(CustomerId, 1, CustomerData.MobilePhone, smsTemplate);
				sendSms.Execute();
			} else {
				Log.Info("Not sending approved user sms to customer {3}, SmsApprovedUserEnabled {0}, is test {1}, SmsTestModeEnabled {2}", 
					(bool)CurrentValues.Instance.SmsApprovedUserEnabled, 
					CustomerData.IsTest, 
					(bool)ConfigManager.CurrentValues.Instance.SmsTestModeEnabled,
					CustomerId);
			}
		}

		private double CalculateLoanAmountInUsd() {
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			double currencyRate = currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			double convertedLoanAmount = (double)this.loanAmount * currencyRate * CurrentValues.Instance.AlibabaCurrencyConversionCoefficient;
			Log.Info("Calculating Alibaba loan amount in USD. CurrencyRate:{0} Coefficient:{1} LoanAmount:{2} ConvertedLoanAmount:{3}", currencyRate, CurrentValues.Instance.AlibabaCurrencyConversionCoefficient, this.loanAmount, convertedLoanAmount);
			return convertedLoanAmount;
		}

		private readonly decimal loanAmount;
		private readonly double amountInUsd;
		private readonly int validHours;
		private readonly bool isFirst;
		private decimal interestRatePercents;
		private decimal setupFeePercents;
	} // class ApprovedUser
} // namespace

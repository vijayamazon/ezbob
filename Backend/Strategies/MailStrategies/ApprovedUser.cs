namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using API;
	using ConfigManager;
	using EZBob.DatabaseLib.Repository;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using StructureMap;

	public class ApprovedUser : ABrokerMailToo {
		#region constructor

		public ApprovedUser(int customerId, decimal nLoanAmount, int nValidHours, bool isFirst, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			m_nLoanAmount = nLoanAmount;
			m_nValidHours = nValidHours;
			m_bIsFirst = isFirst;
			amountInUsd = CalculateLoanAmountInUsd();
		} // constructor

		#endregion constructor

		public override string Name { get { return "Approved User"; } } // Name
		
		protected override void LoadRecipientData()
		{
			base.LoadRecipientData();

			if (CustomerData.IsFilledByBroker)
			{
				SendToCustomer = false;
			}
		}

		public class CashRequestRelevantData : AResultRow
		{
			public decimal InterestRate { get; set; }
			public int ManualSetupFeeAmount { get; set; }
			public int SystemCalculatedSum { get; set; }
			public int ManagerApprovedSum { get; set; }
		}

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables()
		{
			var cashRequestRelevantData = DB.FillFirst<CashRequestRelevantData>(
				"GetCashRequestData",
				new QueryParameter("@CustomerId", CustomerData.Id));

			decimal setupFeePercents;
			if (cashRequestRelevantData.ManagerApprovedSum != 0)
			{
				setupFeePercents = (decimal)cashRequestRelevantData.ManualSetupFeeAmount * 100 / cashRequestRelevantData.ManagerApprovedSum;
			}
			else if (cashRequestRelevantData.SystemCalculatedSum != 0)
			{
				setupFeePercents = (decimal)cashRequestRelevantData.ManualSetupFeeAmount * 100 / cashRequestRelevantData.SystemCalculatedSum;
			}
			else
			{
				setupFeePercents = 0;
			}

			decimal interestRatePercents = cashRequestRelevantData.InterestRate*100;
			setupFeePercents = Round2DecimalDown(setupFeePercents);
			decimal remainingPercentsAfterSetupFee = 100 - setupFeePercents;

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LoanAmount", m_nLoanAmount.ToString(CultureInfo.InvariantCulture) },
				{ "ValidFor", m_nValidHours.ToString(CultureInfo.InvariantCulture) },
				{ "AmountInUsd", Round2DecimalDown(amountInUsd).ToString("#,#.00") },
				{ "AlibabaId", CustomerData.AlibabaId.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", Round2DecimalDown(interestRatePercents).ToString("#,#.00") },
				{ "SetupFee", setupFeePercents.ToString("#,#.00") },
				{ "RemainingPercentsAfterSetupFee", remainingPercentsAfterSetupFee.ToString(CultureInfo.InvariantCulture) },
				{ "RefNum", CustomerData.RefNum.ToString(CultureInfo.InvariantCulture) },
				{ "Surname", CustomerData.Surname.ToString(CultureInfo.InvariantCulture) }
			};

			if (CustomerData.IsAlibaba)
			{
				TemplateName = "Mandrill - Alibaba - Approval";
			}
			else if (CustomerData.IsCampaign)
			{
				TemplateName = "Mandrill - Approval Campaign (1st time)";
			}
			else if (m_bIsFirst)
			{
				TemplateName = "Mandrill - Approval (1st time)";
			}
			else
			{
				TemplateName = "Mandrill - Approval (not 1st time)";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		protected override void ActionAtEnd()
		{
			if (CustomerData.IsAlibaba)
			{
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc);
				Log.Info("Sending Alibaba internal approval mail");
				SendCostumeMail("Mandrill - Alibaba - Internal approval email", Variables, new[] { address });
			}
		}

		private double CalculateLoanAmountInUsd()
		{
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			double currencyRate = currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			double convertedLoanAmount = (double)m_nLoanAmount * currencyRate * CurrentValues.Instance.AlibabaCurrencyConversionCoefficient;
			Log.Info("Calculating Alibaba loan amount in USD. CurrencyRate:{0} Coefficient:{1} LoanAmount:{2} ConvertedLoanAmount:{3}", currencyRate, CurrentValues.Instance.AlibabaCurrencyConversionCoefficient, m_nLoanAmount, convertedLoanAmount);
			return convertedLoanAmount;
		}

		private readonly decimal m_nLoanAmount;
		private readonly double amountInUsd;
		private readonly int m_nValidHours;
		private readonly bool m_bIsFirst;
	} // class ApprovedUser
} // namespace

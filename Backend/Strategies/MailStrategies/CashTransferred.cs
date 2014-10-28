namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using API;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Repository;
	using StructureMap;

	public class CashTransferred : ABrokerMailToo {
		#region constructor

		public CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog, true) {
			this.amount = amount;
			this.loanRefNum = loanRefNum;
			this.isFirst = isFirst;
			ToTrustPilot = true;
			amountInUsd = CalculateLoanAmountInUsd();
		} // constructor

		#endregion consturctor

		public override string Name { get { return "Cash Transferred"; } }

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"Amount", amount.ToString(CultureInfo.InvariantCulture)},
				{"EMAIL", CustomerData.Mail},
				{"LOANREFNUM", loanRefNum},
				{"AmountInUsd", amountInUsd.ToString(CultureInfo.InvariantCulture)},
				{ "AlibabaId", CustomerData.AlibabaId.ToString(CultureInfo.InvariantCulture) }
			};
			
			if (CustomerData.IsAlibaba)
			{
				TemplateName = "Mandrill - Alibaba - Took Loan";
			}
			else if (CustomerData.IsCampaign)
			{
				TemplateName = "Mandrill - Took Loan Campaign (1st loan)";
			}
			else if (isFirst)
			{
				TemplateName = "Mandrill - Took Loan (1st loan)";
			}
			else
			{
				TemplateName = "Mandrill - Took Loan (not 1st loan)";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		protected override void ActionAtEnd()
		{
			if (CustomerData.IsAlibaba)
			{
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc);
				Log.Info("Sending Alibaba internal took loan mail");
				SendCostumeMail("Mandrill - Alibaba - Internal Took Loan", Variables, new[] { address });
			}
		}

		private double CalculateLoanAmountInUsd()
		{
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();
			double currencyRate = currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");
			double convertedLoanAmount = (double)amount * currencyRate * CurrentValues.Instance.AlibabaCurrencyConversionCoefficient;
			Log.Info("Calculating Alibaba loan amount in USD. CurrencyRate:{0} Coefficient:{1} LoanAmount:{2} ConvertedLoanAmount:{3}", currencyRate, CurrentValues.Instance.AlibabaCurrencyConversionCoefficient, amount, convertedLoanAmount);
			return convertedLoanAmount;
		}

		private readonly decimal amount;
		private readonly double amountInUsd;
		private readonly string loanRefNum;
		private readonly bool isFirst;
	} // class CashTransferred
} // namespace

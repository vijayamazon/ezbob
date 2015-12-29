namespace Ezbob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using API;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Lottery;
	using EZBob.DatabaseLib.Repository;
	using Ezbob.Utils;
	using StructureMap;

	public class CashTransferred : ABrokerMailToo {
		public CashTransferred(
			int customerId,
			decimal amount,
			string loanRefNum,
			bool isFirst
		) : base(customerId, true, true) {
			this.amount = amount;
			this.loanRefNum = loanRefNum;
			this.isFirst = isFirst;
			this.amountInUsd = CalculateLoanAmountInUsd();
		} // constructor

		public override string Name { get { return "Cash Transferred"; } }

		protected override void SetTemplateAndVariables() {
			ToTrustPilot = true;
			
			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"Amount", amount.ToString("#,#")},
				{"EMAIL", CustomerData.Mail},
				{"LOANREFNUM", loanRefNum},
				{"AmountInUsd", MathUtils.Round2DecimalDown(amountInUsd).ToString("#,#.00") },
				{ "AlibabaId", CustomerData.AlibabaId.ToString(CultureInfo.InvariantCulture) },
				{ "RefNum", CustomerData.RefNum.ToString(CultureInfo.InvariantCulture) },
				{ "Surname", CustomerData.Surname.ToString(CultureInfo.InvariantCulture) }
			};

			if (CustomerData.IsAlibaba)
				TemplateName = "Mandrill - Alibaba - Took Loan";
			else if (CustomerData.IsCampaign)
				TemplateName = "Mandrill - Took Loan Campaign (1st loan)";
			else if (isFirst)
				TemplateName = "Mandrill - Took Loan (1st loan)";
			else
				TemplateName = "Mandrill - Took Loan (not 1st loan)";
		} // SetTemplateAndVariables

		protected override void ActionAtEnd() {
			if (CustomerData.IsAlibaba) {
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc, addSalesforceActivity:false);
				Log.Info("Sending Alibaba internal took loan mail");
				SendCostumeMail("Mandrill - Alibaba - Internal Took Loan", Variables, new[] { address });
			} // if

			new EnlistLottery(CustomerId).Execute();
		} // ActionAtEnd

		private double CalculateLoanAmountInUsd() {
			var currencyRateRepository = ObjectFactory.GetInstance<CurrencyRateRepository>();

			double currencyRate = currencyRateRepository.GetCurrencyHistoricalRate(DateTime.UtcNow, "USD");

			double convertedLoanAmount =
				(double)amount * currencyRate * CurrentValues.Instance.AlibabaCurrencyConversionCoefficient;

			Log.Info(
				"Calculating Alibaba loan amount in USD. " +
				"CurrencyRate:{0} Coefficient:{1} LoanAmount:{2} ConvertedLoanAmount:{3}",
				currencyRate,
				CurrentValues.Instance.AlibabaCurrencyConversionCoefficient,
				amount,
				convertedLoanAmount
			);

			return convertedLoanAmount;
		} // CalculateLoanAmountInUsd

		private readonly decimal amount;
		private readonly double amountInUsd;
		private readonly string loanRefNum;
		private readonly bool isFirst;
	} // class CashTransferred
} // namespace

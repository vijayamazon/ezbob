namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Database;
	using MailApi;

	public class CalculateMedal : AStrategy {
		public override string Name {
			get { return "CalculateMedal"; }
		} // Name

		public MedalResult Result { get; private set; }

		public CalculateMedal(int customerId, DateTime calculationTime) {
			this.useInternalLoad = true;
			this.customerId = customerId;
			this.calculationTime = calculationTime;
		} // constructor

		public CalculateMedal(
			int customerId,
			string typeOfBusiness,
			int consumerScore,
			int companyScore,
			int numOfHmrcMps,
			int numOfYodleeMps,
			int numOfEbayAmazonPayPalMps,
			DateTime? earliestHmrcLastUpdateDate,
			DateTime? earliestYodleeLastUpdateDate
		) {
			this.useInternalLoad = false;
			this.customerId = customerId;

			this.typeOfBusiness = typeOfBusiness;
			this.consumerScore = consumerScore;
			this.companyScore = companyScore;
			this.numOfHmrcMps = numOfHmrcMps;
			this.numOfYodleeMps = numOfYodleeMps;
			this.numOfEbayAmazonPayPalMps = numOfEbayAmazonPayPalMps;
			this.earliestHmrcLastUpdateDate = earliestHmrcLastUpdateDate;
			this.earliestYodleeLastUpdateDate = earliestYodleeLastUpdateDate;
		} // constructor

		public override void Execute() {
			try {
				if (this.useInternalLoad) {
					SafeReader sr = DB.GetFirst(
						"GetCustomerDataForMedalCalculation",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", this.customerId),
						new QueryParameter("Now", this.calculationTime)
					);

					if (!sr.IsEmpty) {
						this.typeOfBusiness = sr["TypeOfBusiness"];
						this.consumerScore = sr["ConsumerScore"];
						this.companyScore = sr["CompanyScore"];
						this.numOfHmrcMps = sr["NumOfHmrcMps"];
						this.numOfYodleeMps = sr["NumOfYodleeMps"];
						this.numOfEbayAmazonPayPalMps = sr["NumOfEbayAmazonPayPalMps"];
						this.earliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"];
						this.earliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"];
					} // if
				} // if

				// The first scenario (1) for checking medal type and getting medal value
				// namespace Ezbob.Backend.Strategies.MainStrategy 
				MedalResult result1 = new MedalCalculator1(
					this.customerId,
					this.calculationTime,
					this.typeOfBusiness,
					this.consumerScore,
					this.companyScore,
					this.numOfHmrcMps,
					this.numOfYodleeMps,
					this.numOfEbayAmazonPayPalMps,
					this.earliestHmrcLastUpdateDate,
					this.earliestYodleeLastUpdateDate
				).CalculateMedal();

				// Alternative scenario (2) for checking medal type and getting medal value
				// namespace AutomationCalculator.MedalCalculation
				var verification = new MedalChooser(DB, Log);
				MedalOutputModel result2 = verification.GetMedal(this.customerId, this.calculationTime);

				result2.SaveToDb(DB, Log);

				if ((result1 != null) && result1.IsIdentical(result2)) {
					result1.SaveToDb(DB);
					Result = result1;
					return;
				} // if

				// Mismatch in medal calculations

				if (result1 == null)
					result1 = new MedalResult(this.customerId);

				result1.PrintToLog(Log);
				result1.MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification;
				result1.Error = "Mismatch found in the 2 medal calculations";
				result1.SaveToDb(DB);

				SendExplanationMail(result1, result2);
				Log.Error("Mismatch found in the 2 medal calculations of customer: {0}.", this.customerId);

				Result = result1;
			} catch (Exception e) {
				Log.Warn(e, "Medal calculation for customer {0} failed with exception.", this.customerId);

				Result = new MedalResult(this.customerId) {
					Error = "Exception thrown: " + e.Message,
				};
			} // try
		} // Execute

		private void SendExplanationMail(MedalResult result1, MedalOutputModel result2) {
			string msg = string.Format(
				"main:         medal:{0} medal type:{1} score:{2} normalized score:{3} offered amount:£ {4} error:{5} \n" +
					"verification: medal:{6} medal type:{7} score:{8} normalized score:{9} offered amount:£ {10} error:{11}",
				result1.MedalClassification.ToString()
					.PadRight(10), result1.MedalType.ToString()
						.PadRight(30), result1.TotalScore.ToString("N2")
							.PadRight(10), (result1.TotalScoreNormalized).ToString("P2")
								.PadRight(10), result1.OfferedLoanAmount.ToString("N2")
									.PadRight(15), result1.Error,
				result2.Medal.ToString()
					.PadRight(10), result2.MedalType.ToString()
						.PadRight(30), (result2.Score * 100).ToString("N2")
							.PadRight(10), (result2.NormalizedScore).ToString("P2")
								.PadRight(10), result2.OfferedLoanAmount.ToString("N2")
									.PadRight(15), result2.Error
				);

			var message = string.Format(@"<h1><u>Difference in verification for <b style='color:red'>Medal calculation</b> for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b style='color:red'><pre>{1}</pre></b><br></h2>
					<h2><b>main flow:</b></h2>
					<pre><h3>{2}</h3></pre><br>
					<h2><b>verification flow:</b></h2>
					<pre><h3>{3}</h3></pre><br>", this.customerId, HttpUtility.HtmlEncode(msg),
				HttpUtility.HtmlEncode(result1.ToString()),
				HttpUtility.HtmlEncode(result2.ToString())
				);

			new Mail().Send(ConfigManager.CurrentValues.Instance.AutomationExplanationMailReciever, null, message,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				"#Mismatch in medal calculation for customer " + this.customerId);
		} // SendExplanationMail

		private readonly int customerId;
		private readonly bool useInternalLoad;
		private readonly DateTime calculationTime;

		private string typeOfBusiness;
		private int consumerScore;
		private int companyScore;
		private int numOfHmrcMps;
		private int numOfYodleeMps;
		private int numOfEbayAmazonPayPalMps;
		private DateTime? earliestHmrcLastUpdateDate;
		private DateTime? earliestYodleeLastUpdateDate;
	} // class CalculateMedal
} // namespace

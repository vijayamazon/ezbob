namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Globalization;
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using MailApi;

	public class CalculateMedal : AStrategy {
		public override string Name {
			get { return "CalculateMedal"; }
		} // Name

		public MedalResult Result { get; private set; }

		public CalculateMedal(int customerId, DateTime calculationTime, bool primaryOnly, bool doStoreMedal) {
			this.doStoreMedal = doStoreMedal;
			this.primaryOnly = primaryOnly;
			this.customerId = customerId;
			this.calculationTime = calculationTime;
			this.quietMode = false;
		} // constructor

		public virtual string Tag { get; set; }

		public virtual bool QuietMode {
			get { return this.quietMode; }
			set { this.quietMode = value; }
		} // QuietMode

		public override void Execute() {
			try {
				Log.Debug(
					"Loading customer data for medal calculation, customer = {0}, calculation time = {1}.",
					this.customerId,
					this.calculationTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
				);

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

				Log.Debug(
					"customerId = {0}, " +
					"calc time = {1}, " +
					"sr.IsEmpty = {2}, " +
					"type of business = {3}, " +
					"consumer score = {4}, " +
					"company score = {5}, " +
					"HMRC count = {6}, " +
					"Yodlee count = {7}, " +
					"Online count = {8}, " +
					"earliest HMRC update = '{9}', " +
					"earliest Yodlee update = '{10}'.",
					this.customerId,
					this.calculationTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
					sr.IsEmpty,
					this.typeOfBusiness,
					this.consumerScore,
					this.companyScore,
					this.numOfHmrcMps,
					this.numOfYodleeMps,
					this.numOfEbayAmazonPayPalMps,
					this.earliestHmrcLastUpdateDate,
					this.earliestYodleeLastUpdateDate
				);

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

				Log.Debug("\n\nPrimary medal:\n{0}", result1);

				MedalOutputModel result2 = null;

				if (!primaryOnly) {
					// Alternative scenario (2) for checking medal type and getting medal value
					// namespace AutomationCalculator.MedalCalculation
					var verification = new MedalChooser(DB, Log);
					result2 = verification.GetMedal(this.customerId, this.calculationTime);

					Log.Debug("\n\nSecondary medal:\n{0}", result2.ToString());

					if (this.doStoreMedal)
						result2.SaveToDb(Tag, DB, Log);
				} // if

				if ((result1 != null) && result1.IsLike(result2)) {
					if (this.doStoreMedal)
						result1.SaveToDb(Tag, DB, Log);

					Log.Debug("O6a-Ha! Match found in the 2 medal calculations of customer: {0}.", this.customerId);

					Result = result1;
					return;
				} // if

				// Mismatch in medal calculations

				if (result1 == null)
					result1 = new MedalResult(this.customerId, Log);

				result1.MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification;
				result1.Error = (result1.Error ?? string.Empty) + " Mismatch found in the 2 medal calculations";

				if (this.doStoreMedal)
					result1.SaveToDb(Tag, DB, Log);

				SendExplanationMail(result1, result2);
				Log.Error("Mismatch found in the 2 medal calculations of customer: {0}.", this.customerId);

				Result = result1;
			} catch (Exception e) {
				Log.Warn(e, "Medal calculation for customer {0} failed with exception.", this.customerId);

				Result = new MedalResult(this.customerId, Log) {
					Error = "Exception thrown: " + e.Message,
				};
			} // try
		} // Execute

		private void SendExplanationMail(MedalResult result1, MedalOutputModel result2) {
			if (QuietMode) {
				Log.Debug("Not sending explanation email: quiet mode.");
				return;
			} // if

			string medal2 = result2 == null ? string.Empty : result2.Medal.Stringify();
			string medalType2 = result2 == null ? string.Empty : result2.MedalType.ToString();
			string score2 = result2 == null ? string.Empty : (result2.Score * 100).ToString("N2");
			string normalizedScore2 = result2 == null ? string.Empty : result2.NormalizedScore.ToString("P2");
			string offeredLoanAmount2 = result2 == null ? string.Empty : result2.OfferedLoanAmount.ToString("N2");
			string error2 = result2 == null ? string.Empty : result2.Error;

			string msg = string.Format(
				"calculation time (UTC): {12}\n\n" +
				"main:         medal:{0} medal type:{1} score:{2} normalized score:{3} offered amount:£ {4} error:{5} \n" +
				"verification: medal:{6} medal type:{7} score:{8} normalized score:{9} offered amount:£ {10} error:{11}",

				result1.MedalClassification.Stringify(10),
				result1.MedalType.ToString().PadRight(30),
				result1.TotalScore.ToString("N2").PadRight(10),
				result1.TotalScoreNormalized.ToString("P2").PadRight(10),
				result1.OfferedLoanAmount.ToString("N2").PadRight(15),
				result1.Error,

				medal2.PadRight(10),
				medalType2.PadRight(30),
				score2.PadRight(10),
				normalizedScore2.PadRight(10),
				offeredLoanAmount2.PadRight(15),
				error2,

				result1.CalculationTime.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			var message = string.Format(
				"<h1><u>Difference in verification for " +
					"<b style=\"color:red\">Medal calculation</b> for customer " +
					"<b style=\"color:red\">{0}</b>" +
				"</u></h1><br>" +
				"<h2><b style=\"color:red\"><pre>{1}</pre></b><br></h2>" +
				"<h2><b>main flow:</b></h2>" +
				"<pre><h3>{2}</h3></pre><br>" +
				"<h2><b>verification flow:</b></h2>" +
				"<pre><h3>{3}</h3></pre><br>",
				this.customerId,
				HttpUtility.HtmlEncode(msg),
				HttpUtility.HtmlEncode(result1.ToString()),
				HttpUtility.HtmlEncode(result2 == null ? string.Empty : result2.ToString())
			);

			new Mail().Send(
				ConfigManager.CurrentValues.Instance.AutomationExplanationMailReciever,
				null,
				message,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				"#Mismatch in medal calculation for customer " + this.customerId
			);
		} // SendExplanationMail

		private readonly int customerId;
		private readonly DateTime calculationTime;
		private readonly bool primaryOnly;
		private readonly bool doStoreMedal;

		private string typeOfBusiness;
		private int consumerScore;
		private int companyScore;
		private int numOfHmrcMps;
		private int numOfYodleeMps;
		private int numOfEbayAmazonPayPalMps;
		private DateTime? earliestHmrcLastUpdateDate;
		private DateTime? earliestYodleeLastUpdateDate;
		private bool quietMode;
	} // class CalculateMedal
} // namespace

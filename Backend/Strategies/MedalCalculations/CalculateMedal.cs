namespace Ezbob.Backend.Strategies.MedalCalculations {
	using System;
	using System.Globalization;
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;
	using MailApi;

	using PrimaryCalculator = Ezbob.Backend.Strategies.MedalCalculations.Primary.MedalCalculator;

	public class CalculateMedal : AStrategy {
		public override string Name {
			get { return "CalculateMedal"; }
		} // Name

		public MedalResult Result { get; private set; }

		public CalculateMedal(
			int customerId,
			long? cashRequestID,
			long? nlCashRequestID,
			DateTime calculationTime,
			bool primaryOnly,
			bool doStoreMedal
		) {
			this.doStoreMedal = doStoreMedal;
			this.primaryOnly = primaryOnly;
			this.customerId = customerId;
			this.calculationTime = calculationTime;
			this.verificationResult = null;

			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID <= 0 ? null : nlCashRequestID;

			Result = new MedalResult(this.customerId, Log, "Medal calculator has not been executed yet.");
		} // constructor

		public virtual string Tag { get; set; }

		public virtual long? CashRequestID { get; private set; }

		public virtual long? NLCashRequestID { get; private set; }

		public override void Execute() {
			try {
				Log.Debug(
					"Loading customer data for medal calculation, customer = {0}, calculation time = {1}. {2}",
					this.customerId,
					this.calculationTime.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture),
					Tag
				);

				LoadCustomerData();

				Result = CalculatePrimary()
					?? new MedalResult(this.customerId, Log, "Failed to calculate primary medal.");

				this.verificationResult = CalculateVerification();

				Result.CheckForMatch(this.verificationResult);
			} catch (Exception e) {
				Log.Say(
					Severity.Alert,
					e,
					"Medal calculation for customer {0} failed with exception. {1}",
					this.customerId,
					Tag
				);

				Result = new MedalResult(this.customerId, Log, e);
			} // try

			if (!this.doStoreMedal)
				return;

			try {
				Result.SaveToDb(CashRequestID, NLCashRequestID, Tag, DB);

				if (this.verificationResult != null)
					this.verificationResult.SaveToDb(CashRequestID, NLCashRequestID, Tag, DB, Log);
			} catch (Exception e) {
				Log.Say(
					Severity.Alert,
					e,
					"Failed to save medal for customer {0}. {1}",
					this.customerId,
					Tag
				);
			} // try
		} // Execute

		public void Notify() {
			if (Result.HasError) {
				var bothHaveNoMedal =
					(Result.MedalType == MedalType.NoMedal) &&
					(this.verificationResult.MedalType == AutomationCalculator.Common.MedalType.NoMedal);

				if (bothHaveNoMedal)
					Log.Msg("Both medal calculators produced 'No medal' for customer {0}. {1}", this.customerId, Tag);
				else {
					SendExplanationMail();

					Log.Warn(
						"Mismatch/Error found in medal calculations of customer {0}. {1}",
						this.customerId,
						Tag
					);
				} // if
			} else
				Log.Debug("O6a-Ha! Match found in medal calculations of customer {0}. {1}", this.customerId, Tag);
		} // Notify

		private MedalOutputModel CalculateVerification() {
			if (this.primaryOnly)
				return null;

			var result = new MedalChooser(DB, Log).GetMedal(this.customerId, this.calculationTime);

			Log.Debug("\n\nSecondary medal:\n{0}", result);

			return result;
		} // CalculateVerification

		private MedalResult CalculatePrimary() {
			MedalResult result = new PrimaryCalculator(
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

			Log.Debug("\n\nPrimary medal:\n{0}", result);

			return result;
		} // CalculatePrimary

		private void LoadCustomerData() {
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

				if (CashRequestID == null)
					CashRequestID = sr["LastCashRequestID"];

				if (NLCashRequestID == null)
					NLCashRequestID = sr["NLLastCashRequestID"];
			} // if

			Log.Debug(
				"customer id = {0}, " +
				"cash request id = {12}, " +
				"NLcash request id = {13}, " +
				"calc time = {1}, " +
				"sr.IsEmpty = {2}, " +
				"type of business = {3}, " +
				"consumer score = {4}, " +
				"company score = {5}, " +
				"HMRC count = {6}, " +
				"Yodlee count = {7}, " +
				"Online count = {8}, " +
				"earliest HMRC update = '{9}', " +
				"earliest Yodlee update = '{10}'." +
				"tag = '{11}'",
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
				this.earliestYodleeLastUpdateDate,
				Tag,
				CashRequestID,
				NLCashRequestID
			);
		} // LoadCustomerData

		private void SendExplanationMail() {
			string medal2 = this.verificationResult == null ? string.Empty : this.verificationResult.Medal.Stringify();
			string medalType2 = this.verificationResult == null
				? string.Empty
				: this.verificationResult.MedalType.ToString();
			string score2 = this.verificationResult == null
				? string.Empty
				: (this.verificationResult.Score * 100).ToString("N2");
			string normalizedScore2 = this.verificationResult == null
				? string.Empty
				: this.verificationResult.NormalizedScore.ToString("P2");
			string offeredLoanAmount2 = this.verificationResult == null
				? string.Empty
				: this.verificationResult.OfferedLoanAmount.ToString("N2");
			string error2 = this.verificationResult == null ? string.Empty : this.verificationResult.Error;

			string msg = string.Format(
				"calculation time (UTC): {12}\n\n" +
				"main:         medal:{0} medal type:{1} score:{2} normalized score:{3} offered amount:£{4} error:{5} \n" +
				"verification: medal:{6} medal type:{7} score:{8} normalized score:{9} offered amount:£{10} error:{11}",

				Result.MedalClassification.Stringify(10),
				Result.MedalType.ToString().PadRight(30),
				Result.TotalScore.ToString("N2").PadRight(10),
				Result.TotalScoreNormalized.ToString("P2").PadRight(10),
				Result.OfferedLoanAmount.ToString("N2").PadRight(15),
				Result.Error,

				medal2.PadRight(10),
				medalType2.PadRight(30),
				score2.PadRight(10),
				normalizedScore2.PadRight(10),
				offeredLoanAmount2.PadRight(15),
				error2,

				Result.CalculationTime.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			var message = string.Format(
				"<h1><u>Difference in verification for " +
					"<b style=\"color:red\">Medal calculation</b> for customer " +
					"<b style=\"color:red\">{0}</b> {4}" +
				"</u></h1><br>" +
				"<h2><b style=\"color:red\"><pre>{1}</pre></b><br></h2>" +
				"<h2><b>main flow:</b></h2>" +
				"<pre><h3>{2}</h3></pre><br>" +
				"<h2><b>verification flow:</b></h2>" +
				"<pre><h3>{3}</h3></pre><br>",
				this.customerId,
				HttpUtility.HtmlEncode(msg),
				HttpUtility.HtmlEncode(Result.ToString()),
				HttpUtility.HtmlEncode(this.verificationResult == null ? string.Empty : this.verificationResult.ToString()),
				Tag
			);

			new Mail().Send(
				CurrentValues.Instance.AutomationExplanationMailReciever,
				null,
				message,
				CurrentValues.Instance.MailSenderEmail,
				CurrentValues.Instance.MailSenderName,
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
		private MedalOutputModel verificationResult;
	} // class CalculateMedal
} // namespace

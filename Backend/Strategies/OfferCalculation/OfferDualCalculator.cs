namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System.Web;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using MailApi;

	using PrimaryCalculator = Ezbob.Backend.Strategies.OfferCalculation.OfferCalculator;
	using VerificationCalculator = AutomationCalculator.OfferCalculation.OfferCalculator;

	public class OfferDualCalculator {
		public OfferDualCalculator(
			int customerID,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			EZBob.DatabaseLib.Model.Database.Medal medalClassification,
			int loanSourceId = (int)LoanSourceName.COSME,
			int repaymentPeriod = 15,
			bool saveToDB = true
		) {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
			this.saveToDB = saveToDB;
			this.loanScourceID = loanSourceId;
			this.repaymentPeriod = repaymentPeriod;

			CustomerID = customerID;
			CalculationTime = calculationTime;
			Amount = amount;
			HasLoans = hasLoans;
			MedalClassification = medalClassification;

			PrimaryResult = null;
			VerificationResult = null;
		} // constructor

		public int CustomerID { get; private set; }
		public DateTime CalculationTime { get; private set; }
		public int Amount { get; private set; }
		public bool HasLoans { get; private set; }
		public EZBob.DatabaseLib.Model.Database.Medal MedalClassification { get; private set; }

		public OfferResult PrimaryResult { get; private set; }
		public OfferOutputModel VerificationResult { get; private set; }

		public OfferResult CalculateOffer() {
			try {
				DoPrimary();
				DoVerification();

				bool equal = PrimaryResult.Equals(VerificationResult);

				if (this.saveToDB) {
					PrimaryResult.SaveToDb(this.db);
					VerificationResult.SaveToDb(this.log, this.db);
				} // if

				if (equal) {
					this.log.Debug("Match found in offer calculations of customer {0}:\n\t{1}", CustomerID, PrimaryResult);
					return PrimaryResult;
				} // if

				SendExplanationMail();

				this.log.Alert(
					"Mismatch found in offer calculations of customer {0}:\n\tPrimary: {1}\n\tVerification: {2}",
					CustomerID,
					PrimaryResult,
					VerificationResult
				);
			} catch (Exception e) {
				this.log.Warn(e, "Offer calculation for customer {0} failed.", CustomerID);
			} // try

			return null;
		} // CalculateOffer

		public string ResultSummary {
			get {
				return string.Format(
					"main:                    amount: {0} interest rate: {1} setup fee: {2} description: {3}\n" +
					"verification seek:       amount: {4} interest rate: {5} setup fee: {6} description: {7}\n",
					PrimaryResult == null ? "--" : PrimaryResult.Amount.ToString("C2", Culture),
					PrimaryResult == null ? "--" : (PrimaryResult.InterestRate / 100m).ToString("P2", Culture),
					PrimaryResult == null ? "--" : (PrimaryResult.SetupFee / 100m).ToString("P2", Culture),
					PrimaryResult == null ? "--" : PrimaryResult.Description,

					VerificationResult == null ? "-- " : VerificationResult.Amount.ToString("C2", Culture),
					VerificationResult == null ? "-- " : (VerificationResult.InterestRate / 100m).ToString("P2", Culture),
					VerificationResult == null ? "-- " : (VerificationResult.SetupFee / 100m).ToString("P2", Culture),
					VerificationResult == null ? "-- " : VerificationResult.Description
				);
			} // get
		} // ResultSummary

		private void DoPrimary() {
			PrimaryResult = new PrimaryCalculator(
				CustomerID,
				CalculationTime,
				Amount,
				HasLoans,
				MedalClassification,
				this.repaymentPeriod
			).CalculateOffer();
		} // DoPrimary

		private void DoVerification() {
			var input = new OfferInputModel {
				Amount = Amount,
				AspireToMinSetupFee = ConfigManager.CurrentValues.Instance.AspireToMinSetupFee,
				HasLoans = HasLoans,
				Medal = (Medal)Enum.Parse(typeof(Medal), MedalClassification.ToString()),
				CustomerId = CustomerID,
				LoanSourceId = this.loanScourceID,
				RepaymentPeriod = this.repaymentPeriod
			};

			VerificationResult = new VerificationCalculator(input, this.db, this.log).GetCosmeOffer();
		} // DoVerification

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private void SendExplanationMail() {
			var message = string.Format(
@"<h1><u>Difference in verification for <b style='color:red'>Offer calculation</b>
for customer <b style='color:red'>{0}</b></u></h1><br>
<h2><b style='color:red'><pre>{1}</pre></b><br></h2>
<h2><b>main flow:</b></h2>
<pre><h3>{2}</h3></pre><br>
<h2><b>verification flow seek:</b></h2>
<pre><h3>{3}</h3></pre><br>",
				CustomerID, HttpUtility.HtmlEncode(ResultSummary),
				HttpUtility.HtmlEncode(PrimaryResult.ToString()),
				HttpUtility.HtmlEncode(VerificationResult.ToString())
			);

			new Mail().Send(
				ConfigManager.CurrentValues.Instance.AutomationExplanationMailReciever,
				null,
				message,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				"#Mismatch in offer calculation for customer " + CustomerID
			);
		} // SendExplanationMail

		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly bool saveToDB;
		private readonly int loanScourceID;
		private readonly int repaymentPeriod;
	} // class OfferDualCalculator
} // namespace

﻿namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using System.Globalization;
	using MailApi;

	public class OfferDualCalculator {
		public OfferDualCalculator(
			int customerID,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			EZBob.DatabaseLib.Model.Database.Medal medalClassification,
			bool saveToDB = true
		) {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;
			this.saveToDB = saveToDB;

			CustomerID = customerID;
			CalculationTime = calculationTime;
			Amount = amount;
			HasLoans = hasLoans;
			MedalClassification = medalClassification;

			offerCalculator1 = new OfferCalculator1();
			offerCalculator2 = new OfferCalculator(this.db, this.log);

			Primary = null;
			VerifySeek = null;
			VerifyBoundaries = null;
		} // constructor

		public int CustomerID { get; private set; }
		public DateTime CalculationTime { get; private set; }
		public int Amount { get; private set; }
		public bool HasLoans { get; private set; }
		public EZBob.DatabaseLib.Model.Database.Medal MedalClassification { get; private set; }

		public OfferResult Primary { get; private set; }
		public OfferOutputModel VerifySeek { get; private set; }
		public OfferOutputModel VerifyBoundaries { get; private set; }

		public OfferResult CalculateOffer() {
			try {
				Primary = offerCalculator1.CalculateOffer(
					CustomerID,
					CalculationTime,
					Amount,
					HasLoans,
					MedalClassification
				);

				var medal = (Medal)Enum.Parse(typeof(Medal), MedalClassification.ToString());

				var input = new OfferInputModel {
					Amount = Amount,
					AspireToMinSetupFee = ConfigManager.CurrentValues.Instance.AspireToMinSetupFee,
					HasLoans = HasLoans,
					Medal = medal,
					CustomerId = CustomerID
				};

				VerifySeek = offerCalculator2.GetOfferBySeek(input);
				VerifyBoundaries = offerCalculator2.GetOfferByBoundaries(input);

				if (this.saveToDB) {
					VerifySeek.SaveToDb(log, db, OfferCalculationType.Seek);
					VerifyBoundaries.SaveToDb(log, db, OfferCalculationType.Boundaries);
				} // if

				if (!VerifySeek.Equals(VerifyBoundaries)) {
					log.Info(
						"the two verification implementations mismatch \nby seek:\n {0}\nby boundaries\n {1}",
						VerifySeek,
						VerifyBoundaries
					);
				} // if

				if (Primary.Equals(VerifySeek)) {
					log.Debug("Main implementation of offer calculation result: \n{0}", Primary);

					if (this.saveToDB)
						Primary.SaveToDb(db);

					return Primary;
				} // if

				// Difference in offer calculations
				SendExplanationMail();

				if (this.saveToDB)
					Primary.SaveToDb(db);

				log.Error("Mismatch found in the 2 offer calculations of customer: {0} ", CustomerID);
				return null;
			} catch (Exception e) {
				log.Warn(e, "Offer calculation for customer {0} failed with exception.", CustomerID);
			} // try

			return null;
		} // CalculateOffer

		public string ResultSummary {
			get {
				return string.Format(
					"main:                    amount: {0} interest rate: {1} setup fee: {2} description: {3}\n" +
					"verification seek:       amount: {4} interest rate: {5} setup fee: {6} description: {7}\n" +
					"verification boundaries: amount: {8} interest rate: {9} setup fee: {10} description: {11}",
					Primary == null ? "--" : Primary.Amount.ToString("C2", Culture),
					Primary == null ? "--" : (Primary.InterestRate / 100m).ToString("P2", Culture),
					Primary == null ? "--" : (Primary.SetupFee / 100m).ToString("P2", Culture),
					Primary == null ? "--" : Primary.Description,

					VerifySeek == null ? "-- " : VerifySeek.Amount.ToString("C2", Culture),
					VerifySeek == null ? "-- " : (VerifySeek.InterestRate / 100m).ToString("P2", Culture),
					VerifySeek == null ? "-- " : (VerifySeek.SetupFee / 100m).ToString("P2", Culture),
					VerifySeek == null ? "-- " : VerifySeek.Description,

					VerifyBoundaries == null ? "-- " : VerifyBoundaries.Amount.ToString("C2", Culture),
					VerifyBoundaries == null ? "-- " : (VerifyBoundaries.InterestRate / 100m).ToString("P2", Culture),
					VerifyBoundaries == null ? "-- " : (VerifyBoundaries.SetupFee / 100m).ToString("P2", Culture),
					VerifyBoundaries == null ? "-- " : VerifyBoundaries.Description
				);
			} // get
		} // ResultSummary

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture

		private void SendExplanationMail() {
			var message = string.Format(
				@"<h1><u>Difference in verification for <b style='color:red'>Offer calculation</b> for customer <b style='color:red'>{0}</b></u></h1><br>
				<h2><b style='color:red'><pre>{1}</pre></b><br></h2>
				<h2><b>main flow:</b></h2>
				<pre><h3>{2}</h3></pre><br>
				<h2><b>verification flow seek:</b></h2>
				<pre><h3>{3}</h3></pre><br>
				<h2><b>verification flow boundaries:</b></h2>
				<pre><h3>{4}</h3></pre><br>",
				CustomerID, HttpUtility.HtmlEncode(ResultSummary),
				HttpUtility.HtmlEncode(Primary.ToString()),
				HttpUtility.HtmlEncode(VerifySeek.ToString()),
				HttpUtility.HtmlEncode(VerifyBoundaries.ToString())
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
		private readonly OfferCalculator1 offerCalculator1;
		private readonly OfferCalculator offerCalculator2;
		private readonly bool saveToDB;
	} // class OfferDualCalculator
} // namespace

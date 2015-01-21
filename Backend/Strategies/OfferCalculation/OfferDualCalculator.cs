namespace Ezbob.Backend.Strategies.OfferCalculation {
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.OfferCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MailApi;

	public class OfferDualCalculator {
		public OfferDualCalculator() {
			this.log = Library.Instance.Log;
			this.db = Library.Instance.DB;

			offerCalculator1 = new OfferCalculator1();
			offerCalculator2 = new OfferCalculator(this.db, this.log);

			Primary = null;
			VerifySeek = null;
			VerifyBoundaries = null;
		} // constructor

		public OfferResult Primary { get; private set; }
		public OfferOutputModel VerifySeek { get; private set; }
		public OfferOutputModel VerifyBoundaries { get; private set; }

		public OfferResult CalculateOffer(
			int customerId,
			DateTime calculationTime,
			int amount,
			bool hasLoans,
			EZBob.DatabaseLib.Model.Database.Medal medalClassification
		) {
			try {
				Primary = offerCalculator1.CalculateOffer(
					customerId,
					calculationTime,
					amount,
					hasLoans,
					medalClassification
				);

				var medal = (Medal)Enum.Parse(typeof(Medal), medalClassification.ToString());

				var input = new OfferInputModel {
					Amount = amount,
					AspireToMinSetupFee = ConfigManager.CurrentValues.Instance.AspireToMinSetupFee,
					HasLoans = hasLoans,
					Medal = medal,
					CustomerId = customerId
				};

				VerifySeek = offerCalculator2.GetOfferBySeek(input);
				VerifyBoundaries = offerCalculator2.GetOfferByBoundaries(input);

				VerifySeek.SaveToDb(log, db, OfferCalculationType.Seek);
				VerifyBoundaries.SaveToDb(log, db, OfferCalculationType.Boundaries);

				if (!VerifySeek.Equals(VerifyBoundaries)) {
					log.Info(
						"the two verification implementations mismatch \nby seek:\n {0}\nby boundaries\n {1}",
						VerifySeek,
						VerifyBoundaries
					);
				} // if

				if (Primary.Equals(VerifySeek)) {
					log.Debug("Main implementation of offer calculation result: \n{0}", Primary);
					Primary.SaveToDb(db);
					return Primary;
				} // if

				// Difference in offer calculations
				SendExplanationMail(customerId, Primary, VerifySeek, VerifyBoundaries);
				Primary.SaveToDb(db);
				log.Error("Mismatch found in the 2 offer calculations of customer: {0} ", customerId);
				return null;
			} catch (Exception e) {
				log.Warn("Offer calculation for customer {0} failed with exception:{1}", customerId, e);
			} // try

			return null;
		} // CalculateOffer

		private void SendExplanationMail(
			int customerId,
			OfferResult result1,
			OfferOutputModel result2,
			OfferOutputModel result3
		) {
			string msg = string.Format(
				"main:                    amount:{0} interest rate:{1} setup fee:{2} error:{3}\n" +
				"verification seek:       amount:{4} interest rate:{5} setup fee:{6} error:{7}\n" +
				"verification boundaries: amount:{8} interest rate:{9} setup fee:{10} error:{11}",
				result1.Amount, result1.InterestRate, result1.SetupFee, result1.Error,
				result2.Amount, result2.InterestRate, result2.SetupFee, result2.Error,
				result3.Amount, result3.InterestRate, result3.SetupFee, result3.Error
			);

			var message = string.Format(
				@"<h1><u>Difference in verification for <b style='color:red'>Offer calculation</b> for customer <b style='color:red'>{0}</b></u></h1><br>
				<h2><b style='color:red'><pre>{1}</pre></b><br></h2>
				<h2><b>main flow:</b></h2>
				<pre><h3>{2}</h3></pre><br>
				<h2><b>verification flow seek:</b></h2>
				<pre><h3>{3}</h3></pre><br>
				<h2><b>verification flow boundaries:</b></h2>
				<pre><h3>{4}</h3></pre><br>",
				customerId, HttpUtility.HtmlEncode(msg),
				HttpUtility.HtmlEncode(result1.ToString()),
				HttpUtility.HtmlEncode(result2.ToString()),
				HttpUtility.HtmlEncode(result3.ToString())
			);

			new Mail().Send(
				ConfigManager.CurrentValues.Instance.AutomationExplanationMailReciever,
				null,
				message,
				ConfigManager.CurrentValues.Instance.MailSenderEmail,
				ConfigManager.CurrentValues.Instance.MailSenderName,
				"#Mismatch in offer calculation for customer " + customerId
			);
		} // SendExplanationMail

		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly OfferCalculator1 offerCalculator1;
		private readonly OfferCalculator offerCalculator2;
	} // class OfferDualCalculator
} // namespace

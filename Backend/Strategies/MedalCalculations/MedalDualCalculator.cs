namespace Ezbob.Backend.Strategies.MedalCalculations
{
	using System.Web;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;
	using MailApi;

	public class MedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly MedalCalculator1 medalCalculator1;
		private readonly MedalChooser medalCalculatorVerification;
		public MedalResult Results { get; set; }

		public MedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;

			medalCalculator1 = new MedalCalculator1(db, log);
			medalCalculatorVerification = new MedalChooser(db, log);
		}

		public MedalResult CalculateMedalScore(int customerId, DateTime calculationTime, string typeOfBusiness, int consumerScore, int companyScore, int numOfHmrcMps, int numOfYodleeMps, int numOfEbayAmazonPayPalMps, DateTime? earliestHmrcLastUpdateDate, DateTime? earliestYodleeLastUpdateDate) {
			try {
				MedalResult result1 = medalCalculator1.CalculateMedal(customerId, calculationTime, typeOfBusiness, consumerScore, companyScore, numOfHmrcMps, numOfYodleeMps, numOfEbayAmazonPayPalMps, earliestHmrcLastUpdateDate, earliestYodleeLastUpdateDate);
				MedalOutputModel result2 = medalCalculatorVerification.GetMedal(customerId, calculationTime);

				result2.SaveToDb(db, log);
				if (result1 != null && result1.IsIdentical(result2)) {
					result1.SaveToDb(db);
					return result1;
				}

				//Mismatch in medal calculations
				if (result1 == null) {
					result1 = new MedalResult(customerId);
				}
				result1.PrintToLog(log);
				result1.MedalClassification = EZBob.DatabaseLib.Model.Database.Medal.NoClassification;
				result1.Error = "Mismatch found in the 2 medal calculations";
				result1.SaveToDb(db);

				SendExplanationMail(customerId, result1, result2);
				log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				return result1;
			}
			catch (Exception e) {
				log.Warn("Medal calculation for customer {0} failed with exception:{1}", customerId, e);

				return new MedalResult(customerId) {
					Error = "Exception thrown: " + e.Message,
				};
			}
		}

		private void SendExplanationMail(int customerId, MedalResult result1, MedalOutputModel result2) {
			string msg = string.Format("main:         medal:{0} medal type:{1} score:{2} normalized score:{3} offered amount:£ {4} error:{5} \n" +
									   "verification: medal:{6} medal type:{7} score:{8} normalized score:{9} offered amount:£ {10} error:{11}",
				result1.MedalClassification.ToString().PadRight(10), result1.MedalType.ToString().PadRight(30), result1.TotalScore.ToString("N2").PadRight(10), (result1.TotalScoreNormalized).ToString("P2").PadRight(10), result1.OfferedLoanAmount.ToString("N2").PadRight(15), result1.Error,
				result2.Medal.ToString().PadRight(10), result2.MedalType.ToString().PadRight(30), (result2.Score*100).ToString("N2").PadRight(10), (result2.NormalizedScore).ToString("P2").PadRight(10), result2.OfferedLoanAmount.ToString("N2").PadRight(15), result2.Error);
			var message =
				string.Format(@"<h1><u>Difference in verification for <b style='color:red'>Medal calculation</b> for customer <b style='color:red'>{0}</b></u></h1><br>
					<h2><b style='color:red'><pre>{1}</pre></b><br></h2>
					<h2><b>main flow:</b></h2>
					<pre><h3>{2}</h3></pre><br>
					<h2><b>verification flow:</b></h2>
					<pre><h3>{3}</h3></pre><br>",
							customerId, HttpUtility.HtmlEncode(msg),
							HttpUtility.HtmlEncode(result1.ToString()),
							HttpUtility.HtmlEncode(result2.ToString()));

			new Mail().Send(ConfigManager.CurrentValues.Instance.AutomationExplanationMailReciever, null, message,
			                             ConfigManager.CurrentValues.Instance.MailSenderEmail,
			                             ConfigManager.CurrentValues.Instance.MailSenderName,
			                             "#Mismatch in medal calculation for customer " + customerId);
		}
	}
}

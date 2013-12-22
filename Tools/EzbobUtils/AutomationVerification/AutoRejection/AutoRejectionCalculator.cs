namespace AutomationCalculator
{
	using System.Collections.Generic;
	using CommonLib;
	using Ezbob.Logger;

	public class AutoRejectionCalculator
    {
		private static ASafeLog _log ;
		public AutoRejectionCalculator(ASafeLog log)
		{
			_log = log;
		}

		public bool IsAutoRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper(_log);
			var experianScore = dbHelper.GetExperianScore(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var anualTurnover = AnalysisFunctionsHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year,_log);
			var wasApproved = dbHelper.WasApprovedForLoan(customerId);
			var hasDefaultAccounts = dbHelper.HasDefaultAccounts(customerId, Constants.DefaultMinAmount, Constants.DefaultMinMonths);

			return IsAutoRejectedCalculator(experianScore, mps, anualTurnover, wasApproved, hasDefaultAccounts, out reason);
		}

		private bool IsAutoRejectedCalculator(int experianScore, List<MarketPlace> mps, double anualTurnover, bool wasApproved, bool hasDefaultAccounts, out string reason)
		{
			//0 Exceptions to the rejection rules:
			//Do not apply to clients that have been approved at least once before (even if the latest decision was rejection)
			if (wasApproved)
			{
				reason = "Not Rejected. Was approved for loan";
				return false;
			}
			//Do not apply to clients with total annual turnover above £250,000
			if (anualTurnover >= Constants.NoRejectIfTotalAnnualTurnoverAbove)
			{
				reason = string.Format("Not Rejected. Total Annual Turnover Above {0}", Constants.NoRejectIfTotalAnnualTurnoverAbove);
				return false;
			}
			//Do not apply to clients with credit score above 900.
			if (experianScore >= Constants.NoRejectIfCreditScoreAbove)
			{
				reason = string.Format("Not Rejected. Credit Score Above {0}", Constants.NoRejectIfCreditScoreAbove);
				return false;
			}
			//Do not apply to clients with 2 directors, of which at least 1 has a score above 800 ???(on hold)
			//TODO or not TODO

			//1  Low credit score: less than 550 (Consumer credit score<550)

			if (experianScore < Constants.MinCreditScore)
			{
				reason = string.Format("Rejected. Credit Score Below {0}", Constants.MinCreditScore);
				return true;
			}

			//2  Low turnover, one of the following :
			//a Total annual turnover is less than 10,000 GBP

			if (anualTurnover < Constants.MinAnnualTurnover)
			{
				reason = string.Format("Rejected. Annual Turnover Below {0}", Constants.MinAnnualTurnover);
				return true;
			}
			//b Total 3-month turnover is less than 2.000 GBP
			var threeMonthTurnover = AnalysisFunctionsHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Month3,_log);
			if (threeMonthTurnover < Constants.MinThreeMonthTurnover)
			{
				reason = string.Format("Rejected. 3 Month Turnover Below {0}", Constants.MinThreeMonthTurnover);
				return true;
			}

			//3 Defaults:
			//a for clients with credit score below 800: at least 1 default in amount of 300+ GBP on any of the financial accounts in the last 24 months
			if (experianScore < Constants.DefaultScoreBelow && hasDefaultAccounts)
			{
				reason = string.Format("Rejected. Has Default Account");
				return true;
			}
			//b for clients with credit score between 600 - 800: at least 1 default in amount of 300+ GBP on any of the financial accounts in the last 12 months. ???(no need)
			//TODO or not TODO

			//4 Seniority: Marketplace seniority less than 11 months (currently 300 days)
			int seniority = MarketPlacesHelper.GetMarketPlacesSeniority(mps);
			if (seniority < Constants.MinMarketPlaceSeniorityDays)
			{
				reason = string.Format("Rejected. MP Seniority below {0}", Constants.MinMarketPlaceSeniorityDays);
				return true;
			}

			reason = "Not Rejected. None of the auto rejection rules match.";
			return false;
		}
    }
}

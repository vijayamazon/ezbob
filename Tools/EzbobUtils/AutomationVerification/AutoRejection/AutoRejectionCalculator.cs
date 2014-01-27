namespace AutomationCalculator
{
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Logger;

	public class AutoRejectionCalculator
    {
		private static ASafeLog _log ;
		private static RejectionConstants _const;
		public AutoRejectionCalculator(ASafeLog log, RejectionConstants constants)
		{
			_log = log;
			if (constants == null)
			{
				constants = new RejectionConstants
					{
						DefaultMinAmount = Constants.DefaultMinAmount,
						DefaultMinMonths = Constants.DefaultMinMonths,
						DefaultScoreBelow = Constants.DefaultScoreBelow,
						MinAnnualTurnover = Constants.MinAnnualTurnover,
						MinCreditScore = Constants.MinCreditScore,
						MinMarketPlaceSeniorityDays = Constants.MinMarketPlaceSeniorityDays,
						MinThreeMonthTurnover = Constants.MinThreeMonthTurnover,
						NoRejectIfCreditScoreAbove = Constants.NoRejectIfCreditScoreAbove,
						NoRejectIfTotalAnnualTurnoverAbove = Constants.NoRejectIfTotalAnnualTurnoverAbove
					};

			}

			_const = constants;
		}

		public bool IsAutoRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper(_log);
			var experianScore = dbHelper.GetExperianScore(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var paymentMps = dbHelper.GetCustomerPaymentMarketPlaces(customerId);
			var anualTurnover = MarketPlacesHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year, _log);
			var wasApproved = dbHelper.WasApprovedForLoan(customerId);
			var hasDefaultAccounts = dbHelper.HasDefaultAccounts(customerId, _const.DefaultMinAmount, _const.DefaultMinMonths);

			return IsAutoRejectedCalculator(experianScore, mps, paymentMps, anualTurnover, wasApproved, hasDefaultAccounts, out reason);
		}

		private bool IsAutoRejectedCalculator(int experianScore, List<MarketPlace> mps, List<string> paymentMps,  double anualTurnover, bool wasApproved, bool hasDefaultAccounts, out string reason)
		{
			
			//0 Exceptions to the rejection rules:
			//Do not apply to clients that have been approved at least once before (even if the latest decision was rejection)
			if (wasApproved)
			{
				reason = "Not Rejected. Was approved for loan";
				return false;
			}
			//Do not apply to clients with total annual turnover above £250,000
			if (anualTurnover >= _const.NoRejectIfTotalAnnualTurnoverAbove)
			{
				reason = string.Format("Not Rejected. Total Annual Turnover Above {0} ({1})", _const.NoRejectIfTotalAnnualTurnoverAbove, anualTurnover);
				return false;
			}
			//Do not apply to clients with credit score above 900.
			if (experianScore >= _const.NoRejectIfCreditScoreAbove)
			{
				reason = string.Format("Not Rejected. Credit Score Above {0} ({1})", _const.NoRejectIfCreditScoreAbove, experianScore);
				return false;
			}
			//Do not apply to clients with 2 directors, of which at least 1 has a score above 800 ???(on hold)
			//TODO or not TODO

			//1  Low credit score: less than 550 (Consumer credit score<550)

			if (experianScore < _const.MinCreditScore)
			{
				reason = string.Format("Rejected. Credit Score Below {0} ({1})", _const.MinCreditScore, experianScore);
				return true;
			}

			
			//has payment mps
			bool hasSpecialMps = false;
			if (paymentMps.Any())
			{
				hasSpecialMps = true;
			}
			else
			{
				//2  Low turnover, one of the following :
				//a Total annual turnover is less than 10,000 GBP

				if (anualTurnover < _const.MinAnnualTurnover)
				{
					reason = string.Format("Rejected. Annual Turnover Below {0} ({1})", _const.MinAnnualTurnover, anualTurnover);
					return true;
				}
				//b Total 3-month turnover is less than 2.000 GBP
				var threeMonthTurnover = MarketPlacesHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Month3, _log);
				if (threeMonthTurnover < _const.MinThreeMonthTurnover)
				{
					reason = string.Format("Rejected. 3 Month Turnover Below {0} ({1})", _const.MinThreeMonthTurnover,
					                       threeMonthTurnover);
					return true;
				}
			}
			//3 Defaults:
			//a for clients with credit score below 800: at least 1 default in amount of 300+ GBP on any of the financial accounts in the last 24 months
			if (experianScore < _const.DefaultScoreBelow && hasDefaultAccounts)
			{
				reason = string.Format("Rejected. Has Default Account And credit score below {0} ({1})",_const.DefaultScoreBelow, experianScore);
				return true;
			}
			//b for clients with credit score between 600 - 800: at least 1 default in amount of 300+ GBP on any of the financial accounts in the last 12 months. ???(no need)
			//TODO or not TODO

			//4 Seniority: Marketplace seniority less than 11 months (currently 300 days)
			int seniority = MarketPlacesHelper.GetMarketPlacesSeniority(mps);
			if (seniority < _const.MinMarketPlaceSeniorityDays)
			{
				reason = string.Format("Rejected. MP Seniority below {0} ({1})", _const.MinMarketPlaceSeniorityDays, seniority);
				return true;
			}

			reason = string.Format("Not Rejected. None of the auto rejection rules match. {0}", (hasSpecialMps ? "(Has payment mps:)" + string.Join(",",paymentMps) : ""));
			return false;
		}
    }
}

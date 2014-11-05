namespace AutomationCalculator.AutoDecision
{
	using System;
	using Common;
	using Ezbob.Logger;

	public class AutoApprovalCalculator
	{
		private static ASafeLog _log;
		public AutoApprovalCalculator(ASafeLog log)
		{
			_log = log;
		}

		public bool IsAutoApproved(int customerId, out string reason, out int amount)
		{
			amount = 0;
			var dbHelper = new DbHelper(_log);
			var experianScore = dbHelper.GetExperianScore(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var mpHelper = new MarketPlacesHelper(_log);
			var anualTurnover = mpHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year);
			var quarterTurnover = mpHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Month3);
			var lastTurnover = mpHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Month);
			var hasDefaultAccounts = dbHelper.HasDefaultAccounts(customerId, 0);
			var seniorityMonth = mpHelper.GetMarketPlacesSeniority(mps) / 12;
			var birthDate = dbHelper.GetCustomerBirthDate(customerId);
			var medalRate = dbHelper.GetMedalRate(customerId);

			int age = 0;
			if (birthDate.HasValue)
			{
				var zeroTime = new DateTime(1, 1, 1);
				var ts = DateTime.UtcNow - birthDate.Value;
				age = (zeroTime + ts).Year - 1;
			}
			//Only clients that passed AML can be auto-approved 
			//TODO

			//credit score 900+
			if (experianScore < Constants.ApprovalMinCreditScore)
			{
				reason = string.Format("Not Approved. Credit score less then {0}", Constants.ApprovalMinCreditScore);
				return false;
			}

			//no defaults in the history on CAIS accounts
			if (hasDefaultAccounts)
			{
				reason = string.Format("Not Approved. Has defaults in the history on CAIS accounts");
				return false;
			}

			//no delays more than 60 days on CAIS accounts (current status)
			//TODO

			//Annual turnover min £10,000
			if (anualTurnover < Constants.ApprovalMinAnnualTurnover)
			{
				reason = string.Format("Not Approved. Annual turnover less then {0}", Constants.ApprovalMinAnnualTurnover);
				return false;
			}

			//Quarter turnover min £2,000
			if (quarterTurnover < Constants.ApprovalMinQuarterTurnover)
			{
				reason = string.Format("Not Approved. Quarter turnover less then {0}", Constants.ApprovalMinQuarterTurnover);
				return false;
			}

			//Last turnover min £1,000
			if (lastTurnover < Constants.ApprovalMinLastTurnover)
			{
				reason = string.Format("Not Approved. Last turnover less then {0}", Constants.ApprovalMinLastTurnover);
				return false;
			}

			//seniority of min 12 months
			if (seniorityMonth < Constants.ApprovalMinSeniorityMonths)
			{
				reason = string.Format("Not Approved. Seniority Months less then {0}", Constants.ApprovalMinSeniorityMonths);
				return false;
			}

			//age min 22, max 60
			if (age <= Constants.ApprovalMinAge || age >= Constants.ApprovalMaxAge)
			{
				reason = string.Format("Not Approved. Age not in range {0} - {1}", Constants.ApprovalMinAge, Constants.ApprovalMaxAge);
				return false;
			}

			//There is not decrease of turnover by more than 15%: in any of M / Q / 12M for each of the stores
			//TODO

			//no late payments of more than 7 days and no rollovers in the past
			//TODO

			//there is max 1 loan outstanding 
			//TODO

			//and the total outstanding principal is less than 50% of original approved amount
			//TODO

			amount = (int)(Math.Min(Math.Min(anualTurnover, quarterTurnover * 4), lastTurnover * 12) * (double)medalRate);

			//The system can only approve automatically within these limits: min offer £1,000, max offer £10,000. Should the amount qualified be higher the system leaves it for manual decision
			if (amount < Constants.ApprovalMinAmount || amount > Constants.ApprovalMaxAmount)
			{
				reason = string.Format("Not Approved. Approve amount not in range {0} - {1}", Constants.ApprovalMinAmount, Constants.ApprovalMaxAmount);
				return false;
			}

			//max outstanding offers per day (e.g. 200,000 GBP): if the limit is reached the system should stop automatic approvals and re-approvals
			//TODO

			//max issued amount per day (e.g. 150,000 GBP): if we issued more than the limit during the date, the automatic approvals and re-approvals should stop and the system should warn underwriters and managers about this
			//TODO

			
			reason = "Approve. Maybe approved (not all rules are checked)";
			return true;
		}
	}
}

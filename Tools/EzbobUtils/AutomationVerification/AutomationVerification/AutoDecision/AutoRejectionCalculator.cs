namespace AutomationCalculator.AutoDecision
{
	using AutoRejection;
	using Common;
	using Ezbob.Logger;

	public class AutoRejectionCalculator
    {
		private static ASafeLog _log ;
		private static RejectionConfigs _config;
		public AutoRejectionCalculator(ASafeLog log, RejectionConfigs configs)
		{
			_log = log;
			_config = configs;
		}

		public bool IsAutoRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper(_log);

			var data = dbHelper.GetRejectionDataOld(customerId);
			var mps = dbHelper.GetCustomerMarketPlaces(customerId);
			var paymentMps = dbHelper.GetCustomerPaymentMarketPlaces(customerId);
			var mpHelper = new MarketPlacesHelper(_log);
			var anualTurnover = mpHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Year);
			var threeMonthTurnover = mpHelper.GetTurnoverForPeriod(mps, TimePeriodEnum.Month3);
			var seniority = mpHelper.GetMarketPlacesSeniority(mps);

			data.AnualTurnover = anualTurnover;
			data.ThreeMonthTurnover = threeMonthTurnover;
			data.MpsSeniority = seniority;

			return IsAutoRejectedCalculator(data, out reason);
		}

		private bool IsAutoRejectedCalculator(RejectionData data, out string reason)
		{
			
			//0 Exceptions to the rejection rules:
			//1. Do not apply to clients that have been approved at least once before (even if the latest decision was rejection)
			if (data.WasApproved)
			{
				reason = "Not Rejected. Was approved for loan";
				return false;
			}
			//2. Do not apply to clients with total annual turnover above £250,000
			if (data.AnualTurnover >= _config.AutoRejectionException_AnualTurnover)
			{
				reason = string.Format("Not Rejected. Total Annual Turnover Above {0} ({1})", _config.AutoRejectionException_AnualTurnover, data.AnualTurnover);
				return false;
			}
			//3. Do not apply to clients with credit score above 800.
			if (data.ExperianScore >= _config.AutoRejectionException_CreditScore)
			{
				reason = string.Format("Not Rejected. Credit Score Above {0} ({1})", _config.AutoRejectionException_CreditScore, data.ExperianScore);
				return false;
			}

			//4. Do not autoreject if company score is above 40
			if (data.CompanyScore >= _config.RejectionExceptionMaxCompanyScore)
			{
				reason = string.Format("Not Rejected. Company Credit Score Above {0} ({1})", _config.RejectionExceptionMaxCompanyScore, data.CompanyScore);
				return false;
			}

			//Marketplace with error exists for this customer AND consumer score is above 500 or business score is above 10
			if (data.HasErrorMp && (data.ExperianScore > _config.RejectionExceptionMaxConsumerScoreForMpError || data.CompanyScore > _config.RejectionExceptionMaxCompanyScoreForMpError))
			{
				reason = string.Format("Not Rejected. Marketplace with error exists for this customer AND consumer score is above {0} or business score is above {1} ({2},{3})",
					_config.RejectionExceptionMaxConsumerScoreForMpError, _config.RejectionExceptionMaxCompanyScoreForMpError, data.ExperianScore, data.CompanyScore);
				return false;
			}

			if (data.IsBrokerLead) {
				reason = string.Format("Not Rejected. Customer is via broker");
				return false;
			}

			//1.1  Low credit score: less than 500
			if (data.ExperianScore < _config.LowCreditScore && data.ExperianScore > 0)
			{
				reason = string.Format("Rejected. Max Credit Score Below {0} ({1})", _config.LowCreditScore, data.ExperianScore);
				return true;
			}

			//1.2  Low company credit score: less than 500
			if (data.CompanyScore < _config.RejectionCompanyScore)
			{
				reason = string.Format("Rejected. Max Company Credit Score Below {0} ({1})", _config.RejectionCompanyScore, data.CompanyScore);
				return true;
			}

			//TODO understand from Vitas exact rules for turnover calculation!!!

			//2. Low turnover, one of the following :

			//has payment mps
			bool hasSpecialMps = false;
			if (data.HasCompanyFiles)
			{
				hasSpecialMps = true;
			}
			else
			{
				//a Total annual turnover is less than 10,000 GBP
				if (data.AnualTurnover < _config.TotalAnnualTurnover)
				{
					reason = string.Format("Rejected. Annual Turnover Below {0} ({1})", _config.TotalAnnualTurnover, data.AnualTurnover);
					return true;
				}
				
				//b Total 3-month turnover is less than 2.000 GBP
				
				if (data.ThreeMonthTurnover < _config.TotalThreeMonthTurnover)
				{
					reason = string.Format("Rejected. 3 Month Turnover Below {0} ({1})", _config.TotalThreeMonthTurnover,
					                       data.ThreeMonthTurnover);
					return true;
				}
			}

			//3 Defaults:
			//a for clients with credit score below 800: at least 1 default in amount of 300+ GBP on any of the financial accounts in the last 24 months
			if (data.ExperianScore < _config.Reject_Defaults_CreditScore && data.DefaultAccountAmount >= _config.Reject_Defaults_Amount && data.DefaultAccountsNum > _config.Reject_Defaults_AccountsNum)
			{
				reason = string.Format("Rejected. Has Default {4} Account{2} of {3} GBP And credit score below {0} ({1})", _config.Reject_Defaults_CreditScore, data.ExperianScore, data.DefaultAccountsNum > 1 ? "s" : "", data.DefaultAccountAmount, data.DefaultAccountsNum);
				return true;
			}

			//b for clients with business credit score below 20: at least 1 default in amount of 1,000+ GBP on any of the financial accounts in the last 24 months,
			if (data.CompanyScore < _config.Reject_Defaults_CompanyScore &&
			    data.DefaultCompanyAccountsNum >= _config.Reject_Defaults_CompanyAccountsNum &&
			    data.DefaultCompanyAccountAmount > _config.Reject_Defaults_CompanyAmount) {
					reason = string.Format("Rejected. Has Company Default {4} Account{2} of {3} GBP And company credit score below {0} ({1})", _config.Reject_Defaults_CompanyScore,
						data.ExperianScore, data.DefaultCompanyAccountsNum > 1 ? "s" : "", data.DefaultCompanyAccountAmount, data.DefaultCompanyAccountsNum);
				return true;
			}

			//c for clients who are late over 30 days in at least 2 different accounts in the last 3 months
			if (data.NumLateAccounts >= _config.Reject_NumOfLateAccounts) {
				reason = string.Format("Rejected. Late in {0} account{1} over {2} days in the last {3} months", data.NumLateAccounts, data.NumLateAccounts > 1 ? "s" : "", _config.RejectionLastValidLate*30, _config.Reject_NumOfLateAccounts);
				return true;
			}

			//4 Seniority: Marketplace seniority less than 11 months (currently 300 days)
			if (data.MpsSeniority < _config.Reject_Minimal_Seniority)
			{
				reason = string.Format("Rejected. MP Seniority below {0} ({1})", _config.Reject_Minimal_Seniority, data.MpsSeniority);
				return true;
			}

			//5 Tangible Equity (TE) value in the last year is negative and there is a decrease of the TE over the last 2 years)
			//todo or not todo?

			//6 For clients with a customer status which is not: 1. enabled, 2. Fraud suspect
			if (data.CustomerStatus != "Enabled" || data.CustomerStatus != "Fraud Suspect")
			{
				reason = string.Format("Rejected. Customer Status is not 1. Enabled, 2. Fraud suspect ({0})", data.CustomerStatus);
				return true;
			}

			reason = string.Format("Not Rejected. None of the auto rejection rules match. {0}", hasSpecialMps ? "(Has company files)" : "");
			return false;
		}
    }
}

namespace AutomationCalculator.AutoDecision
{
	using Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoReRejectionCalculator
	{
		private static ASafeLog _log;
		private readonly AConnection m_oDB;

		public AutoReRejectionCalculator(AConnection db, ASafeLog log) {
			m_oDB = db;
			_log = log;
		}

		public bool IsAutoReRejected(int customerId, int cashRequestId, out string reason)
		{
			var dbHelper = new DbHelper(m_oDB, _log);
			var rerejectionData = dbHelper.GetReRejectionData(customerId, cashRequestId);

			var days = rerejectionData.ManualRejectDate.HasValue ? (rerejectionData.AutomaticDecisionDate - rerejectionData.ManualRejectDate.Value).Days : 0;

			if (rerejectionData.IsNewClient)
			{
				if (rerejectionData.ManualRejectDate.HasValue && 
					days < Constants.ManualDecisionDateRangeDays && 
					!rerejectionData.NewDataSourceAdded)
				{
					
					reason = string.Format("ReRejection. New Client. Application within min date range ({0} days, manual decision date {1}) and no new data sources added", days, rerejectionData.ManualRejectDate.Value.ToString("dd/MM/yyyy"));
					return true;
				}
			}
			else //Old Client
			{
				var repaymentPercent = rerejectionData.RepaidAmount/rerejectionData.LoanAmount;
				if (rerejectionData.ManualRejectDate.HasValue && 
					days < Constants.ManualDecisionDateRangeDays && 
					!rerejectionData.NewDataSourceAdded && 
					repaymentPercent < Constants.MinRepaidPrincipalPercent)
				{
					reason = string.Format("ReRejection. Old Client. Application within min date range ({0} days, manual decision date {2}) and no new data sources added and not repaid min principal amount ({1}% repaid)", days, repaymentPercent * 100, rerejectionData.ManualRejectDate.Value.ToString("dd/MM/yyyy"));
					return true;
				}
			}
			reason = "No ReRejection. None of the auto rerejection rules match";
			return false;
		}
    }
}

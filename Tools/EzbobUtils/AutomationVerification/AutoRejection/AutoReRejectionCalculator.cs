namespace AutomationCalculator
{
	using System;
	using CommonLib;
	using Ezbob.Logger;

	public class AutoReRejectionCalculator
	{
		private static ASafeLog _log;
		public AutoReRejectionCalculator(ASafeLog log)
		{
			_log = log;
		}

		public bool IsAutoReRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper(_log);
			var rerejectionData = dbHelper.GetReRejectionData(customerId);
			
			var days = rerejectionData.ManualRejectDate.HasValue ? (rerejectionData.ManualRejectDate.Value.AddDays(Constants.ManualDecisionDateRangeDays) - DateTime.UtcNow).Days : 0;

			if (rerejectionData.IsNewClient)
			{
				if (rerejectionData.ManualRejectDate.HasValue && rerejectionData.ManualRejectDate.Value.AddDays(Constants.ManualDecisionDateRangeDays) >= DateTime.UtcNow &&
				    !rerejectionData.NewDataSourceAdded)
				{
					
					reason = string.Format("ReRejection. New Client. Application within min date range ({0} days, manual decision date {1}) and no new data sources added", days, rerejectionData.ManualRejectDate.Value.ToString("dd/MM/yyyy"));
					return true;
				}
			}
			else //Old Client
			{
				var repaymentPercent = rerejectionData.RepaidAmount/rerejectionData.LoanAmount;
				if (rerejectionData.ManualRejectDate.HasValue && rerejectionData.ManualRejectDate.Value.AddDays(Constants.ManualDecisionDateRangeDays) >= DateTime.UtcNow &&
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

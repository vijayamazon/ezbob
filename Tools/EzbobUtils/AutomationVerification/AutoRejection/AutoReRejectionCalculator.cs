﻿namespace AutomationCalculator
{
	using System;
	using CommonLib;

	public class AutoReRejectionCalculator
    {
		public bool IsAutoReRejected(int customerId, out string reason)
		{
			var dbHelper = new DbHelper();
			var rerejectionData = dbHelper.GetReRejectionData(customerId);
			
			if (rerejectionData.IsNewClient)
			{
				if (rerejectionData.ManualRejectDate.HasValue && rerejectionData.ManualRejectDate.Value.AddDays(Constants.ManualDecisionDateRangeDays) >= DateTime.UtcNow &&
				    !rerejectionData.NewDataSourceAdded)
				{
					reason = "ReRejection. New Client. Application within min date range and no new data sources added";
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
					reason = "ReRejection. Old Client. Application within min date range and no new data sources added and not repaid min principal amount";
					return true;
				}
			}
			reason = "No ReRejection. None of the auto rerejection rules match";
			return false;
		}
    }
}

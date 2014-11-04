namespace AutomationCalculator.AutoDecision
{
	using System;
	using Common;
	using Ezbob.Logger;

	public class AutoReApprovalCalculator
    {
		private static ASafeLog _log;

		public AutoReApprovalCalculator(ASafeLog log)
		{
			_log = log;
		}

		public bool IsAutoReApproved(int customerId, int cashRequestId, out string reason, out int amount)
		{
			var dbHelper = new DbHelper(_log);
			var reApprovalData = dbHelper.GetReApprovalData(customerId, cashRequestId);
			amount = 0;
			decimal remainingAmount = 0;
			if (!reApprovalData.ManualApproveDate.HasValue)
			{
				reason = string.Format("No ReApprove. Wasn't manual approved");
				return false;
			}

			if (reApprovalData.IsNewClient)
			{
				if (reApprovalData.ManualApproveDate.HasValue &&
				    reApprovalData.ManualApproveDate.Value.AddDays(Constants.NewReApprovePeriodDays) < DateTime.Today)
				{
					reason = string.Format("No ReApprove. New client. Was manual approved more then {0} days ago", Constants.NewReApprovePeriodDays);
					return false;
				}

				if (!reApprovalData.TookLoanLastRequest)
				{
					amount = reApprovalData.OfferedAmount;
					reason =
						@"ReApprove. New client. Full amount if the offer was made within the last 30 days and the client did not take a loan from that offer. 
					           Only last underwriter offer can be reapproved (in case there have been other offers previously and got overrun by the last one).";
					return true;

				}
				else
				{
					remainingAmount = reApprovalData.OfferedAmount - reApprovalData.TookAmountLastRequest;
					if (remainingAmount >= Constants.NewMinReApproveAmount)
					{
						amount = (int) remainingAmount;
						reason =
							@"ReApprove. New client. Remaining amount [=offered amount minus amount of loan(s) taken from this offer] – can only be done within the last 30 days from the that offer.
								   Minimum remaining amount to reapprove is £1,000.";
						return true;
					}
					else
					{
						reason = string.Format("No ReApprove. New Client. Remaining amount less then {0}", Constants.NewMinReApproveAmount);
						return false;
					}
				}
			}
			else //Old Client
			{
				if (reApprovalData.ManualApproveDate.HasValue &&
					reApprovalData.ManualApproveDate.Value.AddDays(Constants.OldReApprovePeriodDays) < DateTime.Today)
				{
					reason = string.Format("No ReApprove. Old client. Was manual approved more then {0} days ago", Constants.OldReApprovePeriodDays);
					return false;
				}

				if (reApprovalData.NewDataSourceAdded)
				{
					reason = string.Format("No ReApprove. Old client. No new data source was added");
					return false;
				}

				if (reApprovalData.WasLate)
				{
					reason = string.Format("No ReApprove. Old client. Was late");
					return false;
				}

				if (reApprovalData.PrincipalRepaymentsSinceOffer > 0)
				{
					reason = string.Format("No ReApprove. Old client. There were repayments of principal since last offer");
					return false;
				}

				if (!reApprovalData.TookLoanLastRequest)
				{
					amount = reApprovalData.OfferedAmount;
					reason = "ReApprove. Old Customer. Full amount, all of the rules match.";
					return true;
				}
				else
				{
					remainingAmount = reApprovalData.OfferedAmount - reApprovalData.TookAmountLastRequest;
					if (remainingAmount >= Constants.OldMinReApproveAmount)
					{
						amount = (int)remainingAmount;
						reason = "ReApprove. Old Customer. Remaining amount, all of the rules match.";
						return true;
					}
					else
					{
						reason = string.Format("No ReApprove. Old Client. Remaining amount less then {0}", Constants.OldMinReApproveAmount);
						return false;
					}
				}
			}
		}
    }
}

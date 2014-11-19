
namespace AutomationCalculator.AutoDecision.AutoReRejection
{
	using System;
	using Newtonsoft.Json;
	using ProcessHistory;

	public class ReRejectInputData : ITrailInputData
	{
		public void Init(DateTime dataAsOf, bool wasManuallyRejected, DateTime? lastManualRejectDate, bool newDataSourceAdded, int openLoansAmount, decimal principalRepaymentAmount, bool hasLoans, decimal autoReRejectMinRepaidPortion, int autoReRejectMaxLRDAge)
		{
			DataAsOf = dataAsOf;

			WasManuallyRejected = wasManuallyRejected;
			LastManualRejectDate = lastManualRejectDate;
			NewDataSourceAdded = newDataSourceAdded;
			OpenLoansAmount = openLoansAmount;
			PrincipalRepaymentAmount = principalRepaymentAmount;
			HasLoans = hasLoans;

			//config
			AutoReRejectMaxLRDAge = autoReRejectMaxLRDAge;
			AutoReRejectMinRepaidPortion = autoReRejectMinRepaidPortion;

		}
		public DateTime DataAsOf { get; private set; }

		public bool WasManuallyRejected { get; private set; }
		public DateTime? LastManualRejectDate { get; private set; }
		public bool NewDataSourceAdded { get; private set; }
		public int OpenLoansAmount { get; private set; }
		public decimal PrincipalRepaymentAmount { get; private set; }
		public bool HasLoans { get; private set; }

		//config
		public decimal AutoReRejectMinRepaidPortion { get; private set; }
		public int AutoReRejectMaxLRDAge { get; private set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}

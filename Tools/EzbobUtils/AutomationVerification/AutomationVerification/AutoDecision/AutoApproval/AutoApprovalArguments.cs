namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoApprovalArguments {
		public AutoApprovalArguments(
			int customerID,
			long? cashRequestID,
			long? nlCashRequestID,
			decimal systemCalculatedAmount,
			Medal medal,
			MedalType medalType,
			TurnoverType? turnoverType,
			AutoDecisionFlowTypes flowType,
			bool errorInLGData,
			string tag,
			DateTime now,
			AConnection db,
			ASafeLog log
		) {
			CustomerID = customerID;
			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
			SystemCalculatedAmount = systemCalculatedAmount;
			Medal = medal;
			MedalType = medalType;
			TurnoverType = turnoverType;
			FlowType = flowType;
			ErrorInLGData = errorInLGData;
			Tag = tag;
			Now = now;
			DB = db;
			Log = log.Safe();
			TrailUniqueID = Guid.NewGuid();
		} // constructor

		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
		public long? NLCashRequestID { get; private set; }
		public decimal SystemCalculatedAmount { get; private set; }
		public Medal Medal { get; private set; }
		public MedalType MedalType { get; private set; }
		public TurnoverType? TurnoverType { get; private set; }

		public AutoDecisionFlowTypes FlowType { get; private set; }

		public bool ErrorInLGData { get; private set; }

		public string Tag { get; private set; }
		public DateTime Now { get; private set; }
		public AConnection DB { get; private set; }
		public ASafeLog Log { get; private set; }

		public Guid TrailUniqueID { get; private set; }
	} // class AutoApprovalArguments
} // namespace

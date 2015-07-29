﻿namespace AutomationCalculator.AutoDecision.AutoApproval {
	using AutomationCalculator.Common;

	/// <summary>
	///     Contains input arguments for auto approval logic.
	/// </summary>
	public class Arguments {
		public int CustomerID { get; private set; }
		public long? CashRequestID { get; private set; }
		public Medal Medal { get; private set; }
		public MedalType MedalType { get; private set; }
		public TurnoverType? TurnoverType { get; private set; }
		public decimal SystemCalculatedAmount { get; private set; }

		public Arguments() {}

		public Arguments(
			int nCustomerID,
			long? cashRequestID,
			decimal nSystemCalculatedAmount,
			Medal nMedal,
			MedalType medalType,
			TurnoverType? turnoverType
		) {
			CustomerID = nCustomerID;
			CashRequestID = cashRequestID;
			SystemCalculatedAmount = nSystemCalculatedAmount;
			Medal = nMedal;
			MedalType = medalType;
			TurnoverType = turnoverType;
		} // constructor
	} // Arguments
} // namespace

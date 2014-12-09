﻿namespace AutomationCalculator.AutoDecision.AutoApproval {
	using AutomationCalculator.Common;

	/// <summary>
	/// Contains input arguments for auto approval logic.
	/// </summary>
	public class Arguments {

		public Arguments() {}

		public Arguments(int nCustomerID, decimal nSystemCalculatedAmount, Medal nMedal) {
			CustomerID = nCustomerID;
			SystemCalculatedAmount = nSystemCalculatedAmount;
			Medal = nMedal;
		} // constructor

		public int CustomerID { get; private set; }
		public decimal SystemCalculatedAmount { get; private set; }
		public Medal Medal { get; private set; }
	} // Arguments
} // namespace

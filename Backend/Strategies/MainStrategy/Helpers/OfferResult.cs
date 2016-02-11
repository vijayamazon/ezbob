namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using AutomationCalculator.Common;
	using Ezbob.Database;

	public class OfferResult : IEquatable<OfferOutputModel> {
		// Inputs
		public int CustomerId { get; set; }
		public DateTime CalculationTime { get; set; }
		public int Amount { get; set; }
		public EZBob.DatabaseLib.Model.Database.Medal MedalClassification { get; set; }
		public int? GradeID { get; set; }
		public int? SubGradeID { get; set; }
		public long? CashRequestID { get; set; }
		public long? NLCashRequestID { get; set; }

		// Outputs
		public AutoDecisionFlowTypes FlowType { get; set; }
		public string ScenarioName { get; set; }
		public int Period { get; set; }
		public int LoanTypeId { get; set; }
		public int LoanSourceId { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public string Message { get; set; }
		public bool IsError { get; set; }
		public bool IsMismatch { get; set; }
		public bool HasDecision { get; set; }

		public bool Equals(OfferOutputModel other) {
			Message = Message ?? string.Empty;

			if (ScenarioName != other.ScenarioName) {
				Message += " Mismatch in pricing scenario";
				IsMismatch = true;
				IsError = true;
				return false;
			} // if

			if (Math.Abs(InterestRate - other.InterestRate) == 0.05M || Math.Abs(SetupFee - other.SetupFee) == 0.5M) {
				Message += " Mismatch due to rounding issues";
				IsMismatch = true;
				return true;
			} // if

			if (InterestRate == other.InterestRate && SetupFee == other.SetupFee)
				return true;

			IsMismatch = true;
			IsError = true;
			return false;
		} // Equals

		public void SaveToDb(AConnection db) {
			db.ExecuteNonQuery(
				"StoreOffer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId),
				new QueryParameter("CalculationTime", CalculationTime),
				new QueryParameter("Amount", Amount),
				new QueryParameter("MedalClassification", MedalClassification.ToString()),
				new QueryParameter("ScenarioName", ScenarioName),
				new QueryParameter("Period", Period),
				new QueryParameter("LoanTypeId", LoanTypeId),
				new QueryParameter("InterestRate", InterestRate),
				new QueryParameter("SetupFee", SetupFee),
				new QueryParameter("Error", Description),
				new QueryParameter("GradeID", GradeID),
				new QueryParameter("SubGradeID", SubGradeID),
				new QueryParameter("CashRequestID", CashRequestID),
				new QueryParameter("NLCashRequestID", NLCashRequestID)
			);
		} // SaveToDb

		public override string ToString() {
			return string.Format(
				"Amount: {5}, InterestRate {0}, SetupFee: {1}, RepaymentPeriod: {2}, LoanType: {3}, {4}",
				InterestRate, SetupFee, Period, LoanTypeId, Description, Amount
			);
		} // ToString

		public string Description {
			get {
				if (!IsError && !IsMismatch)
					return (Message ?? string.Empty).Trim();

				string description = string.Format(
					"{0}. {1}. {2} has been found in the requested range. {3}",
					IsError ? "Error" : "No error",
					IsMismatch ? "Mismatch" : "Match",
					HasDecision ? "Decision" : "No decision",
					(Message ?? string.Empty).Trim()
				);

				return description.Trim();
			} // get
		} // Description
	} // class OfferResult
} // namespace


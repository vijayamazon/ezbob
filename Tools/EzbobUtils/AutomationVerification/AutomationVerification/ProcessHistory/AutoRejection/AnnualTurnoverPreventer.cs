namespace AutomationCalculator.ProcessHistory.AutoRejection
{
	public class AnnualTurnoverPreventer : AThresholdTrace {
		#region property AllowMismatch

		/// <summary>
		/// Mismatch is allowed because currently turnovers are match only if it is a miracle (different implementations).
		/// </summary>
		public override bool AllowMismatch {
			get { return true; }
		} // AllowMismatch

		#endregion property AllowMismatch

		public AnnualTurnoverPreventer(DecisionStatus nDecisionStatus) : base(nDecisionStatus) {} // constructor

		protected override string ValueName {
			get { return "Annual turnover exception"; }
		} // ValueName
	}  // class
} // namespace

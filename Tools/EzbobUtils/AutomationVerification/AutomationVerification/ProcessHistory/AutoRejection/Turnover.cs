﻿namespace AutomationCalculator.ProcessHistory.AutoRejection {
	public class Turnover : ATrace {
		public Turnover(DecisionStatus nDecisionStatus) : base(nDecisionStatus) { }

		/// <summary>
		/// Mismatch is allowed because currently turnovers are match only if it is a miracle (different implementations).
		/// </summary>
		public override bool AllowMismatch {
			get { return true; }
		} // AllowMismatch

		public DataModel Model { get; private set; }

		public void Init(DataModel model) {
			Model = model;
			Comment = string.Format(
				"customer annual turnover £{0:N0} (min allowed £{1:N0}), " +
				"quarter turnover £{2:N0} (min allowed £{3:N0}), " +
				"and has {4} company files",
				model.AnnualTurnover,
				model.AnnualTurnoverThreshhold,
				model.QuarterTurnover,
				model.QuarterTurnoverThreshhold,
				model.HasCompanyFiles ? string.Empty : "no"
			);
		} // Init

		public class DataModel {
			public bool HasCompanyFiles { get; set; }
			public decimal AnnualTurnover { get; set; }
			public decimal AnnualTurnoverThreshhold { get; set; }
			public decimal QuarterTurnover { get; set; }
			public decimal QuarterTurnoverThreshhold { get; set; }
		} // class DataModel
	} // class Turnover
} // namespace

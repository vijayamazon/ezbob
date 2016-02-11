namespace Ezbob.Backend.Strategies.StoredProcs {
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	internal class LoadMatchingGradeRanges : AStoredProcedure {
			public LoadMatchingGradeRanges(AConnection db, ASafeLog log) : base(db, log) {
			} // constructor

			public override bool HasValidParameters() {
				return (OriginID > 0) && (LoanSourceID > 0) && (Score > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int OriginID { get; set; }

			[UsedImplicitly]
			public bool IsRegulated { get; set; }

			[UsedImplicitly]
			public decimal Score { get; set; }

			[UsedImplicitly]
			public int LoanSourceID { get; set; }

			[UsedImplicitly]
			public bool IsFirstLoan { get; set; }

			public void Execute(MatchingGradeRanges target) {
				target.Clear();

				ForEachRowSafe(sr => target.Add(new MatchingGradeRanges.SubproductGradeRange {
					ProductSubTypeID = sr["ProductSubTypeID"],
					GradeRangeID = sr["GradeRangeID"],
				}));
			} // Execute
		} // class LoadMatchingGradeRanges
} // namespace

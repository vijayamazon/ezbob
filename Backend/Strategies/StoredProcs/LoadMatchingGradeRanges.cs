namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	internal class LoadMatchingGradeRanges : AStoredProcedure {
		public LoadMatchingGradeRanges(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return (CustomerID > 0) && (Now > longTimeAgo);
		} // HasValidParameters

		[UsedImplicitly]
		public int CustomerID { get; set; }

		[UsedImplicitly]
		public int? CompanyID { get; set; }

		[UsedImplicitly]
		public DateTime Now { get; set; }

		public void Execute(MatchingGradeRanges target) {
			if (target == null)
				throw new ArgumentNullException("target", "Target list of grade ranges is NULL.");

			target.Clear();

			ForEachRowSafe(sr => target.Add(new MatchingGradeRanges.SubproductGradeRange {
				ProductSubTypeID = sr["ProductSubTypeID"],
				GradeRangeID = sr["GradeRangeID"],
			}));
		} // Execute

		private static readonly DateTime longTimeAgo = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
	} // class LoadMatchingGradeRanges
} // namespace

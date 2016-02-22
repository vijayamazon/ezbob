namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using AutomationCalculator.AutoDecision.AutoRejection.Models;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadOfferRanges {
		public LoadOfferRanges(int gradeRangeID, int productSubTypeID, AConnection db, ASafeLog log) : this(db, log) {
			this.workingMode = WorkingMode.SpecificRange;

			GradeRangeID = gradeRangeID;
			ProductSubTypeID = productSubTypeID;
		} // constructor

		public LoadOfferRanges(int customerID, int? companyID, DateTime now, AConnection db, ASafeLog log) : this(db, log) {
			this.workingMode = WorkingMode.Search;

			this.customerID = customerID;
			this.companyID = companyID;
			this.now = now;
		} // constructor

		public LoadOfferRanges Execute() {
			LoadRanges();

			if (this.matchingGradeRanges.Count != 1) {
				this.log.Alert("Failed to find one grade range for {0}.", this);
				return this;
			} // if

			GradeRangeID = this.matchingGradeRanges[0].GradeRangeID;
			ProductSubTypeID = this.matchingGradeRanges[0].ProductSubTypeID;

			SafeReader sr = this.db.GetFirst(
				"LoadGradeRangeAndSubproduct",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@GradeRangeID", GradeRangeID),
				new QueryParameter("@ProductSubTypeID", ProductSubTypeID)
			);

			if (sr.IsEmpty) {
				this.log.Alert(
					"Failed to load grade range and product subtype by grade range id {0} and product sub type id {1}.",
					GradeRangeID,
					ProductSubTypeID
				);

				return this;
			} // if

			GradeRangeSubproduct = sr.Fill<GradeRangeSubproduct>();
			Success = true;

			return this;
		} // Execute

		public void ExportMatchingGradeRanges(MatchingGradeRanges target) {
			if (target == null)
				return;

			target.Clear();

			if (this.matchingGradeRanges == null)
				return;

			target.AddRange(this.matchingGradeRanges);
		} // ExportMatchingGradeRanges

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			string description;

			switch (this.workingMode) {
			case WorkingMode.Search:
				description = string.Format(
					"customer '{0}', company '{1}', now '{2}'",
					this.customerID,
					this.companyID.HasValue ? this.companyID.Value.ToString() : "-- null --",
					this.now.MomentStr()
				);
				break;

			case WorkingMode.SpecificRange:
				description = string.Format(
					"grade range '{0}', product subtype '{1}'",
					GradeRangeID,
					ProductSubTypeID
				);
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			return string.Format("offer range loader by {0}", description);
		} // ToString

		public bool Success { get; private set; }

		public GradeRangeSubproduct GradeRangeSubproduct { get; private set; }

		public int GradeRangeID { get; private set; }
		public int ProductSubTypeID { get; private set; }

		private LoadOfferRanges(AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();
			this.matchingGradeRanges = new MatchingGradeRanges();

			Success = false;
		} // constructor

		private void LoadRanges() {
			switch (this.workingMode) {
			case WorkingMode.Search:
				var spRanges = new LoadMatchingGradeRanges(this.db, this.log) {
					CustomerID = this.customerID,
					CompanyID = this.companyID,
					Now = this.now,
				};
				spRanges.Execute(this.matchingGradeRanges);

				break;

			case WorkingMode.SpecificRange:
				this.matchingGradeRanges.Add(new MatchingGradeRanges.SubproductGradeRange {
					GradeRangeID = GradeRangeID,
					ProductSubTypeID = ProductSubTypeID,
				});
				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // LoadRanges

		private enum WorkingMode {
			Search,
			SpecificRange,
		} // enum WorkingMode

		private readonly WorkingMode workingMode;
		private readonly int customerID;
		private readonly int? companyID;
		private readonly DateTime now;
		private readonly AConnection db;
		private readonly ASafeLog log;

		private readonly MatchingGradeRanges matchingGradeRanges;
	} // class LoadOfferRanges
} // namespace

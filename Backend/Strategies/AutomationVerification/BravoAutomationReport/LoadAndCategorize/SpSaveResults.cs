namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.LoadAndCategorize {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpSaveResults : AStoredProcedure {
		public SpSaveResults(string tag, AConnection db, ASafeLog log) : base(db, log) {
			TrailTag = tag;
			ItemsToStore = new List<Item>();
		} // constructor

		public string TrailTag { get; set; }

		[FieldName("Lst")]
		public List<Item> ItemsToStore { get; set; }

		public override bool HasValidParameters() {
			return (ItemsToStore != null) && (ItemsToStore.Count > 0);
		} // HasValidParameters

		/// <summary>
		/// Returns the name of the stored procedure.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			return "BAR_SaveResults";
		} // GetName

		internal class Item {
			public int CustomerID { get; set; }
			public long FirstCashRequestID { get; set; }
			public int? AutoDecisionID { get; set; }
			public int? ManualDecisionID { get; set; }
			public bool HasEnoughData { get; set; }
			public bool IsOldCustomer { get; set; }
			public bool HasSignature { get; set; }
			public Guid? AutoApproveTrailUniqueID { get; set; }
		} // class Item
	} // class SpSaveResults
} // namespace

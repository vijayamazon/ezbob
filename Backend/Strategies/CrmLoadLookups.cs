namespace EzBob.Backend.Strategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class CrmLoadLookups : AStrategy {
		#region constructor

		public CrmLoadLookups(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Actions = new SortedDictionary<int, string>();
			Statuses = new SortedDictionary<int, string>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "CRM load lookups"; } // get
		} // Name

		#endregion property Name

		#region property Actions

		public virtual SortedDictionary<int, string> Actions { get; private set; } // Actions

		#endregion property Actions

		#region property Statuses

		public virtual SortedDictionary<int, string> Statuses { get; private set; } // Statuses

		#endregion property Statuses

		#region class Entry (helper)

		public class Entry : ITraversable {
			public int ID { get; set; }
			public string Name { get; set; }
		} // class Entry

		#endregion class Entry (helper)

		#region method Execute

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Entry e = sr.Fill<Entry>();
					Actions[e.ID] = e.Name;
					return ActionResult.Continue;
				},
				"CrmLoadActions",
				CommandSpecies.StoredProcedure
			);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Entry e = sr.Fill<Entry>();
					Statuses[e.ID] = e.Name;
					return ActionResult.Continue;
				},
				"CrmLoadStatuses",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		#endregion method Execute
	} // class CrmLoadLookups
} // namespace EzBob.Backend.Strategies

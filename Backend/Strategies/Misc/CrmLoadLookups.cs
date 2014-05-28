namespace EzBob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class CrmLoadLookups : AStrategy {
		#region constructor

		public CrmLoadLookups(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Actions = new List<Entry>();
			Statuses = new List<Entry>();
			Ranks = new List<Entry>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "CRM load lookups"; } // get
		} // Name

		#endregion property Name

		#region property Actions

		public virtual List<Entry> Actions { get; private set; } // Actions

		#endregion property Actions

		#region property Statuses

		public virtual List<Entry> Statuses { get; private set; } // Statuses

		#endregion property Statuses

		#region property Ranks

		public virtual List<Entry> Ranks { get; private set; } // Statuses

		#endregion property Ranks

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
					Actions.Add(e);
					return ActionResult.Continue;
				},
				"CrmLoadActions",
				CommandSpecies.StoredProcedure
			);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					Entry e = sr.Fill<Entry>();
					Statuses.Add(e);
					return ActionResult.Continue;
				},
				"CrmLoadStatuses",
				CommandSpecies.StoredProcedure
			);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) =>
				{
					Entry e = sr.Fill<Entry>();
					Ranks.Add(e);
					return ActionResult.Continue;
				},
				"CrmLoadRanks",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		#endregion method Execute
	} // class CrmLoadLookups
} // namespace EzBob.Backend.Strategies

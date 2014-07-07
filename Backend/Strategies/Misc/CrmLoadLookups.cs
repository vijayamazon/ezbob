namespace EzBob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class CrmLoadLookups : AStrategy {
		#region constructor

		public CrmLoadLookups(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Actions = new List<IdNameModel>();
			Statuses = new List<CrmStatusGroup>();
			Ranks = new List<IdNameModel>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "CRM load lookups"; } // get
		} // Name

		#endregion property Name

		#region property Actions

		public virtual List<IdNameModel> Actions { get; private set; } // Actions

		#endregion property Actions

		#region property Statuses

		public virtual List<CrmStatusGroup> Statuses { get; private set; } // Statuses

		#endregion property Statuses

		#region property Ranks

		public virtual List<IdNameModel> Ranks { get; private set; } // Statuses

		#endregion property Ranks

		#region method Execute

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					IdNameModel e = sr.Fill<IdNameModel>();
					Actions.Add(e);
					return ActionResult.Continue;
				},
				"CrmLoadActions",
				CommandSpecies.StoredProcedure
			);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					IdNameModel e = sr.Fill<IdNameModel>();
					Ranks.Add(e);
					return ActionResult.Continue;
				},
				"CrmLoadRanks",
				CommandSpecies.StoredProcedure
			);

			var oStatuses = new SortedDictionary<int, CrmStatusGroup>();

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nGroupID = sr["GroupID"];

					CrmStatusGroup grp = null;

					if (oStatuses.ContainsKey(nGroupID))
						grp = oStatuses[nGroupID];
					else {
						grp = new CrmStatusGroup {
							Id = nGroupID,
							Name = sr["GroupName"],
							Priority = sr["Priority"],
							Statuses = new List<IdNameModel>(),
						};

						oStatuses[grp.Id] = grp;
					} // if

					grp.Statuses.Add(new IdNameModel {
						Id = sr["StatusID"],
						Name = sr["StatusName"],
					});

					return ActionResult.Continue;
				},
				"CrmLoadStatuses",
				CommandSpecies.StoredProcedure
			);

			Statuses = oStatuses.Values.ToList();
		} // Execute

		#endregion method Execute
	} // class CrmLoadLookups
} // namespace EzBob.Backend.Strategies

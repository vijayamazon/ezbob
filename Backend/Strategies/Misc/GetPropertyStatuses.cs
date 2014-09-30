﻿namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetPropertyStatuses : AStrategy
	{
		public GetPropertyStatuses(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "GetPropertyStatuses"; }
		}

		public List<PropertyStatusGroupModel> Groups { get; set; }

		public override void Execute()
		{
			var groups = new Dictionary<string, PropertyStatusGroupModel>();

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				string title = sr["Title"];
				if (!groups.ContainsKey(title))
				{
					groups.Add(title, new PropertyStatusGroupModel { Title = title, Statuses = new List<PropertyStatusModel>() });
				}

				string description = sr["Description"];
				bool isOwnerOfMainAddress = sr["IsOwnerOfMainAddress"];
				bool isOwnerOfOtherProperties = sr["IsOwnerOfOtherProperties"];
				int id = sr["Id"];
				groups[title].Statuses.Add(new PropertyStatusModel { Description = description, IsOwnerOfMainAddress = isOwnerOfMainAddress, IsOwnerOfOtherProperties = isOwnerOfOtherProperties, Id = id });

				return ActionResult.Continue;
			}, "GetActivePropertyStatuses", CommandSpecies.StoredProcedure);

			Groups = new List<PropertyStatusGroupModel>();
			foreach (string key in groups.Keys)
			{
				Groups.Add(groups[key]);
			}
		}
	}
}

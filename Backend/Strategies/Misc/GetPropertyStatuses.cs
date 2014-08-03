namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
	using System.Data;
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
			DataTable dt = DB.ExecuteReader("GetActivePropertyStatuses", CommandSpecies.StoredProcedure);

			var groups = new Dictionary<string, PropertyStatusGroupModel>();
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);

				string title = sr["Title"];
				if (!groups.ContainsKey(title))
				{
					groups.Add(title, new PropertyStatusGroupModel { Title = title, Statuses = new List<PropertyStatusModel>() });
				}

				string description = sr["Description"];
				bool isOwner = sr["IsOwner"];
				int id = sr["Id"];
				groups[title].Statuses.Add(new PropertyStatusModel { Description = description, IsOwner = isOwner, Id = id });
			}

			Groups = new List<PropertyStatusGroupModel>();
			foreach (string key in groups.Keys)
			{
				Groups.Add(groups[key]);
			}
		}
	}
}

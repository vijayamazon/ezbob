namespace Ezbob.Backend.Strategies.Misc {
	using System.Text.RegularExpressions;
	using Ezbob.Database;

	public class BackfillZooplaValue : AStrategy {
		public override string Name {
			get { return "BackfillZooplaValue"; }
		}

		public override void Execute() {
			var regexObj = new Regex(@"[^\d]");
			var lst = DB.ExecuteEnumerable("GetZooplaStrEstimates", CommandSpecies.StoredProcedure);

			foreach (var sr in lst) {
				int zooplaEntryId = sr["Id"];
				string zooplaEstimateStr = sr["ZooplaEstimate"];

				var stringVal = string.IsNullOrEmpty(zooplaEstimateStr) ? "" : regexObj.Replace(zooplaEstimateStr.Trim(), "");
				int intVal;

				if (!int.TryParse(stringVal, out intVal))
					intVal = 0;

				DB.ExecuteNonQuery("SetZooplaIntEstimate", CommandSpecies.StoredProcedure,
					new QueryParameter("Id", zooplaEntryId),
					new QueryParameter("IntValue", intVal));
			}
		}
	}
}

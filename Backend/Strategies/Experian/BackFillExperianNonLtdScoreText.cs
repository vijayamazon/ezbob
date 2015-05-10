namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Xml;
	using EZBob.DatabaseLib.Model.Experian;
	using EZBob.DatabaseLib.Repository;
	using ExperianLib;
	using Ezbob.Database;
	using StructureMap;

	public class BackFillExperianNonLtdScoreText : AStrategy {


		public override string Name {
			get { return "BackFillExperianNonLtdScoreText"; }
		} // Name

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForNonLtdBackfill", CommandSpecies.StoredProcedure);

			foreach (SafeReader sr in lst) {
				long nServiceLogID = sr["Id"];
				string ResponseXML = sr["ResponseData"];

				try {

					XmlDocument doc = new XmlDocument();
					doc.LoadXml(ResponseXML);

					XmlNode node = doc.SelectSingleNode("/GEODS/REQUEST/DN74");

					string risk = TryRead(node, "RISKTEXT");
					string credit = TryRead(node, "CREDITTEXT");
					string conc = TryRead(node, "CONCLUDINGTEXT");
					string noc = TryRead(node, "NOCTEXT");
					string poss=TryRead(node, "POSSRELATEDTEXT");

					DB.ExecuteNonQuery(
								"UpdateExperianNonLtdScoreText",
								CommandSpecies.StoredProcedure,
								new QueryParameter("ServiceLogId", nServiceLogID),
								new QueryParameter("RiskText", risk),
								new QueryParameter("CreditText", credit),
								new QueryParameter("ConcludingText", conc),
								new QueryParameter("NocText", noc),
								new QueryParameter("PossiblyRelatedDataText", poss)
							);

				} catch (Exception ex) {
					Log.Warn(ex, "Failed to save experian nonlimited history for service log id {0}.", nServiceLogID);
				} // try
			} // for each
		} // Execute

		private string TryRead(XmlNode node, string attr) {
			try {
				return node[attr].InnerText;
			} catch (Exception e) {

				return null;
			}
		}

	} // class BackfillExperianNonLtdScoreText
} // namespace

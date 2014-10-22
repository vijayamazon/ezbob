namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Web;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SaveSourceRefHistory : AStrategy {
		#region public

		#region constructor

		public SaveSourceRefHistory(
			int nUserID,
			string sSourceRefList,
			string sVisitTimeList,
			CampaignSourceRef campaignSourceRef,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_nUserID = nUserID;
			m_sSourceRefList = (sSourceRefList ?? string.Empty).Trim();
			m_sVisitTimeList = (sVisitTimeList ?? string.Empty).Trim();
			m_CampaignSourceRef = campaignSourceRef;
			Log.Debug(
				"Will save sourceref history for user {0} from sourceref list '{1}' and visit time list '{2}'.",
				m_nUserID, m_sSourceRefList, m_sVisitTimeList
			);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "SaveSourceRefHistory"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Debug("Saving sourceref history for user {0}...", m_nUserID);

			var lst = new List<SourceRefEntry>();

			string[] arySourceRefs = (HttpUtility.UrlDecode(m_sSourceRefList) ?? string.Empty).Split(';');
			string[] aryVisitTimes = (HttpUtility.UrlDecode(m_sVisitTimeList) ?? string.Empty).Split(';');

			for (int i = 0; i < arySourceRefs.Length; i++) {
				string sSourceRef = arySourceRefs[i].Trim();

				if (string.IsNullOrWhiteSpace(sSourceRef))
					continue;

				DateTime? oVisitTime = null;

				if (i < aryVisitTimes.Length) {
					DateTime oTime;

					if (DateTime.TryParseExact(aryVisitTimes[i], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out oTime))
						oVisitTime = oTime;
				} // if

				lst.Add(new SourceRefEntry { SourceRef = sSourceRef, VisitTime = oVisitTime, });

				Log.Debug(
					"Sourceref entry for customer {0}: '{1}' on {2}.",
					m_nUserID,
					sSourceRef,
					oVisitTime.HasValue ? oVisitTime.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture) : "unknown"
				);
			} // for each sourceref

			if (lst.Count > 0) {
				Log.Msg(
					"{1} entr{2} in sourceref history for user {0} complete.",
					m_nUserID,
					lst.Count,
					lst.Count == 1 ? "y" : "ies"
				);

				DB.ExecuteNonQuery(
					"SaveSourceRefHistory",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserID", m_nUserID),
					DB.CreateTableParameter<SourceRefEntry>("Lst", lst)
				);
			}
			else
				Log.Msg("No sourceref history to save for user {0}.", m_nUserID);


			if (m_CampaignSourceRef != null) {
				m_CampaignSourceRef.RSource = m_CampaignSourceRef.RSource ?? "Direct";
				m_CampaignSourceRef.RDate = m_CampaignSourceRef.RDate ?? DateTime.UtcNow;
				DB.ExecuteNonQuery(
					"SaveCampaignSourceRef",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nUserID),
					DB.CreateTableParameter<CampaignSourceRef>("Tbl", new List<CampaignSourceRef>() { m_CampaignSourceRef })
				);
			}
			Log.Debug("Saving sourceref history for user {0} complete.", m_nUserID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nUserID;
		private readonly string m_sSourceRefList;
		private readonly string m_sVisitTimeList;
		private readonly CampaignSourceRef m_CampaignSourceRef;

		#region class SourceRefEntry

		private class SourceRefEntry {
			public string SourceRef { get; set; }
			public DateTime? VisitTime { get; set; }
		} // class SourceRefEntry

		#endregion class SourceRefEntry

		#endregion private
	} // class SaveSourceRefHistory
} // namespace

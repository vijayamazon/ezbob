namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Web;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using JetBrains.Annotations;

	public class SaveSourceRefHistory : AStrategy {
		public SaveSourceRefHistory(
			int nUserID,
			string sSourceRefList,
			string sVisitTimeList,
			CampaignSourceRef campaignSourceRef
		) {
			Transaction = null;

			this.userID = nUserID;
			this.sourceRefList = (sSourceRefList ?? string.Empty).Trim();
			this.visitTimeList = (sVisitTimeList ?? string.Empty).Trim();
			this.campaignSourceRef = campaignSourceRef;

			Log.Debug(
				"Will save sourceref history for user {0} from sourceref list '{1}' and visit time list '{2}'.",
				this.userID, this.sourceRefList, this.visitTimeList
			);
		} // constructor

		public override string Name {
			get { return "SaveSourceRefHistory"; }
		} // Name

		public ConnectionWrapper Transaction { get; set; }

		public override void Execute() {
			Log.Debug("Saving sourceref history for user {0}...", this.userID);

			var lst = new List<SourceRefEntry>();

			string[] arySourceRefs = (HttpUtility.UrlDecode(this.sourceRefList) ?? string.Empty).Split(';');
			string[] aryVisitTimes = (HttpUtility.UrlDecode(this.visitTimeList) ?? string.Empty).Split(';');

			for (int i = 0; i < arySourceRefs.Length; i++) {
				string sSourceRef = arySourceRefs[i].Trim();

				if (string.IsNullOrWhiteSpace(sSourceRef))
					continue;

				DateTime? oVisitTime = null;

				if (i < aryVisitTimes.Length) {
					DateTime oTime;

					bool isParsed = DateTime.TryParseExact(
						aryVisitTimes[i],
						"dd/MM/yyyy HH:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
						out oTime
					);

					if (isParsed)
						oVisitTime = oTime;
				} // if

				lst.Add(new SourceRefEntry { SourceRef = sSourceRef, VisitTime = oVisitTime, });

				Log.Debug(
					"Sourceref entry for customer {0}: '{1}' on {2}.",
					this.userID,
					sSourceRef,
					oVisitTime.HasValue
						? oVisitTime.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
						: "unknown"
				);
			} // for each sourceref

			if (lst.Count > 0) {
				Log.Msg(
					"{1} entr{2} in sourceref history for user {0} complete.",
					this.userID,
					lst.Count,
					lst.Count == 1 ? "y" : "ies"
				);

				DB.ExecuteNonQuery(
					Transaction,
					"SaveSourceRefHistory",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserID", this.userID),
					DB.CreateTableParameter<SourceRefEntry>("Lst", lst)
				);
			}
			else
				Log.Msg("No sourceref history to save for user {0}.", this.userID);

			if (this.campaignSourceRef != null) {
				this.campaignSourceRef.RSource = this.campaignSourceRef.RSource ?? "Direct";
				this.campaignSourceRef.RDate = this.campaignSourceRef.RDate ?? DateTime.UtcNow;
				DB.ExecuteNonQuery(
					Transaction,
					"SaveCampaignSourceRef",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.userID),
					DB.CreateTableParameter<CampaignSourceRef>("Tbl", new List<CampaignSourceRef> { this.campaignSourceRef })
				);
			}
			Log.Debug("Saving sourceref history for user {0} complete.", this.userID);
		} // Execute

		private readonly int userID;
		private readonly string sourceRefList;
		private readonly string visitTimeList;
		private readonly CampaignSourceRef campaignSourceRef;

		private class SourceRefEntry {
			public string SourceRef { [UsedImplicitly] get; set; }
			public DateTime? VisitTime { [UsedImplicitly] get; set; }
		} // class SourceRefEntry
	} // class SaveSourceRefHistory
} // namespace

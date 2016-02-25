namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using EchoSignService;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpSaveSignedDocument : AStoredProc {

		public SpSaveSignedDocument(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		public override bool HasValidParameters() {
			var oErrors = new List<string>();

			if (EsignatureID <= 0)
				oErrors.Add("Invalid EsignatureID.");

			if (string.IsNullOrWhiteSpace(MimeType))
				oErrors.Add("No MIME type was specified.");

			if (DocumentContent == null)
				oErrors.Add("No document content was specified.");
			else if (DocumentContent.Length <= 0)
				oErrors.Add("Document content is empty.");

			if (oErrors.Count > 0)
				Log.Warn("Cannot save signed document: {0}", string.Join(" ", oErrors));

			return oErrors.Count < 1;
		} // HasValidParameters

		public int EsignatureID { get; set; }

		public bool DoSaveDoc { get; set; }

		public int StatusID { get; set; }

		public string MimeType { get; set; }

		public byte[] DocumentContent { get; set; }

		public List<EsignerStatus> SignerStatuses { get; set; }

		public List<HistoryEvent> HistoryEvents { get; set; }

		public class EsignerStatus {
			public int EsignerID { get; set; }
			public int StatusID { get; set; }
			public DateTime? SignatureTime { get; set; }
		} // class EsignerStatus

		public class HistoryEvent {

            public HistoryEvent() { }

			public HistoryEvent(DocumentHistoryEvent oEvent) {
				EventTime = oEvent.date;
				Description = oEvent.description;
				VersionKey = oEvent.documentVersionKey;
				EventTypeID = (int)oEvent.type.Value;
				ActingUserEmail = oEvent.actingUserEmail;
				ActingUserIp = oEvent.actingUserIpAddress;
				ParticipantEmail = oEvent.participantEmail;
				Comment = oEvent.comment;
				Latitude = oEvent.deviceLocation == null ? null : oEvent.deviceLocation.latitude;
				Longitude = oEvent.deviceLocation == null ? null : oEvent.deviceLocation.longitude;
				SynchronizationKey = oEvent.synchronizationKey;
			} // constructor

			public DateTime EventTime { get; set; }
			public string Description { get; set; }
			public string VersionKey { get; set; }
			public int EventTypeID { get; set; }
			public string ActingUserEmail { get; set; }
			public string ActingUserIp { get; set; }
			public string ParticipantEmail { get; set; }
			public string Comment { get; set; }
			public double? Latitude { get; set; }
			public double? Longitude { get; set; }
			public string SynchronizationKey { get; set; }
		} // class HistoryEvent

	} // class SpSaveSignedDocument
} // namespace

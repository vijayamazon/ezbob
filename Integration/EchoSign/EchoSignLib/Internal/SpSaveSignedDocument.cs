namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using EchoSignService;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpSaveSignedDocument : AStoredProc {
		#region constructor

		public SpSaveSignedDocument(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		public override bool HasValidParameters() {
			return
				(EsignatureID > 0) &&
				!string.IsNullOrWhiteSpace(MimeType) &&
				(DocumentContent != null) &&
				(DocumentContent.Length > 0);
		} // HasValidParameters

		public int EsignatureID { get; set; }

		public bool DoSaveDoc { get; set; }

		public int StatusID { get; set; }

		public string MimeType { get; set; }

		public byte[] DocumentContent { get; set; }

		public List<EsignerStatus> SignerStatuses { get; set; }

		public List<HistoryEvent> HistoryEvents { get; set; }

		#region class EsignerStatus

		public class EsignerStatus {
			public int EsignerID { get; set; }
			public int StatusID { get; set; }
			public DateTime? SignatureTime { get; set; }
		} // class EsignerStatus

		#endregion class EsignerStatus

		#region class HistoryEvent

		public class HistoryEvent {
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

		#endregion class HistoryEvent
	} // class SpSaveSignedDocument
} // namespace

namespace EchoSignLib {
	using System.Collections.Generic;
	using System.Linq;
	using EchoSignService;

	internal class Esignature {
		public Esignature(int nSignatureID) {
			ID = nSignatureID;
			Signers = new SortedDictionary<int, Esigner>();
		} // constructor

		public int ID { get; private set; }
		public int CustomerID { get; set; }
		public string DocumentKey { get; set; }

		public SortedDictionary<int, Esigner> Signers { get; private set; }

		public List<SpSaveSignedDocument.EsignerStatus> SignerStatuses { get; private set; }

		public List<SpSaveSignedDocument.HistoryEvent> HistoryEvents { get; private set; }

		public void SetHistoryAndStatus(DocumentHistoryEvent[] events, ParticipantInfo[] participants) {
			SignerStatuses = SaveSignerStatus(participants);

			HistoryEvents = events
				.Where(e => e.type.HasValue)
				.Select(ConvertEvent)
				.ToList();
		} // SetHistoryAndStatus

		#region private

		#region method ConvertEvent

		private SpSaveSignedDocument.HistoryEvent ConvertEvent(DocumentHistoryEvent oEvent) {
			switch (oEvent.type) {
			case AgreementEventType.ESIGNED:
			case AgreementEventType.SIGNED:
				if (m_oSignerStatuses.ContainsKey(oEvent.participantEmail))
					m_oSignerStatuses[oEvent.participantEmail].SignatureTime = oEvent.date;

				break;
			} // switch

			return new SpSaveSignedDocument.HistoryEvent(oEvent);
		} // ConvertEvent 

		#endregion method ConvertEvent

		#region method EmailToSigner

		private SortedDictionary<string, int> EmailToSigner() {
			var res = new SortedDictionary<string, int>();

			foreach (var pair in Signers)
				res[pair.Value.DirectorEmail] = pair.Value.ID;

			return res;
		} // EmailToSigner

		#endregion method EmailToSigner

		#region method SaveSignerStatus

		private List<SpSaveSignedDocument.EsignerStatus> SaveSignerStatus(IEnumerable<ParticipantInfo> participants) {
			m_oSignerStatuses = new SortedDictionary<string, SpSaveSignedDocument.EsignerStatus>();

			var res = new List<SpSaveSignedDocument.EsignerStatus>();

			var es = EmailToSigner();

			foreach (ParticipantInfo pi in participants) {
				if (!pi.status.HasValue || !es.ContainsKey(pi.email))
					continue;

				var st = new SpSaveSignedDocument.EsignerStatus {
					EsignerID = es[pi.email],
					StatusID = (int)pi.status.Value,
				};

				res.Add(st);

				m_oSignerStatuses[pi.email] = st;
			} // if

			return res;
		} // SaveSignerStatus

		#endregion method SaveSignerStatus

		private SortedDictionary<string, SpSaveSignedDocument.EsignerStatus> m_oSignerStatuses;

		#endregion private
	} // class Esignature
} // namespace

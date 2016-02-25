namespace EchoSignLib {
	using System.Collections.Generic;
	using System.Linq;
	using EchoSignLib.Rest.Models;
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

	    public void SetHistoryAndStatus(EchoSignDocumentHistoryEvent[] events, EchoSignParticipantSetInfo[] setInfos) {
	        SignerStatuses = SaveSignerStatus(setInfos);

	        HistoryEvents = events
                .Select(ConvertToHistoryEvent)
	            .ToList();
	    }

	    private List<SpSaveSignedDocument.EsignerStatus> SaveSignerStatus(EchoSignParticipantSetInfo[] setInfos) {
	        m_oSignerStatuses = new SortedDictionary<string, SpSaveSignedDocument.EsignerStatus>();
	        List<SpSaveSignedDocument.EsignerStatus> res = new List<SpSaveSignedDocument.EsignerStatus>();

            var es = EmailToSigner();

	        EchoSignParticipantSetInfo setInfo = setInfos[0];
	        int status = (int)setInfo.status;

			foreach (EchoSignParticipantInfo pi in setInfo.participantSetMemberInfos) {
				if (!es.ContainsKey(pi.email))
					continue;

				var st = new SpSaveSignedDocument.EsignerStatus {
					EsignerID = es[pi.email],
					StatusID = status,
				};

				res.Add(st);

				m_oSignerStatuses[pi.email] = st;
			}

            return res;
        }

        /// <summary>
        /// Converts to history event.
        /// </summary>
        /// <param name="historyEvent">The echo sign history event.</param>
        /// <returns></returns>
        private SpSaveSignedDocument.HistoryEvent ConvertToHistoryEvent(EchoSignDocumentHistoryEvent historyEvent)
        {
            SpSaveSignedDocument.HistoryEvent history = new SpSaveSignedDocument.HistoryEvent
            {
                ActingUserEmail = historyEvent.actingUserEmail,
                ActingUserIp = historyEvent.actingUserIpAddress,
                Comment = historyEvent.comment,
                Description = historyEvent.description,
                EventTime = historyEvent.date,
                EventTypeID = (int)historyEvent.type,
                ParticipantEmail = historyEvent.participantEmail,
                SynchronizationKey = historyEvent.synchronizationId,
                VersionKey = historyEvent.versionId
            };

            if (historyEvent.deviceLocation != null)
            {
                history.Latitude = historyEvent.deviceLocation.latitude;
                history.Longitude = historyEvent.deviceLocation.longitude;
            }

            return history;
        }


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

		private SortedDictionary<string, int> EmailToSigner() {
			var res = new SortedDictionary<string, int>();

			foreach (var pair in Signers)
				res[pair.Value.DirectorEmail] = pair.Value.ID;

			return res;
		} // EmailToSigner

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

		private SortedDictionary<string, SpSaveSignedDocument.EsignerStatus> m_oSignerStatuses;

	} // class Esignature
} // namespace

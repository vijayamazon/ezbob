namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerDownloadCustomerFile : AStrategy {
		public BrokerDownloadCustomerFile(
			string sCustomerRefNum,
			string sContactEmail,
			int nFileID,
			CustomerOriginEnum origin
		) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			m_nFileID = nFileID;
			this.origin = (int)origin;

			FileName = null;
			Contents = null;
		} // constructor

		public byte[] Contents { get; private set; } // Files

		public string FileName { get; private set; } // FileName

		public override string Name {
			get { return "Broker download customer file"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || string.IsNullOrWhiteSpace(m_sCustomerRefNum) || (m_nFileID < 1))
				return;

			DB.ForEachRow(
				(oReader, bRowsetStart) => {
					FileName = oReader["FileName"].ToString();
					Contents = (byte[])oReader["FileContents"];

					return ActionResult.SkipAll;
				},
				"BrokerDownloadCustomerFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@FileID", m_nFileID),
				new QueryParameter("@Origin", this.origin)
			);
		} // Execute

		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly int m_nFileID;
		private readonly int origin;
	} // class BrokerDownloadCustomerFile 
} // namespace Ezbob.Backend.Strategies.Broker

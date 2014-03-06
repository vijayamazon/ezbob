namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerDownloadCustomerFile

	public class BrokerDownloadCustomerFile : AStrategy {
		#region public

		#region constructor

		public BrokerDownloadCustomerFile(int nCustomerID, string sContactEmail, int nFileID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sContactEmail = sContactEmail;
			m_nFileID = nFileID;

			FileName = null;
			Contents = null;
		} // constructor

		#endregion constructor

		#region property Contents

		public byte[] Contents { get; private set; } // Files

		#endregion property Contents

		#region property FileName

		public string FileName { get; private set; } // FileName

		#endregion property FileName

		#region property Name

		public override string Name {
			get { return "Broker download customer file"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || (m_nCustomerID < 1) || (m_nFileID < 1))
				return;

			DB.ForEachRow(
				(oReader, bRowsetStart) => {
					FileName = oReader["FileName"].ToString();
					Contents = (byte[])oReader["FileContents"];

					return ActionResult.SkipAll;
				},
				"BrokerDownloadCustomerFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@FileID", m_nFileID)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sContactEmail;
		private readonly int m_nFileID;

		#endregion private
	} // class BrokerDownloadCustomerFile 

	#endregion class BrokerDownloadCustomerFile
} // namespace EzBob.Backend.Strategies.Broker

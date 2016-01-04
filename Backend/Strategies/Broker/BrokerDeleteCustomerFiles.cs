namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerDeleteCustomerFiles : AStrategy {
		public BrokerDeleteCustomerFiles(
			string sCustomerRefNum,
			string sContactEmail,
			int[] aryFileIDs,
			CustomerOriginEnum origin
		) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			m_aryFileIDs = aryFileIDs;
			this.origin = (int)origin;
		} // constructor

		public override string Name {
			get { return "Broker delete customer files"; } // get
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sCustomerRefNum) || string.IsNullOrWhiteSpace(m_sContactEmail) || (m_aryFileIDs == null) || (m_aryFileIDs.Length < 1))
				return;

			DB.ExecuteNonQuery(
				"BrokerDeleteCustomerFiles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				DB.CreateVectorParameter<int>("@FileIDs", m_aryFileIDs),
				new QueryParameter("@Origin", this.origin)
			);
		} // Execute

		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly int[] m_aryFileIDs;
		private readonly int origin;
	} // class BrokerDeleteCustomerFiles
} // namespace Ezbob.Backend.Strategies.Broker

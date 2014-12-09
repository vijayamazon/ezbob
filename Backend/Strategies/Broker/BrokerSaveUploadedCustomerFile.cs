namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerSaveUploadedCustomerFile : AStrategy {

		public BrokerSaveUploadedCustomerFile(string sCustomerRefNum, string sContactEmail, byte[] aryFileContents, string sFileName, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			m_aryFileContents = aryFileContents;
			m_sFileName = sFileName;
		} // constructor

		public override string Name {
			get { return "Broker save customer file"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || string.IsNullOrWhiteSpace(m_sFileName) || string.IsNullOrWhiteSpace(m_sCustomerRefNum) || (m_aryFileContents == null) || (m_aryFileContents.Length < 1))
				return;

			DB.ExecuteNonQuery(
				"BrokerSaveUploadedCustomerFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@FileName", m_sFileName),
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@FileContents", m_aryFileContents),
				new QueryParameter("@UploadedTime", DateTime.UtcNow)
			);
		} // Execute

		private readonly string m_sFileName;
		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly byte[] m_aryFileContents;

	} // class BrokerSaveUploadedCustomerFile 

} // namespace EzBob.Backend.Strategies.Broker

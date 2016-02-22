namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerSaveUploadedCustomerFile : AStrategy {
		public BrokerSaveUploadedCustomerFile(
			string sCustomerRefNum,
			string sContactEmail,
			byte[] aryFileContents,
			string sFileName,
			CustomerOriginEnum origin
		) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			m_aryFileContents = aryFileContents;
			m_sFileName = sFileName;
			this.origin = (int)origin;
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
				new QueryParameter("@UploadedTime", DateTime.UtcNow),
				new QueryParameter("@Origin", this.origin)
			);
		} // Execute

		private readonly string m_sFileName;
		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly byte[] m_aryFileContents;
		private readonly int origin;
	} // class BrokerSaveUploadedCustomerFile 
} // namespace Ezbob.Backend.Strategies.Broker

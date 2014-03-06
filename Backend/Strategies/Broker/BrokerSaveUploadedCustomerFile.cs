namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerSaveUploadedCustomerFile

	public class BrokerSaveUploadedCustomerFile : AStrategy {
		#region public

		#region constructor

		public BrokerSaveUploadedCustomerFile(int nCustomerID, string sContactEmail, byte[] aryFileContents, string sFileName, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sContactEmail = sContactEmail;
			m_aryFileContents = aryFileContents;
			m_sFileName = sFileName;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker save customer file"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || string.IsNullOrWhiteSpace(m_sFileName) || (m_nCustomerID < 1) || (m_aryFileContents == null) || (m_aryFileContents.Length < 1))
				return;

			DB.ExecuteNonQuery(
				"BrokerSaveUploadedCustomerFile",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@FileName", m_sFileName),
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@FileContents", m_aryFileContents),
				new QueryParameter("@UploadedTime", DateTime.UtcNow)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sFileName;
		private readonly int m_nCustomerID;
		private readonly string m_sContactEmail;
		private readonly byte[] m_aryFileContents;

		#endregion private
	} // class BrokerSaveUploadedCustomerFile 

	#endregion class BrokerSaveUploadedCustomerFile
} // namespace EzBob.Backend.Strategies.Broker

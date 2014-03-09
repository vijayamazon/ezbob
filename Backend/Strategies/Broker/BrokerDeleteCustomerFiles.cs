namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerDeleteCustomerFiles

	public class BrokerDeleteCustomerFiles : AStrategy {
		#region public

		#region constructor

		public BrokerDeleteCustomerFiles(int nCustomerID, string sContactEmail, int[] aryFileIDs, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sContactEmail = sContactEmail;
			m_aryFileIDs = aryFileIDs;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker delete customer files"; } // get
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if ((m_nCustomerID < 0) || string.IsNullOrWhiteSpace(m_sContactEmail) || (m_aryFileIDs == null) || (m_aryFileIDs.Length < 1))
				return;

			DB.ExecuteNonQuery(
				"BrokerDeleteCustomerFiles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryTableParameter<int>("@FileIDs", "IntList", m_aryFileIDs)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sContactEmail;
		private readonly int[] m_aryFileIDs;

		#endregion private
	} // class BrokerDeleteCustomerFiles

	#endregion class BrokerDeleteCustomerFiles
} // namespace EzBob.Backend.Strategies.Broker

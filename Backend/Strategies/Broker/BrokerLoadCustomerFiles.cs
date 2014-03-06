namespace EzBob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerLoadCustomerFiles

	public class BrokerLoadCustomerFiles : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCustomerFiles(int nCustomerID, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_sContactEmail = sContactEmail;
			Files = new List<BrokerCustomerFile>();
		} // constructor

		#endregion constructor

		#region property Files

		public List<BrokerCustomerFile> Files { get; private set; } // Files

		#endregion property Files

		#region property Name

		public override string Name {
			get { return "Broker load customer files"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || (m_nCustomerID < 1))
				return;

			Files = DB.Fill<BrokerCustomerFile>(
				"BrokerLoadCustomerFiles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", m_nCustomerID),
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly int m_nCustomerID;
		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerLoadCustomerFiles

	#endregion class BrokerLoadCustomerFiles
} // namespace EzBob.Backend.Strategies.Broker

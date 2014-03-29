namespace EzBob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	#region class BrokerLoadCustomerFiles

	public class BrokerLoadCustomerFiles : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCustomerFiles(string sCustomerRefNum, string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sCustomerRefNum = sCustomerRefNum;
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
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || string.IsNullOrWhiteSpace(m_sCustomerRefNum))
				return;

			Files = DB.Fill<BrokerCustomerFile>(
				"BrokerLoadCustomerFiles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;

		#endregion private
	} // class BrokerLoadCustomerFiles

	#endregion class BrokerLoadCustomerFiles
} // namespace EzBob.Backend.Strategies.Broker

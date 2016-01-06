namespace Ezbob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadCustomerFiles : AStrategy {
		public BrokerLoadCustomerFiles(string sCustomerRefNum, string sContactEmail, CustomerOriginEnum origin) {
			m_sCustomerRefNum = sCustomerRefNum;
			m_sContactEmail = sContactEmail;
			this.origin = (int)origin;
			Files = new List<BrokerCustomerFile>();
		} // constructor

		public List<BrokerCustomerFile> Files { get; private set; } // Files

		public override string Name {
			get { return "Broker load customer files"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail) || string.IsNullOrWhiteSpace(m_sCustomerRefNum))
				return;

			Files = DB.Fill<BrokerCustomerFile>(
				"BrokerLoadCustomerFiles",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@RefNum", m_sCustomerRefNum),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@Origin", this.origin)
			);
		} // Execute

		private readonly string m_sCustomerRefNum;
		private readonly string m_sContactEmail;
		private readonly int origin;
	} // class BrokerLoadCustomerFiles
} // namespace Ezbob.Backend.Strategies.Broker

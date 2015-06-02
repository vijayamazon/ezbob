namespace Ezbob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadMarketingFiles : AStrategy {
		public BrokerLoadMarketingFiles(int originID) {
			m_oSp = new SpBrokerLoadMarketingFiles(DB, Log) { OriginID = originID, };
			Files = new List<FileDescription>();
		} // constructor

		public override string Name {
			get { return "Broker load marketing files"; }
		} // Name

		public List<FileDescription> Files { get; private set; }

		public override void Execute() {
			Files = m_oSp.Fill<FileDescription>();
		} // Execute

		private readonly SpBrokerLoadMarketingFiles m_oSp;

		private class SpBrokerLoadMarketingFiles : AStoredProc {
			public SpBrokerLoadMarketingFiles(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public int OriginID { get; set; }

			public override bool HasValidParameters() {
				return OriginID > 0;
			} // HasValidParameters
		} // class SpBrokerLoadMarketingFiles
	} // class BrokerLoadMarketingFiles
} // namespace Ezbob.Backend.Strategies.Broker

namespace EzBob.Backend.Strategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadMarketingFiles : AStrategy {
		#region public

		#region constructor

		public BrokerLoadMarketingFiles(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpBrokerLoadMarketingFiles(DB, Log);

			Files = new List<FileDescription>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load marketing files"; }
		} // Name

		#endregion property Name

		#region property Files

		public List<FileDescription> Files { get; private set; }

		#endregion property Files

		#region method Execute

		public override void Execute() {
			Files = m_oSp.Fill<FileDescription>();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpBrokerLoadMarketingFiles m_oSp;

		private class SpBrokerLoadMarketingFiles : AStoredProc {
			public SpBrokerLoadMarketingFiles(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return true;
			} // HasValidParameters
		} // class SpBrokerLoadMarketingFiles

		#endregion private
	} // class BrokerLoadMarketingFiles
} // namespace EzBob.Backend.Strategies.Broker

using Ezbob.Logger;

namespace Ezbob.HmrcHarvester {
	#region class AThrasher

	public abstract class AThrasher : SafeLog {
		#region public

		public abstract ISeeds Run(SheafMetaData oFileID, byte[] oFile);

		#endregion public

		#region protected

		#region constructor

		protected AThrasher(ASafeLog oLog = null) : base(oLog) {
		} // constructor

		#endregion constructor

		#endregion protected
	} // class AThrasher

	#endregion class AThrasher
} // namespace Ezbob.HmrcHarvester

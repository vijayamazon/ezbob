using Ezbob.Database;
using Ezbob.Logger;

namespace EzService {
	#region class EzServiceInstanceRuntimeData

	public class EzServiceInstanceRuntimeData {
		#region public

		public IHost Host { get; set; }

		public ASafeLog Log { get; set; }

		public AConnection DB { get; set; }

		#endregion public
	} // class EzServiceInstanceRuntimeData

	#endregion class EzServiceInstanceRuntimeData
} // namespace EzService

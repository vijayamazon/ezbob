using Ezbob.Database;
using Ezbob.Logger;

namespace EzService {
	public class EzServiceInstanceRuntimeData {
		#region public

		public IHost Host { get; set; }

		public ASafeLog Log { get; set; }

		public AConnection DB { get; set; }

		public string InstanceName { get; set; }

		public int InstanceID { get; set; }

		#endregion public
	} // class EzServiceInstanceRuntimeData
} // namespace EzService

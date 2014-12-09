namespace EzService {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EzServiceInstanceRuntimeData {
		public Ezbob.Context.Environment Env { get; set; }

		public IHost Host { get; set; }

		public ASafeLog Log { get; set; }

		public AConnection DB { get; set; }

		public string InstanceName { get; set; }

		public int InstanceID { get; set; }
	} // class EzServiceInstanceRuntimeData
} // namespace EzService

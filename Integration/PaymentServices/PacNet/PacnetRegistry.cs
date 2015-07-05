namespace PaymentServices.PacNet {
	using System;
	using StructureMap.Configuration.DSL;
	using ConfigManager;
	using Ezbob.Logger;

	public class PacnetRegistry : Registry {
		public PacnetRegistry() {
			try {
				int compareResult = string.Compare(
					CurrentValues.Instance.PacnetSERVICE_TYPE,
					ServiceType.Production.ToString(),
					StringComparison.InvariantCultureIgnoreCase
				);

				if (compareResult == 0)
					For<IPacnetService>().Use<LogPacnet<PacnetService>>();
				else
					For<IPacnetService>().Use<LogPacnet<FakePacnetService>>();
			} catch (Exception e) {
				log.Alert(
					e,
					"Failed to create Pacnet Service instance for money transfer; money won't be transferred to customers!"
				);

				For<IPacnetService>().Use<FakePacnetService>();
			} // try
		} // constructor

		private enum ServiceType {
			Production,
		} // enum ServiceType

		private static readonly ASafeLog log = new SafeILog(typeof(PacnetRegistry));
	} // class PacnetRegistry
} // namespace

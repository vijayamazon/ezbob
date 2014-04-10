namespace PaymentServices.PacNet {
	using System;
	using StructureMap.Configuration.DSL;
	using ConfigManager;

	public class PacnetRegistry : Registry {
		public PacnetRegistry() {
			try {
				string serviceType = CurrentValues.Instance.PacnetSERVICE_TYPE;

				if (serviceType == "Testing")
					For<IPacnetService>().Use<LogPacnet<FakePacnetService>>();
				else
					For<IPacnetService>().Use<LogPacnet<PacnetService>>();
			}
			catch (Exception) {
				For<IPacnetService>().Use<PacnetService>();
			} // try
		} // constructor
	} // class PacnetRegistry
} // namespace

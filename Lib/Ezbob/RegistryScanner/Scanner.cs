namespace Ezbob.RegistryScanner {
	using System.Reflection;

	using StructureMap;
	using StructureMap.Graph;

	using log4net;

	public interface IRequiresConfigurationOnStartup
	{
		void Configure();
	} // interface

	public class Scanner {
		public static void Register() {
			ILog oLog = LogManager.GetLogger(typeof (Scanner));

			ObjectFactory.Initialize((IInitializationExpression oInitExp) => oInitExp.Scan((IAssemblyScanner oScanner) => { 
				oScanner.AssembliesFromApplicationBaseDirectory((Assembly oAssembly) => {
					if (oAssembly.FullName.EndsWith("Tests"))
						return false;

					foreach (string sPrefix in ms_aryTypes)
						if (oAssembly.FullName.StartsWith(sPrefix))
							return false;

					return true;
				});

				oScanner.Exclude((System.Type oType) => oType.Name.StartsWith("Fake") || oType.Name.EndsWith("Fake"));

				oScanner.LookForRegistries();

				oScanner.AddAllTypesOf(typeof(IRequiresConfigurationOnStartup));
			}));

			oLog.Debug("Ezbob.RegistryScanner.Scanner.Register - configure list - begin");

			foreach (IRequiresConfigurationOnStartup current in ObjectFactory.GetAllInstances<IRequiresConfigurationOnStartup>()) {
				// This loop should never be enetered but just in case...
				// It should be debug, but Error was chosen in order to receive an email.
				oLog.ErrorFormat("Ezbob.RegistryScanner.Scanner.Register - configure {0}.", current.GetType());

				current.Configure();
			} // for each

			oLog.Debug("Ezbob.RegistryScanner.Scanner.Register - configure list - end");
		} // Register

		private static readonly string[] ms_aryTypes = new [] {
			"Antlr3.Runtime",
			"NHibernate",
			"NVelocity",
			"log4net",
			"Microsoft",
			"FluentNHibernate",
			"DevExpress",
			"AjaxControlToolkit",
		};
	} // class Scanner
}

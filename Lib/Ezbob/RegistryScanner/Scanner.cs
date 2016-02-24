namespace Ezbob.RegistryScanner {
	using System.Reflection;
	using StructureMap;
	using StructureMap.Graph;

	public class Scanner {
		public static void Register() {
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
			}));
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
            "FSharp.Core",
            "NUnit.CompareNetObjects"
		};
	} // class Scanner
}

namespace MaamClient {
	using System.Reflection;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Maam;

	class Program {
		static void Main(string[] args) {
			string sAppName = Assembly.GetExecutingAssembly().GetName().Name;

			ASafeLog oLog = new FileLog(sAppName);

			var oArgs = new Args(sAppName, args, oLog);

			oLog.NotifyStart();

			if (oArgs.IsGood) {
				var oEnv = new Ezbob.Context.Environment(oLog);

				var oDB = new SqlConnection(oEnv, oLog);

				CurrentValues.Init(oDB, oLog);

				var stra = new YesMaam(oArgs.Count, oArgs.LastCheckedID, oDB, oLog);
				stra.Execute();
			} // if

			oLog.NotifyStop();
		} // Main
	} // class Program
} // namespace

namespace MaamClient {
	using System.Reflection;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Maam;

	class Program {
		static void Main(string[] args) {
			string AppName = Assembly.GetExecutingAssembly().GetName().Name;

			ASafeLog log = new FileLog(AppName);

			var oArgs = new Args(AppName, args, log);

			log.NotifyStart();

			if (oArgs.IsGood) {
				var db = new SqlConnection(new Ezbob.Context.Environment(log), log);

				CurrentValues.Init(db, log);

				var stra = new YesMaam(oArgs.Count, oArgs.LastCheckedID, db, log);
				stra.Execute();
			} // if

			log.NotifyStop();
		} // Main
	} // class Program
} // namespace

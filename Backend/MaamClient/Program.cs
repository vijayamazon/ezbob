namespace MaamClient {
	using System.IO;
	using System.Reflection;
	using System.Text;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Maam;

	class Program {
		static void Main(string[] args) {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;

			Log = new FileLog(AppName);
			Log.NotifyStart();

			var env = new Ezbob.Context.Environment(Log);

			DB = new SqlConnection(env, Log);

			Ezbob.Backend.Strategies.Library.Initialize(env, DB, Log);

			CompareMaam(args);

			// LoadFromJson();

			Log.NotifyStop();
		} // Main

		private static void CompareMaam(string[] args) {
			var oArgs = new Args(AppName, args, Log);

			if (oArgs.IsGood) {
				CurrentValues.Init(DB, Log);

				var stra = new YesMaam(oArgs.Count, oArgs.LastCheckedID, DB, Log);
				stra.Execute();
			} // if
		} // CompareMaam

		private static void LoadFromJson() {
			var log = new ConsoleLog(Log);

			string json = File.ReadAllText(@"c:\ezbob\test-data\automation\approval-data.json", Encoding.UTF8);

			ApprovalInputData aid = ApprovalInputData.Deserialize(json);

			log.Debug("Data read from file:\n{0}", aid.Serialize());
		} // LoadFromJson

		private static string AppName { get; set; }
		private static ASafeLog Log { get; set; }
		private static AConnection DB { get; set; }
	} // class Program
} // namespace

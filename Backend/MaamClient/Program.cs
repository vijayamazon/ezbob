namespace MaamClient {
	using System.IO;
	using System.Reflection;
	using System.Text;
	using AutomationCalculator.ProcessHistory.Trails;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Maam;

	internal class Program {
		private static string AppName { get; set; }

		private static AConnection DB { get; set; }

		private static ASafeLog Log { get; set; }

		private static string CompareMaam(string[] args) {
			var oArgs = new Args(AppName, args, Log);

			if (oArgs.IsGood) {
				CurrentValues.Init(DB, Log);

				var stra = new YesMaam(oArgs.Count, oArgs.LastCheckedID, DB, Log);
				stra.Execute();

				return stra.Tag;
			} // if

			return null;
		} // CompareMaam

		private static void LoadFromJson() {
			var log = new ConsoleLog(Log);

			string json = File.ReadAllText(@"c:\ezbob\test-data\automation\approval-data.json", Encoding.UTF8);

			ApprovalInputData aid = ApprovalInputData.Deserialize(json);

			log.Debug("Data read from file:\n{0}", aid.Serialize());
		} // LoadFromJson

		private static void LoadTurnovers(string[] args) {
			var oArgs = new Args(AppName, args, Log);

			if (oArgs.IsGood) {
				CurrentValues.Init(DB, Log);
				new LoadTurnovers(oArgs, DB, Log).Run();
			} // if
		} // LoadTurnovers

		private static void CompareAndExport(string[] args, AConnection db, ASafeLog log) {
			string tag = CompareMaam(args);

			if (!string.IsNullOrWhiteSpace(tag))
				ExportApprovalData.Run(new [] { tag }, db, log);
		} // CompareAndExport

		private static void Main(string[] args) {
			AppName = Assembly.GetExecutingAssembly().GetName().Name;

			Log = new FileLog(AppName);
			Log.NotifyStart();

			var env = new Ezbob.Context.Environment(Log);

			DB = new SqlConnection(env, Log);

			Ezbob.Backend.Strategies.Library.Initialize(env, DB, Log);

			// LoadFromJson();

			// LoadTurnovers(args);

			ExportApprovalData.Run(args, DB, Log);

			// CompareAndExport(args, DB, Log);

			Log.NotifyStop();
		} // Main
	} // class Program
} // namespace

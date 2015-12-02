namespace Ezbob.Integration.LogicalGlue.Tests {
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class BasicFlow {
		[SetUp]
		public void Init() {
			var oLog4NetCfg = new Log4Net().Init();

			var env = oLog4NetCfg.Environment;
			this.log = new ConsoleLog(new SafeILog(this));
			this.db = new SqlConnection(oLog4NetCfg.Environment, log);

			Ezbob.Integration.LogicalGlue.Library.Initialize(env, this.db, this.log);

			ConfigManager.CurrentValues.Init(this.db, this.log);
		} // Init

		[Test]
		public void TestBasicFlow() {
			IEngine engine = InjectorStub.GetTestEngine();

			// TODO: save "session level errors" (that are returned as hard rejects, etc)
			// TODO: also save ETL objects

			Inference inference = engine.GetInference(269, GetInferenceMode.DownloadIfOld);

			this.log.Info("Inference read: {0}", inference);
		} // TestBasicFlow

		private AConnection db;
		private ASafeLog log;
	} // class BasicFlow
} // namespace

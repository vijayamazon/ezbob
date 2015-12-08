﻿namespace Ezbob.Integration.LogicalGlue.Tests {
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Implementation;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Implementation;
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

			TestHarvester harvester = (TestHarvester)((Engine)engine).Harvester;

			harvester.ReplyMode = TestHarvester.ReplyModes.Success;

			Inference inference = engine.GetInference(1417, GetInferenceMode.DownloadIfOld);

			this.log.Info("Inference read: {0}", inference);
		} // TestBasicFlow

		[Test]
		public void TestEndToEndFlow() {
			IEngine engine = InjectorStub.GetEngine();

			Inference inference = engine.GetInference(1417, GetInferenceMode.ForceDownload);

			this.log.Info("Inference read: {0}", inference);
		} // TestEndToEndFlow

		private AConnection db;
		private ASafeLog log;
	} // class BasicFlow
} // namespace

namespace Ezbob.Integration.LogicalGlue.Tests {
	using Ezbob.Integration.LogicalGlue.Engine.Implementation;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Implementation;
	using NUnit.Framework;

	[TestFixture]
	class BasicFlow : ABaseTest {
		[Test]
		public void TestBasicFlow() {
			IEngine engine = InjectorStub.GetTestEngine();

			TestHarvester harvester = (TestHarvester)((Engine)engine).Harvester;

			harvester.ReplyMode = TestHarvester.ReplyModes.Success;

			Inference inference = engine.GetInference(1393, 0, false,GetInferenceMode.DownloadIfOld);

			Log.Info("Inference read: {0}", inference);
		} // TestBasicFlow

		[Test]
		public void TestEndToEndFlow() {
			IEngine engine = InjectorStub.GetEngine();

			Inference inference = engine.GetInference(1437, 0, false, GetInferenceMode.ForceDownload);

			Log.Info("Inference read: {0}", inference);
		} // TestEndToEndFlow
	} // class BasicFlow
} // namespace

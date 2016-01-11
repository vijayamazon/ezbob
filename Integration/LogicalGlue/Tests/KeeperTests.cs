namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using System.Collections.Generic;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using NUnit.Framework;

	[TestFixture]
	class KeeperTests : ABaseTest {
		[Test]
		public void TestLoadHistory() {
			IEngine engine = InjectorStub.GetEngine();

			List<Inference> inferences = engine.GetInferenceHistory(1417, DateTime.UtcNow, false, 0);

			Log.Info("Inferences read:\n\n{0}", string.Join("\n\n", inferences));
		} // TestLoadHistory
	} // class KeeperTests
} // namespace

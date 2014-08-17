namespace EzBobTest {
	using Ezbob.Utils;
	using NUnit.Framework;

	[TestFixture]
	class Misc : BaseTestFixtue {
		[Test]
		public void TestMime() {
			var mtr = new MimeTypeResolver();
			mtr.TestVsBuiltIn(m_oLog);
		} // TestMime
	} // class Misc
} // namespace

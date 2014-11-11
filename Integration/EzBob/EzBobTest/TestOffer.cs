namespace EzBobTest
{
	using EzBob.Backend.Strategies.MedalCalculations;
	using EzBob.Backend.Strategies.OfferCalculation;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;
	
	[TestFixture]
	public class TestOffer : BaseTestFixtue
	{
		[SetUp]
		public new void Init()
		{
			base.Init();

			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());

			EzServiceAccessorShort.Set(m_oDB, m_oLog);
		} // Init

		[Test]
		public void Test_FirstOfferTest()
		{
			int customerId = 18112;

			var offer = new CalculateOffer(customerId, 10000, MedalClassification.Gold, m_oDB, m_oLog);
			offer.Execute();
			var f = offer.Result;
			int h = 9;
		}
	}
}
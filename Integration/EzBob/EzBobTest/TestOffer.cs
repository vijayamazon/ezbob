namespace EzBobTest
{
	using System;
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
		}

		[Test]
		public void Test_FirstOfferTest()
		{
			var offer = new OfferCalculator1(m_oDB, m_oLog);
			OfferResult res = offer.CalculateOffer(14029, DateTime.UtcNow, 20000, false, MedalClassification.Gold);
		}
	}
}
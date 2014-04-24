namespace ExperianLib.Tests
{
	using System;
	using IdIdentityHub;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using NUnit.Framework;
	using Integration;

	[TestFixture]
	public class IdHubServiceParseTests : BaseTest
	{
		private IdHubService _service;
		private const string BwaXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ProcessConfigResponseType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <EIHHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03/EIHHeader\">    <ClientUser>User1</ClientUser>    <ReferenceId>1234</ReferenceId>  </EIHHeader>  <DecisionHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <UniqueReferenceNo>123527058781</UniqueReferenceNo>    <AuthenticationDecision>Authenticated</AuthenticationDecision>    <AuthenticationText>The applicant has been authenticated to your required minimum level based on the rules implemented in the selected query.</AuthenticationText>  </DecisionHeader>  <ProcessConfigResultsBlock xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <BWAResultBlock>      <NameScore>9</NameScore>      <AddressScore>9</AddressScore>      <AccountStatus>Match</AccountStatus>    </BWAResultBlock>  </ProcessConfigResultsBlock></ProcessConfigResponseType>";
		private const string AmlXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ProcessConfigResponseType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <EIHHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03/EIHHeader\">    <ClientUser>User1</ClientUser>    <ReferenceId>1234</ReferenceId>  </EIHHeader>  <DecisionHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <UniqueReferenceNo>123027546717</UniqueReferenceNo>    <AuthenticationDecision>Authenticated</AuthenticationDecision>    <AuthenticationText>The applicant has been authenticated to your required minimum level based on the rules implemented in the selected query.</AuthenticationText>  </DecisionHeader>  <ProcessConfigResultsBlock xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <EIAResultBlock>      <AuthenticationIndex>80</AuthenticationIndex>      <AuthIndexText>A high level of Authentication has been found for the identity supplied</AuthIndexText>      <EIAResults>        <IDandLocDataAtCL>          <NumPrimDataItems>5</NumPrimDataItems>          <NumPrimDataSources>5</NumPrimDataSources>          <StartDateOldestPrim>2001-10</StartDateOldestPrim>          <NumSecDataItems>2</NumSecDataItems>          <NumSecDataSources>1</NumSecDataSources>          <StartDateOldestSec>2012-5</StartDateOldestSec>        </IDandLocDataAtCL>        <LocDataOnlyAtCLoc>          <NumPrimDataItems>15</NumPrimDataItems>          <NumPrimDataSources>4</NumPrimDataSources>          <StartDateOldestPrim>1990-10</StartDateOldestPrim>          <NumSecDataItems>7</NumSecDataItems>          <NumSecDataSources>1</NumSecDataSources>          <StartDateOldestSec>2012-4</StartDateOldestSec>        </LocDataOnlyAtCLoc>        <IDandLocDataAtPL>          <NumPrimDataItems>0</NumPrimDataItems>          <NumPrimDataSources>0</NumPrimDataSources>          <NumSecDataItems>0</NumSecDataItems>          <NumSecDataSources>0</NumSecDataSources>        </IDandLocDataAtPL>        <LocDataOnlyAtPLoc>          <NumPrimDataItems>0</NumPrimDataItems>          <NumPrimDataSources>0</NumPrimDataSources>          <NumSecDataItems>0</NumSecDataItems>          <NumSecDataSources>0</NumSecDataSources>        </LocDataOnlyAtPLoc>        <DataMatchCounts>          <NumAgeMatchesToPrimSource>4</NumAgeMatchesToPrimSource>          <NumAgeMatchToSecSource>2</NumAgeMatchToSecSource>          <NumTimeAtCLMtchPrimSource>0</NumTimeAtCLMtchPrimSource>          <NumTimeAtCLMatchSecSource>0</NumTimeAtCLMatchSecSource>        </DataMatchCounts>        <ReturnedHRPCount>0</ReturnedHRPCount>      </EIAResults>    </EIAResultBlock>  </ProcessConfigResultsBlock></ProcessConfigResponseType>";

		[SetUp]
		public void SetUp()
		{
			_service = new IdHubService(new ExperianBankCacheRepositoryFake());
		}

		[Test]
		public void TestAmlDebugMode()
		{
			var results = _service.Authenticate("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
															   "33 North Court Close", "Rustington", null,
															   "Littlehampton", null, "BN16 3HZ", 1, false, AmlXml);
			Assert.That(!results.HasError);
		}

		[Test]
		public void TestAmlDebugMode2()
		{
			var results = _service.AuthenticateForcedWithCustomAddress("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
															   "33 North Court Close", "Rustington", null,
															   "Littlehampton", null, "BN16 3HZ", "", 1, AmlXml);
			Assert.That(!results.HasError);
		}
		[Test]
		public void TestBwaDebugMode()
		{
			var results = _service.AccountVerification("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
															   "33 North Court Close", "Rustington", null,
															   "Littlehampton", null, "BN16 3HZ", "306782", "23356268",
															   1, false, BwaXml);
			Assert.That(!results.HasError);
		}

		[Test]
		public void TestBwaDebugMode2()
		{
			var results = _service.AccountVerificationForcedWithCustomAddress("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
															   "33 North Court Close", "Rustington", null,
															   "Littlehampton", null, "BN16 3HZ", "306782", "23356268",
															   "", 1, BwaXml);
			Assert.That(!results.HasError);
		}

		[Test]
		public void TestFillAddress()
		{
			var address = _service.FillAddress("51A Mossvale Street", null, null, "Redruth", "Cornwall", "TR15 2EH");
			//var address = _service.FillAddress("Unit B01", "Park Hall Trading Estate", "40 Martell Road", "London", null, "SE21 8EN");
			//51A Mossvale Street
			//161-165 Borough High Street
			//25 Victoria Avenue
			//Unit B01
		}

		private class ExperianBankCacheRepositoryFake : ExperianBankCacheRepository
		{
			public ExperianBankCacheRepositoryFake()
				: base(null)
			{
			}

			public override T Get<T>(string key, long? expirationPeriodSec)
			{
				return null;
			}

			public override void Set<T>(string key, T value, MP_ServiceLog serviceLog)
			{
			}
		}
	}
}
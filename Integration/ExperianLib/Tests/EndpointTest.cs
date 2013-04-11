using System;
using ExperianLib.IdIdentityHub;
using NUnit.Framework;

namespace ExperianLib.Tests
{
    class EndpointTest:BaseTest
    {
        const string BwaXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ProcessConfigResponseType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <EIHHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03/EIHHeader\">    <ClientUser>User1</ClientUser>    <ReferenceId>1234</ReferenceId>  </EIHHeader>  <DecisionHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <UniqueReferenceNo>122308143612</UniqueReferenceNo>    <AuthenticationDecision>Not Authenticated</AuthenticationDecision>    <AuthenticationText>The applicant has not been authenticated to your required minimum level based on the rules implemented in the selected query. You should seek other proofs of identity before dealing with this applicant.</AuthenticationText>  </DecisionHeader>  <ProcessConfigResultsBlock xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <BWAResultBlock>      <NameScore>1</NameScore>      <AccountStatus>Match</AccountStatus>    </BWAResultBlock>  </ProcessConfigResultsBlock></ProcessConfigResponseType>";
        const string AmlXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ProcessConfigResponseType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">  <EIHHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03/EIHHeader\">    <ClientUser>User1</ClientUser>    <ReferenceId>1234</ReferenceId>  </EIHHeader>  <DecisionHeader xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <UniqueReferenceNo>123027546717</UniqueReferenceNo>    <AuthenticationDecision>Authenticated</AuthenticationDecision>    <AuthenticationText>The applicant has been authenticated to your required minimum level based on the rules implemented in the selected query.</AuthenticationText>  </DecisionHeader>  <ProcessConfigResultsBlock xmlns=\"http://schema.uk.experian.com/eih/2011/03\">    <EIAResultBlock>      <AuthenticationIndex>80</AuthenticationIndex>      <AuthIndexText>A high level of Authentication has been found for the identity supplied</AuthIndexText>      <EIAResults>        <IDandLocDataAtCL>          <NumPrimDataItems>5</NumPrimDataItems>          <NumPrimDataSources>5</NumPrimDataSources>          <StartDateOldestPrim>2001-10</StartDateOldestPrim>          <NumSecDataItems>2</NumSecDataItems>          <NumSecDataSources>1</NumSecDataSources>          <StartDateOldestSec>2012-5</StartDateOldestSec>        </IDandLocDataAtCL>        <LocDataOnlyAtCLoc>          <NumPrimDataItems>15</NumPrimDataItems>          <NumPrimDataSources>4</NumPrimDataSources>          <StartDateOldestPrim>1990-10</StartDateOldestPrim>          <NumSecDataItems>7</NumSecDataItems>          <NumSecDataSources>1</NumSecDataSources>          <StartDateOldestSec>2012-4</StartDateOldestSec>        </LocDataOnlyAtCLoc>        <IDandLocDataAtPL>          <NumPrimDataItems>0</NumPrimDataItems>          <NumPrimDataSources>0</NumPrimDataSources>          <NumSecDataItems>0</NumSecDataItems>          <NumSecDataSources>0</NumSecDataSources>        </IDandLocDataAtPL>        <LocDataOnlyAtPLoc>          <NumPrimDataItems>0</NumPrimDataItems>          <NumPrimDataSources>0</NumPrimDataSources>          <NumSecDataItems>0</NumSecDataItems>          <NumSecDataSources>0</NumSecDataSources>        </LocDataOnlyAtPLoc>        <DataMatchCounts>          <NumAgeMatchesToPrimSource>4</NumAgeMatchesToPrimSource>          <NumAgeMatchToSecSource>2</NumAgeMatchToSecSource>          <NumTimeAtCLMtchPrimSource>0</NumTimeAtCLMtchPrimSource>          <NumTimeAtCLMatchSecSource>0</NumTimeAtCLMatchSecSource>        </DataMatchCounts>        <ReturnedHRPCount>0</ReturnedHRPCount>      </EIAResults>    </EIAResultBlock>  </ProcessConfigResultsBlock></ProcessConfigResponseType>";

        [Test]
        public void TestAndrewMilburnAMLA()
        {
            var service = new IdHubService();
            var results = service.Authenticate("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", 1);
            if(!results.HasError) Log.InfoFormat("AML Completed successfully");
            else Log.InfoFormat("AML Failed: " + results.Error);
        }

        [Test]
        public void TestAndrewMilburnBwa()
        {
            var service = new IdHubService();
            var results = service.AccountVerification("Andrew", "JB", "Milburn", "M", new DateTime(1964, 12, 8), null, null, "3 Linford Close", "Rugeley", null, "WS15 4EF", "110730", "00742205", 1);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }

        [Test]
        public void TestAdamPhilpottBwa()
        {
            var service = new IdHubService();
            var results = service.AccountVerification("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16), "33 North Court Close", "Rustington", null, "Littlehampton", null, "BN16 3HZ", "306782", "23356268", 1);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }

        [Test]
        public void TestBwaDebugMode()
        {
            var service = new IdHubService();
            var results = service.AccountVerification("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
                                                               "33 North Court Close", "Rustington", null,
                                                               "Littlehampton", null, "BN16 3HZ", "306782", "23356268",
                                                               1,false, BwaXml);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }

        [Test]
        public void TestBwaDebugMode2()
        {
            var service = new IdHubService();
            var results = service.AccountVerificationForcedWithCustomAddress("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
                                                               "33 North Court Close", "Rustington", null,
                                                               "Littlehampton", null, "BN16 3HZ", "306782", "23356268",
                                                               "", 1, BwaXml);
            if (!results.HasError) Log.InfoFormat("BWA test passed.");
            else Log.InfoFormat("BWA test fail: " + results.Error);
        }

        [Test]
        public void TestAmlDebugMode()
        {
            var service = new IdHubService();
            var results = service.Authenticate("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
                                                               "33 North Court Close", "Rustington", null,
                                                               "Littlehampton", null, "BN16 3HZ", 1, false, AmlXml);
            if (!results.HasError) Log.InfoFormat("AML test passed.");
            else Log.InfoFormat("AML test fail: " + results.Error);
        }

        [Test]
        public void TestAmlDebugMode2()
        {
            var service = new IdHubService();
            var results = service.AuthenticateForcedWithCustomAddress("Adam", null, "Philpott", "M", new DateTime(1987, 12, 16),
                                                               "33 North Court Close", "Rustington", null,
                                                               "Littlehampton", null, "BN16 3HZ","", 1, AmlXml);
            if (!results.HasError) Log.InfoFormat("AML test passed.");
            else Log.InfoFormat("AML test fail: " + results.Error);
        }
    }
}

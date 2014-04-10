namespace PaymentServices.Tests
{
	using System;
	using NUnit.Framework;
	using PacNet;

	//send money
	[TestFixture]
    class PacnetTests
    {
        [Test]
        public void SendMoney()
        {
            var service = new PacnetService();
            var ret = service.SendMoney(1,5, "11-13-90", "00355973", "Test");
            ret = service.CheckStatus(1,ret.TrackingNumber);
        }

        [Test]
        public void CloseFile()
        {
            var service = new PacnetService();
            var ret = service.CloseFile(1,"orangemoney.wf-RT2012-04-25");
        }  

        [Test]
        public void CloseTodayAndYesterdayFiles()
        {
            var service = new PacnetService();
            var ret = service.CloseTodayAndYesterdayFiles(1);
        } 

        [Test]
        public void CheckStatus()
        {
            var service = new PacnetService();
            var ret = service.CheckStatus(1,"860270115");
        }

		[Test]
		public void GetReport()
		{
			//Debugger.Launch();
			var service = new PacnetService();
			// Default is past 30 days
			var endTime = DateTime.Now;
			var startTime = endTime.AddDays(-30);
			var ret = service.GetReport(endTime, startTime);
		} 
    }
}

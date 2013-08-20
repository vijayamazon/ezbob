﻿using System;
using EzBob.Configuration;
using NUnit.Framework;
using PaymentServices.PayPoint;
using Scorto.Configuration;
using Scorto.Configuration.Loader;

namespace PaymentServices.Tests
{
    // return money
    class PayPointTests:BaseTest
    {
        private ConfigurationRootBob _bobconfig;

        [SetUp]
        public void SetUp()
        {
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\EzBob\App\clients\Maven\maven.exe";
            _bobconfig = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRootBob>();
        }

        [Test]
        public void Test1()
        {
            PayPointApi papi = new PayPointApi(null, null, _bobconfig);
            papi.RefundCard("Mr Cardholder", "4444333322221111", 50.99M, new DateTime(2015,1, 1), "1", new DateTime(2009, 1,1), "product=ezbob", "123", true);

            string repo = papi.GetReport("CSV", "Date", DateTime.Now.ToString("yyyyMMdd"), "GBP");
            /*SECVPNService service = new SECVPNService();
            service.

            string r = service.validateCardFull("secpay", "secpay", "TRAN00001_SC", "127.0.0.1", "Mr Cardholder",
                                     "4444333322221111", "50.99", "0115", "1", "0109",
                                     "prod=funny_book,item_amount=25.00x1;prod=sad_book,item_amount=12.50x2", "", "",
                                     "test_status=true,dups=false,card_type=Visa,cv2=123");
            string res = service.refundCardFull("secpay", "secpay", "TRAN00001_SC", "50.99", "secpay", "TRAN00001_SC_refund");
            var report = service.getReport("secpay", "secpay", "secpay", "CSV", "Date", "201204", "GBP", "", false, false);*/
        }

        [Test]
        public void RepeatTransaction()
        {
            PayPointApi papi = new PayPointApi(null, null, _bobconfig);
            papi.RepeatTransaction("edf7951a-225b-4fd3-bd28-cde7c03f7df7", 100.99M);
            string repo = papi.GetReport("CSV", "Date", DateTime.Now.ToString("yyyyMMdd"), "GBP");
        }

		[Test]
		public void PayPointPayPal()
		{
			var papi = new PayPointApi(null,null,null);
			papi.PayPointPayPal("www.google.com", "www.google.com", "www.google.com", 5.0M, "GBP", true);
		}
    }
}

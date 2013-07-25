﻿using ExperianLib.Ebusiness;
using NUnit.Framework;
using Newtonsoft.Json;

namespace ExperianLib.Tests
{
    class EBusinessTests:BaseTest
    {
        /// <summary>
        /// Need working vm with installed targeting service
        /// </summary>
        /// 
        [Test]
        [Ignore]
        public void TargetingTest()
        {
            var service = new EBusinessService();
            var result = service.TargetBusiness("SONY", "CR7 7JN", 1);
            Log.Debug("Targeting results: " + JsonConvert.SerializeObject(result));
            /*foreach (var targetResult in result.Targets)
            {
                if(targetResult.LegalStatus == "L")
                {
                    var f = service.GetLimitedBusinessData(targetResult.BusRefNum);
                }
                /*if (targetResult.LegalStatus == "N")
                {
                    var t = service.GetNotLimitedBusinessData(targetResult.BusRefNum);
                }*/
            //}
        }

        [Test]
        [Ignore]
        public void GetLimitedCompanyTest()
        {
            var service = new EBusinessService();
            const string refNum = "05860211";
            var result = service.GetLimitedBusinessData(refNum, 1);
            Log.DebugFormat("Limited business with ref number = {0} results: {1}", refNum, JsonConvert.SerializeObject(result));
        }

        [Test]
        [Ignore]
        public void GetNonLimitedCompanyTest()
        {
            var service = new EBusinessService();
            const string refNum = "02406500";
            var result = service.GetNotLimitedBusinessData(refNum, 1);
            Log.DebugFormat("NonLimited business with ref number = {0} results: {1}", refNum, JsonConvert.SerializeObject(result));
        }
    }
}

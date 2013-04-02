using System;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using EzBobIntegration.Web_References.Consumer;
using Newtonsoft.Json;

namespace ExperianLib.Tests
{
    class ConsumerTests : BaseTest
    {
        [Test]
        public void TestConsumer()
        {
            var service = new ConsumerService();
            //MR AARON T HOLTON***3*ADRIAN CLOSE*HARTLEY WINTNEY*HOOK*HANTS*RG278DP 
            //MR ABDUL GARTWRIGHT***33*ABBEY ROAD*MEDSTEAD*ALTON*HANTS*GU345PB 
            //MR ADAM D THURSTON**24B**ABBOTT ROAD**BOURNEMOUTH*DORSET*BH9 1HA 
            //MR ABUL FAYER***1*DIBGATE COTTAGES*NEWINGTON*FOLKESTONE*KENT*CT188BJ 

            //DB:
            //3 Adrian Close|Hartley Wintney|NULL|Hook|Hampshire|RG27 8DP
            //33 Abbey Road|Medstead|NULL|Alton|Hampshire|GU34 5PB
            //24B Abbott Road|NULL|NULL|Bournemouth|Dorset|BH9 1HA
            //1 Dibgate Cottages|Newington|NULL|Folkestone|Kent|CT18 8BJ

            var loc = new InputLocationDetailsMultiLineLocation()
            {
                LocationLine1 = "1 Dibgate Cottages".ToUpper(),
                LocationLine2 = "Newington".ToUpper(),
                LocationLine3 = "".ToUpper(),
                LocationLine4 = "Folkestone".ToUpper(),
                LocationLine5 = "Kent".ToUpper(),
                LocationLine6 = "CT18 8BJ".ToUpper(),
            };
            var dob = new DateTime(1990, 08, 17);
            var result = service.GetConsumerInfo("ABUL", "FAYER", "M", dob, null, loc, "PL", 1);
            if(result.IsError)
            {
                Log.Error("Error from consumer service: " + result.Error);
                Assert.Fail();
            }
        }

        /*[Test]
        public void TestFile()
        {
            var service = new ConsumerService();
            const int skipRows = 200;
            const int maxRequests = 1000;

            using(var sr = new StreamReader("D:\\ExperianDemo.txt"))
            {
                var infoLine = sr.ReadLine();
                var reqId = 0;
                var ser = new XmlSerializer(typeof (OutputRoot));

                while(!string.IsNullOrEmpty(infoLine))
                {
                    if(reqId < skipRows)
                    {
                        infoLine = sr.ReadLine();
                        reqId++;
                    }
                    string[] items = infoLine.Split('*');
                    var fullName = items[0];
                    var flat = items[1];
                    var houseName = items[2];
                    var houseNumber = items[3];
                    var street = items[4];
                    var postcode = items[8];

                    var nameItems = fullName.Split(' ');
                    var sex = ((nameItems[0] == "MR") || (nameItems[0] == "SIR")) ? "M" : "F";
                    var firstName = nameItems[1];
                    var lastName = nameItems[nameItems.Length - 1];

                    var location = new InputLocationDetailsUKLocation
                    {
                        Flat = flat,
                        HouseName = houseName,
                        HouseNumber = houseNumber,
                        Street = street,
                        District = "",
                        PostTown = "",
                        Country = "",
                        County = "",
                        Postcode = postcode,
                        SharedLetterbox = "N"
                    };

                    var result = service.GetConsumerInfo(firstName, lastName, sex, null, location, null, "PL");
                    using(var sw = new StreamWriter(string.Format("D:\\Projects\\EZBOB\\Experian\\output{0}.xml", reqId)))
                    {
                        ser.Serialize(sw, result.Output);
                        sw.Flush();
                        sw.Close();
                    }

                    if(++reqId >= maxRequests)
                        break;
                    infoLine = sr.ReadLine();
                }
            }

        }*/

    }
}

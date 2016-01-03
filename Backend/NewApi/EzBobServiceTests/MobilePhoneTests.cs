using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobServiceTests
{
    using System.Globalization;
    using EzBobModels.ThirdParties.Twilio;
    using EzBobService;
    using EzBobService.Mobile;
    using Moq;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class MobilePhoneTests : TestBase
    {
        //!Don't delete the code it will serve as reference for new Test
        //the structure and flow changed test should be rewritten!!! 

//        [Ignore] 
//        public void TestAuthorizeMobilePhoneNumber()
//        {
//            IContainer container = InitContainer(typeof(MobilePhone));
//            container.Configure(c => Init(c, PrepareTwilioMock(isDates: true, isUnderwriterId: true)));
//
//            MobilePhone mobilePhone = container.GetInstance<MobilePhone>();
//            var info = mobilePhone.AuthorizeMobilePhoneNumber("333333333333");
//            Assert.False(info.HasErrors);
//        }
//
//        [Ignore] //the structure and flow changed test should be rewritten
//        public void TestIncompleteSmsMessage() {
//            IContainer container = InitContainer(typeof(MobilePhone));
//            //without dates and underwriter id
//            container.Configure(c => Init(c, PrepareTwilioMock(isDates: false, isUnderwriterId: false)));
//
//            MobilePhone mobilePhone = container.GetInstance<MobilePhone>();
//            var info = mobilePhone.AuthorizeMobilePhoneNumber("3434343434");
//            Assert.True(info.HasErrors);
//        }
//
//
//        private void Init(ConfigurationExpression registry, Mock<ITwilio> twilioMock) {
//            registry
//                .ForSingletonOf<ITwilio>()
//                .Use(() => twilioMock.Object);
//        }

//        private Mock<ITwilio> PrepareTwilioMock(bool isDates = true, bool isUnderwriterId = true)
//        {
//            var twilio = new Mock<ITwilio>();
//
//            TwilioSms twilioSms;
//
//            twilio.Setup(o => o.SendSms(It.IsAny<string>(), It.IsAny<string>()))
//                .Callback<string, string>(Verify)
//                .Returns(() => {
//                    twilioSms = GenerateTwilioSms(isDates, isUnderwriterId);
//                    return twilioSms;
//                });
//
//            return twilio;
//        }
//
//        private void Verify(string a, string b) {
//            int kk = 0;
//        }
//
//        private TwilioSms GenerateTwilioSms(bool isDates = true, bool isUnderwriterId = true) {
//
//            DateTime now = DateTime.Now;
//
//            return new TwilioSms {
//                Body = "test " + now.ToString(CultureInfo.InvariantCulture),
//                DateCreated = isDates ? now : default(DateTime),
//                DateSent = isDates ? now : default(DateTime),
//                DateUpdated = isDates ? now : default(DateTime),
//                UnderwriterId = isUnderwriterId ? Random.Next(1, 15000) : default(int),
//                Price = 2000
//            };
//        }       
    }
}

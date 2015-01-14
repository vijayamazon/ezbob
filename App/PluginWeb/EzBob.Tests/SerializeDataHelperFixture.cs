using System;
using System.Text;
using EzBob.AmazonServiceLib;
using EzBob.CommonLib;
using NUnit.Framework;

namespace EzBob.Tests
{
	using Ezbob.Utils.Serialization;

	[TestFixture]
    public class SerializeDataHelperFixture
    {
        [Test]
        public void can_deserialize_amazon()
        {
            //0x3C3F786D6C2076657273696F6E3D22312E30223F3E0D0A3C416D617A6F6E5365637572697479496E666F20786D6C6E733A7873693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E63652220786D6C6E733A7873643D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D61223E0D0A20203C4D65726368616E7449643E4132573249444D35434A354F314B3C2F4D65726368616E7449643E0D0A20203C4D61726B6574706C61636549643E0D0A202020203C737472696E673E41314638334738433241524F37503C2F737472696E673E0D0A20203C2F4D61726B6574706C61636549643E0D0A3C2F416D617A6F6E5365637572697479496E666F3E 
            var data = Convert.FromBase64String("PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8QW1hem9uU2VjdXJpdHlJbmZvIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiIHhtbG5zOnhzZD0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiPg0KICA8TWVyY2hhbnRJZD5BMlcySURNNUNKNU8xSzwvTWVyY2hhbnRJZD4NCiAgPE1hcmtldHBsYWNlSWQ+DQogICAgPHN0cmluZz5BMUY4M0c4QzJBUk83UDwvc3RyaW5nPg0KICA8L01hcmtldHBsYWNlSWQ+DQo8L0FtYXpvblNlY3VyaXR5SW5mbz4=");
            var info = Serialized.Deserialize<AmazonSecurityInfo>(data);
            Assert.That(info.MerchantId, Is.EqualTo("A2W2IDM5CJ5O1K"));
        }

        [Test]
        public void can_serialize_amazon_non_generic()
        {
            var data = new AmazonSecurityInfo("A2W2IDM5CJ5O1K", null /*todo*/);
            var serialized = new Serialized(data);
            var actual = Serialized.Deserialize<AmazonSecurityInfo>(serialized);
            Assert.That(actual.MerchantId, Is.EqualTo("A2W2IDM5CJ5O1K"));
        }
    }
}
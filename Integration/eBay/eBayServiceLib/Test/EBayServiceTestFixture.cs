namespace EzBob.eBayServiceLib.Test {
	using System.Collections.Generic;
	using EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests;
	using EzBob.eBayServiceLib.com.ebay.developer.soap;
	using NUnit.Framework;

	using System.IO;
	using Ezbob.Utils.Serialization;

	[TestFixture]
	class EBayServiceTestFixture {
		[Test]
		public void Deserialize() {
			using (var fs = new FileStream(@"D:\5055375325_report.xml", FileMode.Open)) {
				var data = Serialized.Deserialize<FileAttachmentDataInfo>(fs);
				Assert.IsNotNull(data);
			}
		}

		[Test]
		public void Deserialize2() {
			using (var fs = new FileStream(@"d:\attach1.xml", FileMode.Open)) {
				var data = Serialized.Deserialize<FileAttachmentDataInfo>(fs);
				Assert.IsNotNull(data);
			}
		}

		[Test]
		public void Serialize() {
			var attach = new FileAttachmentDataInfo {
				ActiveInventoryReport = new ActiveInventoryReport {
					SKUDetails = new List<SKUDetails>
					{
						new SKUDetails
						{
							ItemID = "2",
							Price = new Price
							{
								currencyID = CurrencyCodeType.USD.ToString(),
								Value = 20.15
							},
							Quantity = 25,
							SKU = "555"
						},

						new SKUDetails
						{
							ItemID = "3",
							Price = new Price
							{
								currencyID = CurrencyCodeType.GBP.ToString(),
								Value = 10
							},
							Quantity = 1,
							SKU = "111"
						}
					}
				}
			};
			Serialized.Serialize(@"d:\attach1.xml", attach);
		}

	}
}

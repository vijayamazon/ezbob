using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests;
using EzBob.eBayServiceLib.com.ebay.developer.soap;
using NUnit.Framework;

namespace EzBob.eBayServiceLib.Test
{
	[TestFixture]
	class EBayServiceTestFixture
	{
		[Test]
		public void Deserialize()
		{
			string fileName = @"D:\5055375325_report.xml";

			var data = SerializeDataHelper.DeserializeTypeFromFile<FileAttachmentDataInfo>( fileName );

			Assert.IsNotNull( data );
		}

		[Test]
		public void Deserialize2()
		{
			string fileName = @"d:\attach1.xml";

			var data = SerializeDataHelper.DeserializeTypeFromFile<FileAttachmentDataInfo>( fileName );

			Assert.IsNotNull( data );
		}

		[Test]
		public void Serialize()
		{
			var attach = new FileAttachmentDataInfo
			{
				ActiveInventoryReport = new ActiveInventoryReport
				{
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
			SerializeDataHelper.SerializeToFile( @"d:\attach1.xml", attach );
		}

		
	}
}

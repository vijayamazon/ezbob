using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.Inventory;
using EzBob.CommonLib;
using EzBob.eBayServiceLib.FileTransferServiceReference;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.BulkData;
using EzBob.eBayServiceLib.LargeMerchantServiceCore.ServiceLogic.FileTransfer;
using EzBob.eBayServiceLib.TradingServiceCore.TokenProvider;
using Ionic.Zip;

namespace EzBob.eBayServiceLib.LargeMerchantServiceCore.Requests
{
	using Ezbob.Utils.Serialization;

	public class ActiveInventoryReport
	{
		[XmlElement]
		public List<SKUDetails> SKUDetails { get;  set;}
	}

	public class SKUDetails
	{
		public string SKU { get; set; }
		public Price Price { get; set; }
		public int Quantity { get; set; }
		public string ItemID { get; set; }
		public int BidCount { get; set; }
	}

	[XmlRoot( "BulkDataExchangeResponses", Namespace = "urn:ebay:apis:eBLBaseComponents" )]
	public class FileAttachmentDataInfo
	{
		public ActiveInventoryReport ActiveInventoryReport { get; set;}

	}

	public class Price
	{
		[XmlAttribute]
		public string currencyID { get; set; }

		[XmlText]
		public double Value { get; set; }
	}

	public static class ExtractFilesHelper
	{
		public static IEnumerable<T> Extract<T>( FileAttachment fileAttach )
		{
			var list = new List<T>();

			if ( fileAttach == null || fileAttach.Data == null)
			{
				return list;
			}			

			var data = fileAttach.Data;

			using ( var mem = new MemoryStream( data ) )
			{
				var zip = ZipFile.Read( mem );
				
				using ( var rezMem = new MemoryStream() )
				{
					foreach ( ZipEntry entry in zip )
					{
						entry.Extract( rezMem );
						rezMem.Flush();
						rezMem.Seek( 0, SeekOrigin.Begin);

						var item = Serialized.Deserialize<T>( rezMem );

						list.Add( item );
					}
				}
			}
			return list;
		}
	}

	public class ActiveInventoryReportRequest : LargeMerchantServiceRequestBase
	{
		public ActiveInventoryReportRequest(EbayBulkDataServiceProvider bulkServiceProvider, EbayFileTransferServiceProvider fileTransferProvider,  IServiceTokenProvider tokenProvider)
			: base(bulkServiceProvider , fileTransferProvider, tokenProvider )
		{

		}

		public DatabaseInventoryList GetReport()
		{
			var requestsCounter = new RequestsCounterData();
			ResultDownloadFileRequest fileAttach = InternalGetReport( requestsCounter );

			if ( fileAttach == null )
			{
				return null;
			}

			var rezList = ExtractFilesHelper.Extract<FileAttachmentDataInfo>( fileAttach.FileAttachment );

			var rez = new DatabaseInventoryList(fileAttach.SubmittedDate)
				{					
					RequestsCounter = requestsCounter
				};

			var inventoryItems = rezList.Where(r => r.ActiveInventoryReport != null && r.ActiveInventoryReport.SKUDetails != null && r.ActiveInventoryReport.SKUDetails.Count > 0)
										.SelectMany( r => r.ActiveInventoryReport.SKUDetails)				
										.Where( s => s != null && s.Price != null )
										.Select( s => new DatabaseInventoryItem
										{
											ItemId = s.ItemID,
											Amount = new AmountInfo { Value = s.Price.Value, CurrencyCode = s.Price.currencyID.ToString( CultureInfo.InvariantCulture ) },
											Quantity = s.Quantity,
											Sku = s.SKU
										} );
			if ( inventoryItems.Any() )
			{
				rez.AddRange(inventoryItems);
			}
			return rez;
		}

		public override BulkDataReportType ReportType
		{
			get { return BulkDataReportType.ActiveInventoryReport; }
		}
	}	
}
using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;

namespace EzBob.AmazonDbLib
{
	/*
	Note:
	 * 1) create function in AmazonDatabaseFunctionStorage
	 * 2) define new case in class AmazonDatabaseFunctionTypeConverter for display name	 
	 */
	public enum AmazonDatabaseFunctionType
	{
		NumOfOrders,
		TotalItemsOrdered,
		TotalSumOfOrders,
		TotalSumOfOrdersAnnualized,
		AverageItemsPerOrder,
		AverageSumOfOrder,
		CancelledOrdersCount,
		OrdersCancellationRate,
		InventoryTotalItems,
		InventoryTotalValue,
		/*TotalReturns,
		ReturnsToSalesRate,
		SoldItemsTrend,
		SoldValueTrend,
		BuyerName,
		BuyerEmail,
		ShippingAddress,
		UserRating,
		FeedbackCount,
		FeedbackNegative,
		FeedbackNeutral,
		FeedbackPositive,

		InventoryUseAFN*/
	}

	internal class AmazonDatabaseFunctionStorage : DatabaseFunctionStorage<AmazonDatabaseFunctionType>
	{
		private static AmazonDatabaseFunctionStorage _Instance;

		private AmazonDatabaseFunctionStorage()
			:base(new AmazonDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{88F980B5-4930-4866-9331-9853E5B996B4}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.AverageItemsPerOrder, DatabaseValueTypeEnum.Integer ,"{B252CF02-3D84-492C-9E8A-3D18718BF727}" );
			CreateFunctionAndAddToCollection(  AmazonDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{F6921ACC-9E18-49C3-8E21-94E2289EF05A}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.CancelledOrdersCount, DatabaseValueTypeEnum.Integer,"{A8B6DBE6-46F9-468C-8179-E9B714A1D131}" ) ;
			CreateFunctionAndAddToCollection(  AmazonDatabaseFunctionType.OrdersCancellationRate, DatabaseValueTypeEnum.Double, "{0192BC7F-A3E6-4640-A607-0CB2C14C3DF5}" ) ;
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.TotalItemsOrdered, DatabaseValueTypeEnum.Integer, "{1E360E0A-AAE6-496A-9769-508B9428FDAC}" );
			CreateFunctionAndAddToCollection(AmazonDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{63235650-ACD7-4F73-9537-5D762B0B7D0A}");
			CreateFunctionAndAddToCollection(AmazonDatabaseFunctionType.TotalSumOfOrdersAnnualized, DatabaseValueTypeEnum.Double, "{1F5C801E-B845-400C-BA34-8F2552165B74}");
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.InventoryTotalItems, DatabaseValueTypeEnum.Integer, "{04A9EB90-E0E6-4CBF-9003-55AA414A16BA}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.InventoryTotalValue, DatabaseValueTypeEnum.Double, "{94BFA976-BFEF-4DFF-AF25-6D31EF1EDFDD}" );

			/*CreateFunctionAndAddToCollection(  AmazonDatabaseFunctionType.ReturnsToSalesRate, DatabaseValueTypeEnum.Integer, "{0130006B-173E-4B48-A245-E594E85D63F4}" ) ;
			CreateFunctionAndAddToCollection(  AmazonDatabaseFunctionType.SoldItemsTrend, DatabaseValueTypeEnum.Integer , "{90AC41D6-5514-4314-A1DD-2FA45EED44FE}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.SoldValueTrend, DatabaseValueTypeEnum.Integer, "{7AA7384C-1476-4545-A45C-1DAE7A2E7081}" );			
			CreateFunctionAndAddToCollection(  AmazonDatabaseFunctionType.TotalReturns, DatabaseValueTypeEnum.Double,"{82C0AF50-8A3E-4509-BBF9-9AA5BC052282}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.UserRating, DatabaseValueTypeEnum.Double, "{0A0C7E3A-A93A-4A26-84CE-0D64836FEA4F}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.FeedbackCount, DatabaseValueTypeEnum.Integer, "{8D052289-AC01-48A8-81BB-4819FB3DC898}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.FeedbackNegative, DatabaseValueTypeEnum.Integer, "{F9057644-B6EE-4DB5-AF02-BA9D26881CBC}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.FeedbackNeutral, DatabaseValueTypeEnum.Integer, "{0AAD83F3-1A8E-4AA3-97EE-377C6D06F131}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.FeedbackPositive, DatabaseValueTypeEnum.Integer, "{7825CEC6-9122-43BB-88B6-84AFFC24C873}" );
			CreateFunctionAndAddToCollection( AmazonDatabaseFunctionType.InventoryUseAFN, DatabaseValueTypeEnum.Boolean, "{0D79B064-C9BA-4048-9A6B-7A8A62DE5F11}" );
			 */
		}

		public static AmazonDatabaseFunctionStorage Instance
		{
			get 
			{
				return _Instance ?? ( _Instance = new AmazonDatabaseFunctionStorage() );
			}
		}
	}

	class AmazonDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<AmazonDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert( AmazonDatabaseFunctionType type )
		{
			string displayName = string.Empty;
			string description = string.Empty;

			string name = type.ToString();

			switch ( type )
			{
				case AmazonDatabaseFunctionType.NumOfOrders:
					displayName = "Num of Orders";
					break;

				case AmazonDatabaseFunctionType.AverageItemsPerOrder:
					displayName = "Average Items per Order";
					break;

				case AmazonDatabaseFunctionType.AverageSumOfOrder:
					displayName = "Average Sum of Order";
					break;

				case AmazonDatabaseFunctionType.CancelledOrdersCount:
					displayName = "Cancelled Orders Count";
					break;

				case AmazonDatabaseFunctionType.OrdersCancellationRate:
					displayName = "Orders Cancellation Rate";
					break;

				case AmazonDatabaseFunctionType.TotalItemsOrdered:
					displayName = "Total Items Ordered";
					break;

				case AmazonDatabaseFunctionType.TotalSumOfOrders:
					displayName = "Total Sum of Orders";
					break;

				case AmazonDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					displayName = "Total Sum of Orders Annualized";
					break;

				case AmazonDatabaseFunctionType.InventoryTotalValue:
					displayName = "Total Value of Inventory";
					break;

				case AmazonDatabaseFunctionType.InventoryTotalItems:
					displayName = "Total Items in Inventory";
					break;

				/*case AmazonDatabaseFunctionType.ReturnsToSalesRate:
					name = "Returns to Sales Rate";
					break;

				case AmazonDatabaseFunctionType.SoldItemsTrend:
					name = "Sold Items Trend";
					break;

				case AmazonDatabaseFunctionType.SoldValueTrend:
					name = "Sold Value Trend";
					break;

				case AmazonDatabaseFunctionType.TotalReturns:
					name = "Total Returns";
					break;

				case AmazonDatabaseFunctionType.UserRating:
					name = "User Raining";
					break;

				case AmazonDatabaseFunctionType.FeedbackCount:
					name = "Feedback Count";
					break;

				case AmazonDatabaseFunctionType.FeedbackNegative:
					name = "Feedback Negative";
					break;

				case AmazonDatabaseFunctionType.FeedbackNeutral:
					name = "Feedback Neutral";
					break;

				case AmazonDatabaseFunctionType.FeedbackPositive:
					name = "Feedback Positive";
					break;

				case AmazonDatabaseFunctionType.InventoryUseAFN:
					name = "InventoryUseAFN";
					break;
				*/

				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo( name, displayName, description );
		}
	}
}

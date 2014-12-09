using System.Collections.Generic;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonServiceLib;
using NUnit.Framework;
using StructureMap;

namespace EzBob.AmazonLib
{
	using Ezbob.RegistryScanner;

	public class AmazonTest
	{

		[SetUp]
		public void Init()
		{
			Scanner.Register();
		}

		[Test]
		public void SaveAmazonMarketplace()
		{
			int customerId = 1;

			var databaseCustomer = GetCustomerInfo( customerId );

			var siList = new[]
				{
					"A4G97F1RP09ZP",
					"A1CZA06GMVNMNX"
				};

			var amazons = new List<AmazonSecurityInfo>();

			foreach (var si in siList)
			{
				var amazonSecurityInfo = new AmazonSecurityInfo( si );
				amazonSecurityInfo.AddMarketplace( "A1F83G8C2ARO7P" );
				amazons.Add( amazonSecurityInfo );	
			}

			foreach (var amazonSecurityInfo in amazons)
			{
				AmazonRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo( databaseCustomer, amazonSecurityInfo, "Real Amazon Account" );	
			}

		}

		private static AmazonRetriveDataHelper AmazonRetriveDataHelper
		{
			get
			{
				var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
				var marketPlace = ObjectFactory.GetInstance<AmazonDatabaseMarketPlace>();
				return marketPlace.GetRetrieveDataHelper( helper ) as AmazonRetriveDataHelper;
			}
		}

		private static Customer GetCustomerInfo( int customerId )
		{
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			return helper.GetCustomerInfo( customerId );
		}

		public static void TestRetrieveAmazonOrdersData(int customerId)
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateClientOrdersInfo( databaseCustomer);
		}

		public static void TestRetrieveAmazonInventoryData( int customerId )
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateClientInventoriesInfo( databaseCustomer );
		}

		public static void TestRetrieveAmazonUserFeedbackInfo( int customerId )
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateAmazonUserFeedbackInfo( databaseCustomer );
		}

		public static void TestStoreCustomerSecurityData( int customerId )
		{
			var databaseCustomer = GetCustomerInfo(customerId);

			var amazonSecurityInfo = new AmazonSecurityInfo( "A1OXZLJTRHTZJ3" );
			amazonSecurityInfo.AddMarketplace( "A1F83G8C2ARO7P" );

            AmazonRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(databaseCustomer, amazonSecurityInfo, "Real Amazon Account");
		}

		/*private static void AddOrUpdateCustomer2()
		{
			int customerId = 2;
			IDatabaseCustomer databaseCustomer = GetCustomerInfo( customerId );

			var amazonSecurityInfo = new AmazonSecurityInfo( "ADT771YAIMFKH" );

            AmazonRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(databaseCustomer, amazonSecurityInfo, "Test2");
		}

		private static void AddOrUpdateCustomer3()
		{
			int customerId = 3;
            IDatabaseCustomer databaseCustomer = GetCustomerInfo(customerId);

			var amazonSecurityInfo = new AmazonSecurityInfo( "A26O6R3HT824TS" );

            AmazonRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(databaseCustomer, amazonSecurityInfo, "Test3");
		}*/

        private static void UpdateClientOrdersInfo(Customer databaseCustomer)
		{
            //AmazonRetriveDataHelper.UpdateClientOrdersInfo(databaseCustomer, ActionAccessType.Full);
		}

        private static void UpdateClientInventoriesInfo(Customer databaseCustomer)
		{
            //AmazonRetriveDataHelper.UpdateClientInventoryInfo(databaseCustomer, ActionAccessType.Full);
		}

        private static void UpdateAmazonUserFeedbackInfo(Customer databaseCustomer)
		{
            //AmazonRetriveDataHelper.UpdateClientFeedbackInfo(databaseCustomer);
		}
	}
}

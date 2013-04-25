using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.eBayServiceLib;
using StructureMap;

namespace EzBob.eBayLib
{
	public static class eBayTest
	{
		public static eBayRetriveDataHelper EBayRetriveDataHelper
		{
			get
			{
				var marketPlace = ObjectFactory.GetInstance<eBayDatabaseMarketPlace>();
				
				var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
				return marketPlace.GetRetrieveDataHelper( helper ) as eBayRetriveDataHelper;
			}
		}

		public static Customer GetCustomerInfo( int customerId )
		{
			return EBayRetriveDataHelper.GetCustomerInfo( customerId );			
		}

		public static void TestStoreCustomerSecurityData( int customerId, string token, string marketplaceName = null )
		{
			var databaseCustomer = GetCustomerInfo( customerId );

			var data = new eBaySecurityInfo
			{
				Token = token
			};

			if ( string.IsNullOrEmpty( marketplaceName ) )
			{
				var info = EBayRetriveDataHelper.GetCustomerUserInfo(data);
				marketplaceName = info.UserID;
			}
			EBayRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo( databaseCustomer, data, marketplaceName );
		}

		public static void TestStoreCustomerSecurityData( int customerId )
		{
			string token = @"AgAAAA**AQAAAA**aAAAAA**O3k6Tw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wHkYKiDJOLqQmdj6x9nY+seQ**qWEBAA**AAMAAA**nRW2tLgiukfpQJQ6eiLxXbzXO0C7UHhoQMFnXihl+AWomDL6Tw+5IZ/7FZ/EWSjap1znb/GbXnNQxRXJOrmlcBNgdf0FKxvfBjJFq4XSr6X6tjdmIbZLKoX89VrwmRd7Gcf7PuvxQBM0Jncr5LTQ+/ycA2dHcWQ5VFSujWYfn0E6ixMeDtRdUTGhXzng5OOrgTQfZztde6erYLeO5YnaWzSVypoui5tVOBP5EYTHQxzQPOZ1GV0/w7ww5ar3aLnZ56UMfungRscgz5lN+kTo4/lgbgDsv/GUBvUUjb0ZsYEGGJWHUGFK8X7AN40KtcQW9q+TGcgz5QU1G70IGDy+etg3xa9pPmhRA6+ZGbp+cKw8wzZLja/iMz5jEfLlCngBytHSTy4/KRdyLraWpR2ZDNoc0HKjnlew6mDmsGejV6J1EPm/r22vIOzpiPJa4Ndu5kDRISv9FHuUt3fgNANbu0j6abMIAhhb6VS6pby0m5NOxx7SDilEf8iFIMgBJ5U5DV9mXvjmzFNLf+GD6okyJ8uH5MvxRLRBD/PW/ybN5MMRWqF1ZA2NkNCUPKciUNxtIkTrDPc7Nr4nksEoZh+cEPcL9lFVBU8emMjhwF7HIioJMiC7KsOeJ3uCISYZpRwFEPIYzC5Qm3JiuD71T1PBCTKzVfhfcx4XfQyb7HZuhPU6qwkqm1sjoId9mJBGP1+QS85fo9wE8sCeBfZOrfHw89nX2G7chz4/zI5pbomXUAn0Wuqb4wtvQ+fNMWM7ssIp";
			TestStoreCustomerSecurityData(customerId, token, "Real eBay account");
		}

		public static void TestUpdateAllUserInfo( int customerId )
		{
			var databaseCustomer = GetCustomerInfo( customerId );

			UpdateUserInfo( databaseCustomer );
		}

		public static void TestUpdateAllAccountInfo( int customerId )
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateAccountInfo( databaseCustomer );
		}

		public static void TestUpdateFeedbackInfo( int customerId )
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateFeedbackInfo( databaseCustomer );
		}
		public static void TestUpdateInventoryInfo( int customerId )
		{
            var databaseCustomer = GetCustomerInfo(customerId);

			UpdateInventoryInfo( databaseCustomer );
		}

		private static void UpdateUserInfo( Customer databaseCustomer )
		{
            //EBayRetriveDataHelper.UpdateUserInfo(databaseCustomer);
		}

        private static void UpdateAccountInfo(Customer databaseCustomer)
		{
            //EBayRetriveDataHelper.UpdateAccountInfo(databaseCustomer);
		}

        private static void UpdateFeedbackInfo(Customer databaseCustomer)
		{
            //EBayRetriveDataHelper.UpdateFeedbackInfo(databaseCustomer);
		}

        private static void UpdateInventoryInfo(Customer databaseCustomer)
		{
            //EBayRetriveDataHelper.UpdateInventoryInfo(databaseCustomer);
		}

		public static void TestUpdateOrdersInfo( int customerId )
		{
			var databaseCustomer = GetCustomerInfo( customerId );

			UpdateOrdersInfo( databaseCustomer );
		}

        private static void UpdateOrdersInfo(Customer databaseCustomer)
		{
			//EBayRetriveDataHelper.UpdateOrdersInfo( databaseCustomer );
		}

		public static void TestUpdateCategoriesInfo(int customerId)
		{
			//IDatabaseCustomer databaseCustomer = GetCustomerInfo( customerId );
			//EBayRetriveDataHelper.UpdateCategoriesInfo( databaseCustomer );
		}
	}
}
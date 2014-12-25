using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.com.paypal.service;
using StructureMap;
using NUnit.Framework;

namespace EzBob.PayPal {
	public class PayPalTest {
		public static void StoreTestData(int customerId) {
			// SandBox			
			var customer = GetCustomerInfo(customerId);
			var payPalRermissionsGranted = new PayPalPermissionsGranted {
				RequestToken = "AAAAAAAVmrAQ76F.gMx3",
				VerificationCode = "crvExLZKaxtirew2JyUTGg",
				TokenSecret = "G6VvI9y1VUkCpcPcClZan4pIPOk",
				AccessToken = "EcyNhOBodjBPddhExkH29I53SOQ30tdHg0w5eybIIrFl1R77ljQ3bA",
			};

			var secData = new PayPalSecurityData {
				PermissionsGranted = payPalRermissionsGranted,
				UserId = "cvitaly@ukr.net"
			};

			PayPalRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(customer, secData, "Test PayPal Account");
		}

		public static void UpdateTransactionInfo(int customerId) {
			var customer = GetCustomerInfo(customerId);

			//PayPalRetriveDataHelper.UpdateTransactionInfo( customer );
		}

		private static PayPalRetriveDataHelper PayPalRetriveDataHelper {
			get {
				var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();

				var marketPlace = ObjectFactory.GetInstance<PayPalDatabaseMarketPlace>();

				return marketPlace.GetRetrieveDataHelper(helper) as PayPalRetriveDataHelper;
			}
		}
		private static Customer GetCustomerInfo(int customerId) {
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			return helper.GetCustomerInfo(customerId);
		}
	}

	internal class Test {
		[Test]
		public void TestGetAccountInfo() {
			//int customerId = 1;
			//PayPalTest.UpdateAccountInfo( customerId );
		}

		[Test]
		public void TestMethod1() {
			var client = new PayPalAPIInterfaceClient();
			var clientAA = new PayPalAPIAAInterfaceClient();
			//client.BillAgreementUpdate
			//clientAA.DoReferenceTransaction
			//clientAA.ManagePendingTransactionStatus
			//clientAA.ReverseTransaction
			//client.GetTransactionDetails
			//client.RefundTransaction
			//client.TransactionSearch
		}
	}
}

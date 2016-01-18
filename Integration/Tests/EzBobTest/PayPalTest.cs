using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib.com.paypal.service;
using StructureMap;
using NUnit.Framework;

namespace EzBobTest {
	using System;
	using Ezbob.Utils;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.PayPalServiceLib;
	using EzBob.PayPalServiceLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper.Transactions;

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

		public static PayPalRetriveDataHelper PayPalRetriveDataHelper {
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

	[TestFixture]
	internal class Test:BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => {x.For<IServiceEndPointFactory>().Use<ServiceEndPointFactory>();});
			//Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		[Test]
		public void TestGetAccessToken() {
			//int customerId = 1;
			var access = PayPalTest.PayPalRetriveDataHelper.GetAccessToken("AAAAAAvnM4DyQLD5CARQ", "ykDHCR5791EhvPiMxXF6Aw");

		}

		[Test]
		public void TestGetAccountInfo() {
			//int customerId = 1;
			var access = PayPalTest.PayPalRetriveDataHelper.GetAccountInfo(new PayPalPermissionsGranted{
				RequestToken = "AAAAAAvoMRxkeqwRbNet",
				VerificationCode = "BUZDemaiLtrC-008xfzUug",

				AccessToken = "i0JEalB9y89Uq9VA.fjjsrdWnlPsVe8rwkIbPgvvP1clFnyEbHt29A",
				TokenSecret = "kw1bQNNMe0bJj-81OAVfFXJh9ZY",
			});

		}

		[Test]
		public void TestGetTransactions() {
			//int customerId = 1;
			var reqInfo = new PayPalRequestInfo {
				SecurityInfo = new PayPalSecurityData() {
					PermissionsGranted = new PayPalPermissionsGranted {
						RequestToken = "AAAAAAvoMRxkeqwRbNet",
						VerificationCode = "BUZDemaiLtrC-008xfzUug",

						AccessToken = "i0JEalB9y89Uq9VA.fjjsrdWnlPsVe8rwkIbPgvvP1clFnyEbHt29A",
						TokenSecret = "kw1bQNNMe0bJj-81OAVfFXJh9ZY",
					},
					UserId = "info@bambinos2tots.co.uk"
				},
				StartDate = DateTime.Today.AddDays(-30),
				EndDate = DateTime.Today,
				ErrorRetryingInfo = new ErrorRetryingInfo(false),
				OpenTimeOutInMinutes = 1,
				SendTimeoutInMinutes = 1,
				DaysPerRequest = 30
			};

			PayPalServiceHelper.GetTransactionData(reqInfo, data => {
				Console.WriteLine("data {0}", data.Capacity);
				return true;
			});
		}

		[Test]
		public void TestMethod1() {
			//var client = new PayPalAPIInterfaceClient();
			//var clientAA = new PayPalAPIAAInterfaceClient();
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

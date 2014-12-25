namespace PayPoint {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;

	public static class StringToStreamExtension {
		public static Stream ToStream(this string str) {
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}

	public class PayPointRetrieveDataHelper : MarketplaceRetrieveDataHelperBase {
		public PayPointRetrieveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		)
			: base(helper, marketplace) {
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			IDatabaseCustomerMarketPlace customerMarketPlace = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);
			return Serialized.Deserialize<PayPointSecurityInfo>(customerMarketPlace.SecurityData);
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			var securityInfo = (PayPointSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

			return UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);
		}

		private ElapsedTimeInfo UpdateClientOrdersInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			PayPointSecurityInfo securityInfo,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			string condition = Helper.GetPayPointDeltaPeriod(databaseCustomerMarketPlace);

			var payPointTransactions = PayPointConnector.GetOrders(
				condition,
				securityInfo.Mid,
				securityInfo.VpnPassword,
				securityInfo.RemotePassword
			);

			var payPointOrders = new List<PayPointOrderItem>();

			foreach (PayPointDataSet.TransactionRow x in payPointTransactions) {
				var order = new PayPointOrderItem {
					acquirer = x.acquirer,
					amount = x.amount,
					auth_code = x.auth_code,
					authorised = x.authorised,
					card_type = x.card_type,
					cid = x.cid,
					classType = x._class,
					company_no = x.company_no,
					country = x.country,
					currency = x.currency,
					cv2avs = x.cv2avs,
					deferred = x.deferred,
					emvValue = x.emvValue,
					fraud_code = x.fraud_code,
					FraudScore = x.FraudScore,
					ip = x.ip,
					lastfive = x.lastfive,
					merchant_no = x.merchant_no,
					message = x.message,
					MessageType = x.MessageType,
					mid = x.mid,
					name = x.name,
					options = x.options,
					status = x.status,
					tid = x.tid,
					trans_id = x.trans_id
				};

				DateTime result;
				order.date = !DateTime.TryParse(x.date, out result) ? (DateTime?)null : result;
				order.ExpiryDate = !DateTime.TryParse(x.ExpiryDate, out result) ? (DateTime?)null : result;
				order.start_date = !DateTime.TryParse(x.start_date, out result) ? (DateTime?)null : result;
				payPointOrders.Add(order);
			}

			var elapsedTimeInfo = new ElapsedTimeInfo();

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StorePayPointOrdersData(
					databaseCustomerMarketPlace,
					new PayPointOrdersList(DateTime.UtcNow, payPointOrders),
					historyRecord
				)
			);

			/*

			This stored procedure does not exist as of today (December 25 2014, Merry Xmas!).
			It should be created similarly to other UpdateMpTotals*** procedures based on PayPointOrdersAggregator.cs.

			DbConnectionGenerator.Get().ExecuteNonQuery(
				"UpdateMpTotalsPayPoint",
				CommandSpecies.StoredProcedure,
				new QueryParameter("HistoryID", historyRecord.Id)
			);
			*/

			return elapsedTimeInfo;
		}
	}
}

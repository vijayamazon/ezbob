using System.Linq;
using EzBob.CommonLib;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.Model.Database;
using System;
using System.Collections.Generic;
using Ezbob.HmrcHarvester;
using Ezbob.Logger;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using log4net;
using Coin = EZBob.DatabaseLib.Common.Coin;

namespace Integration.ChannelGrabberFrontend {
	#region class RetrieveDataHelper

	public class RetrieveDataHelper : MarketplaceRetrieveDataHelperBase<FunctionType> {
		#region public

		#region constructor

		public RetrieveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBase<FunctionType> marketplace,
			VendorInfo oVendorInfo
		) : base(helper, marketplace) {
			m_oVendorInfo = oVendorInfo;
		} // constructor

		#endregion constructor

		#region method RetrieveCustomerSecurityInfo

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(
			int customerMarketPlaceId
		) {
			var account = GetDatabaseCustomerMarketPlace(customerMarketPlaceId);

			try {
				return SerializeDataHelper.DeserializeType<AccountModel>(account.SecurityData);
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})",
					account.DisplayName, account.Id), e);
			}
		} // RetrieveSecurityInfo

		#endregion method RetrieveCustomerSecurityInfo

		#endregion public

		#region protected

		#region method InternalUpdateInfo

		protected override void InternalUpdateInfo(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			AccountModel oSecInfo = null;

			try {
				oSecInfo = SerializeDataHelper.DeserializeType<AccountModel>(databaseCustomerMarketPlace.SecurityData);
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})",
					databaseCustomerMarketPlace.DisplayName, databaseCustomerMarketPlace.Id), e);
			} // try

			AccountData ad = oSecInfo.Fill();

			bool bRunningInWebEnv = Connector.FetchRunningInWebEnvFlag(ad.AccountTypeName(), databaseCustomerMarketPlace.Id, new SafeILog(ms_oLog));

			ms_oLog.DebugFormat(
				"{0} marketplace with id {1} is {2}running in web env now.",
				ad.AccountTypeName(), databaseCustomerMarketPlace.Id, bRunningInWebEnv ? "" : "not "
			);

			switch (ad.VendorInfo.Behaviour) {
			case Behaviour.Default:
				// nothing to do here
				break;

			case Behaviour.HMRC:
				if (!bRunningInWebEnv) {
					if ((ad.Login == databaseCustomerMarketPlace.Customer.Name) && (ad.Password == "topsecret")) {
						ms_oLog.DebugFormat(
							"{0} account with id {1} was created by uploading files, nothing to update, bailing out.",
							ad.AccountTypeName(), databaseCustomerMarketPlace.Id
						);

						return;
					} // if account was created by uploading files
				} // if not in web env

				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch

			var ctr = new Connector(ad, ms_oLog, databaseCustomerMarketPlace.Customer);

			if (ctr.Init()) {
				try {
					ctr.Run(false, databaseCustomerMarketPlace.Id);

					switch (ad.VendorInfo.Behaviour) {
					case Behaviour.Default:
						ProcessRetrieved(
							ctr.DataHarvester,
							databaseCustomerMarketPlace,
							historyRecord,
							DefaultConversion,
							Helper.StoreChannelGrabberOrdersData,
							Helper.GetAllChannelGrabberOrdersData
						);
						break;

					case Behaviour.HMRC:
						ProcessRetrieved(
							ctr.DataHarvester,
							databaseCustomerMarketPlace,
							historyRecord,
							HmrcVatReturnConversion,
							Helper.StoreHmrcVatReturnData,
							Helper.GetAllHmrcVatReturnData
						);

						ProcessRetrieved(
							ctr.DataHarvester,
							databaseCustomerMarketPlace,
							historyRecord,
							HmrcRtiTaxMonthConversion,
							Helper.StoreHmrcRtiTaxMonthData,
							Helper.GetAllHmrcRtiTaxMonthData
						);

						break;

					default:
						throw new ApiException("Unsupported behaviour for CG flavour: " + ad.VendorInfo.Behaviour.ToString());
					} // switch
				}
				catch (ApiException) {
					ctr.Done();
					throw;
				} // try
			} // if Init succeeded
			else
				throw new ApiException("Failed to initialise CG connector.");
		} // InternalUpdateInfo

		#endregion method InternalUpdateInfo

		#region method AddAnalysisValues

		protected override void AddAnalysisValues(
			IDatabaseCustomerMarketPlace marketPlace,
			AnalysisDataInfo data
		) {
		} // AddAnalysisValues

		#endregion method AddAnalysisValues

		#endregion protected

		#region private

		#region method ProcessRetrieved

		private void ProcessRetrieved(
			IHarvester oHarvester,
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord,
			Func<IHarvester, List<AInternalOrderItem>> oConversion,
			Action<IDatabaseCustomerMarketPlace, InternalDataList, MP_CustomerMarketplaceUpdatingHistory> oStoreToDatabaseAction,
			Func<DateTime, IDatabaseCustomerMarketPlace, InternalDataList> oLoadAllFromDatabaseFunc
		) {
			// Convert orders into internal format.
			List<AInternalOrderItem> oChaGraOrders = oConversion(oHarvester);

			var elapsedTimeInfo = new ElapsedTimeInfo();

			// store retrieved orders to DB.
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => oStoreToDatabaseAction(
					databaseCustomerMarketPlace,
					new InternalDataList(DateTime.UtcNow, oChaGraOrders),
					historyRecord
				)
			);

			// retrieve ALL the orders from DB
			InternalDataList allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => oLoadAllFromDatabaseFunc(DateTime.UtcNow, databaseCustomerMarketPlace)
			);

			// calculate aggregated
			IEnumerable<IWriteDataInfo<FunctionType>> aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.AggregateData,
				() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter)
			);

			// Store aggregated
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord)
			);
		} // ProcessRetrieved

		#endregion method ProcessRetrieved

		#region Orders list: remote to internal conversion

		#region method DefaultConversion

		private List<AInternalOrderItem> DefaultConversion(IHarvester oHarvester) {
			List<Order> oRawOrders = ((Integration.ChannelGrabberAPI.Harvester)oHarvester).RetrievedOrders;

			var oChaGraOrders = new List<AInternalOrderItem>();

			if (oRawOrders != null) {
				foreach (var oRaw in oRawOrders) {
					oChaGraOrders.Add(new ChannelGrabberOrderItem {
						CurrencyCode = oRaw.CurrencyCode,
						OrderStatus = oRaw.OrderStatus,
						NativeOrderId = oRaw.NativeOrderId,
						PaymentDate = oRaw.PaymentDate,
						PurchaseDate = oRaw.PurchaseDate,
						TotalCost = oRaw.TotalCost,
						IsExpense = oRaw.IsExpense
					});
				} // foreach
			} // if

			return oChaGraOrders;
		} // DefaultConversion
 
		#endregion method DefaultConversion

		#region method HmrcVatReturnConversion

		private List<AInternalOrderItem> HmrcVatReturnConversion(IHarvester oHarvester) {
			var oVatEntries = new List<AInternalOrderItem>();

			foreach (KeyValuePair<string, ISeeds> pair in ((Ezbob.HmrcHarvester.Harvester)oHarvester).Hopper.Seeds[DataType.VatReturn]) {
				string sFileName = pair.Key;
				var oData = (VatReturnSeeds)pair.Value;

				var oEntry = new VatReturnEntry {
					BusinessAddress = oData.BusinessAddress,
					BusinessName = oData.BusinessName,
					DateDue = oData.DateDue,
					DateFrom = oData.DateFrom,
					DateTo = oData.DateTo,
					Period = oData.Period,
					RegistrationNo = oData.RegistrationNo,
				};

				foreach (KeyValuePair<string, Ezbob.HmrcHarvester.Coin> oBoxData in oData.ReturnDetails) {
					oEntry.Data[oBoxData.Key] = new EZBob.DatabaseLib.Common.Coin(
						oBoxData.Value.Amount,
						oBoxData.Value.CurrencyCode
					);
				} // for each box

				oVatEntries.Add(oEntry);
			} // for each file

			return oVatEntries;
		} // HmrcVatReturnConversion
 
		#endregion method HmrcVatReturnConversion

		#region method HmrcRtiTaxMonthConversion

		private List<AInternalOrderItem> HmrcRtiTaxMonthConversion(IHarvester oHarvester) {
			var oOutput = new List<AInternalOrderItem>();

			foreach (KeyValuePair<string, ISeeds> pair in ((Ezbob.HmrcHarvester.Harvester)oHarvester).Hopper.Seeds[DataType.PayeRtiTaxYears]) {
				var oData = (RtiTaxYearSeeds)pair.Value;

				oOutput.AddRange(
					oData.Months.Select(rtms => new RtiTaxMonthEntry {
						DateStart = rtms.DateStart,
						DateEnd = rtms.DateEnd,
						AmountPaid = new Coin(rtms.AmountPaid.Amount, rtms.AmountPaid.CurrencyCode),
						AmountDue = new Coin(rtms.AmountDue.Amount, rtms.AmountDue.CurrencyCode),
					})
				);
			} // for each file

			return oOutput;
		} // HmrcRtiTaxMonthConversion
 
		#endregion method HmrcVatReturnConversion

		#endregion Orders list: remote to internal conversion

		#region method CreateOrdersAggregationInfo

		private IEnumerable<IWriteDataInfo<FunctionType>> CreateOrdersAggregationInfo(
			InternalDataList orders,
			ICurrencyConvertor currencyConverter
		) {
			var oFunctionTypes = new List<FunctionType>();
			m_oVendorInfo.Aggregators.ForEach(a => oFunctionTypes.Add(a.FunctionType()));

			var updated = orders.SubmittedDate;

			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(orders, (submittedDate, o) => new InternalDataList(submittedDate, o));

			var factory = new OrdersAggregatorFactory();

			var aggData = DataAggregatorHelper.AggregateData(
				factory,
				timePeriodData,
				oFunctionTypes.ToArray(),
				updated,
				currencyConverter
			);

			return aggData;
		} // CreateOrdersAggreationInfo

		#endregion method CreateOrdersAggregationInfo

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(RetrieveDataHelper));
		private readonly VendorInfo m_oVendorInfo;

		#endregion private
	} // class RetrieveDataHelper

	#endregion class RetrieveDataHelper
} // namespace
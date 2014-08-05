namespace Integration.ChannelGrabberFrontend {
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
	using Ezbob.Backend.Models;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Integration.ChannelGrabberAPI;
	using Integration.ChannelGrabberConfig;
	using System.Diagnostics;
	using System.Text;
	using EzServiceAccessor;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using StructureMap;

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
				return Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(account.SecurityData));
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to de-serialise security data for marketplace {0} ({1})",
					account.DisplayName, account.Id), e);
			}
		} // RetrieveSecurityInfo

		#endregion method RetrieveCustomerSecurityInfo

		#endregion public

		#region protected

		#region method RetrieveAndAggregate

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			if (historyRecord == null) { // should never happen
				var os = new StringBuilder();
				var st = new StackTrace(true);

				os.Append("historyRecord is NULL when creating/updating a Channel Grabber account!");

				for (var i = 0; i < st.FrameCount; i++) {
					StackFrame oFrame = st.GetFrame(i);
					os.AppendFormat("\n\t{0}: {1} at {2}:{3}:{4}",
						i,
						oFrame.GetMethod().Name,
						oFrame.GetFileName(), 
						oFrame.GetFileLineNumber(),
						oFrame.GetFileColumnNumber()
					);
				} // for

				ms_oLog.Error("{0}", os.ToString());

				throw new ApiException("History record is not specified.");
			} // if

			AccountModel oSecInfo;

			try {
				oSecInfo = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(databaseCustomerMarketPlace.SecurityData));
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to de-serialise security data for marketplace {0} ({1})",
					databaseCustomerMarketPlace.DisplayName, databaseCustomerMarketPlace.Id), e);
			} // try

			AccountData ad = oSecInfo.Fill();
			
			var ctr = new Connector(
				ad,
				ms_oLog,
				databaseCustomerMarketPlace.Customer.Id,
				databaseCustomerMarketPlace.Customer.Name
			);

			if (ctr.Init()) {
				try {
					ctr.Run(false, databaseCustomerMarketPlace.Id);

					switch (ad.VendorInfo.Behaviour) {
					case Behaviour.Default:
						return ProcessRetrieved(
							ctr.DataHarvester,
							databaseCustomerMarketPlace,
							historyRecord
						);

					case Behaviour.HMRC:
						if (ctr.DataHarvester.ErrorsToEmail.Count > 0) {
							ObjectFactory.GetInstance<IEzServiceAccessor>().EmailHmrcParsingErrors(
								databaseCustomerMarketPlace.Customer.Id,
								databaseCustomerMarketPlace.Id,
								ctr.DataHarvester.ErrorsToEmail
							);
						} // if

						return ObjectFactory.GetInstance<IEzServiceAccessor>().SaveVatReturnData(
							databaseCustomerMarketPlace.Id,
							historyRecord.Id,
							ctr.SourceID,
							HmrcVatReturnConversion(ctr.DataHarvester),
							HmrcRtiTaxMonthConversion(ctr.DataHarvester)
						);

					default:
						throw new ApiException("Unsupported behaviour for CG flavour: " + ad.VendorInfo.Behaviour.ToString());
					} // switch
				}
				catch (ApiException) {
					ctr.Done();
					throw;
				} // try
			} // if Init succeeded

			throw new ApiException("Failed to initialise CG connector.");
		} // RetrieveAndAggregate

		#endregion method RetrieveAndAggregate

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

		private ElapsedTimeInfo ProcessRetrieved(
			IHarvester oHarvester,
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// Convert orders into internal format.
			List<Order> oRawOrders = ((Integration.ChannelGrabberAPI.Harvester)oHarvester).RetrievedOrders;

			var oChaGraOrders = new List<AInternalOrderItem>();

			if (oRawOrders != null) {
				oChaGraOrders.AddRange(oRawOrders.Select(oRaw => new ChannelGrabberOrderItem {
					CurrencyCode = oRaw.CurrencyCode,
					OrderStatus = oRaw.OrderStatus,
					NativeOrderId = oRaw.NativeOrderId,
					PaymentDate = oRaw.PaymentDate,
					PurchaseDate = oRaw.PurchaseDate,
					TotalCost = oRaw.TotalCost,
					IsExpense = oRaw.IsExpense,
				}));
			} // if

			var elapsedTimeInfo = new ElapsedTimeInfo();

			// store retrieved orders to DB.
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreChannelGrabberOrdersData(
					databaseCustomerMarketPlace,
					new InternalDataList(DateTime.UtcNow, oChaGraOrders),
					historyRecord,
					oHarvester.SourceID
			));

			// retrieve ALL the orders from DB
			InternalDataList allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetAllChannelGrabberOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace)
			);

			// calculate aggregated
			IEnumerable<IWriteDataInfo<FunctionType>> aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.AggregateData,
				() => CreateOrdersAggregationInfo(allOrders, Helper.CurrencyConverter)
			);

			// Store aggregated
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreAggregatedData,
				() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord)
			);

			return elapsedTimeInfo;
		} // ProcessRetrieved

		#endregion method ProcessRetrieved

		#region method HmrcVatReturnConversion

		private VatReturnRawData[] HmrcVatReturnConversion(IHarvester oHarvester) {
			var oVatRecords = new List<VatReturnRawData>();

			foreach (KeyValuePair<string, ISeeds> pair in ((Ezbob.HmrcHarvester.Harvester)oHarvester).Hopper.Seeds[DataType.VatReturn]) {
				var oData = (VatReturnSeeds)pair.Value;

				var oEntry = new VatReturnRawData {
					BusinessAddress = oData.BusinessAddress,
					BusinessName = oData.BusinessName,
					DateDue = oData.DateDue,
					DateFrom = oData.DateFrom,
					DateTo = oData.DateTo,
					Period = oData.Period,
					RegistrationNo = oData.RegistrationNo,
				};

				foreach (KeyValuePair<string, Ezbob.HmrcHarvester.Coin> oBoxData in oData.ReturnDetails) {
					oEntry.Data[oBoxData.Key] = new Ezbob.Backend.Models.Coin {
						Amount = oBoxData.Value.Amount,
						CurrencyCode = oBoxData.Value.CurrencyCode,
					};
				} // for each box

				oVatRecords.Add(oEntry);
			} // for each file

			return oVatRecords.ToArray();
		} // HmrcVatReturnConversion
 
		#endregion method HmrcVatReturnConversion

		#region method HmrcRtiTaxMonthConversion

		private RtiTaxMonthRawData[] HmrcRtiTaxMonthConversion(IHarvester oHarvester) {
			var oOutput = new List<RtiTaxMonthRawData>();

			foreach (KeyValuePair<string, ISeeds> pair in ((Ezbob.HmrcHarvester.Harvester)oHarvester).Hopper.Seeds[DataType.PayeRtiTaxYears]) {
				var oData = (RtiTaxYearSeeds)pair.Value;

				oOutput.AddRange(
					oData.Months.Select(rtms => new RtiTaxMonthRawData {
						DateStart = rtms.DateStart,
						DateEnd = rtms.DateEnd,
						AmountPaid = new Ezbob.Backend.Models.Coin(rtms.AmountPaid.Amount, rtms.AmountPaid.CurrencyCode),
						AmountDue = new Ezbob.Backend.Models.Coin(rtms.AmountDue.Amount, rtms.AmountDue.CurrencyCode),
					})
				);
			} // for each file

			return oOutput.ToArray();
		} // HmrcRtiTaxMonthConversion
 
		#endregion method HmrcVatReturnConversion

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

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(RetrieveDataHelper));
		private readonly VendorInfo m_oVendorInfo;

		#endregion private
	} // class RetrieveDataHelper

	#endregion class RetrieveDataHelper
} // namespace
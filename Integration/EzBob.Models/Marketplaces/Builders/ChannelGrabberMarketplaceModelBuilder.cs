namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using Web.Areas.Customer.Models;
	using Integration.ChannelGrabberConfig;
	using NHibernate;
	using NHibernate.Linq;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	class ChannelGrabberMarketplaceModelBuilder : MarketplaceModelBuilder {
		#region public

		#region constructor

		public ChannelGrabberMarketplaceModelBuilder(ISession session) : base(session) {
		} // constructor

		#endregion constructor

		#region method GetPaymentAccountModel

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
			VendorInfo vi = Configuration.Instance.GetVendorInfo(mp.Marketplace.Name);

			if (!vi.HasExpenses && (vi.Behaviour == Behaviour.Default))
				return null;

			var paymentAccountModel = new PaymentAccountsModel {
				TotalNetInPayments = 0,
				TotalNetOutPayments = 0,
				TransactionsNumber = 0,
				MonthInPayments = 0
			};

			if (!vi.HasExpenses)
				return paymentAccountModel;

			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, FunctionType.NumOfExpenses.ToString());
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, FunctionType.TotalSumOfExpenses.ToString());

			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, FunctionType.NumOfOrders.ToString());
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, FunctionType.TotalSumOfOrders.ToString());

			MP_AnalyisisFunctionValue monthSumOfInvoices = GetMonthValueFor(mp, FunctionType.TotalSumOfOrders.ToString());

			if ((earliestNumOfExpenses != null) && earliestNumOfExpenses.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfExpenses.ValueInt.Value;

			if ((earliestNumOfInvoices != null) && earliestNumOfInvoices.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfInvoices.ValueInt.Value;

			if (earliestSumOfInvoices != null && earliestSumOfInvoices.ValueFloat.HasValue)
				paymentAccountModel.TotalNetInPayments = earliestSumOfInvoices.ValueFloat.Value;

			if (earliestSumOfExpenses != null && earliestSumOfExpenses.ValueFloat.HasValue)
				paymentAccountModel.TotalNetOutPayments = earliestSumOfExpenses.ValueFloat.Value;

			if (monthSumOfInvoices != null && monthSumOfInvoices.ValueFloat.HasValue)
				paymentAccountModel.MonthInPayments = monthSumOfInvoices.ValueFloat.Value;

			return paymentAccountModel;
		} // GetPaymentAccountModel

		#endregion method GetPaymentAccountModel

		#region method InitializeSpecificData

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
			VendorInfo vi = Configuration.Instance.GetVendorInfo(mp.Marketplace.Name);

			switch (vi.Behaviour) {
			case Behaviour.Default: // nothing here
				break;

			case Behaviour.HMRC:
				List<VatReturnEntry> oVatReturn = DatabaseDataHelper
					.GetAllHmrcVatReturnData(DateTime.UtcNow, mp)
					.Distinct(new InternalOrderComparer())
					.Select(x => (VatReturnEntry)x)
					.ToList();

				List<RtiTaxMonthEntry> oRtiTaxMonths = DatabaseDataHelper
					.GetAllHmrcRtiTaxMonthData(DateTime.UtcNow, mp)
					.GroupBy(
						x => x.NativeOrderId, // key selector - split into groups having the same key
						x => (RtiTaxMonthEntry)x, // element selector - convert each element in each group
						(oIgnoredKey, lst) => { // result selector - select one element from each group
							RtiTaxMonthEntry oResult = null;

							lst.ForEach(o => {
								if ((oResult == null) || (oResult.FetchTime < o.FetchTime))
									oResult = o;
							});

							// At this point oResult should not be null because
							// at least one element with the oIgnoredKey was found...

							return oResult;
						} // end of result selector
					)
					.ToList();

				oVatReturn.Sort(VatReturnEntry.CompareForSort);
				oRtiTaxMonths.Sort(RtiTaxMonthEntry.CompareForSort);

				var oServiceClient = new ServiceClient();
				VatReturnDataActionResult vrsar = oServiceClient.Instance.LoadVatReturnSummary(mp.Customer.Id, mp.Id);

				var datesSummary = new List<VatReturnSummaryDates>();
				
				if (vrsar.Summary != null) {
					foreach (var oSummary in vrsar.Summary) {
						if (oSummary.Quarters.Any()) {
							datesSummary.Add(new VatReturnSummaryDates(
								oSummary.Quarters.Min(x => x.DateFrom),
								oSummary.Quarters.Max(x => x.DateTo)
							));
						} // if has quarters
					} // for each summary item
				} // if has summary items

				model.CGData = new ChannelGrabberHmrcData {
					VatReturn = oVatReturn,
					RtiTaxMonths = oRtiTaxMonths,
					BankStatement = new BankStatementDataModel(),
					BankStatementAnnualized = new BankStatementDataModel(),
					SalariesMultiplier = CurrentValues.Instance.HmrcSalariesMultiplier,
					VatReturnSummary = vrsar.Summary,
					VatReturnSummaryDates = datesSummary.ToArray(),
				};

				break;

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // InitializeSpecificData

		#endregion method InitializeSpecificData

		#region method GetSeniority

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			return GetDateFromList(mp, lst => lst.Min());
		} // GetSeniority

		#endregion method GetSeniority

		#region method GetLastTransaction

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			return GetDateFromList(mp, lst => lst.Max());
		} // GetSeniority

		#endregion method GetLastTransaction

		#endregion public

		#region private

		#region method GetDateFromList

		private DateTime? GetDateFromList(MP_CustomerMarketPlace mp, Func<IQueryable<DateTime>, DateTime> oExtractDate) {
			if (null == Configuration.Instance.GetVendorInfo(mp.Marketplace.Name))
				return null;

			IQueryable<DateTime> oListOfDates = _session
				.Query<MP_ChannelGrabberOrderItem>()
				.Where(oi =>
					(oi.Order.CustomerMarketPlace.Id == mp.Id) && (oi.PaymentDate != null)
				)
				.Select(oi => oi.PaymentDate);

			return oListOfDates.Any() ? oExtractDate(oListOfDates) : (DateTime?)null;
		} // GetDateFromList

		#endregion method GetDateFromList

		#endregion private
	} // class ChannelGrabberMarketplaceBuilder
} // namespace

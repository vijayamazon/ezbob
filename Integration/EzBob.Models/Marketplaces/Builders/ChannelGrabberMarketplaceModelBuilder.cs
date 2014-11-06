namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceAccessor;
	using Integration.ChannelGrabberConfig;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;

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
				VatReturnFullData vrd = ObjectFactory.GetInstance<IEzServiceAccessor>().LoadVatReturnFullData(mp.Customer.Id, mp.Id);

				var datesSummary = new List<VatReturnSummaryDates>();
				
				if (vrd.Summary != null) {
					foreach (var oSummary in vrd.Summary) {
						if (oSummary.Quarters.Any()) {
							datesSummary.Add(new VatReturnSummaryDates(
								oSummary.Quarters.Min(x => x.DateFrom),
								oSummary.Quarters.Max(x => x.DateTo)
							));
						} // if has quarters
					} // for each summary item
				} // if has summary items

				model.HmrcData = new HmrcData {
					VatReturn = vrd.VatReturnRawData,
					RtiTaxMonths = vrd.RtiTaxMonthRawData,
					BankStatement = vrd.BankStatement,
					BankStatementAnnualized = vrd.BankStatementAnnualized,
					SalariesMultiplier = CurrentValues.Instance.HmrcSalariesMultiplier,
					VatReturnSummary = vrd.Summary,
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

			IQueryable<MP_VatReturnRecord> oVatPeriods = _session
				.Query<MP_VatReturnRecord>()
				.Where(r => r.CustomerMarketPlace.Id == mp.Id && (r.IsDeleted == null || !r.IsDeleted.Value));

			IQueryable<DateTime> oAllDates = oListOfDates
				.Union(oVatPeriods.Select(r => r.DateFrom))
				.Union(oVatPeriods.Select(r => r.DateTo));

			return oAllDates.Any() ? oExtractDate(oAllDates) : (DateTime?)null;
		} // GetDateFromList

		#endregion method GetDateFromList

		#endregion private
	} // class ChannelGrabberMarketplaceBuilder
} // namespace

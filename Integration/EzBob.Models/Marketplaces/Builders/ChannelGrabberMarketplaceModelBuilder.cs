namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceAccessor;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using Integration.ChannelGrabberConfig;
	using NHibernate.Linq;
	using StructureMap;

	class ChannelGrabberMarketplaceModelBuilder : MarketplaceModelBuilder {
		public ChannelGrabberMarketplaceModelBuilder() : base(null) { } // constructor

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			return GetDateFromList(mp, WhichDateToTake.Max);
		} // GetLastTransaction

		protected override PaymentAccountsModel GetPaymentAccountModel(
			MP_CustomerMarketPlace mp,
			MarketPlaceModel model,
			DateTime? history,
			List<IAnalysisDataParameterInfo> av
		) {
			VendorInfo vi = Configuration.Instance.GetVendorInfo(mp.Marketplace.Name);

			if (!vi.HasExpenses && (vi.Behaviour == Behaviour.Default))
				return null;

			var paymentAccountModel = new HmrcPaymentAccountsModel(mp, history);
			paymentAccountModel.Load(av);

			return paymentAccountModel;
		} // GetPaymentAccountModel

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			return GetDateFromList(mp, WhichDateToTake.Min);
		}

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

		private enum WhichDateToTake {
			Min,
			Max,
		} // enum WhichDateToTake

		private static DateTime? SelectOne(DateTime? oResult, DateTime oDate, WhichDateToTake nWhich) {
			if (oResult == null)
				return oDate;

			switch (nWhich) {
			case WhichDateToTake.Min:
				return (oDate < oResult) ? oDate : oResult;

			case WhichDateToTake.Max:
				return (oDate > oResult) ? oDate : oResult;

			default:
				throw new ArgumentOutOfRangeException("nWhich");
			} // switch
		} // SelectOne

		private DateTime? GetDateFromList(MP_CustomerMarketPlace mp, WhichDateToTake nWhich) {
			if (null == Configuration.Instance.GetVendorInfo(mp.Marketplace.Name))
				return null;

			DateTime? oResult = null;

			IQueryable<DateTime> oListOfPaymentDates = _session
				.Query<MP_ChannelGrabberOrderItem>()
				.Where(oi =>
					(oi.Order.CustomerMarketPlace.Id == mp.Id) && (oi.PaymentDate != null)
				)
				.Select(oi => oi.PaymentDate);

			foreach (DateTime oDate in oListOfPaymentDates)
				oResult = SelectOne(oResult, oDate, nWhich);

			IQueryable<MP_VatReturnRecord> oVatPeriods = _session
				.Query<MP_VatReturnRecord>()
				.Where(r => r.CustomerMarketPlace.Id == mp.Id && (r.IsDeleted == null || !r.IsDeleted.Value));

			foreach (MP_VatReturnRecord oPeriod in oVatPeriods) {
				oResult = SelectOne(oResult, oPeriod.DateFrom, nWhich);
				oResult = SelectOne(oResult, oPeriod.DateTo, nWhich);
			} // for each

			return oResult;
		} // GetDateFromList
	} // class ChannelGrabberMarketplaceBuilder
} // namespace

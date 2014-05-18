namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models.Marketplaces;
	using EzBob.Models.Marketplaces.Builders;
	using StructureMap;
	using Areas.Customer.Models;
	using log4net;

	public class WebMarketPlacesFacade {
		public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(Customer customer, DateTime? history) {
			List<MP_CustomerMarketPlace> marketplaces = history.HasValue
				? customer.CustomerMarketPlaces.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= history.Value.Date).ToList()
				: customer.CustomerMarketPlaces.ToList();

			MarketPlaceModel oYodleeModel = null;
			var oHmrcList = new List<MarketPlaceModel>();

			var oHmrcID = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
			var oYodleeID = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");

			List<MarketPlaceModel> models = marketplaces.Select(mp => {
				try {
					var builder = GetBuilder(mp);

					var model = builder.Create(mp, history);

					model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model, history);

					if ((oYodleeModel == null) && (mp.Marketplace.InternalId == oYodleeID))
						oYodleeModel = model;

					if (mp.Marketplace.InternalId == oHmrcID)
						oHmrcList.Add(model);

					return model;
				}
				catch (Exception e) {
					ms_oLog.WarnFormat("Something went wrong while building marketplace model for marketplace id {0} of type {1}.", mp.Id, mp.Marketplace.Name);
					ms_oLog.Warn(e);

					return new MarketPlaceModel {
						Id = mp.Id,
						Type = mp.DisplayName,
						Name = mp.Marketplace.Name,
						PaymentAccountBasic = new PaymentAccountsModel {
							displayName = mp.DisplayName,
						},
					};
				} // try
			}).ToList();

			if (oYodleeModel != null) {
				foreach (var oModel in oHmrcList) {
					try {
						var hmrcData = (ChannelGrabberHmrcData)oModel.CGData;

						if (hmrcData != null) {
							hmrcData.BankStatement = oYodleeModel.Yodlee.BankStatementDataModel;
							hmrcData.BankStatementAnnualized = CalculateAnnualizedBankStatement(hmrcData);
						} // if
					}
					catch (Exception e) {
						ms_oLog.WarnFormat("Something went wrong while building bank statement for marketplace id {0} of type HMRC.", oModel.Id);
						ms_oLog.Warn(e);
					} // try
				} // for each oModel
			} // if HMRC or Yodlee

			return models;
		} // GetMarketPlaceModels

		public IEnumerable<MarketPlaceHistoryModel> GetMarketPlaceHistoryModel(Customer customer) {
			var modelsMpUpdates = customer
				.CustomerMarketPlaces
				.Where(mp => mp.Customer == customer)
				.SelectMany(mp => mp.UpdatingHistory)
				.Where(x => x.UpdatingEnd.HasValue)
				.ToList()
				.Select(h => h.UpdatingEnd.Value.Date)
				.Distinct()
				.Select(x => new MarketPlaceHistoryModel {
					HistoryDate = x,
					HistoryType = "MP Update"
				});

			var modelsCashRequests =
				customer.CashRequests.Where(cr => cr.CreationDate.HasValue)
						.Select(cr => cr.CreationDate.Value.Date)
						.Distinct()
						.Select(x => new MarketPlaceHistoryModel {
							HistoryDate = x,
							HistoryType = "Cash Request"
						});

			var models = new List<MarketPlaceHistoryModel>();
			models.AddRange(modelsMpUpdates);
			models.AddRange(modelsCashRequests);

			return models.OrderByDescending(m => m.HistoryDate);
		} // GetMarketPlaceHistoryModel

		private BankStatementDataModel CalculateAnnualizedBankStatement(ChannelGrabberHmrcData hmrcData) {
			var annualized = new BankStatementDataModel();
			var lastVatReturn = hmrcData.VatReturn.LastOrDefault();

			decimal box3 = 0M, box4 = 0M, box6 = 0M, box7 = 0M;

			if (lastVatReturn != null) {
				foreach (var vat in lastVatReturn.Data) {
					if (vat.Key.Contains("(Box 3)")) {
						box3 = vat.Value.Amount;
						continue;
					} // if

					if (vat.Key.Contains("(Box 4)")) {
						box4 = vat.Value.Amount;
						continue;
					} // if

					if (vat.Key.Contains("(Box 6)")) {
						box6 = vat.Value.Amount;
						continue;
					} // if

					if (vat.Key.Contains("(Box 7)")) {
						box7 = vat.Value.Amount;
						continue;
					} // if
				} // foreach
			} // if

			var vatRevenues = 1 + (box6 == 0 ? 0 : (box3 / (box6)));
			var vatOpex = 1 + (box7 == 0 ? 0 : (box4 / (box7)));

			var bankStat = hmrcData.BankStatement;

			bankStat.Revenues = vatRevenues == 0M ? bankStat.Revenues : bankStat.Revenues / (double)vatRevenues;
			bankStat.Opex = Math.Abs(vatOpex == 0M ? bankStat.Opex : bankStat.Opex / (double)vatOpex);
			bankStat.TotalValueAdded = bankStat.Revenues - bankStat.Opex;
			bankStat.PercentOfRevenues = Math.Abs(bankStat.Revenues - 0) < 0.01 ? 0 : bankStat.TotalValueAdded / bankStat.Revenues;
			bankStat.Ebida = bankStat.TotalValueAdded + (bankStat.Salaries + bankStat.Tax);
			bankStat.FreeCashFlow = bankStat.Ebida - bankStat.ActualLoansRepayment;

			if (bankStat.PeriodMonthsNum == 0)
				return annualized;

			const int year = 12;

			annualized.Revenues = (bankStat.Revenues / bankStat.PeriodMonthsNum * year);
			annualized.Opex = (bankStat.Opex / bankStat.PeriodMonthsNum * year);
			annualized.TotalValueAdded = annualized.Revenues - annualized.Opex;
			annualized.PercentOfRevenues = Math.Abs(annualized.Revenues) < 0.01 ? 0 : annualized.TotalValueAdded / annualized.Revenues;
			annualized.Salaries = (bankStat.Salaries / bankStat.PeriodMonthsNum * year);
			annualized.Tax = (bankStat.Tax / bankStat.PeriodMonthsNum * year);
			annualized.Ebida = annualized.TotalValueAdded + (annualized.Salaries + annualized.Tax);
			annualized.ActualLoansRepayment = (bankStat.ActualLoansRepayment / bankStat.PeriodMonthsNum * year);
			annualized.FreeCashFlow = annualized.Ebida - annualized.ActualLoansRepayment;

			return annualized;
		} // CalculateAnnualizedBankStatement

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilderr

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(WebMarketPlacesFacade));
	} // WebMarketPlacesFacade
} // namespace

namespace EzBob.Models.Marketplaces
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using Builders;
	using StructureMap;
	using Web.Areas.Customer.Models;

	public class MarketPlacesFacade
	{
		public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(Customer customer, DateTime? history)
		{
			var marketplaces = history.HasValue
								   ? customer.CustomerMarketPlaces.Where(mp => mp.Created.HasValue && mp.Created.Value.Date <= history.Value.Date).ToList()
								   : customer.CustomerMarketPlaces.ToList();


			var models = marketplaces.Select(mp =>
			{
				try
				{
					var builder = GetBuilder(mp);

					var model = builder.Create(mp, history);

					model.PaymentAccountBasic = builder.GetPaymentAccountModel(mp, model, history);

					return model;
				}
				catch
				{
					return new MarketPlaceModel
					{
						Id = mp.Id,
						Type = mp.DisplayName,
						Name = mp.Marketplace.Name,
						PaymentAccountBasic = new PaymentAccountsModel()
							{
								displayName = mp.DisplayName,
							}
					};
				}
			}).ToList();
			try
			{
				if (models.Any(m => m.Name == "HMRC") && models.Any(m => m.Name == "Yodlee"))
				{
					var hmrcData = ((ChannelGrabberHmrcData) models.First(m => m.Name == "HMRC").CGData);
					hmrcData.BankStatement = models.First(m => m.Name == "Yodlee").Yodlee.BankStatementDataModel;
					hmrcData.BankStatementAnnualized = CalculateAnnualizedBankStatement(hmrcData);
				}
			}catch{}

			return models;
		}

		private BankStatementDataModel CalculateAnnualizedBankStatement(ChannelGrabberHmrcData hmrcData)
		{
			var annualized = new BankStatementDataModel();
			var lastVatReturn = hmrcData.VatReturn.LastOrDefault();
			
			decimal box3 = 0M, box4 = 0M, box6 = 0M, box7 = 0M;
			if (lastVatReturn != null)
			{
				foreach (var vat in lastVatReturn.Data)
				{

					if (vat.Key.Contains("(Box 3)"))
					{
						box3 = vat.Value.Amount;
						continue;
					}

					if (vat.Key.Contains("(Box 4)"))
					{
						box4 = vat.Value.Amount;
						continue;
					}

					if (vat.Key.Contains("(Box 6)"))
					{
						box6 = vat.Value.Amount;
						continue;
					}

					if (vat.Key.Contains("(Box 7)"))
					{
						box7 = vat.Value.Amount;
						continue;
					}
				}
			}

			var vatRevenues = 1 + (box6 == 0 && box3 == 0 ? 0 : (box3 / (box6 + box3)));
			var vatOpex = 1 + (box7 == 0 && box4 == 0 ? 0 : (box4 / (box7 + box4)));
			
			var bankStat = hmrcData.BankStatement;

			bankStat.Revenues = vatRevenues == 0M ? bankStat.Revenues : bankStat.Revenues / (double)vatRevenues;
			bankStat.Opex = Math.Abs(vatOpex == 0M ? bankStat.Opex : bankStat.Opex / (double)vatOpex);
			bankStat.TotalValueAdded = bankStat.Revenues - bankStat.Opex;
			bankStat.PercentOfRevenues = Math.Abs(bankStat.Revenues - 0) < 0.01 ? 0 : bankStat.TotalValueAdded/bankStat.Revenues;
			bankStat.Ebida = bankStat.TotalValueAdded + (bankStat.Salaries + bankStat.Tax);
			bankStat.FreeCashFlow = bankStat.Ebida + bankStat.ActualLoansRepayment;

			if (bankStat.PeriodMonthsNum == 0) return annualized;
			const int year = 12;
			
			annualized.Revenues = (bankStat.Revenues / bankStat.PeriodMonthsNum * year);
			annualized.Opex = (bankStat.Opex / bankStat.PeriodMonthsNum * year);
			annualized.TotalValueAdded = annualized.Revenues - annualized.Opex;
			annualized.PercentOfRevenues = Math.Abs(annualized.Revenues) < 0.01 ? 0 : annualized.TotalValueAdded / annualized.Revenues;
			annualized.Salaries = (bankStat.Salaries / bankStat.PeriodMonthsNum * year);
			annualized.Tax = (bankStat.Tax / bankStat.PeriodMonthsNum * year);
			annualized.Ebida = annualized.TotalValueAdded + (annualized.Salaries + annualized.Tax);
			annualized.ActualLoansRepayment = (bankStat.ActualLoansRepayment / bankStat.PeriodMonthsNum * year);
			annualized.FreeCashFlow = annualized.Ebida + annualized.ActualLoansRepayment;
			return annualized;
		}

// GetMarketPlaceModels


		public DateTime MarketplacesSeniority(Customer customer, bool onlyForEluminationPassed = false, bool? isPaymentAccount = null)
		{
			var marketplaces = customer.CustomerMarketPlaces.Where(m => m.Disabled == false).ToList();

			if (onlyForEluminationPassed)
			{
				marketplaces = marketplaces.Where(m => m.EliminationPassed).ToList();
			}

			if (isPaymentAccount != null)
			{
				marketplaces = marketplaces.Where(m => m.Marketplace.IsPaymentAccount == isPaymentAccount.Value).ToList();
			}

			var dates = marketplaces.Where(mp => !mp.Marketplace.IsPaymentAccount || mp.Marketplace.Name == "Pay Pal").Select(mp =>
			{
				var builder = GetBuilder(mp);
				builder.UpdateOriginationDate(mp);
				return mp.OriginationDate;
			}).Where(d => d != null).Select(d => d.Value).ToList();

			return dates.Any() ? dates.Min() : DateTime.UtcNow;
		}

		public IEnumerable<MarketPlaceHistoryModel> GetMarketPlaceHistoryModel(Customer customer)
		{
			var modelsMpUpdates = customer
				.CustomerMarketPlaces
				.Where(mp => mp.Customer == customer)
				.SelectMany(mp => mp.UpdatingHistory)
				.Where(x => x.UpdatingEnd.HasValue)
				.ToList()
				.Select(h => h.UpdatingEnd.Value.Date)
				.Distinct()
				.Select(x => new MarketPlaceHistoryModel
					{
						HistoryDate = x,
						HistoryType = "MP Update"
					});

			var modelsCashRequests =
				customer.CashRequests.Where(cr => cr.CreationDate.HasValue)
						.Select(cr => cr.CreationDate.Value.Date)
						.Distinct()
						.Select(x => new MarketPlaceHistoryModel
							{
								HistoryDate = x,
								HistoryType = "Cash Request"
							});

			var models = new List<MarketPlaceHistoryModel>();
			models.AddRange(modelsMpUpdates);
			models.AddRange(modelsCashRequests);
			
			return models.OrderByDescending(m => m.HistoryDate);
		}

		private static IMarketplaceModelBuilder GetBuilder(MP_CustomerMarketPlace mp)
		{
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetBuilder
	} // MarketPlacesFacade
} // namespace

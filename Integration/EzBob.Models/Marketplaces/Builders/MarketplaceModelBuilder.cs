namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using NHibernate;
	using CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using StructureMap;

	public class MarketplaceModelBuilder : IMarketplaceModelBuilder {
		public MarketplaceModelBuilder(ISession session) {
			_session = session ?? ObjectFactory.GetInstance<ISession>();
		}
		
		public MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history) {
			var lastChecked = mp.UpdatingEnd.HasValue ? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value) : "never/in progress";
			var updatingStatus = mp.GetUpdatingStatus(history);
			var updatingError = mp.GetUpdatingError(history);
			var age = GetAccountAge(mp);
			var url = GetUrl(mp, mp.GetRetrieveDataHelper().RetrieveCustomerSecurityInfo(mp.Id));
			var lastTransactionDate = GetLastTransactionDate(mp);

			var model = new MarketPlaceModel {
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				LastChecked = lastChecked,
				UpdatingStatus = updatingStatus,
				UpdateError = updatingError,
				AccountAge = age,
				PositiveFeedbacks = 0,
				NegativeFeedbacks = 0,
				NeutralFeedbacks = 0,
				RaitingPercent = 0,
				SellerInfoStoreURL = url,
				IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
				UWPriority = mp.Marketplace.UWPriority,
				Disabled = mp.Disabled,
				IsNew = mp.IsNew,
				IsHistory = history.HasValue,
				History = history,
				LastTransactionDate = lastTransactionDate
			};

			var aggregations = mp.Marketplace.GetAggregations(mp, history).ToList();
			SetAggregationData(model, aggregations);

			var monthSales = aggregations.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Month && x.ParameterName == AggregationFunction.Turnover.ToString());
			model.MonthSales = monthSales == null ? 0 : (decimal)monthSales.Value;

			var yearSales = aggregations.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.Turnover.ToString());
			model.AnnualSales = yearSales == null ? 0 : (decimal)yearSales.Value;

			InitializeSpecificData(mp, model, history);
			var feedbacks = GetFeedbackData(aggregations);
			model.RaitingPercent = feedbacks.RaitingPercent;
			model.PositiveFeedbacks = feedbacks.PositiveFeedbacks;
			model.NegativeFeedbacks = feedbacks.NegativeFeedbacks;
			model.NeutralFeedbacks = feedbacks.NeutralFeedbacks;
			model.AmazonSelerRating = feedbacks.AmazonSelerRating;

			if (model.IsPaymentAccount) {
				var paymentModel = GetPaymentAccountModel(mp, history, aggregations);
				model.TotalNetInPayments = paymentModel.TotalNetInPayments;
				model.TotalNetOutPayments = paymentModel.TotalNetOutPayments;
				model.TransactionsNumber = paymentModel.TransactionsNumber;
				model.MonthInPayments = paymentModel.MonthInPayments;
			}
			return model;
		}

		public MarketPlaceDataModel CreateLightModel(MP_CustomerMarketPlace mp, DateTime? history) {
			var lastChecked = mp.UpdatingEnd.HasValue ? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value) : "never/in progress";
			var updatingStatus = mp.GetUpdatingStatus(history);
			var updatingError = mp.GetUpdatingError(history);
			var age = GetAccountAge(mp);
			var url = GetUrl(mp, mp.GetRetrieveDataHelper().RetrieveCustomerSecurityInfo(mp.Id));
			var lastTransactionDate = GetLastTransactionDate(mp);

			var model = new MarketPlaceDataModel {
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				LastChecked = lastChecked,
				UpdatingStatus = updatingStatus,
				UpdateError = updatingError,
				AccountAge = age,
				RaitingPercent = 0,
				SellerInfoStoreURL = url,
				IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
				UWPriority = mp.Marketplace.UWPriority,
				Disabled = mp.Disabled,
				IsNew = mp.IsNew,
				IsHistory = history.HasValue,
				History = history,
				LastTransactionDate = lastTransactionDate,
				OriginationDate = mp.OriginationDate
			};

			var aggregations = mp.Marketplace.GetAggregations(mp, history).ToList();

			model.TurnoverTrend = mp.MarketplaceTurnovers
				.Where(x => x.IsActive)
				.OrderByDescending(x => x.TheMonth)
				.Take(12)
				.Select(x => new TurnoverTrend {
					TheMonth = x.TheMonth,
					Turnover = x.Turnover
				}).ToList();
			
			var monthSales = aggregations.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Month && x.ParameterName == AggregationFunction.Turnover.ToString());
			model.MonthSales = monthSales == null ? 0 : (decimal)monthSales.Value;

			var yearSales = aggregations.FirstOrDefault(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year && x.ParameterName == AggregationFunction.Turnover.ToString());
			model.AnnualSales = yearSales == null ? 0 : (decimal)yearSales.Value;

			
			var feedbacks = GetFeedbackData(aggregations);
			model.RaitingPercent = feedbacks.RaitingPercent;

			if (model.IsPaymentAccount) {
				var paymentModel = GetPaymentAccountModel(mp, history, aggregations);
				model.TotalNetInPayments = paymentModel.TotalNetInPayments;
				model.TotalNetOutPayments = paymentModel.TotalNetOutPayments;
				model.MonthInPayments = paymentModel.MonthInPayments;
				model.TransactionsNumber = paymentModel.TransactionsNumber;
			}
			return model;
		}

		protected virtual MarketPlaceFeedbackModel GetFeedbackData(List<IAnalysisDataParameterInfo> aggregations) {
			return new MarketPlaceFeedbackModel();
		}

		public string GetAccountAge(MP_CustomerMarketPlace mp) {
			UpdateOriginationDate(mp);
			return mp.OriginationDate == null
					   ? "-"
					   : Convert.ToString(Math.Round((DateTime.UtcNow - mp.OriginationDate).Value.TotalDays / 30.0, 1), CultureInfo.InvariantCulture);
		}

		public virtual DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			return null;
		}

		public DateTime? GetLastTransactionDate(MP_CustomerMarketPlace mp) {
			UpdateLastTransactionDate(mp);
			return mp.LastTransactionDate;
		}

		protected virtual PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, DateTime? history, List<IAnalysisDataParameterInfo> av) {
			return null;
		}

		public virtual DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			return null;
		}

		public virtual string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo) {
			return string.Format("https://www.google.com/search?q={0}+{1}", HttpUtility.UrlEncode(mp.Marketplace.Name), mp.DisplayName);
		}

		public void UpdateLastTransactionDate(MP_CustomerMarketPlace mp) {
			bool bShouldLastTransactionDateBeUpdated =
				!mp.LastTransactionDate.HasValue ||
				(mp.UpdatingEnd.HasValue && mp.LastTransactionDate.Value < mp.UpdatingEnd.Value.AddDays(-2));

			if (!bShouldLastTransactionDateBeUpdated)
				return;

			mp.LastTransactionDate = GetLastTransaction(mp);

			ObjectFactory.GetInstance<CustomerMarketPlaceRepository>().SaveOrUpdate(mp);
		}

		public void UpdateOriginationDate(MP_CustomerMarketPlace mp) {
			mp.OriginationDate = mp.OriginationDate ?? GetSeniority(mp);
		}

		public virtual void SetAggregationData(MarketPlaceModel model, List<IAnalysisDataParameterInfo> av) {
			var data = new Dictionary<string, string>();

			if (av != null) {
				foreach (var info in av) {
					if (!string.IsNullOrEmpty(info.ParameterName)) {
						var val = info.ParameterName.Replace(" ", "")
							.Replace("%", "") + info.TimePeriod;
						string temp;
						data.TryGetValue(val, out temp);
						if (temp == null) {
							data.Add(val, info.Value.ToString());
						}
					}
				}
			}

			model.AnalysisDataInfo = data;
		}

		protected virtual void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
		}

		protected static readonly ASafeLog Log = new SafeILog(typeof(MarketplaceModelBuilder));
		protected readonly ISession _session;
	}
}
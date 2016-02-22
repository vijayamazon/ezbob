namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using NHibernate;
	using CommonLib.TimePeriodLogic;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using log4net;
	using StructureMap;

	using ThisLibrary = Ezbob.Models.Library;

	public class MarketplaceModelBuilder : IMarketplaceModelBuilder {
		public MarketplaceModelBuilder(ISession session) {
			_session = session ?? ObjectFactory.GetInstance<ISession>();
		} // constructor

		public MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history) {
			string lastChecked = "";
			string updatingStatus = "";
			string updatingError = "";
			string age = "";
			string url = "";
			DateTime? lastTransactionDate = null;
			TimeCounter tc = new TimeCounter("MarketplaceModelBuilder building time for mp " + mp.Id);

			using (tc.AddStep("lastChecked Time taken")) {
				lastChecked = mp.UpdatingEnd.HasValue
					? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value)
					: "never/in progress";
			}
			using (tc.AddStep("GetUpdatingStatus Time taken")) {
				updatingStatus = mp.GetUpdatingStatus(history);
			}
			using (tc.AddStep("GetUpdatingError Time taken")) {
				updatingError = mp.GetUpdatingError(history);
			}
			using (tc.AddStep("GetAccountAge Time taken")) {
				DateTime? originationDate;
				age = GetAccountAge(mp, out originationDate);
			}
			using (tc.AddStep("GetUrl Time taken")) {
				url = GetUrl(mp, mp.GetRetrieveDataHelper()
					.RetrieveCustomerSecurityInfo(mp.Id));
			}
			using (tc.AddStep("GetLastTransactionDate Time taken")) {
				lastTransactionDate = GetLastTransactionDate(mp);
			}

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
				LastTransactionDate = lastTransactionDate,
			};
			List<IAnalysisDataParameterInfo> aggregations = new List<IAnalysisDataParameterInfo>();

			using (tc.AddStep("SetAggregationData Time taken")) {
				aggregations = mp.Marketplace.GetAggregations(mp, history).ToList();
				SetAggregationData(model, aggregations);
			}

			using (tc.AddStep("monthSales Time taken")) {
				var monthSales = aggregations.FirstOrDefault(x =>
					x.TimePeriod.TimePeriodType == TimePeriodEnum.Month &&
						x.ParameterName == AggregationFunction.Turnover.ToString()
					);
				model.MonthSales = monthSales == null ? 0 : (decimal)monthSales.Value;
			}
			using (tc.AddStep("yearSales Time taken")) {
				var yearSales = aggregations.FirstOrDefault(x =>
					x.TimePeriod.TimePeriodType == TimePeriodEnum.Year &&
						x.ParameterName == AggregationFunction.Turnover.ToString()
					);
				model.AnnualSales = yearSales == null ? 0 : (decimal)yearSales.Value;
			}

			using (tc.AddStep("InitializeSpecificData Time taken")) {
				InitializeSpecificData(mp, model, history);
			}
			using (tc.AddStep("GetFeedbackData Time taken")) {
				var feedbacks = GetFeedbackData(aggregations);
				model.RaitingPercent = feedbacks.RaitingPercent;
				model.PositiveFeedbacks = feedbacks.PositiveFeedbacks;
				model.NegativeFeedbacks = feedbacks.NegativeFeedbacks;
				model.NeutralFeedbacks = feedbacks.NeutralFeedbacks;
				model.AmazonSelerRating = feedbacks.AmazonSelerRating;
			}

			using (tc.AddStep("GetPaymentAccountModel Time taken")) {
				if (model.IsPaymentAccount) {
					var paymentModel = GetPaymentAccountModel(mp, history, aggregations);
					model.TotalNetInPayments = paymentModel.TotalNetInPayments;
					model.TotalNetOutPayments = paymentModel.TotalNetOutPayments;
					model.TransactionsNumber = paymentModel.TransactionsNumber;
					model.MonthInPayments = paymentModel.MonthInPayments;
				} // if
			}

			Log.Info(tc.ToString());
			return model;
		} // Create

		public MarketPlaceDataModel CreateLightModel(MP_CustomerMarketPlace mp, DateTime? history) {
			var lastChecked = mp.UpdatingEnd.HasValue
				? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value)
				: "never/in progress";
			var updatingStatus = mp.GetUpdatingStatus(history);
			var updatingError = mp.GetUpdatingError(history);
			DateTime? originationDate;
			var age = GetAccountAge(mp, out originationDate);
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
				OriginationDate = originationDate
			};

			var aggregations = mp.Marketplace.GetAggregations(mp, history).ToList();

			model.TurnoverTrend = 
				ThisLibrary.Instance.DB.Fill<TurnoverTrend>(
					"LoadActiveMarketplaceTurnovers",
					CommandSpecies.StoredProcedure,
					new QueryParameter("MpID", mp.Id)
				)
				.OrderByDescending(x => x.TheMonth)
				.Take(12)
				.ToList();

			var monthSales = aggregations.FirstOrDefault(x =>
				x.TimePeriod.TimePeriodType == TimePeriodEnum.Month &&
				x.ParameterName == AggregationFunction.Turnover.ToString()
			);
			model.MonthSales = monthSales == null ? 0 : (decimal)monthSales.Value;

			var yearSales = aggregations.FirstOrDefault(x =>
				x.TimePeriod.TimePeriodType == TimePeriodEnum.Year &&
				x.ParameterName == AggregationFunction.Turnover.ToString()
			);
			model.AnnualSales = yearSales == null ? 0 : (decimal)yearSales.Value;

			var feedbacks = GetFeedbackData(aggregations);
			model.RaitingPercent = feedbacks.RaitingPercent;

			if (model.IsPaymentAccount) {
				var paymentModel = GetPaymentAccountModel(mp, history, aggregations);
				model.TotalNetInPayments = paymentModel.TotalNetInPayments;
				model.TotalNetOutPayments = paymentModel.TotalNetOutPayments;
				model.MonthInPayments = paymentModel.MonthInPayments;
				model.TransactionsNumber = paymentModel.TransactionsNumber;
			} // if

			return model;
		} // CreateLightModel

		protected virtual MarketPlaceFeedbackModel GetFeedbackData(List<IAnalysisDataParameterInfo> aggregations) {
			return new MarketPlaceFeedbackModel();
		} // GetFeedbackData

		public string GetAccountAge(MP_CustomerMarketPlace mp, out DateTime? originationDate) {
			originationDate = UpdateOriginationDate(mp);

			return
				originationDate == null
					? "-"
					: Convert.ToString(
						Math.Round((DateTime.UtcNow - originationDate).Value.TotalDays / 30.0, 1),
						CultureInfo.InvariantCulture
					);
		} // GetAccountAge

		public virtual DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			return null;
		} // GetLastTransaction

		public DateTime? GetLastTransactionDate(MP_CustomerMarketPlace mp) {
			DateTime? lastTransactionDate = UpdateLastTransactionDate(mp);
			return lastTransactionDate;
		} // GetLastTransactionDate

		protected virtual PaymentAccountsModel GetPaymentAccountModel(
			MP_CustomerMarketPlace mp,
			DateTime? history,
			List<IAnalysisDataParameterInfo> av
		) {
			return null;
		} // GetPaymentAccountModel

		public virtual DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			return null;
		} // GetSeniority

		public virtual string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo) {
			return string.Format(
				"https://www.google.com/search?q={0}+{1}",
				HttpUtility.UrlEncode(mp.Marketplace.Name),
				mp.DisplayName
			);
		} // GetUrl

		public virtual DateTime? UpdateLastTransactionDate(MP_CustomerMarketPlace mp) {
			bool bShouldLastTransactionDateBeUpdated =
				!mp.LastTransactionDate.HasValue ||
				(mp.UpdatingEnd.HasValue && mp.LastTransactionDate.Value < mp.UpdatingEnd.Value.AddDays(-2));

			if (!bShouldLastTransactionDateBeUpdated)
				return mp.LastTransactionDate;

			DateTime? lastTransactionDate = GetLastTransaction(mp);
			try {
				if (lastTransactionDate.HasValue) {
					ThisLibrary.Instance.DB.ExecuteNonQuery(
						"UpdateMarketPlaceLastTransactionDate",
						CommandSpecies.StoredProcedure,
						new QueryParameter("MpID", mp.Id),
						new QueryParameter("LastTransactionDate", lastTransactionDate)
					);
				} // if
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to update LastTransactionDate for mp {0}", mp.Id);
			} // try

			return lastTransactionDate;
		} // UpdateLastTransactionDate

		public DateTime? UpdateOriginationDate(MP_CustomerMarketPlace mp) {
			if (mp.OriginationDate.HasValue)
				return mp.OriginationDate;

			DateTime? seniority = GetSeniority(mp);

			try {
				if (seniority.HasValue) {
					ThisLibrary.Instance.DB.ExecuteNonQuery(
						"UpdateMarketPlaceOriginationDate",
						CommandSpecies.StoredProcedure,
						new QueryParameter("MpID", mp.Id),
						new QueryParameter("OriginationDate", seniority)
					);
				} // if
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to update LastTransactionDate for mp {0}", mp.Id);
			} // try

			return seniority;
		} // UpdateOriginationDate

		public virtual void SetAggregationData(MarketPlaceModel model, List<IAnalysisDataParameterInfo> av) {
			var data = new Dictionary<string, string>();

			if (av != null) {
				foreach (var info in av) {
					if (info != null && !string.IsNullOrEmpty(info.ParameterName)) {
						var val = info.ParameterName.Replace(" ", "").Replace("%", "") + info.TimePeriod;
						string temp;
						data.TryGetValue(val, out temp);
						if (temp == null) {
							data.Add(val, info.Value.ToString());
						}
					}
				}
			}

			model.AnalysisDataInfo = data;
		} // SetAggregationData

		protected virtual void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
		} // InitializeSpecificData

		protected static readonly ASafeLog Log = new SafeILog(typeof(MarketplaceModelBuilder));
		protected readonly ISession _session;
	} // class MarketplaceModelBuilder
} // namespace

namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Web;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Logger;
	using NHibernate;
	using CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using StructureMap;

	public class MarketplaceModelBuilder : IMarketplaceModelBuilder {
		public MarketplaceModelBuilder(ISession session) {
			_session = session ?? ObjectFactory.GetInstance<ISession>();
		}
		/*
		public static IAnalysisDataParameterInfo GetClosestToYear(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault) {
			int closestTime = 0;
			IAnalysisDataParameterInfo closestSoFar = null;
			foreach (var x in firstOrDefault) {
				switch (x.TimePeriod.TimePeriodType) {
				case TimePeriodEnum.Year:
					return x;
				case TimePeriodEnum.Month6:
					closestSoFar = x;
					closestTime = 6;
					break;
				case TimePeriodEnum.Month3:
					if (closestTime < 6) {
						closestSoFar = x;
						closestTime = 3;
					}
					break;
				case TimePeriodEnum.Month:
					if (closestTime < 3) {
						closestSoFar = x;
						closestTime = 1;
					}
					break;
				}
			}
			return closestSoFar;
		}

		public static IAnalysisDataParameterInfo GetMonth(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault) {
			foreach (var x in firstOrDefault) {
				switch (x.TimePeriod.TimePeriodType) {
				case TimePeriodEnum.Month:
					return x;
				}
			}
			return null;
		}
		*/


		public MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history) {
			var model = new MarketPlaceModel {
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				LastChecked = mp.UpdatingEnd.HasValue ? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value) : "never/in progress",
				UpdatingStatus = mp.GetUpdatingStatus(history),
				UpdateError = mp.GetUpdatingError(history),
				AccountAge = GetAccountAge(mp),
				PositiveFeedbacks = 0,
				NegativeFeedbacks = 0,
				NeutralFeedbacks = 0,
				RaitingPercent = "-",
				SellerInfoStoreURL = GetUrl(mp, mp.GetRetrieveDataHelper().RetrieveCustomerSecurityInfo(mp.Id)),
				IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
				UWPriority = mp.Marketplace.UWPriority,
				Disabled = mp.Disabled,
				IsNew = mp.IsNew,
				IsHistory = history.HasValue,
				History = history.HasValue ? history.Value : (DateTime?)null,
				LastTransactionDate = GetLastTransactionDate(mp)
			};

			InitializeSpecificData(mp, model, history);
			SetAggregationData(model, mp, history);
			return model;
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

		public virtual PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
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

		public virtual void SetAggregationData(MarketPlaceModel model, MP_CustomerMarketPlace mp, DateTime? history) {
			var data = new Dictionary<string, string> { // TODO: fill with real aggregation data for each mp
				{"some key", "0"},
				{"some other key", "1"},
			};
			model.AnalysisDataInfo = data;
		}

		protected virtual void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
		}

		protected static readonly ASafeLog Log = new SafeILog(typeof(MarketplaceModelBuilder));
		protected readonly ISession _session;
	}
}
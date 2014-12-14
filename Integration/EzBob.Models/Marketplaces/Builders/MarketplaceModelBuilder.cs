namespace EzBob.Models.Marketplaces.Builders
{
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
	using StructureMap;

	public class MarketplaceModelBuilder : IMarketplaceModelBuilder
	{
		protected readonly ISession _session;
		protected static readonly ASafeLog Log = new SafeILog(typeof(MarketplaceModelBuilder));

		public MarketplaceModelBuilder(ISession session)
		{
			_session = session ?? ObjectFactory.GetInstance<ISession>();
		}

		public virtual PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			return null;
		}

		public virtual string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
		{
			return string.Format("https://www.google.com/search?q={0}+{1}", HttpUtility.UrlEncode(mp.Marketplace.Name), mp.DisplayName);
		}

		public MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history)
		{
			var data = GetAnalysisFunctionValuesModel(GetAnalysisFunctionValues(mp, history));

			var model = new MarketPlaceModel
			{
				Id = mp.Id,
				Type = mp.DisplayName,
				Name = mp.Marketplace.Name,
				LastChecked = mp.UpdatingEnd.HasValue ? FormattingUtils.FormatDateToString(mp.UpdatingEnd.Value) : "never/in progress",
				UpdatingStatus = mp.GetUpdatingStatus(history),
				UpdateError = mp.GetUpdatingError(history),
				AnalysisDataInfo = data,
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

			return model;
		}

		public DateTime? GetLastTransactionDate(MP_CustomerMarketPlace mp)
		{
			UpdateLastTransactionDate(mp);
			return mp.LastTransactionDate;
		}

		public string GetAccountAge(MP_CustomerMarketPlace mp)
		{
			UpdateOriginationDate(mp);
			return mp.OriginationDate == null
					   ? "-"
					   : Convert.ToString(Math.Round((DateTime.UtcNow - mp.OriginationDate).Value.TotalDays / 30.0, 1), CultureInfo.InvariantCulture);
		}

		public void UpdateOriginationDate(MP_CustomerMarketPlace mp)
		{
			mp.OriginationDate = mp.OriginationDate ?? GetSeniority(mp);
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

		public virtual DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		public virtual DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		protected static List<IAnalysisDataParameterInfo> GetAnalysisFunctionValues(MP_CustomerMarketPlace mp, DateTime? history)
		{
			var analisysFunction = mp.GetRetrieveDataHelper().GetAnalysisValuesByCustomerMarketPlace(mp.Id);

			List<IAnalysisDataParameterInfo> av = null;
			try {
				av = history.HasValue
						 ? analisysFunction.Data.FirstOrDefault(
							 x => x.Key == analisysFunction.Data.Where(pair => pair.Key.Date <= history.Value.Date).DefaultIfEmpty().Max(pair => pair.Key))
										   .Value
						 : analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;
			}
			catch (Exception e) {
				Log.Debug(e, "Something went wrong while executing GetAnalysisFunctionValues(mp.Id = {0}, history = {1}), ignoring the exception.", mp.Id, history == null ? "-- null --" : history.Value.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture));
			}
			return av;
		}

		private static Dictionary<string, string> GetAnalysisFunctionValuesModel(IEnumerable<IAnalysisDataParameterInfo> av)
		{
			var data = new Dictionary<string, string>();
			if (av != null)
			{
				foreach (var info in av)
				{
					if (!string.IsNullOrEmpty(info.ParameterName))
					{
						var val = info.ParameterName.Replace(" ", "").Replace("%", "") + info.TimePeriod;
						string temp;
						data.TryGetValue(val, out temp);
						if (temp == null)
						{
							data.Add(val, info.Value.ToString());
						}
					}
				}
			}

			return data;
		}

		protected virtual void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
		}

		protected MP_AnalyisisFunctionValue GetEarliestValueFor(MP_CustomerMarketPlace mp, string functionName)
		{
			var functionsMatchingName = mp.AnalysysFunctionValues.Where(v => v.AnalyisisFunction.Name == functionName).OrderByDescending(v => v.Updated);
			MP_AnalyisisFunctionValue latest = null;
			long max = 0;
			DateTime? latestTime = null;
			foreach (var mpAnalyisisFunctionValue in functionsMatchingName)
			{
				if (latestTime == null)
				{
					latestTime = mpAnalyisisFunctionValue.Updated;
				}
				else if (latestTime != mpAnalyisisFunctionValue.Updated)
				{
					return latest;
				}

				if (mpAnalyisisFunctionValue.AnalysisFunctionTimePeriod.Id > max && mpAnalyisisFunctionValue.AnalysisFunctionTimePeriod.Id < 9)
				{
					max = mpAnalyisisFunctionValue.AnalysisFunctionTimePeriod.Id;
					latest = mpAnalyisisFunctionValue;
				}
			}
			return latest;
		}

		public MP_AnalyisisFunctionValue GetMonthValueFor(MP_CustomerMarketPlace mp, string functionName)
		{
			var functionsMatchingName = mp.AnalysysFunctionValues.Where(v => v.AnalyisisFunction.Name == functionName).OrderByDescending(v => v.Updated);
			MP_AnalyisisFunctionValue month = null;

			foreach (var mpAnalyisisFunctionValue in functionsMatchingName)
			{
				if (mpAnalyisisFunctionValue.AnalysisFunctionTimePeriod.Name == "30")
				{
					month = mpAnalyisisFunctionValue;
				}
			}
			return month;
		}

		public static IAnalysisDataParameterInfo GetMonth(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault)
		{
			foreach (var x in firstOrDefault)
			{
				switch (x.TimePeriod.TimePeriodType)
				{
					case TimePeriodEnum.Month:
						return x;
				}
			}
			return null;
		}

		public static IAnalysisDataParameterInfo GetClosestToYear(IEnumerable<IAnalysisDataParameterInfo> firstOrDefault)
		{
			int closestTime = 0;
			IAnalysisDataParameterInfo closestSoFar = null;
			foreach (var x in firstOrDefault)
			{
				switch (x.TimePeriod.TimePeriodType)
				{
					case TimePeriodEnum.Year:
						return x;
					case TimePeriodEnum.Month6:
						closestSoFar = x;
						closestTime = 6;
						break;
					case TimePeriodEnum.Month3:
						if (closestTime < 6)
						{
							closestSoFar = x;
							closestTime = 3;
						}
						break;
					case TimePeriodEnum.Month:
						if (closestTime < 3)
						{
							closestSoFar = x;
							closestTime = 1;
						}
						break;
				}
			}
			return closestSoFar;
		}
	}
}
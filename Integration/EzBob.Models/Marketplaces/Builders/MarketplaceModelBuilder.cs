using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using NHibernate;

namespace EzBob.Models
{
    public class MarketplaceModelBuilder : IMarketplaceModelBuilder
    {
        protected readonly ISession _session;

        public MarketplaceModelBuilder(ISession session)
        {
            _session = session;
        }

        public virtual PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            return null;
        }

        public virtual string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
        {
            return string.Format("https://www.google.com/search?q={0}+{1}", HttpUtility.UrlEncode(mp.Marketplace.Name), mp.DisplayName);
        }

        public MarketPlaceModel Create(MP_CustomerMarketPlace mp)
        {
            var data = GetAnalysisFunctionValues(mp);

            var eluminatingStatus = mp.EliminationPassed ? "Pass" : "Fail";

            var model = new MarketPlaceModel
            {
                Id = mp.Id,
                Type = mp.DisplayName,
                Name = mp.Marketplace.Name,
                LastChecked = FormattingUtils.FormatDateToString(mp.Updated, "-"),
                EluminatingStatus = eluminatingStatus,
                UpdatingStatus = mp.GetUpdatingStatus(),
                UpdateError = mp.UpdateError,
                AnalysisDataInfo = data,
                AccountAge = GetAccountAge(mp),
                PositiveFeedbacks = 0,
                NegativeFeedbacks = 0,
                NeutralFeedbacks = 0,
                RaitingPercent = "-",
                SellerInfoStoreURL = GetUrl(mp, RetrieveDataHelper.RetrieveCustomerSecurityInfo(mp.Id)),
                IsPaymentAccount = mp.Marketplace.IsPaymentAccount,
                UWPriority = mp.Marketplace.UWPriority,
                Disabled = mp.Disabled
            };

            InitializeSpecificData(mp, model);

            return model;
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

        public virtual DateTime? GetSeniority(MP_CustomerMarketPlace mp)
        {
            return null;
        }

        private static Dictionary<string, string> GetAnalysisFunctionValues(MP_CustomerMarketPlace mp)
        {
            var data = new Dictionary<string, string>();

            var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
            var av = analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

            if (av != null)
            {
                foreach (var info in av)
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

            return data;
        }

        protected virtual void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
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
    }
}
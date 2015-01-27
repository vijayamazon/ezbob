namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using AmazonServiceLib;
	using EzBob.CommonLib.TimePeriodLogic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces;
	using NHibernate;
	using NHibernate.Linq;
	using StructureMap;
	using EZBob.DatabaseLib.Model.Marketplaces.Amazon;

	class AmazonMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        private readonly EbayAmazonCategoryRepository _ebayAmazonCategoryRepository;

        public AmazonMarketplaceModelBuilder(EbayAmazonCategoryRepository ebayAmazonCategoryRepository, ISession session): base(session)
        {
            _ebayAmazonCategoryRepository = ebayAmazonCategoryRepository;
        }

        public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
        {
            return string.Format("http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller={0}", ((AmazonSecurityInfo)securityInfo).MerchantId);
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
			var askville = ObjectFactory.GetInstance<AskvilleRepository>();
            var askvilleTmp = askville.GetAskvilleByMarketplace(mp);

            model.AskvilleStatus =
                askvilleTmp != null
                    ? askvilleTmp.Status.ToString()
                    : AskvilleStatus.NotPerformed.ToString();

            model.AskvilleGuid = askvilleTmp != null ? askvilleTmp.Guid : "";

            model.Categories = _ebayAmazonCategoryRepository.GetAmazonCategories(mp);
        }

		protected virtual new  void InitializeFeedbackData(MarketPlaceModel model, List<IAnalysisDataParameterInfo> aggregations) {
			var sellerRating = aggregations.FirstOrDefault(x => x.ParameterName == AggregationFunction.UserRating.ToString());
			model.AmazonSelerRating = sellerRating == null ? 0 : (double)sellerRating.Value;

			var raitingPercent = aggregations.FirstOrDefault(x => x.ParameterName == AggregationFunction.PositiveFeedbackPercentRate.ToString() && x.TimePeriod.TimePeriodType == TimePeriodEnum.Year);
			model.RaitingPercent = raitingPercent == null ? null : (decimal?)raitingPercent.Value;

			var yearAggregations = aggregations.Where(x => x.TimePeriod.TimePeriodType == TimePeriodEnum.Year).ToList();
			if (!yearAggregations.Any()) {
				return;
			}

			var positive = yearAggregations.FirstOrDefault(x => x.ParameterName == AggregationFunction.PositiveFeedbackRate.ToString());
			var negative = yearAggregations.FirstOrDefault(x => x.ParameterName == AggregationFunction.NegativeFeedbackRate.ToString());
			var neutral = yearAggregations.FirstOrDefault(x => x.ParameterName == AggregationFunction.NegativeFeedbackRate.ToString());

			model.PositiveFeedbacks = positive == null ? null : (int?)positive.Value;
			model.NegativeFeedbacks = negative == null ? null : (int?)negative.Value;
			model.NeutralFeedbacks = neutral == null ? null : (int?)neutral.Value;
		}

        public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
        {
            var s = _session.Query<MP_AmazonOrderItem>()
                                           .Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
                                           .Where(oi => oi.PurchaseDate != null)
                                           .Select(oi => oi.PurchaseDate);
            return !s.Any() ? null : s.Min();
        }

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_AmazonOrderItem>().Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id).Where(oi => oi.PurchaseDate != null);

			if (s.Count() != 0)
			{
				return s.Max(oi => oi.PurchaseDate);
			}

			return null;
		}
    }
}

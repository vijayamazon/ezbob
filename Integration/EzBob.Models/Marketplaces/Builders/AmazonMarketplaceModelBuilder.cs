using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.AmazonServiceLib;
using EzBob.Web.Areas.Underwriter.Models;
using StructureMap;

namespace EzBob.Models
{
    class AmazonMarketplaceModelBuilder : MarketplaceModelBuilder
    {
        private readonly EbayAmazonCategoryRepository _ebayAmazonCategoryRepository;

        public AmazonMarketplaceModelBuilder(EbayAmazonCategoryRepository ebayAmazonCategoryRepository, CustomerMarketPlaceRepository customerMarketplaces) : base(customerMarketplaces)
        {
            _ebayAmazonCategoryRepository = ebayAmazonCategoryRepository;
        }

        public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
        {
            return string.Format("http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller={0}", ((AmazonSecurityInfo)securityInfo).MerchantId);
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            var amazonFeedback = mp.AmazonFeedback.LastOrDefault();
            var amazonSellerRating = amazonFeedback != null ? amazonFeedback.UserRaining : 0;



            var feedbackByPeriodAmazon = amazonFeedback != null
                                             ? amazonFeedback.FeedbackByPeriodItems.FirstOrDefault(
                                                 x => x.TimePeriod.Id == 4)
                                             : null;


            var negative = feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Negative : null;
            var positive = feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Positive : null;
            var neutral = feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Neutral : null;

            var raiting = (positive + negative + neutral) != 0
                              ? (positive * 100) / (positive + negative + neutral)
                              : 0;

            model.PositiveFeedbacks = positive ?? 0;
            model.NegativeFeedbacks = negative ?? 0;
            model.NeutralFeedbacks = neutral ?? 0;

            model.RaitingPercent = raiting != null ? raiting.ToString() : "-";

            model.AmazonSelerRating = amazonSellerRating;

            var askville = ObjectFactory.GetInstance<AskvilleRepository>();
            var askvilleTmp = askville.GetAskvilleByMarketplace(mp);

            model.AskvilleStatus =
                askvilleTmp != null
                    ? askvilleTmp.Status.ToString()
                    : AskvilleStatus.NotPerformed.ToString();

            model.AskvilleGuid = askvilleTmp != null ? askvilleTmp.Guid : "";

            model.Categories = _ebayAmazonCategoryRepository.GetEbayCategories(mp);
        }
    }
}
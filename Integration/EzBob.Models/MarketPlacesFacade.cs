using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.AmazonServiceLib;
using EzBob.PayPal;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter
{
    public class MarketPlacesFacade
    {
        private readonly EbayAmazonCategoryRepository _ebayAmazonCategoryRepository;
        private readonly CustomerMarketPlaceRepository _customerMarketplaces;

        public MarketPlacesFacade(EbayAmazonCategoryRepository ebayAmazonCategoryRepository,
                                  CustomerMarketPlaceRepository customerMarketplaces)
        {
            _ebayAmazonCategoryRepository = ebayAmazonCategoryRepository;
            _customerMarketplaces = customerMarketplaces;
        }

        public IEnumerable<MarketPlaceModel> GetMarketPlaceModels(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var paypal = new PayPalDatabaseMarketPlace();
            var askville = ObjectFactory.GetInstance<AskvilleRepository>();

            var marketplaces = customer.CustomerMarketPlaces
                                       .Where(mp => mp.Marketplace.InternalId != paypal.InternalId).ToList();


            var models = marketplaces.Select(
                mp =>
                    {
                        var isAmazon = mp.Marketplace.Name == "Amazon";
                        var data = new Dictionary<string, string>();

                        var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
                        var av =
                            analisysFunction.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

                        var amazonFeedback = mp.AmazonFeedback.LastOrDefault();
                        var amazonSellerRating = amazonFeedback != null ? amazonFeedback.UserRaining : 0;

                        var ebayUserData = mp.EbayUserData.LastOrDefault();
                        var ebayFeedBack = mp.EbayFeedback.LastOrDefault();
                        var ebayAccount = mp.EbayUserAccountData.LastOrDefault();

                        var accountAge = _customerMarketplaces.Seniority(mp.Id);

                        var feedbackByPeriodAmazon = amazonFeedback != null
                                                         ? amazonFeedback.FeedbackByPeriodItems.FirstOrDefault(
                                                             x => x.TimePeriod.Id == 4)
                                                         : null;
                        var feedbackByPeriodEbay = ebayFeedBack != null
                                                       ? ebayFeedBack.FeedbackByPeriodItems.FirstOrDefault(
                                                           x => x.TimePeriod.Id == 4)
                                                       : null;

                        var negative = isAmazon
                                           ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Negative : null)
                                           : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Negative : null);
                        var positive = isAmazon
                                           ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Positive : null)
                                           : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Positive : null);
                        var neutral = isAmazon
                                          ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Neutral : null)
                                          : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Neutral : null);

                        var raiting = (positive + negative + neutral) != 0
                                          ? (positive*100)/(positive + negative + neutral)
                                          : 0;

                        var categories = _ebayAmazonCategoryRepository.CategoryForMarketplace(mp);
                        var categorieValues = categories.Select(x => x.Name);

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
                        var sellerInfoStoreUrl = SellerInfoStoreUrl(mp, isAmazon, ebayUserData);

                        var askvilleTmp = askville.GetAskvilleByMarketplace(mp);

                        var eluminatingStatus = mp.EliminationPassed ? "Pass" : "Fail";
                        var updatingStatus =
                            (mp.UpdatingStart != null && mp.UpdatingEnd == null)
                                ? "In progress"
                                : (!String.IsNullOrEmpty(mp.UpdateError))
                                      ? "Error"
                                      : "Done";

                        return new MarketPlaceModel
                            {
                                Id = mp.Id,
                                Type = mp.DisplayName,
                                Name = mp.Marketplace.Name,
                                LastChecked = FormattingUtils.FormatDateToString(mp.Updated, "-"),
                                EluminatingStatus = eluminatingStatus,
                                UpdatingStatus = updatingStatus,
                                UpdateError = mp.UpdateError,
                                AnalysisDataInfo = data,
                                AmazonSelerRating = amazonSellerRating,
                                AskvilleStatus = askvilleTmp != null
                                                ? askvilleTmp.Status.ToString()
                                                : AskvilleStatus.NotPerformed.ToString(),
                                AskvilleGuid = askvilleTmp != null ? askvilleTmp.Guid : "",
                                AccountAge = accountAge != null
                                                ? Convert.ToString((DateTime.Now - accountAge).Value.TotalDays/30)
                                                : "-",
                                RaitingPercent = raiting != null ? raiting.ToString() : "-",
                                PositiveFeedbacks = positive ?? 0,
                                NegativeFeedbacks = negative ?? 0,
                                NeutralFeedbacks = neutral ?? 0,
                                SellerInfoStoreURL = sellerInfoStoreUrl,
                                Categories = categorieValues,
                                EBay = BuildEBay(ebayUserData, ebayAccount, ebayFeedBack)
                            };
                    });
            return models;
        }

        private static EBayModel BuildEBay(MP_EbayUserData ebayUserData, MP_EbayUserAccountData ebayAccount, MP_EbayFeedback ebayFeedBack)
        {
            var ebayFeedBackScore = ebayUserData != null ? ebayUserData.FeedbackScore : 0;
            var ebayAdditionalAccount = ebayAccount != null
                                            ? ebayAccount.EbayUserAdditionalAccountData.LastOrDefault()
                                            : null;

            var model = new EBayModel();

            model.EBayFeedBackScore = ebayFeedBackScore.ToString();

            BuildEbayUserData(ebayUserData, model);
            BuildEbayAccountData(ebayAccount, model);
            BuildEbayAdditionalAccountData(model, ebayAdditionalAccount);
            BuildEbayFeedbackData(ebayFeedBack, model);

            return model;
        }

        private static void BuildEbayFeedbackData(MP_EbayFeedback ebayFeedBack, EBayModel model)
        {
            if (ebayFeedBack == null) return;

            model.NegativeFeedbackCount         = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Negative));
            model.NeutralFeedbackCount          = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Neutral));
            model.PositiveFeedbackCount         = Convert.ToString(ebayFeedBack.FeedbackByPeriodItems.Sum(x => x.Positive));
            model.UniqueNegativeFeedbackCount   = Convert.ToString(ebayFeedBack.UniqueNegativeCount);
            model.UniqueNeutralFeedbackCount    = Convert.ToString(ebayFeedBack.UniqueNeutralCount);
            model.UniqueBuyerCount              = Convert.ToString(ebayFeedBack.UniqueBuyerCount);
            model.UniquePositiveFeedbackCount   = Convert.ToString(ebayFeedBack.UniquePositiveCount);
            model.TransactionPercent            = Convert.ToString(ebayFeedBack.TransactionPercent);
            model.RepeatBuyerCount              = Convert.ToString(ebayFeedBack.RepeatBuyerCount);
            model.RepeatBuyerPercent            = Convert.ToString(ebayFeedBack.RepeatBuyerPercent);
        }

        private static void BuildEbayAdditionalAccountData(EBayModel model, MP_EbayUserAdditionalAccountData ebayAdditionalAccount)
        {
            if (ebayAdditionalAccount == null) return;

            model.AdditionalAccountAccountCode  = ebayAdditionalAccount.AccountCode;
            model.AdditionalAccountBalance      = ebayAdditionalAccount.Balance.ToString();
            model.AdditionalAccountCurrency     = ebayAdditionalAccount.Currency;
        }

        private static void BuildEbayAccountData(MP_EbayUserAccountData ebayAccount, EBayModel model)
        {
            if (ebayAccount == null) return;

            model.AccountId             = ebayAccount.AccountId;
            model.AccountState          = ebayAccount.AccountState;
            model.AmountPastDueAmount   = ebayAccount.AmountPastDueValue != null 
                                            ? ebayAccount.AmountPastDueValue.Value.ToString(CultureInfo.InvariantCulture) 
                                            : "-";
            model.BankAccountInfo       = ebayAccount.BankAccountInfo ?? "-";
            model.BankModifyDate        = FormattingUtils.FormatDateTimeToString(ebayAccount.BankModifyDate, "-");
            model.CreditCardExpiration  = FormattingUtils.FormatDateTimeToString(ebayAccount.CreditCardExpiration, "-");
            model.CreditCardInfo        = ebayAccount.CreditCardInfo;
            model.CreditCardModifyDate  = FormattingUtils.FormatDateTimeToString(ebayAccount.CreditCardModifyDate, "-");
            model.CurrentBalance        = ebayAccount.CurrentBalance.ToString();
            model.PastDue               = ((bool?) ebayAccount.PastDue).ToString();
            model.PaymentMethod         = ebayAccount.PaymentMethod;
            model.Currency              = ebayAccount.Currency;
        }

        private static void BuildEbayUserData(MP_EbayUserData ebayUserData, EBayModel model)
        {
            if (ebayUserData == null) return;

            model.EBayGoodStanding      = ebayUserData.eBayGoodStanding.ToString();
            model.EIASToken             = ebayUserData.EIASToken;
            model.FeedbackPrivate       = ebayUserData.FeedbackPrivate.ToString();
            model.IDChanged             = ebayUserData.IDChanged.ToString();
            model.IDLastChanged         = FormattingUtils.FormatDateTimeToString(ebayUserData.IDLastChanged, "-");
            model.IdVerified            = ebayUserData.IDVerified.ToString();
            model.NewUser               = ebayUserData.NewUser.ToString();
            model.PayPalAccountStatus   = ebayUserData.PayPalAccountStatus;
            model.PayPalAccountType     = ebayUserData.PayPalAccountType;
            model.QualifiesForSelling   = ebayUserData.QualifiesForSelling.ToString();

            BuildSellerInfo(ebayUserData, model);
        }

        private static void BuildSellerInfo(MP_EbayUserData ebayUserData, EBayModel model)
        {
            if (ebayUserData == null || ebayUserData.SellerInfo == null) return;

            model.SellerInfoStoreOwner      = ebayUserData.SellerInfo.SellerInfoStoreOwner.ToString();
            model.SellerInfoStoreSite       = ebayUserData.SellerInfo.SellerInfoStoreSite;
            model.SellerInfoTopRatedProgram = ebayUserData.SellerInfo.SellerInfoTopRatedProgram ?? "-";
            model.SellerInfoTopRatedSeller  = ebayUserData.SellerInfo.SellerInfoTopRatedSeller.ToString();
        }

        private static string SellerInfoStoreUrl(MP_CustomerMarketPlace mp, bool isAmazon, MP_EbayUserData ebayUserData)
        {
            string sellerInfoStoreUrl;

	        var oEsi = new EKM.EkmServiceInfo();
	        var oVsi = new Integration.Volusion.VolusionServiceInfo();
	        var oPsi = new Integration.Play.PlayServiceInfo();

            if (mp.Marketplace.InternalId == oEsi.InternalId)
                sellerInfoStoreUrl = "https://www.google.com/search?q=ekm+" + mp.DisplayName;
			else if (mp.Marketplace.InternalId == oVsi.InternalId)
                sellerInfoStoreUrl = "https://www.google.com/search?q=volusion+" + mp.DisplayName;
			else if (mp.Marketplace.InternalId == oPsi.InternalId)
                sellerInfoStoreUrl = "https://www.google.com/search?q=play+" + mp.DisplayName;
            else if (mp.Marketplace.Name == "PayPoint")
            {
                sellerInfoStoreUrl = "https://www.google.com/search?q=payPoint+" + mp.DisplayName;
            }
            else if (!isAmazon)
            {
                sellerInfoStoreUrl = ebayUserData != null &&
                                     ebayUserData.SellerInfo != null &&
                                     !string.IsNullOrEmpty(ebayUserData.SellerInfo.SellerInfoStoreURL)
                                         ? ebayUserData.SellerInfo.SellerInfoStoreURL
                                         : "https://www.google.com/search?q=ebay+" + mp.DisplayName;
            }
            else
            {
                sellerInfoStoreUrl = "http://www.amazon.co.uk/gp/aag/main?ie=UTF8&seller=" +
                                     (((AmazonSecurityInfo)
                                       RetrieveDataHelper.RetrieveCustomerSecurityInfo(mp.Id)).MerchantId);
                //sellerInfoStoreUrl = string.Format("http://google.com/search?pws=0&q={0}", mp.DisplayName);
            }
            return sellerInfoStoreUrl;
        }
    }
}
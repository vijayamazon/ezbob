using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.EBay {
    using System.Collections.Concurrent;
    using eBay.Service.Call;
    using eBay.Service.Core.Soap;
    using EzBob3dPartiesApi.EBay;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobModels.EBay;
    using NServiceBus;

    /// <summary>
    /// Handles GetUserData3dPartyCommand
    /// </summary>
    public class EBayUserDataHandler : HandlerBase<EbayGetUserData3dPartyCommandResponse>, IHandleMessages<EbayGetUserData3dPartyCommand> {

        [Injected]
        public EBayService EBayService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(EbayGetUserData3dPartyCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            string token = command.Token;
            if (string.IsNullOrEmpty(token)) {
                token = await EBayService.FetchToken(command.SessionId);
            }
            var userTask = EBayService.GetUserData(token);
            var accountTask = EBayService.GetAccount(token);
            var feedbackTask = EBayService.GetUserFeedback(token);
            var getOrdersCallsTask = EBayService.GetOrders(token, command.GetOrdersTimeFrom, DateTime.Now);

            await Task.WhenAll(userTask, accountTask, feedbackTask, getOrdersCallsTask);

            SendReply(info, command, resp => {
                resp.Token = token;

                resp.EbayUserRegistrationAddressData = GetUserAddress(userTask.Result.RegistrationAddress);
                resp.EbayUserSellerPaymentAddressData = GetUserAddress(userTask.Result.SellerInfo.SellerPaymentAddress);
                resp.EbayUserData = GetUserData(userTask.Result);

                resp.EbayUserAccountData = GetUserAccountData(accountTask.Result);
                resp.AdditionalUserAccounts = GetAdditionalUserAccountData(accountTask.Result);

                resp.EbayFeedback = GetFeedback(feedbackTask.Result);
                resp.EbayRatings = GetRatings(feedbackTask.Result);
                resp.EbayFeedbackItems = GetFeedbackItems(feedbackTask.Result);

                resp.EbayOrders = GetOrders(getOrdersCallsTask.Result);
                resp.Payload = command.Payload;
            });
        }

        /// <summary>
        /// Gets the orders.
        /// </summary>
        /// <param name="orders">The orders.</param>
        private IEnumerable<EbayOrderInfo> GetOrders(IEnumerable<GetOrdersCall> orders)
        {
            return orders.AsParallel()
                .SelectMany(o => o.ApiResponse.OrderArray.Cast<OrderType>())
                .Distinct(new EqComparer<OrderType>((o1, o2) => o1.OrderID.Equals(o2.OrderID), od => od.OrderID.GetHashCode()))
                .Select(GetEbayOrderInfo).ToArray();
        }

        /// <summary>
        /// Gets the ebay order information.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        private EbayOrderInfo GetEbayOrderInfo(OrderType order) {
            return new EbayOrderInfo {
                ShippingAddress = GetUserAddress(order.ShippingAddress),
                OrderItem = GetOrderItem(order),
                Transactions = GetTransactions(order),
                ExternalTransactions = GetExternalTransactions(order)
            };
        }

        /// <summary>
        /// Gets the order item.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        private EBayOrderItem GetOrderItem(OrderType order) {
            return new EBayOrderItem {
                CreatedTime = order.CreatedTimeSpecified ? order.CreatedTime.ToUniversalTime() : (DateTime?)null,
                ShippedTime = order.ShippedTimeSpecified ? order.ShippedTime.ToUniversalTime() : (DateTime?)null,
                PaymentTime = order.PaidTimeSpecified ? order.PaidTime.ToUniversalTime() : (DateTime?)null,
                BuyerName = order.BuyerUserID,
                AdjustmentAmount = order.AdjustmentAmount.Value,
                AdjustmentCurrency = order.AdjustmentAmount.currencyID.ToString(),
                AmountPaidAmount = order.AmountPaid.Value,
                AmountPaidCurrency = order.AmountPaid.currencyID.ToString(),
                SubTotalAmount = order.Subtotal.Value,
                SubTotalCurrency = order.Subtotal.currencyID.ToString(),
                TotalAmount = order.Total.Value,
                TotalCurrency = order.Total.currencyID.ToString(),
                OrderStatus = order.OrderStatusSpecified ? order.OrderStatus.ToString() : OrderStatusCodeType.Default.ToString(),
                PaymentHoldStatus = order.OrderStatusSpecified ? order.OrderStatus.ToString() : String.Empty,
                CheckoutStatus = order.CheckoutStatus != null && order.CheckoutStatus.StatusSpecified ? order.CheckoutStatus.Status.ToString() : string.Empty,
                PaymentMethod = order.CheckoutStatus != null && order.CheckoutStatus.PaymentMethodSpecified ? order.CheckoutStatus.PaymentMethod.ToString() : string.Empty,
                PaymentStatus = order.CheckoutStatus != null && order.CheckoutStatus.eBayPaymentStatusSpecified ? order.CheckoutStatus.eBayPaymentStatus.ToString() : string.Empty,
                PaymentMethodsList = CollectionUtils.IsNotEmpty(order.PaymentMethods) ? string.Join(",", order.PaymentMethods) : null,
            };
        }

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        private ICollection<EbayTransaction> GetTransactions(OrderType order) {
            if (CollectionUtils.IsEmpty(order.TransactionArray)) {
                return CollectionUtils.GetEmptyList<EbayTransaction>();
            }

            return order.TransactionArray.AsParallel()
                .Cast<TransactionType>()
                .Select(GetSingleTransaction)
                .ToList();
        }

        /// <summary>
        /// Gets the single transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        private EbayTransaction GetSingleTransaction(TransactionType transaction) {
            return new EbayTransaction {
                CreatedDate = transaction.CreatedDate,
                QuantityPurchased = transaction.QuantityPurchased,
                PaymentHoldStatus = transaction.Status != null && transaction.Status.PaymentHoldStatusSpecified ? transaction.Status.PaymentHoldStatus.ToString() : string.Empty,
                PaymentMethodUsed = transaction.Status != null && transaction.Status.PaymentMethodUsedSpecified ? transaction.Status.PaymentMethodUsed.ToString() : string.Empty,
                Price = transaction.TransactionPrice.Value,
                PriceCurrency = transaction.TransactionPrice.currencyID.ToString(),
                ItemSKU = transaction.Item.SKU,
                ItemId = transaction.Item.ItemID,
                ItemPrivateNotes = transaction.Item.PrivateNotes,
                ItemSellerInventoryID = transaction.Item.SellerInventoryID,
                eBayTransactionId = transaction.TransactionID
            };
        }

        /// <summary>
        /// Gets the external transactions.
        /// </summary>
        /// <param name="order">The order.</param>
        private ICollection<EbayExternalTransaction> GetExternalTransactions(OrderType order) {

            if (CollectionUtils.IsEmpty(order.ExternalTransaction)) {
                return CollectionUtils.GetEmptyList<EbayExternalTransaction>();
            }

            return order.ExternalTransaction.AsParallel()
                .Cast<ExternalTransactionType>()
                .Select(GetSingleExternalTransaction)
                .ToList();
        }

        /// <summary>
        /// Gets the single external transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        private EbayExternalTransaction GetSingleExternalTransaction(ExternalTransactionType transaction) {
            return new EbayExternalTransaction {
                TransactionID = transaction.ExternalTransactionID,
                TransactionTime = transaction.ExternalTransactionTimeSpecified ? transaction.ExternalTransactionTime : (DateTime?)null,
                FeeOrCreditPrice = transaction.FeeOrCreditAmount.Value,
                FeeOrCreditCurrency = transaction.FeeOrCreditAmount.currencyID.ToString(),
                PaymentOrRefundAPrice = transaction.PaymentOrRefundAmount.Value,
                PaymentOrRefundACurrency = transaction.PaymentOrRefundAmount.currencyID.ToString()
            };
        }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private EbayUserData GetUserData(UserType user) {
            var userData = new EbayUserData {
                BillingEmail = user.BillingEmail,
                EIASToken = user.EIASToken,
                EMail = user.Email,
                eBayGoodStanding = user.eBayGoodStandingSpecified ? user.eBayGoodStanding : (bool?)null,
                UserID = user.UserID,
                FeedbackPrivate = user.FeedbackPrivateSpecified ? user.FeedbackPrivate : (bool?)null,
                FeedbackRatingStar = user.FeedbackRatingStarSpecified ? user.FeedbackRatingStar.ToString() : null,
                FeedbackScore = user.FeedbackScoreSpecified ? user.FeedbackScore : (int?)null,
                IDChanged = user.UserIDChangedSpecified ? user.UserIDChanged : (bool?)null,
                IDLastChanged = user.UserIDLastChangedSpecified ? user.UserIDLastChanged : (DateTime?)null,
                IdVerified = user.IDVerifiedSpecified ? user.IDVerified : (bool?)null,
                RegistrationDate = user.RegistrationDateSpecified ? user.RegistrationDate.ToUniversalTime() : (DateTime?)null,

                NewUser = user.NewUserSpecified ? user.NewUser : (bool?)null,
                PayPalAccountStatus = user.PayPalAccountStatusSpecified ? user.PayPalAccountStatus.ToString() : null,
                PayPalAccountType = user.PayPalAccountTypeSpecified ? user.PayPalAccountType.ToString() : null,
                QualifiesForSelling = user.QualifiesForSellingSpecified ? user.QualifiesForSelling : (bool?)null,
                Site = user.SiteSpecified ? user.Site.ToString() : null,
                SkypeID = user.SkypeID != null ? string.Join(", ", user.SkypeID.Cast<string>()) : null,
            };

            SellerType sellerInfo = user.SellerInfo;
            if (sellerInfo != null) {
                userData.SellerInfoQualifiesForB2BVAT = sellerInfo.QualifiesForB2BVAT;
                userData.SellerInfoSellerBusinessType = sellerInfo.SellerBusinessTypeSpecified ? sellerInfo.SellerBusinessType.ToString() : null;
                userData.SellerInfoStoreOwner = sellerInfo.StoreOwner;
                userData.SellerInfoStoreSite = sellerInfo.StoreSiteSpecified ? sellerInfo.StoreSite.ToString() : null;
                userData.SellerInfoStoreURL = sellerInfo.StoreURL;
                userData.SellerInfoTopRatedSeller = sellerInfo.TopRatedSellerSpecified ? sellerInfo.TopRatedSeller : (bool?)null;

                var details = sellerInfo.TopRatedSellerDetails;
                if (details != null && CollectionUtils.IsNotEmpty(details.TopRatedProgram)) {
                    userData.SellerInfoTopRatedProgram = string.Join(", ", details.TopRatedProgram.Cast<string>());
                }
            }

            return userData;
        }

        /// <summary>
        /// Gets the user address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns></returns>
        private EbayUserAddressData GetUserAddress(AddressType address) {
            return new EbayUserAddressData {
                AddressId = address.AddressID,
                AddressOwner = address.AddressOwner.ToString(),
                AddressRecordType = address.AddressRecordType.ToString(),
                AddressStatus = address.AddressStatus.ToString(),
                AddressUsage = address.AddressUsage.ToString(),
                CityName = address.CityName,
                CompanyName = address.CompanyName,
                CountryCode = address.Country.ToString(),
                CountryName = address.CountryName,
                County = address.County,
                ExternalAddressID = address.ExternalAddressID,
                FirstName = address.FirstName,
                InternationalName = address.InternationalName,
                InternationalStateAndCity = address.InternationalStateAndCity,
                InternationalStreet = address.InternationalStreet,
                LastName = address.LastName,
                Name = address.Name,
                Phone = address.Phone,
                PhoneAreaOrCityCode = address.PhoneAreaOrCityCode,
                PhoneCountryCode = address.PhoneCountryCode.ToString(),
                PhoneCountryCodePrefix = address.PhoneCountryPrefix,
                PhoneLocalNumber = address.PhoneLocalNumber,
                Phone2 = address.Phone2,
                Phone2AreaOrCityCode = address.Phone2AreaOrCityCode,
                Phone2CountryCode = address.Phone2CountryCode.ToString(),
                Phone2CountryPrefix = address.Phone2CountryPrefix,
                Phone2LocalNumber = address.Phone2LocalNumber,
                PostalCode = address.PostalCode,
                StateOrProvince = address.StateOrProvince,
                Street = address.Street,
                Street1 = address.Street1,
                Street2 = address.Street2
            };
        }

        /// <summary>
        /// Gets the user account data.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        private EbayUserAccountData GetUserAccountData(GetAccountCall account) {
            var s = account.AccountSummary;
            return new EbayUserAccountData {
                Created = account.ApiResponse.Timestamp,
                AccountId = account.AccountID,
                AccountState = s.AccountState.ToString(),
                AmountPastDueCurrency = s.AmountPastDue.ToString(),
                BankAccountInfo = s.BankAccountInfo,
                BankModifyDate = s.BankModifyDate.ToUniversalTime(),
                CreditCardExpiration = s.CreditCardExpiration.ToUniversalTime(),
                CreditCardInfo = s.CreditCardInfo,
                CreditCardModifyDate = s.CreditCardModifyDate,
                Currency = account.Currency.ToString(),
                CurrentBalance = s.CurrentBalance.Value,
                PastDue = s.PastDue,
                PaymentMethod = s.PaymentMethod.ToString()
            };
        }

        /// <summary>
        /// Gets the additional user account data.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns></returns>
        private EbayAdditionalUserAccountData[] GetAdditionalUserAccountData(GetAccountCall account) {
            if (account.AccountSummary == null) {
                Log.Warn("got empty additional account summary");
                return null;
            }
            if (CollectionUtils.IsEmpty(account.AccountSummary.AdditionalAccount)) {
                Log.Warn("got empty additional account");
                return null;
            }

            EbayAdditionalUserAccountData[] res = new EbayAdditionalUserAccountData[account.AccountSummary.AdditionalAccount.Count];
            for (int i = 0; i < res.Length; ++i) {
                AdditionalAccountType acc = account.AccountSummary.AdditionalAccount[i];

                res[i] = new EbayAdditionalUserAccountData {
                    AccountCode = acc.AccountCode,
                    Balance = acc.Balance.Value,
                    Currency = acc.Currency.ToString()
                };
            }

            return res;
        }

        /// <summary>
        /// Gets the ratings.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        /// <returns></returns>
        private ICollection<EbayRatingData> GetRatings(GetFeedbackCall feedback) {

            List<EbayRatingData> ratings = new List<EbayRatingData>();

            if (feedback.FeedbackSummary == null || CollectionUtils.IsEmpty(feedback.FeedbackSummary.SellerRatingSummaryArray)) {
                return ratings;
            }

            //group the relevant items by time period
            IEnumerable<IGrouping<FeedbackSummaryPeriodCodeType, AverageRatingSummaryType>> groupsByPeriod = feedback
                .FeedbackSummary
                .SellerRatingSummaryArray
                .Cast<AverageRatingSummaryType>()
                .Where(o => o.FeedbackSummaryPeriodSpecified && CollectionUtils.IsNotEmpty(o.AverageRatingDetails))
                .GroupBy(o => o.FeedbackSummaryPeriod);

            //group each previous group by rating detail
            foreach (var periodGroup in groupsByPeriod) {
                //group each previous group by rating detail
                var groupPeriodByRating = periodGroup.First()
                    .AverageRatingDetails
                    .Cast<AverageRatingDetailsType>()
                    .GroupBy(o => o.RatingDetail);

                EbayRatingData ratingData = new EbayRatingData();
                ratingData.TimePeriodId = ToEbayTimePeriod(periodGroup.Key);

                //completes ratingData
                FillEbayRatingData(groupPeriodByRating, ratingData);

                ratings.Add(ratingData);
            }

            return ratings;
        }

        /// <summary>
        /// Gets the feedback.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        /// <returns></returns>
        private EbayFeedbackData GetFeedback(GetFeedbackCall feedback) {
            if (feedback.FeedbackSummary == null) {
                Log.Warn("got empty feedback summary");
                return null;
            }

            FeedbackSummaryType summary = feedback.FeedbackSummary;

            EbayFeedbackData data = new EbayFeedbackData {
                Created = feedback.ApiResponse.Timestamp,
                UniqueNeutralCount = summary.UniqueNeutralFeedbackCount,
                UniquePositiveCount = summary.UniquePositiveFeedbackCount,
                UniqueNegativeCount = summary.UniqueNegativeFeedbackCount,
            };

            if (summary.SellerRoleMetrics != null) {
                data.RepeatBuyerCount = summary.SellerRoleMetrics.RepeatBuyerCount;
                data.RepeatBuyerPercent = summary.SellerRoleMetrics.RepeatBuyerPercent;
                data.TransactionPercent = summary.SellerRoleMetrics.TransactionPercent;
                data.UniqueBuyerCount = summary.SellerRoleMetrics.UniqueBuyerCount;
            }

            return data;
        }

        /// <summary>
        /// Gets the feedback items.
        /// </summary>
        /// <param name="feedback">The feedback.</param>
        private ICollection<EbayFeedbackItem> GetFeedbackItems(GetFeedbackCall feedback) {

            Dictionary<int, EbayFeedbackItem> daysToFeedBackItem = new Dictionary<int, EbayFeedbackItem>();

            var negativeGroupedByPeriod = feedback.FeedbackSummary.NegativeFeedbackPeriodArray
                .Cast<FeedbackPeriodType>()
                .GroupBy(o => o.PeriodInDays)
                .OrderBy(g => g.Key);

            var positiveGroupedByPeriod = feedback.FeedbackSummary.PositiveFeedbackPeriodArray
                .Cast<FeedbackPeriodType>()
                .GroupBy(o => o.PeriodInDays)
                .OrderBy(g => g.Key);

            var neutralGroupedByPeriod = feedback.FeedbackSummary.NeutralFeedbackPeriodArray
                .Cast<FeedbackPeriodType>()
                .GroupBy(o => o.PeriodInDays)
                .OrderBy(g => g.Key);

            negativeGroupedByPeriod
                .Concat(positiveGroupedByPeriod)
                .Concat(neutralGroupedByPeriod)
                .Aggregate(daysToFeedBackItem, (dict, group) => {
                    if (!dict.ContainsKey(group.Key)) {
                        var feedbackItem = new EbayFeedbackItem();
                        if (group.Key == 0) {
                            feedbackItem.TimePeriodId = EbayTimePeriod.Zero;
                        } else if (group.Key == 30) {
                            feedbackItem.TimePeriodId = EbayTimePeriod.Month;
                        } else if (group.Key == 180) {
                            feedbackItem.TimePeriodId = EbayTimePeriod.Month6;
                        } else if (group.Key == 365) {
                            feedbackItem.TimePeriodId = EbayTimePeriod.Year;
                        }

                        dict.Add(group.Key, feedbackItem);
                    }
                    return dict;
                });

            foreach (var group in negativeGroupedByPeriod) {
                var ebayFeedbackItem = daysToFeedBackItem[group.Key];
                ebayFeedbackItem.Negative = group.First()
                    .Count;
            }

            foreach (var group in positiveGroupedByPeriod) {
                var ebayFeedbackItem = daysToFeedBackItem[group.Key];
                ebayFeedbackItem.Positive = group.First()
                    .Count;
            }

            foreach (var group in neutralGroupedByPeriod) {
                var ebayFeedbackItem = daysToFeedBackItem[group.Key];
                ebayFeedbackItem.Neutral = group.First()
                    .Count;
            }

            return daysToFeedBackItem.Values;
        }

        /// <summary>
        /// Fills the e-bay rating data.
        /// </summary>
        /// <param name="groupPeriodByRating">The group period by rating.</param>
        /// <param name="ratingData">The rating data.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private static void FillEbayRatingData(IEnumerable<IGrouping<FeedbackRatingDetailCodeType, AverageRatingDetailsType>> groupPeriodByRating, EbayRatingData ratingData) {
            foreach (var group in groupPeriodByRating) {
                switch (group.Key) {
                case FeedbackRatingDetailCodeType.ItemAsDescribed:
                    ratingData.ItemAsDescribed = group.First()
                        .Rating;
                    ratingData.ItemAsDescribedCount = group.First()
                        .RatingCount;
                    break;
                case FeedbackRatingDetailCodeType.Communication:
                    ratingData.Communication = group.First()
                        .Rating;
                    ratingData.Communication = group.First()
                        .RatingCount;
                    break;
                case FeedbackRatingDetailCodeType.ShippingTime:
                    ratingData.ShippingTime = group.First()
                        .Rating;
                    ratingData.ShippingTimeCount = group.First()
                        .RatingCount;
                    break;
                case FeedbackRatingDetailCodeType.ShippingAndHandlingCharges:
                    ratingData.ShippingAndHandlingCharges = group.First()
                        .Rating;
                    ratingData.ShippingAndHandlingCharges = group.First()
                        .RatingCount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// To the e-bay time period.
        /// </summary>
        /// <param name="periodCode">The period code.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">periodCode;null</exception>
        private EbayTimePeriod ToEbayTimePeriod(FeedbackSummaryPeriodCodeType periodCode) {
            switch (periodCode) {
            case FeedbackSummaryPeriodCodeType.ThirtyDays:
                return EbayTimePeriod.Month;
            case FeedbackSummaryPeriodCodeType.FiftyTwoWeeks:
                return EbayTimePeriod.Year;
            default:
                throw new ArgumentOutOfRangeException("periodCode", periodCode, null);
            }
        }
    }
}

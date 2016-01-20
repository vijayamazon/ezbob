namespace EzBobService.ThirdParties.PayPal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobApi.Commands.PayPal;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels.MarketPlace;
    using EzBobModels.PayPal;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.ThirdParty.PayPal;
    using EzBobService.Currency;
    using EzBobService.ThirdParties.PayPal.SendRecieve;
    using NServiceBus;

    public class PayPalRegisterCustomerCommandHandler : HandlerBase<PayPalRegisterCustomerCommandResponse>, IHandleMessages<PayPalRegisterCustomerCommand> {

        private static readonly Guid PayPalInternalId = Guid.Parse("3FA5E327-FCFD-483B-BA5A-DC1815747A28");

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyServiceConfig { get; set; }

        [Injected]
        public PayPalGetAccessTokenSendRecieve GetAccessTokenSendRecieve { get; set; }

        [Injected]
        public PayPalGetCustomerPersonalDataSendRecieve GetCustomerPersonalDataSendRecieve { get; set; }

        [Injected]
        public PayPalGetTransactionsSendRecieve GetTransactionsSendRecieve { get; set; }

        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        [Injected]
        public IPayPalQueries PayPalQueries { get; set; }

        [Injected]
        public ICurrencyConverter CurrencyConverter { get; set; }

        [Injected]
        public PayPalConfig Config { get; set; }

        /// <summary>
        /// Handles the specified command. (Sent from REST endpoint)
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(PayPalRegisterCustomerCommand command) {
            var getAccessTokenCommand = new PayPalGetAccessToken3dPartyCommand {
                RequestToken = command.RequestToken,
                VerificationCode = command.VerificationToken,
            };

            //get access token
            var accessTokenResponse = await GetAccessTokenSendRecieve.SendAsync(ThirdPartyServiceConfig.Address, getAccessTokenCommand);

            var getPersonalDataCommand = new PayPalGetCustomerPersonalData3dPartyCommand {
                TokenSecret = accessTokenResponse.TokenSecret,
                Token = accessTokenResponse.Token
            };

            //get account info
            var personalDataResponse = await GetCustomerPersonalDataSendRecieve.SendAsync(ThirdPartyServiceConfig.Address, getPersonalDataCommand);

            InfoAccumulator info = new InfoAccumulator();

            //validates market place
            if (!MarketPlaceQueries.IsMarketPlaceInWhiteList(PayPalInternalId, personalDataResponse.UserPersonalInfo.EMail)) {
                if (MarketPlaceQueries.IsMarketPlaceExists(PayPalInternalId, personalDataResponse.UserPersonalInfo.EMail)) {
                    string msg = string.Format("the market place with already exists");
                    info.AddError(msg);
                    SendReply(info, command);
                    return;
                }
            }

            int marketPlaceId = MarketPlaceQueries.GetMarketPlaceIdFromTypeId(PayPalInternalId)
                .GetValue();

            CustomerMarketPlace marketPlace = new CustomerMarketPlace {
                CustomerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId)),
                DisplayName = personalDataResponse.UserPersonalInfo.EMail,
                MarketPlaceId = marketPlaceId,
                SecurityData = SerializationUtils.SerializeToBinaryXml(new PayPalSecurityInfo {
                    TokenSecret = accessTokenResponse.TokenSecret,
                    AccessToken = accessTokenResponse.Token,
                    VerificationCode = command.RequestToken,
                    RequestToken = command.RequestToken,
                    UserId = personalDataResponse.UserPersonalInfo.EMail
                })
            };

            int marketPlaceTableId = (int)MarketPlaceQueries.UpsertMarketPlace(marketPlace, PayPalInternalId);
            personalDataResponse.UserPersonalInfo.CustomerMarketPlaceId = marketPlaceId;

            var updateHistory = new CustomerMarketPlaceUpdateHistory {
                CustomerMarketPlaceId = marketPlaceTableId,
                UpdatingStart = DateTime.UtcNow
            };

            int marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);

            PayPalQueries.SavePersonalInfo(personalDataResponse.UserPersonalInfo);

            DateTime startDate;
            var lastTransactionTime = PayPalQueries.GetLastTransactionDate(marketPlaceTableId);
            if (!lastTransactionTime.HasValue) {
                startDate = DateTime.UtcNow.AddMonths(-Config.TransactionSearchMonthsBack);
            } else {
                startDate = (DateTime)lastTransactionTime;
            }

            var getTransactionsCommand = new PayPalGetTransations3dPartyCommand {
                AccessToken = accessTokenResponse.Token,
                AccessTokenSecret = accessTokenResponse.TokenSecret,
                UtcDateFrom = startDate,
                UtcDateTo = DateTime.UtcNow
            };

            var response = await GetTransactionsSendRecieve.SendAsync(ThirdPartyServiceConfig.Address, getTransactionsCommand);
            SaveTransactions(response.Transactions, marketPlaceTableId, marketPlaceHistoryId);

            updateHistory = new CustomerMarketPlaceUpdateHistory
            {
                CustomerMarketPlaceId = marketPlaceTableId,
                UpdatingEnd = DateTime.UtcNow
            };

            marketPlaceHistoryId = (int)MarketPlaceQueries.UpsertMarketPlaceUpdatingHistory(updateHistory);
            if (marketPlaceTableId < 1) {
                throw new InvalidOperationException("could not save market place history update");
            }
        }

        /// <summary>
        /// Saves the transactions.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <param name="marketPlaceId">The market place identifier.</param>
        /// <param name="historyId">The history identifier.</param>
        /// <exception cref="System.InvalidOperationException">could not save transactions</exception>
        private void SaveTransactions(IList<PayPal3dPartyTransactionItem> transactions, int marketPlaceId, int historyId) {
            if (transactions.Any()) {
                return;
            }

            PayPalTransaction transaction = new PayPalTransaction {
                Created = DateTime.UtcNow,
                CustomerMarketPlaceId = marketPlaceId,
                CustomerMarketPlaceUpdatingHistoryRecordId = historyId
            };

            int transactionId = (int)PayPalQueries.SaveTransaction(transaction);
            if (transactionId < 1) {
                throw new InvalidOperationException("could not save paypal transactionItem");
            }

            var res = PayPalQueries.SaveTransactionItems(transactions.Select(t => CreateSingleTransaction(t, transactionId)));
            if (!res.HasValue || !res.Value) {
                throw new InvalidOperationException("could not save transactionItem items");
            }
        }

        /// <summary>
        /// Creates the single transactionItem.
        /// </summary>
        /// <param name="transactionItem">The transactionItem.</param>
        /// <param name="transactionId">The transactionItem identifier.</param>
        /// <returns></returns>
        private PayPalTransactionItem CreateSingleTransaction(PayPal3dPartyTransactionItem transactionItem, int transactionId) {
            var feeAmount = CurrencyConverter.ConvertToGBP(transactionItem.FeeAmount, transactionItem.Created ?? default(DateTime));
            var grossAmount = CurrencyConverter.ConvertToGBP(transactionItem.GrossAmount, transactionItem.Created ?? default(DateTime));
            var netAmount = CurrencyConverter.ConvertToGBP(transactionItem.NetAmount, transactionItem.Created ?? default(DateTime));
            return new PayPalTransactionItem {
                Created = transactionItem.Created,
                PayPalTransactionId = transactionItem.PayPalTransactionId,
                Status = transactionItem.Status,
                TimeZone = transactionItem.TimeZone,
                FeeAmount = feeAmount.Amount,
                GrossAmount = grossAmount.Amount,
                NetAmount = netAmount.Amount,
                CurrencyId = 164, //GBP //TODO: some normal way
                TransactionId = transactionId,
                Type = transactionItem.Type
            };
        }
    }
}

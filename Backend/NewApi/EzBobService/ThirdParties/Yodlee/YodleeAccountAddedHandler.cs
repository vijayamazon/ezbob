namespace EzBobService.ThirdParties.Yodlee {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobApi.Commands.Yodlee;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using EzBobCommon.NSB;
    using EzBobModels.Yodlee;
    using EzBobPersistence.ThirdParty.Yodlee;
    using EzBobService.Currency;
    using NServiceBus;

    public class YodleeAccountAddedHandler : HandlerBase<YodleeUserAddedAccountCommandResponse>, IHandleMessages<YodleeUserAddedAccountCommand>, IHandleMessages<YodleeGetUserAccountsCommandResponse>, IHandleMessages<YodleeGetTransactionsCommandResponse> {

        [Injected]
        public IYodleeQueries YodleeQueries { get; set; }

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        [Injected]
        public YodleeConfig Config { get; set; }

        [Injected]
        public ICurrencyConverter CurrencyConverter { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public void Handle(YodleeUserAddedAccountCommand cmd) {

            InfoAccumulator info = new InfoAccumulator();

            var userAccount = YodleeQueries.GetUserAccount(cmd.CustomerId);

            if (userAccount == null) {
                string err = "could not get useraccount from db for id: " + cmd.CustomerId;
                info.AddError(err);
                Log.Error(err);
                RegisterError(info, cmd);
                //go to retry
                throw new InvalidDataException(err);
            }

            YodleeGetUserAccountsCommand userAccountsCommand = new YodleeGetUserAccountsCommand {
                UserName = userAccount.Username,
                UserPassword = userAccount.Password,
                CobrandUserName = Config.CoBrandUserName,
                CobrandPassword = Config.CoBrandPassword,
                CustomerId = cmd.CustomerId
            };

            SendCommand(ThirdPartyService.Address, userAccountsCommand, cmd);
        }

        /// <summary>
        /// Handles the specified response.
        /// </summary>
        /// <param name="response">The response.</param>
        public void Handle(YodleeGetUserAccountsCommandResponse response) {
            if (response.IsFailed) {
                ReplyToOrigin(response);
                return;
            }

            var existingContentServicesIds = YodleeQueries.GetUserContentServicesIds(response.CustomerId)
                .ToLookup(o => o);

            List<YodleeContentServiceAccount> newAccounts = response.Accounts.Where(o => !existingContentServicesIds.Contains(o.ContentServiceId))
                .ToList();
            if (newAccounts.Count > 0) {
                InfoAccumulator info = new InfoAccumulator();
                info.AddInfo("customer " + response.CustomerId + "added " + newAccounts.Count + " accounts");
                SaveAccounts(newAccounts, response.UserName, response.UserPassword);
                SendTransactionsRequestCommand(newAccounts, response.UserName, response.UserPassword);
                SendReply(info, response);
            } else {
                InfoAccumulator info = new InfoAccumulator();
                info.AddError("could not find added account for customer: " + response.CustomerId);
                SendReply(info, response);
            }
        }


        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <remarks>
        /// This method will be called when a message arrives on the bus and should contain
        ///             the custom logic to execute when the message is received.
        /// </remarks>
        public void Handle(YodleeGetTransactionsCommandResponse message)
        {
            foreach (IGrouping<int, YodleeTransaction> group in message.Transactions.GroupBy(t => t.AccountId))
            {
                YodleeOrderItem account = new YodleeOrderItem
                {
                    accountNumber = group.First()
                        .AccountNumber,
                    bankAccountId = group.Key
                };

                YodleeQueries.UpsertContentServiceAccount(account);

                YodleeQueries.UpsertTransactions(group.Select(ConvertToYodleeOrderItemTransaction));
            }
        }

        /// <summary>
        /// Sends the transactions request command.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        /// <param name="yodleeUserName">Name of the yodlee user.</param>
        /// <param name="yodleeUserPassword">The yodlee user password.</param>
        private void SendTransactionsRequestCommand(IList<YodleeContentServiceAccount> accounts, string yodleeUserName, string yodleeUserPassword)
        {
            YodleeGetTransactionsCommand getTransactionsCommand = new YodleeGetTransactionsCommand
            {
                UserName = yodleeUserName,
                UserPassword = yodleeUserPassword,
                CobrandUserName = Config.CoBrandUserName,
                CobrandPassword = Config.CoBrandPassword,
                AccountsNumbers = accounts.Select(o => o.SiteAccountId)
            };

            Bus.Send(ThirdPartyService.Address, getTransactionsCommand);
        }

        /// <summary>
        /// Saves the accounts.
        /// </summary>
        /// <param name="accounts">The accounts.</param>
        /// <param name="yodleeUserName">Name of the yodlee user.</param>
        /// <param name="yodleeUserPassword">The yodlee user password.</param>
        /// <exception cref="System.InvalidOperationException">could not save account</exception>
        private void SaveAccounts(IList<YodleeContentServiceAccount> accounts, string yodleeUserName, string yodleeUserPassword) {
            foreach (var userAccount in accounts) {

                var res = YodleeQueries.UpsertContentServiceAccount(ConvertToOrderItem(userAccount));
                if (!res.HasValue || !res.Value) {
                    throw new InvalidOperationException("could not save account");
                }
            }
        }


        /// <summary>
        /// Converts to order item.
        /// </summary>
        /// <param name="acc">The acc.</param>
        /// <returns></returns>
        private YodleeOrderItem ConvertToOrderItem(YodleeContentServiceAccount acc) {
            return new YodleeOrderItem() {

                bankAccountId = acc.SiteAccountId,
                OrderId = acc.ContentServiceId,
                created = acc.CreatedInSeconds,
                link = acc.LoginUrl
            };
        }

        /// <summary>
        /// Converts to yodlee order item transaction.
        /// </summary>
        /// <param name="yTransaction">The y transaction.</param>
        /// <returns></returns>
        private YodleeOrderItemTransaction ConvertToYodleeOrderItemTransaction(YodleeTransaction yTransaction) {
            YodleeOrderItemTransaction t = new YodleeOrderItemTransaction();
            t.bankAccountId = yTransaction.AccountId;
            t.bankTransactionId = yTransaction.TransactionId;
            t.categoryId = yTransaction.CategoryId;
//            t.OrderItemId =;//TODO:
            t.category = yTransaction.Category;
//            t.EzbobCategory = ConvertToEzBobCategory(yTransaction.category.categoryName);//TODO:
            t.transactionBaseType = yTransaction.TransactionBaseType;
            t.transactionBaseTypeId = yTransaction.TransactionBaseTypeId;
            t.localizedTransactionBaseType = yTransaction.LocalizedTransactionBaseType;
            t.runningBalance = yTransaction.RunningBalance;
            t.categorizationKeyword = yTransaction.CategorizationKeyword;
            t.postDate = yTransaction.PostDate;
            t.transactionDate = yTransaction.TransactionDate;

            var GBP = ConvertToGBP(yTransaction.TransactionAmount, yTransaction.TransactionAmountCurrency, yTransaction.TransactionDate);

            t.transactionAmount = GBP.Amount;
            t.transactionAmountCurrency = GBP.ISOCurrencySymbol;
            t.transactionStatus = yTransaction.TransactionStatus;
            t.categorisationSourceId = yTransaction.CategorisationSourceId;
            t.description = yTransaction.Description;
           

            GBP = ConvertToGBP(yTransaction.RunningBalance, yTransaction.RunningBalanceCurrency, yTransaction.TransactionDate);
            t.runningBalance = GBP.Amount;
            t.runningBalanceCurrency = GBP.ISOCurrencySymbol;

            return t;
        }

        /// <summary>
        /// Converts to GBP.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="currencyISOString">The currency iso string.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private Money ConvertToGBP(decimal amount, string currencyISOString, DateTime date) {
            Money m = new Money(amount, currencyISOString);
            var GBP = CurrencyConverter.ConvertToGBP(m, date);
            return GBP;
        }
    }
}

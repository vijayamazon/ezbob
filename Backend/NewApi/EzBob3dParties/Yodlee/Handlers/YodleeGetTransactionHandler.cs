namespace EzBob3dParties.Yodlee.Handlers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dParties.Yodlee.Models;
    using EzBob3dParties.Yodlee.Models.Transactions;
    using EzBob3dParties.Yodlee.RequestResponse;
    using EzBob3dPartiesApi.Yodlee;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Yodlee;
    using Newtonsoft.Json;
    using NServiceBus;

    internal class YodleeGetTransactionHandler : HandlerBase<YodleeGetTransactionsCommandResponse>, IHandleMessages<YodleeGetTransactionsCommand> {

        [Injected]
        public YodleeService Yodlee { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public async void Handle(YodleeGetTransactionsCommand cmd) {
            YCobrandLoginRequest cobrandLoginRequest = new YCobrandLoginRequest().SetCobrandUserName(cmd.CobrandUserName)
                .SetCobrandPassword(cmd.CobrandPassword);

            YCobrandLoginResponse yCobrandLoginResponse = await Yodlee.LoginCobrand(cobrandLoginRequest);

            YUserLoginRequest userLoginRequest = new YUserLoginRequest()
                .SetCobrandSessionToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserName(cmd.UserName)
                .SetUserPassword(cmd.UserPassword);

            YUserLoginResponse yUserLoginResponse = await Yodlee.LoginUser(userLoginRequest);

            YTransactionsSearchRequest transactionsSearchRequest = new YTransactionsSearchRequest()
                .SetCoBrandToken(yCobrandLoginResponse.cobrandConversationCredentials.sessionToken)
                .SetUserSessionToken(yUserLoginResponse.userContext.conversationCredentials.sessionToken)
                .SetContainerType(EContainerType.bank);

            var lookup = cmd.AccountsNumbers.ToLookup(o => o);

            IEnumerable<YTransactionsSearchResponse> transactions = Yodlee.GetTransactions(transactionsSearchRequest);
            foreach (var transaction in transactions) {
                var currentTransation = transaction;
                SendReply(new InfoAccumulator(), cmd, reply => {
                    reply.HitsCount = currentTransation.numberOfHits;
                    reply.TotalTransactionsCount = currentTransation.countOfAllTransaction;
                    reply.Transactions = currentTransation.searchResult.Transactions.Where(t => !lookup.Contains(t.account.itemAccountId))
                        .Select(ConvertToApiTransaction);
                    reply.AccountNumbers = cmd.AccountsNumbers;
                });
            }
        }

        /// <summary>
        /// Converts to API transaction.
        /// </summary>
        /// <param name="yTransaction">The y transaction.</param>
        /// <returns></returns>
        private YodleeTransaction ConvertToApiTransaction(YTransaction yTransaction) {
            return new YodleeTransaction {
                AccountNumber = yTransaction.account.accountNumber,
                AccountId = yTransaction.account.itemAccountId,
                TransactionId = yTransaction.viewKey.transactionId,
                CategoryId = yTransaction.category.categoryId,
                OrderItemId = yTransaction.account.itemAccountId,
                Category = yTransaction.category.categoryName,
                TransactionBaseType = yTransaction.transactionBaseType,
                TransactionBaseTypeId = yTransaction.transactionBaseTypeId,
                LocalizedTransactionBaseType = yTransaction.localizedTransactionBaseType,
                CategorizationKeyword = yTransaction.categorizationKeyword,
                PostDate = DateTimeOffset.Parse(yTransaction.postDate)
                    .UtcDateTime,
                TransactionDate = DateTimeOffset.Parse(yTransaction.transactionDate)
                    .UtcDateTime,
                TransactionAmount = yTransaction.amount.amount,
                TransactionAmountCurrency = yTransaction.amount.currencyCode,
                TransactionStatus = yTransaction.status.description,
                CategorisationSourceId = yTransaction.categorisationSourceId.ToString(),
                Description = yTransaction.description.description,
                RunningBalance = yTransaction.runningBalance,
                RunningBalanceCurrency = yTransaction.amount.currencyCode
            };
        }
    }
}

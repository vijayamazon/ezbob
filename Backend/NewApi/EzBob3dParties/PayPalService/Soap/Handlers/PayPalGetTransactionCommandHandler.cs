using System;
using System.Collections.Generic;
using System.Linq;

namespace EzBob3dParties.PayPalService.Soap.Handlers {
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;
    using NServiceBus;
    using PayPal.PayPalAPIInterfaceService.Model;

    public class PayPalGetTransactionCommandHandler : HandlerBase<PayPalGetTransations3dPartyCommandResponse>, IHandleMessages<PayPalGetTransations3dPartyCommand> {

        [Injected]
        public PayPalSoapService PayPalService { get; set; }


        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(PayPalGetTransations3dPartyCommand command) {
            var res = Enumerable.Empty<PayPal3dPartyTransactionItem>();

            foreach (var interval in GetIntervals(command.UtcDateFrom, command.UtcDateTo)) {
                var transactionsResponse = await PayPalService.GetTransactions(command.AccessToken, command.AccessTokenSecret, interval.Item1, interval.Item2);
                res = res.Concat(ParseTransactions(transactionsResponse));
            }

            var info = new InfoAccumulator();
            SendReply(info, command, resp => resp.Transactions = res.ToList());
        }

        /// <summary>
        /// Parses the transactions.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private IEnumerable<PayPal3dPartyTransactionItem> ParseTransactions(TransactionSearchResponseType response) {
            foreach (var tr in response.PaymentTransactions.Select(CreateSingleTransactionItem)) {
                yield return tr;
            }
        }

        /// <summary>
        /// Creates the single transaction item.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns></returns>
        private PayPal3dPartyTransactionItem CreateSingleTransactionItem(PaymentTransactionSearchResultType response) {
            return new PayPal3dPartyTransactionItem {
                Created = DateTime.Parse(response.Timestamp),
                TimeZone = response.Timezone,
                Type = response.Type,
                Status = response.Status,
                PayPalTransactionId = response.TransactionID,
                FeeAmount = ConvertToMoney(response.FeeAmount),
                GrossAmount = ConvertToMoney(response.GrossAmount),
                NetAmount = ConvertToMoney(response.NetAmount)
            };
        }

        /// <summary>
        /// Converts to money.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        private Money ConvertToMoney(BasicAmountType amount) {
            return new Money(decimal.Parse(amount.value), amount.value);
        }

        /// <summary>
        /// Gets the intervals.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        private List<Tuple<DateTime, DateTime>> GetIntervals(DateTime startDate, DateTime endDate) {
            var res = new List<Tuple<DateTime, DateTime>>();
            DateTime fromDate = startDate;
            DateTime toDate = fromDate.AddDays(PayPalService.Config.DaysPerTransactionRequest);
            while (true) {
                if (toDate >= endDate) {
                    res.Add(new Tuple<DateTime, DateTime>(fromDate, endDate));
                    break;
                }

                res.Add(new Tuple<DateTime, DateTime>(fromDate, toDate));
                fromDate = toDate;
                toDate = toDate.AddDays(PayPalService.Config.DaysPerTransactionRequest);
                fromDate = fromDate.AddSeconds(1);
            }

            return res;
        }
    }
}

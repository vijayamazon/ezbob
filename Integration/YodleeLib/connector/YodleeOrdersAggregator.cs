namespace YodleeLib.connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using EzBob.CommonLib.TimePeriodLogic;
    using log4net;

    internal class YodleeOrdersAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType>
    {
        public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<YodleeTransactionItem> data, ICurrencyConvertor currencyConverter)
        {
            return new YodleeOrdersAggregator(data, currencyConverter);
        }
    }

    internal class YodleeOrdersAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeOrdersAggregator));

        public YodleeOrdersAggregator(ReceivedDataListTimeDependentInfo<YodleeTransactionItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {

        }

        protected override object InternalCalculateAggregatorValue(YodleeDatabaseFunctionType functionType, IEnumerable<YodleeTransactionItem> orders)
        {
            //switch (functionType)
            //{

            //    case YodleeDatabaseFunctionType.TotlaIncome:
            //        return GetTotlaIncome(orders);

            //    case YodleeDatabaseFunctionType.TotalExpense:
            //        return GetTotalExpense(orders);

            //    case YodleeDatabaseFunctionType.CurrentBalance:
            //        return GetCurrentBalance(orders);

            //    case YodleeDatabaseFunctionType.AvailableBalance:
            //        return GetAvailableBalance(orders);

            //    default:
            //        throw new NotImplementedException();
            //}
            return null;
        }

        private double GetAvailableBalance(IEnumerable<YodleeOrderItem> orders)
        {
            var availableBalance = orders.First().Data
                                         .Where(
                                             x =>
                                             x.Key.availableBalance != null && x.Key.availableBalance.amountSpecified)
                                         .Sum(
                                             x =>
                                             CurrencyConverter.ConvertToBaseCurrency(
                                                 x.Key.availableBalance.currencyCode,
                                                 x.Key.availableBalance.amount.Value,
                                                 (x.Key.asOfDate != null &&
                                                  x.Key.asOfDate.dateSpecified)
                                                     ? x.Key.asOfDate.date
                                                     : null).Value);
            return availableBalance;
        }

        private double GetCurrentBalance(IEnumerable<YodleeOrderItem> orders)
        {
            var currentBalance = orders.First().Data
                                       .Where(x => x.Key.currentBalance != null && x.Key.currentBalance.amountSpecified)
                                       .Sum(
                                           x =>
                                           CurrencyConverter.ConvertToBaseCurrency(x.Key.currentBalance.currencyCode,
                                                                                   x.Key.currentBalance.amount.Value,
                                                                                   (x.Key.asOfDate != null &&
                                                                                    x.Key.asOfDate.dateSpecified)
                                                                                       ? x.Key.asOfDate.date
                                                                                       : null).Value);
            return currentBalance;
        }

        private double GetTotalExpense(IEnumerable<YodleeOrderItem> orders)
        {
            var totlalExpense =
                orders.First()
                      .Data
                      .SelectMany(x => x.Value)
                      .Where(
                          b =>
                          b.transactionStatusIdSpecified &&
                          b.transactionBaseTypeIdSpecified &&
                          b.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
                          b.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Debit &&
                          b.transactionAmount != null &&
                          b.transactionAmount.amountSpecified)
                      .Sum(
                          s =>
                          CurrencyConverter.ConvertToBaseCurrency(s.transactionAmount.currencyCode,
                                                                  s.transactionAmount.amount.Value,
                                                                  (s.transactionDate != null &&
                                                                   s.transactionDate.dateSpecified)
                                                                      ? s.transactionDate.date
                                                                      : null).Value);

            return totlalExpense;
        }

        private double GetTotlaIncome(IEnumerable<YodleeOrderItem> orders)
        {
            var totlalExpense =
                orders.First()
                      .Data
                      .SelectMany(x => x.Value)
                      .Where(
                          b =>
                          b.transactionStatusIdSpecified &&
                          b.transactionBaseTypeIdSpecified &&
                          b.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
                          b.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Credit &&
                          b.transactionAmount != null &&
                          b.transactionAmount.amountSpecified)
                      .Sum(
                          s =>
                          CurrencyConverter.ConvertToBaseCurrency(s.transactionAmount.currencyCode,
                                                                  s.transactionAmount.amount.Value,
                                                                  (s.transactionDate != null &&
                                                                   s.transactionDate.dateSpecified)
                                                                      ? s.transactionDate.date
                                                                      : null).Value);

            return totlalExpense;
        }
    }
}
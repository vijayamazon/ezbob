namespace YodleeLib.connector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using EzBob.CommonLib.TimePeriodLogic;
    using log4net;

    internal class YodleeTransactionsAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType>
    {
        public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<YodleeTransactionItem> data, ICurrencyConvertor currencyConverter)
        {
            return new YodleeTransactionsAggregator(data, currencyConverter);
        }
    }

    internal class YodleeTransactionsAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<YodleeTransactionItem>, YodleeTransactionItem, YodleeDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeTransactionsAggregator));

        public YodleeTransactionsAggregator(ReceivedDataListTimeDependentInfo<YodleeTransactionItem> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {

        }

        protected override object InternalCalculateAggregatorValue(YodleeDatabaseFunctionType functionType, IEnumerable<YodleeTransactionItem> orders)
        {
            switch (functionType)
            {

                case YodleeDatabaseFunctionType.TotlaIncome:
                    return GetTotlaIncome(orders);

                case YodleeDatabaseFunctionType.TotalExpense:
                    return GetTotalExpense(orders);

                default:
                    throw new NotImplementedException();
            }
        }

        private double GetTotalExpense(IEnumerable<YodleeTransactionItem> orders)
        {
            var totlalExpense =
                orders.Where(
                          b =>
                          b._Data.transactionStatusIdSpecified &&
                          b._Data.transactionBaseTypeIdSpecified &&
                          b._Data.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
                          b._Data.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Debit &&
                          b._Data.transactionAmount != null &&
                          b._Data.transactionAmount.amountSpecified)
                      .Sum(
                          s =>
                          CurrencyConverter.ConvertToBaseCurrency(s._Data.transactionAmount.currencyCode,
                                                                  s._Data.transactionAmount.amount.Value,
                                                                  (s._Data.transactionDate != null &&
                                                                   s._Data.transactionDate.dateSpecified)
                                                                      ? s._Data.transactionDate.date
                                                                      : null).Value);

            return totlalExpense;
        }

        private double GetTotlaIncome(IEnumerable<YodleeTransactionItem> orders)
        {
            var totlalExpense =
                      orders.Where(
                          b =>
                          b._Data.transactionStatusIdSpecified &&
                          b._Data.transactionBaseTypeIdSpecified &&
                          b._Data.transactionStatusId == (int)datatypes.TransactionStatus.Posted &&
                          b._Data.transactionBaseTypeId == (int)datatypes.TransactionBaseType.Credit &&
                          b._Data.transactionAmount != null &&
                          b._Data.transactionAmount.amountSpecified)
                      .Sum(
                          s =>
                          CurrencyConverter.ConvertToBaseCurrency(s._Data.transactionAmount.currencyCode,
                                                                  s._Data.transactionAmount.amount.Value,
                                                                  (s._Data.transactionDate != null &&
                                                                   s._Data.transactionDate.dateSpecified)
                                                                      ? s._Data.transactionDate.date
                                                                      : null).Value);

            return totlalExpense;
        }
    }
}
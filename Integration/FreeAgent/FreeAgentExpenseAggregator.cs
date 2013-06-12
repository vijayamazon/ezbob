namespace FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class FreeAgentExpenseAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<FreeAgentExpense>, FreeAgentExpense, FreeAgentDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentExpense>, FreeAgentExpense, FreeAgentDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<FreeAgentExpense> data, ICurrencyConvertor currencyConverter)
        {
			return new FreeAgentExpenseAggregator(data, currencyConverter);
        }
    }

	internal class FreeAgentExpenseAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<FreeAgentExpense>, FreeAgentExpense, FreeAgentDatabaseFunctionType>
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(FreeAgentExpenseAggregator));

		public FreeAgentExpenseAggregator(ReceivedDataListTimeDependentInfo<FreeAgentExpense> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetExpensesCount(IEnumerable<FreeAgentExpense> expenses)
		{
			return expenses.Count();
        }

		private double GetTotalSumOfExpenses(IEnumerable<FreeAgentExpense> expenses)
		{
			return expenses.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.gross_value, o.dated_on).Value);
        }

		protected override object InternalCalculateAggregatorValue(FreeAgentDatabaseFunctionType functionType, IEnumerable<FreeAgentExpense> expenses)
        {
            switch (functionType)
            {
                case FreeAgentDatabaseFunctionType.NumOfExpenses:
					return GetExpensesCount(expenses);

				case FreeAgentDatabaseFunctionType.TotalSumOfExpenses:
					return GetTotalSumOfExpenses(expenses);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;
	using log4net;

	internal class SageIncomeAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SageIncome>, SageIncome, SageDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageIncome>, SageIncome, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SageIncome> data, ICurrencyConvertor currencyConverter)
        {
			return new SageIncomeAggregator(data, currencyConverter);
        }
    }

	internal class SageIncomeAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageIncome>, SageIncome, SageDatabaseFunctionType>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SageIncomeAggregator));

		public SageIncomeAggregator(ReceivedDataListTimeDependentInfo<SageIncome> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetIncomesCount(IEnumerable<SageIncome> incomes)
		{
			return incomes.Count();
        }

		private double GetTotalSumOfIncomes(IEnumerable<SageIncome> incomes)
		{
			// consider handling currency conversion like:
			// return incomes.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.amount, o.date).Value);
			return incomes.Sum(o => (double)o.amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SageIncome> incomes)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfIncomes:
					return GetIncomesCount(incomes);

				case SageDatabaseFunctionType.TotalSumOfIncomes:
					return GetTotalSumOfIncomes(incomes);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
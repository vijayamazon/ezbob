namespace Sage
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EzBob.CommonLib.TimePeriodLogic;

	internal class SageExpenditureAggregatorFactory : DataAggregatorFactoryBase<ReceivedDataListTimeDependentInfo<SageExpenditure>, SageExpenditure, SageDatabaseFunctionType>
    {
		public override DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageExpenditure>, SageExpenditure, SageDatabaseFunctionType> CreateDataAggregator(ReceivedDataListTimeDependentInfo<SageExpenditure> data, ICurrencyConvertor currencyConverter)
        {
			return new SageExpenditureAggregator(data, currencyConverter);
        }
    }

	internal class SageExpenditureAggregator : DataAggregatorBase<ReceivedDataListTimeDependentInfo<SageExpenditure>, SageExpenditure, SageDatabaseFunctionType>
    {
		public SageExpenditureAggregator(ReceivedDataListTimeDependentInfo<SageExpenditure> orders, ICurrencyConvertor currencyConvertor)
            : base(orders, currencyConvertor)
        {
        }

		private int GetExpendituresCount(IEnumerable<SageExpenditure> expenditures)
		{
			return expenditures.Count();
        }

		private double GetTotalSumOfExpenditures(IEnumerable<SageExpenditure> expenditures)
		{
			// consider handling currency conversion like:
			// return expenditures.Sum(o => CurrencyConverter.ConvertToBaseCurrency(o.currency, (double)o.amount, o.date).Value);
			return expenditures.Sum(o => (double)o.amount);
		}

		protected override object InternalCalculateAggregatorValue(SageDatabaseFunctionType functionType, IEnumerable<SageExpenditure> expenditures)
        {
            switch (functionType)
            {
                case SageDatabaseFunctionType.NumOfExpenditures:
					return GetExpendituresCount(expenditures);

				case SageDatabaseFunctionType.TotalSumOfExpenditures:
					return GetTotalSumOfExpenditures(expenditures);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
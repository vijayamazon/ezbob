namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Scorto.NHibernate.Repository;
	using NHibernate;

	public class YodleeCashFlowReportModel
	{
		public SortedDictionary<string/*type,name*/, SortedDictionary<int/*yearmonth*/, double/*amount*/>> YodleeCashFlowReportModelDict { get; set; }
		private const int Total = 999999;
		private readonly CurrencyConvertor _currencyConvertor;
		public YodleeCashFlowReportModel(ISession session)
		{
			YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<int, double>>();
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(session));
		}

		public void Add(MP_YodleeOrderItemBankTransaction transaction)
		{
			var amount = transaction.transactionAmount.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
															 transaction.transactionAmountCurrency,
															 transaction.transactionAmount.Value,
															 transaction.postDate ?? transaction.transactionDate).Value : 0;
			var catType = transaction.transactionCategory.Type;
			var catName = transaction.transactionCategory.Name;
			var baseType = transaction.transactionBaseType;
			var date = transaction.postDate.HasValue ? transaction.postDate.Value : transaction.transactionDate.Value;
			var cat = string.Format("{2}{0},{1}", catName, catType, baseType == "credit" ? "0" : "2");
			var yearmonth = date.Year * 100 + date.Month;


			Add(cat, amount, yearmonth);

			Add(cat, amount, Total);

		}


		private void Add(string cat, double amount, int yearmonth)
		{
			if (!YodleeCashFlowReportModelDict.ContainsKey(cat))
			{
				YodleeCashFlowReportModelDict[cat] = new SortedDictionary<int, double>();
			}

			var x = YodleeCashFlowReportModelDict[cat];

			if (!x.ContainsKey(yearmonth))
			{
				x[yearmonth] = amount;
			}
			else
			{
				x[yearmonth] += amount;
			}
		}


		public void AddMissingAndSort()
		{

			CalculateOther();

			//retrieving month list
			var monthList = (from cat in YodleeCashFlowReportModelDict from month in YodleeCashFlowReportModelDict[cat.Key] select month.Key).ToList();

			//adding amount 0 for missing month/categories
			foreach (var cat in YodleeCashFlowReportModelDict)
			{
				foreach (var m in monthList)
				{
					if (!YodleeCashFlowReportModelDict[cat.Key].ContainsKey(m))
					{
						YodleeCashFlowReportModelDict[cat.Key][m] = 0;
					}
				}
			}
		}

		private void CalculateOther()
		{
			var incomeOtherList = new List<string>();
			var expensesOtherList = new List<string>();
			//calculating other (less than 500& in totals

			foreach (var cat in YodleeCashFlowReportModelDict)
			{
				var x = YodleeCashFlowReportModelDict[cat.Key];
				if (x[Total] < 500 && cat.Key[0] == '0')
				{
					incomeOtherList.Add(cat.Key);
				}

				if (x[Total] < 500 && cat.Key[0] == '2')
				{
					expensesOtherList.Add(cat.Key);
				}
			}


			if (incomeOtherList.Count > 0)
			{
				Add("1Other Income", 0, Total);
			}

			if (expensesOtherList.Count > 0)
			{
				Add("3Other Expenses", 0, Total);
			}


			foreach (var incomeOther in incomeOtherList)
			{
				foreach (var cat in YodleeCashFlowReportModelDict[incomeOther])
				{
					var x = YodleeCashFlowReportModelDict["1Other Income"];

					if (!x.ContainsKey(cat.Key))
					{
						x[cat.Key] = cat.Value;
					}
					else
					{
						x[cat.Key] += cat.Value;
					}
				}
			}

			foreach (var expensesOther in expensesOtherList)
			{
				foreach (var cat in YodleeCashFlowReportModelDict[expensesOther])
				{
					var x = YodleeCashFlowReportModelDict["3Other Expenses"];

					if (!x.ContainsKey(cat.Key))
					{
						x[cat.Key] = cat.Value;
					}
					else
					{
						x[cat.Key] += cat.Value;
					}
				}
			}

			foreach (var expensesOther in expensesOtherList)
			{
				YodleeCashFlowReportModelDict.Remove(expensesOther);
			}

			foreach (var incomeOther in incomeOtherList)
			{
				YodleeCashFlowReportModelDict.Remove(incomeOther);
			}
		}
	}
}
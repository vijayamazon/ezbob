namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model;
	using NHibernate;
	
	public class YodleeCashFlowReportModel
	{
		public SortedDictionary<string/*type,name*/, SortedDictionary<int/*yearmonth*/, double/*amount*/>> YodleeCashFlowReportModelDict { get; set; }
		public SortedDictionary<int/*yearmonth*/, int/*minday*/ > MinDateDict { get; set; }
		public SortedDictionary<int/*yearmonth*/, int/*maxday*/> MaxDateDict { get; set; }
		public double MonthInPayments = 0;
		public double BankFrame = 0;
		public DateTime AsOfDate;
		private const int TotalColumn = 999999;
		private const string OtherIncomeCat = "Other Income";
		private const string OtherExpensesCat = "Other Expenses";
		private const string TotalIncomeCat = "Total Income";
		private const string TotalExpensesCat = "Total Expenses";
		private const string NumOfTransactionsCat = "Num Of Transactions";
		private const string AverageIncomeCat = "Average Income";
		private const string AverageExpensesCat = "Average Expenses";
		private const string NetCashFlowCat = "Net Cash Flow";

		private const char TotalCredit = '0';
		private const char TotalDedit = '1';
		private const char NetCashFlow = '2';

		private const char Credit = '3';
		private const char OtherCredit = '4';
		private const char NumTransCredit = '5';
		private const char AverageCredit = '6';
		
		private const char Dedit = '7';
		private const char OtherDedit = '8';
		private const char NumTransDedit = '9';
		private const char AverageDedit = 'a';
		

		private readonly IConfigurationVariablesRepository _configVariables;
		public YodleeCashFlowReportModel(ISession session)
		{
			YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<int, double>>();
			MinDateDict = new SortedDictionary<int, int>();
			MaxDateDict = new SortedDictionary<int, int>();
			_configVariables = new ConfigurationVariablesRepository(session);
		}

		public void Add(YodleeTransactionModel transaction)
		{
			var amount = transaction.transactionAmount.HasValue ? transaction.transactionAmount.Value : 0;
			//var catType = transaction.transactionCategory.Type;
			var catName = transaction.categoryName;
			var baseType = transaction.transactionBaseType;
			var date = transaction.transactionDate;
			var cat = string.Format("{1}{0}", catName, baseType == "credit" ? Credit : Dedit);
			var yearmonth = date.Year * 100 + date.Month;

			var runningBalance = transaction.runningBalance.HasValue ? transaction.runningBalance.Value: 0;

			Add(cat, amount, yearmonth);
			UpdateMinMaxDay(yearmonth, date);
			Add(cat, amount, TotalColumn);

			//Calc Total Row
			Add(baseType == "credit"
					? string.Format("{0}{1}", TotalCredit, TotalIncomeCat)
					: string.Format("{0}{1}", TotalDedit, TotalExpensesCat), amount, yearmonth);
			Add(baseType == "credit"
					? string.Format("{0}{1}", TotalCredit, TotalIncomeCat)
					: string.Format("{0}{1}", TotalDedit, TotalExpensesCat), amount, TotalColumn);

			//Calc Num Of Transactions Row
			Add(baseType == "credit"
					? string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat)
					: string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat), 1, yearmonth);
			Add(baseType == "credit"
					? string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat)
					: string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat), 1, TotalColumn);

			var monthAgo = DateTime.Today.AddMonths(-1);
			if (date >= monthAgo)
			{
				MonthInPayments += amount;
			}
		}

		private void UpdateMinMaxDay(int yearmonth, DateTime? date)
		{
			if (!date.HasValue) return;

			if (!MinDateDict.ContainsKey(yearmonth))
			{
				MinDateDict[yearmonth] = date.Value.Day;
			}
			else
			{
				if (MinDateDict[yearmonth] > date.Value.Day)
				{
					MinDateDict[yearmonth] = date.Value.Day;
				}
			}


			if (!MaxDateDict.ContainsKey(yearmonth))
			{
				MaxDateDict[yearmonth] = date.Value.Day;
			}
			else
			{
				if (MaxDateDict[yearmonth] < date.Value.Day)
				{
					MaxDateDict[yearmonth] = date.Value.Day;
				}
			}
		}

		public void AddMissingAndSort()
		{
			CalculateOther(Credit, string.Format("{0}{1}", OtherCredit, OtherIncomeCat));
			CalculateOther(Dedit, string.Format("{0}{1}", OtherDedit, OtherExpensesCat));

			//retrieving month list
			var monthList = (from cat in YodleeCashFlowReportModelDict from month in YodleeCashFlowReportModelDict[cat.Key] select month.Key).ToList();

			//adding missing categories
			AddIfMissing(TotalCredit, TotalIncomeCat);
			AddIfMissing(TotalDedit, TotalExpensesCat);
			AddIfMissing(NumTransCredit, NumOfTransactionsCat);
			AddIfMissing(NumTransDedit, NumOfTransactionsCat);

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

			if (YodleeCashFlowReportModelDict.Count > 0)
			{
				//Calc Avarage Rows
				CalculateAverage(string.Format("{0}{1}", AverageCredit, AverageIncomeCat),
								 string.Format("{0}{1}", TotalCredit, TotalIncomeCat),
								 string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat));
				CalculateAverage(string.Format("{0}{1}", AverageDedit, AverageExpensesCat),
								 string.Format("{0}{1}", TotalDedit, TotalExpensesCat),
								 string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat));

				CalculateNetCashFlow(string.Format("{0}{1}", NetCashFlow, NetCashFlowCat),
									 string.Format("{0}{1}", TotalCredit, TotalIncomeCat),
									 string.Format("{0}{1}", TotalDedit, TotalExpensesCat));
			}
		}

		private void AddIfMissing(char catPrefix, string cat)
		{
			if (!YodleeCashFlowReportModelDict.ContainsKey(string.Format("{0}{1}", catPrefix, cat)))
			{
				var total = new SortedDictionary<int, double> {{TotalColumn, 0}};
				YodleeCashFlowReportModelDict[string.Format("{0}{1}", catPrefix, cat)] = total;
			}
		}

		private void CalculateNetCashFlow(string netCashFlowCat, string totalIncomeCat, string totalExpensesCat)
		{
			var totalIncomeRow = YodleeCashFlowReportModelDict[totalIncomeCat];
			var totalExpensesRow = YodleeCashFlowReportModelDict[totalExpensesCat];

			foreach (var yearmonth in totalIncomeRow.Keys)
			{
				Add(netCashFlowCat, totalIncomeRow[yearmonth] - totalExpensesRow[yearmonth], yearmonth);
			}
		}

		private void CalculateAverage(string averageCat, string totalCat, string numOfTransCat)
		{
			var totalRow = YodleeCashFlowReportModelDict[totalCat];
			var numOfTransRow = YodleeCashFlowReportModelDict[numOfTransCat];

			foreach (var yearmonth in totalRow.Keys)
			{
				if (numOfTransRow[yearmonth] != 0)
				{
					Add(averageCat, totalRow[yearmonth] / numOfTransRow[yearmonth], yearmonth);
				}
				else
				{
					Add(averageCat, 0, yearmonth);
				}
			}
		}

		private void CalculateOther(char baseType, string otherCat)
		{
			var otherList = new List<string>();
			//calculating other (less than 500 pound in totals (configurable)
			var maxYodleeOtherCategoryAmount = double.Parse(_configVariables.GetByName("MaxYodleeOtherCategoryAmount").Value);

			foreach (var cat in YodleeCashFlowReportModelDict)
			{
				var x = YodleeCashFlowReportModelDict[cat.Key];
				if (cat.Key[0] == baseType && x[TotalColumn] < maxYodleeOtherCategoryAmount)
				{
					otherList.Add(cat.Key);
				}
			}

			if (otherList.Count > 0)
			{
				Add(otherCat, 0, TotalColumn);
			}

			foreach (var other in otherList)
			{
				foreach (var cat in YodleeCashFlowReportModelDict[other])
				{
					Sum(otherCat, cat.Key, cat.Value);
				}
			}

			foreach (var other in otherList)
			{
				YodleeCashFlowReportModelDict.Remove(other);
			}
		}

		private void Add(string cat, double amount, int yearmonth)
		{
			if (!YodleeCashFlowReportModelDict.ContainsKey(cat))
			{
				YodleeCashFlowReportModelDict[cat] = new SortedDictionary<int, double>();
			}

			Sum(cat, yearmonth, amount);
		}

		private void Sum(string cat, int yearmonth, double amount)
		{
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
	}
}
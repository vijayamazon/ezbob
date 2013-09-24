namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using Scorto.NHibernate.Repository;
	using NHibernate;

	public class YodleeCashFlowReportModel
	{
		public SortedDictionary<string/*type,name*/, SortedDictionary<int/*yearmonth*/, double/*amount*/>> YodleeCashFlowReportModelDict { get; set; }

		public SortedDictionary<int/*yearmonth*/, int/*minday*/ > MinDateDict { get; set; }
		public SortedDictionary<int/*yearmonth*/, int/*maxday*/> MaxDateDict { get; set; }
		private const int TotalColumn = 999999;
		private const string OtherIncomeCat = "Other Income";
		private const string OtherExpensesCat = "Other Expenses";
		private const string TotalIncomeCat = "Total Income";
		private const string TotalExpensesCat = "Total Expenses";
		private const string NumOfTransactionsCat = "Num Of Transactions";
		private const string AverageIncomeCat = "Average Income";
		private const string AverageExpensesCat = "Average Expenses";

		private const char Credit = '0';
		private const char OtherCredit = '1';
		private const char TotalCredit = '2';
		private const char NumTransCredit = '3';
		private const char AverageCredit = '4';
		private const char Dedit = '5';
		private const char OtherDedit = '6';
		private const char TotalDedit = '7';
		private const char NumTransDedit = '8';
		private const char AverageDedit = '9';

		private readonly CurrencyConvertor _currencyConvertor;
		private readonly IConfigurationVariablesRepository _configVariables;
		public YodleeCashFlowReportModel(ISession session)
		{
			YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<int, double>>();
			MinDateDict = new SortedDictionary<int, int>();
			MaxDateDict = new SortedDictionary<int, int>();
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(session));
			_configVariables = new ConfigurationVariablesRepository(session);
		}

		public void Add(MP_YodleeOrderItemBankTransaction transaction)
		{
			var amount = transaction.transactionAmount.HasValue
							 ? _currencyConvertor.ConvertToBaseCurrency(
								 transaction.transactionAmountCurrency,
								 transaction.transactionAmount.Value,
								 transaction.postDate ?? transaction.transactionDate).Value
							 : 0;
			//var catType = transaction.transactionCategory.Type;
			var catName = transaction.transactionCategory.Name;
			var baseType = transaction.transactionBaseType;
			var date = (transaction.postDate ?? transaction.transactionDate);
			var cat = string.Format("{1}{0}", catName, baseType == "credit" ? Credit : Dedit);
			var yearmonth = date.HasValue ? date.Value.Year * 100 + date.Value.Month : 0;


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

			//Calc Avarage Rows
			CalculateAverage(string.Format("{0}{1}", AverageCredit, AverageIncomeCat),
			                 string.Format("{0}{1}", TotalCredit, TotalIncomeCat),
			                 string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat));
			CalculateAverage(string.Format("{0}{1}", AverageDedit, AverageExpensesCat),
							 string.Format("{0}{1}", TotalDedit, TotalExpensesCat),
							 string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat));
		}

		private void CalculateAverage(string averageCat, string totalCat, string numOfTransCat)
		{
			var totalRow = YodleeCashFlowReportModelDict[totalCat];
			var numOfTransRow = YodleeCashFlowReportModelDict[numOfTransCat];

			foreach (var yearmonth in totalRow.Keys)
			{
				Add(averageCat, totalRow[yearmonth] / numOfTransRow[yearmonth], yearmonth);
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
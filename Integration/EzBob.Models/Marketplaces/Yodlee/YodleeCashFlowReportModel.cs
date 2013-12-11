namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model;
	using NHibernate;

	[Serializable]
	public class YodleeCashFlowReportModel
	{
		public BankStatementDataModel BankStatementDataModel { get; set; }
		public SortedDictionary<string /*type,name*/, SortedDictionary<int /*yearmonth*/, double /*amount*/>>
			YodleeCashFlowReportModelDict { get; set; }

		public SortedDictionary<int /*yearmonth*/, int /*minday*/> MinDateDict { get; set; }
		public SortedDictionary<int /*yearmonth*/, int /*maxday*/> MaxDateDict { get; set; }
		public double MonthInPayments = 0;
		public DateTime AsOfDate;
	}

	public class YodleeCashFlowReportModelBuilder
	{
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

		private YodleeCashFlowReportModel yodlee;
		public YodleeCashFlowReportModelBuilder(ISession session)
		{
			yodlee = new YodleeCashFlowReportModel
				{
					YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<int, double>>(),
					MinDateDict = new SortedDictionary<int, int>(),
					MaxDateDict = new SortedDictionary<int, int>(),

				};
			_configVariables = new ConfigurationVariablesRepository(session);
		}

		public void Add(YodleeTransactionModel transaction)
		{
			var amount = transaction.transactionAmount.HasValue ? transaction.transactionAmount.Value : 0;
			//var catType = transaction.transactionCategory.Type;
			var catName = transaction.ezbobCategory;
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
				yodlee.MonthInPayments += amount;
			}
		}

		private void UpdateMinMaxDay(int yearmonth, DateTime? date)
		{
			if (!date.HasValue) return;

			if (!yodlee.MinDateDict.ContainsKey(yearmonth))
			{
				yodlee.MinDateDict[yearmonth] = date.Value.Day;
			}
			else
			{
				if (yodlee.MinDateDict[yearmonth] > date.Value.Day)
				{
					yodlee.MinDateDict[yearmonth] = date.Value.Day;
				}
			}


			if (!yodlee.MaxDateDict.ContainsKey(yearmonth))
			{
				yodlee.MaxDateDict[yearmonth] = date.Value.Day;
			}
			else
			{
				if (yodlee.MaxDateDict[yearmonth] < date.Value.Day)
				{
					yodlee.MaxDateDict[yearmonth] = date.Value.Day;
				}
			}
		}

		public void AddMissingAndSort()
		{
			CalculateOther(Credit, string.Format("{0}{1}", OtherCredit, OtherIncomeCat));
			CalculateOther(Dedit, string.Format("{0}{1}", OtherDedit, OtherExpensesCat));

			//retrieving month list
			var monthList = (from cat in yodlee.YodleeCashFlowReportModelDict from month in yodlee.YodleeCashFlowReportModelDict[cat.Key] select month.Key).ToList();

			//adding missing categories
			AddIfMissing(TotalCredit, TotalIncomeCat);
			AddIfMissing(TotalDedit, TotalExpensesCat);
			AddIfMissing(NumTransCredit, NumOfTransactionsCat);
			AddIfMissing(NumTransDedit, NumOfTransactionsCat);

			//adding amount 0 for missing month/categories
			foreach (var cat in yodlee.YodleeCashFlowReportModelDict)
			{
				foreach (var m in monthList)
				{
					if (!yodlee.YodleeCashFlowReportModelDict[cat.Key].ContainsKey(m))
					{
						yodlee.YodleeCashFlowReportModelDict[cat.Key][m] = 0;
					}
				}
			}

			if (yodlee.YodleeCashFlowReportModelDict.Count > 0)
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

			yodlee.BankStatementDataModel = new BankStatementDataModel();
			DateTime from = new DateTime(yodlee.MinDateDict.First().Key / 100, yodlee.MinDateDict.First().Key % 100, yodlee.MinDateDict.Last().Value);
			DateTime to = new DateTime(yodlee.MaxDateDict.Last().Key / 100, yodlee.MaxDateDict.Last().Key % 100, yodlee.MaxDateDict.Last().Value);

			yodlee.BankStatementDataModel.Period = string.Format("{0} - {1}", from.ToString("MMM yy", CultureInfo.InvariantCulture), to.ToString("MMM yy", CultureInfo.InvariantCulture));
			yodlee.BankStatementDataModel.Revenues = yodlee.YodleeCashFlowReportModelDict.ContainsKey("3Revenues Online") ? yodlee.YodleeCashFlowReportModelDict["3Revenues Online"][TotalColumn] : 0;
			yodlee.BankStatementDataModel.Revenues += yodlee.YodleeCashFlowReportModelDict.ContainsKey("3Revenues Transfers") ? yodlee.YodleeCashFlowReportModelDict["3Revenues Transfers"][TotalColumn] : 0;
			yodlee.BankStatementDataModel.Opex = yodlee.YodleeCashFlowReportModelDict.ContainsKey("7Opex ") ? yodlee.YodleeCashFlowReportModelDict["7Opex "][TotalColumn] : 0;
			yodlee.BankStatementDataModel.Salaries = yodlee.YodleeCashFlowReportModelDict.ContainsKey("7Salaries and Tax Salaries") ? yodlee.YodleeCashFlowReportModelDict["7Salaries and Tax Salaries"][TotalColumn] : 0;
			yodlee.BankStatementDataModel.Tax = yodlee.YodleeCashFlowReportModelDict.ContainsKey("7Salaries and Tax Taxes") ? yodlee.YodleeCashFlowReportModelDict["7Salaries and Tax Taxes"][TotalColumn] : 0;
			yodlee.BankStatementDataModel.ActualLoansRepayment = yodlee.YodleeCashFlowReportModelDict.ContainsKey("7Loan Repayments ") ? yodlee.YodleeCashFlowReportModelDict["7Loan Repayments "][TotalColumn] : 0;
			yodlee.BankStatementDataModel.TotalValueAdded = yodlee.BankStatementDataModel.Revenues - yodlee.BankStatementDataModel.Opex;
			yodlee.BankStatementDataModel.PercentOfRevenues = yodlee.BankStatementDataModel.TotalValueAdded / yodlee.BankStatementDataModel.Revenues;
			yodlee.BankStatementDataModel.Ebida = yodlee.BankStatementDataModel.TotalValueAdded - (yodlee.BankStatementDataModel.Salaries + yodlee.BankStatementDataModel.Tax);
			yodlee.BankStatementDataModel.FreeCashFlow = yodlee.BankStatementDataModel.Ebida - yodlee.BankStatementDataModel.ActualLoansRepayment;

			

		}

		private void AddIfMissing(char catPrefix, string cat)
		{
			if (!yodlee.YodleeCashFlowReportModelDict.ContainsKey(string.Format("{0}{1}", catPrefix, cat)))
			{
				var total = new SortedDictionary<int, double> {{TotalColumn, 0}};
				yodlee.YodleeCashFlowReportModelDict[string.Format("{0}{1}", catPrefix, cat)] = total;
			}
		}

		private void CalculateNetCashFlow(string netCashFlowCat, string totalIncomeCat, string totalExpensesCat)
		{
			var totalIncomeRow = yodlee.YodleeCashFlowReportModelDict[totalIncomeCat];
			var totalExpensesRow = yodlee.YodleeCashFlowReportModelDict[totalExpensesCat];

			foreach (var yearmonth in totalIncomeRow.Keys)
			{
				Add(netCashFlowCat, totalIncomeRow[yearmonth] - totalExpensesRow[yearmonth], yearmonth);
			}
		}

		private void CalculateAverage(string averageCat, string totalCat, string numOfTransCat)
		{
			var totalRow = yodlee.YodleeCashFlowReportModelDict[totalCat];
			var numOfTransRow = yodlee.YodleeCashFlowReportModelDict[numOfTransCat];

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

			foreach (var cat in yodlee.YodleeCashFlowReportModelDict)
			{
				var x = yodlee.YodleeCashFlowReportModelDict[cat.Key];
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
				foreach (var cat in yodlee.YodleeCashFlowReportModelDict[other])
				{
					Sum(otherCat, cat.Key, cat.Value);
				}
			}

			foreach (var other in otherList)
			{
				yodlee.YodleeCashFlowReportModelDict.Remove(other);
			}
		}

		private void Add(string cat, double amount, int yearmonth)
		{
			if (!yodlee.YodleeCashFlowReportModelDict.ContainsKey(cat))
			{
				yodlee.YodleeCashFlowReportModelDict[cat] = new SortedDictionary<int, double>();
			}

			Sum(cat, yearmonth, amount);
		}

		private void Sum(string cat, int yearmonth, double amount)
		{
			var x = yodlee.YodleeCashFlowReportModelDict[cat];

			if (!x.ContainsKey(yearmonth))
			{
				x[yearmonth] = amount;
			}
			else
			{
				x[yearmonth] += amount;
			}
		}

		public void SetAsOfDate(DateTime asOfDate)
		{
			yodlee.AsOfDate = asOfDate;
		}

		public YodleeCashFlowReportModel GetModel()
		{
			return yodlee;
		}
	}
}
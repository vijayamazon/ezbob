namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using Ezbob.Backend.Models;
	using NHibernate;

	[Serializable]
	public class YodleeCashFlowReportModel
	{

		public SortedDictionary<string /*type,name*/, SortedDictionary<string /*yearmonth*/, double /*amount*/>>
			YodleeCashFlowReportModelDict { get; set; }

		public SortedDictionary<string /*yearmonth*/, int /*minday*/> MinDateDict { get; set; }
		public SortedDictionary<string /*yearmonth*/, int /*maxday*/> MaxDateDict { get; set; }
		public double MonthInPayments = 0;
		public DateTime AsOfDate;
	}

	public class YodleeCashFlowReportModelBuilder
	{
		public const string TotalColumn = "999999";

		private const string TotalIncomeCat = "Total Income";
		private const string TotalExpensesCat = "Total Expenses";
		private const string NumOfTransactionsCat = "Num Of Transactions";
		private const string AverageIncomeCat = "Average Income";
		private const string AverageExpensesCat = "Average Expenses";
		private const string NetCashFlowCat = "Net Cash Flow";

		private const string TotalCredit = "0a";
		private const string TotalDedit = "0b";
		private const string NetCashFlow = "0c";

		//private const char Credit = '3';
		private const char Major = 'a';
		private const char Minor = 'd';
		private const string NumTransCredit = "c";
		private const string AverageCredit = "d";

		//private const char Dedit = '7';
		private const string NumTransDedit = "e";
		private const string AverageDedit = "f";

		//private const string OtherIncomeCat = "Other Income";
		//private const string OtherExpensesCat = "Other Expenses";
		//private const char OtherCredit = '4';
		//private const char OtherDedit = '8';

		private YodleeCashFlowReportModel yodlee;
		public YodleeCashFlowReportModelBuilder(ISession session)
		{
			yodlee = new YodleeCashFlowReportModel {
				YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<string, double>>(),
				MinDateDict = new SortedDictionary<string, int>(),
				MaxDateDict = new SortedDictionary<string, int>(),
			};
		}

		public void Add(YodleeTransactionModel transaction)
		{
			var amount = transaction.transactionAmount.HasValue ? transaction.transactionAmount.Value : 0;
			//var catType = transaction.transactionCategory.Type;
			var catName = string.IsNullOrEmpty(transaction.ezbobSubGroup) ? transaction.ezbobGroup : transaction.ezbobSubGroup;
			var isCredit = transaction.transactionBaseType == "credit";
			var date = transaction.transactionDate;
			var cat = string.Format("{0}{1}{2}", transaction.ezbobGroupPriority, Minor, catName);
			var major = string.Format("{0}{1}{2}", transaction.ezbobGroupPriority, Major, transaction.ezbobGroup);
			string yearmonth = (date.Year * 100 + date.Month).ToString();

			//var runningBalance = transaction.runningBalance.HasValue ? transaction.runningBalance.Value: 0;

			Add(cat, amount, isCredit, yearmonth);
			Add(major, amount, isCredit, yearmonth);
			UpdateMinMaxDay(yearmonth, date);
			Add(cat, amount, isCredit, TotalColumn);
			Add(major, amount, isCredit, TotalColumn);

			//Calc Total Row
			Add(isCredit
					? string.Format("{0}{1}", TotalCredit, TotalIncomeCat)
					: string.Format("{0}{1}", TotalDedit, TotalExpensesCat), amount, isCredit, yearmonth);
			Add(isCredit
					? string.Format("{0}{1}", TotalCredit, TotalIncomeCat)
					: string.Format("{0}{1}", TotalDedit, TotalExpensesCat), amount, isCredit, TotalColumn);

			//Calc Num Of Transactions Row
			Add(isCredit
					? string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat)
					: string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat), 1, true, yearmonth);
			Add(isCredit
					? string.Format("{0}{1}", NumTransCredit, NumOfTransactionsCat)
					: string.Format("{0}{1}", NumTransDedit, NumOfTransactionsCat), 1, true, TotalColumn);

			var monthAgo = DateTime.Today.AddMonths(-1);
			if (date >= monthAgo)
			{
				yodlee.MonthInPayments += amount;
			}
		}

		private void UpdateMinMaxDay(string yearmonth, DateTime? date)
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
			//CalculateOther(Credit, string.Format("{0}{1}", OtherCredit, OtherIncomeCat));
			//CalculateOther(Dedit, string.Format("{0}{1}", OtherDedit, OtherExpensesCat));
			//CalculateMajor();

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
		}


		private void AddIfMissing(string catPrefix, string cat)
		{
			if (!yodlee.YodleeCashFlowReportModelDict.ContainsKey(string.Format("{0}{1}", catPrefix, cat)))
			{
				var total = new SortedDictionary<string, double> { { TotalColumn, 0 } };
				yodlee.YodleeCashFlowReportModelDict[string.Format("{0}{1}", catPrefix, cat)] = total;
			}
		}

		private void CalculateNetCashFlow(string netCashFlowCat, string totalIncomeCat, string totalExpensesCat)
		{
			var totalIncomeRow = yodlee.YodleeCashFlowReportModelDict[totalIncomeCat];
			var totalExpensesRow = yodlee.YodleeCashFlowReportModelDict[totalExpensesCat];

			foreach (var yearmonth in totalIncomeRow.Keys)
			{
				Add(netCashFlowCat, totalIncomeRow[yearmonth] + totalExpensesRow[yearmonth], true, yearmonth);
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
					Add(averageCat, totalRow[yearmonth] / numOfTransRow[yearmonth], true, yearmonth);
				}
				else
				{
					Add(averageCat, 0, true, yearmonth);
				}
			}
		}

		private void CalculateOther(char baseType, string otherCat)
		{
			var otherList = new List<string>();
			//calculating other (less than 500 pound in totals (configurable)
			double maxYodleeOtherCategoryAmount = CurrentValues.Instance.MaxYodleeOtherCategoryAmount;

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
				Add(otherCat, 0, true, TotalColumn);
			}

			foreach (var other in otherList)
			{
				foreach (var cat in yodlee.YodleeCashFlowReportModelDict[other])
				{
					Sum(otherCat, cat.Key, cat.Value, true);
				}
			}

			foreach (var other in otherList)
			{
				yodlee.YodleeCashFlowReportModelDict.Remove(other);
			}
		}

		private void Add(string cat, double amount, bool isCredit, string yearmonth)
		{
			if (!yodlee.YodleeCashFlowReportModelDict.ContainsKey(cat))
			{
				yodlee.YodleeCashFlowReportModelDict[cat] = new SortedDictionary<string, double>();
			}

			Sum(cat, yearmonth, amount, isCredit);
		}

		private void Sum(string cat, string yearmonth, double amount, bool isCredit)
		{
			var x = yodlee.YodleeCashFlowReportModelDict[cat];
			if (!isCredit)
			{
				amount = -amount;
			}
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

		public BankStatementDataModel GetBankStatementDataModel()
		{
			var bankStatementDataModel = new BankStatementDataModel();
			if (yodlee.MinDateDict.Any() && yodlee.MinDateDict.Any())
			{
				var minDate = int.Parse(yodlee.MinDateDict.First().Key);
				var maxDate = int.Parse(yodlee.MaxDateDict.Last().Key);
				var from = new DateTime(minDate / 100, minDate % 100, yodlee.MinDateDict.Last().Value);
				var to = new DateTime(maxDate / 100, maxDate % 100, yodlee.MaxDateDict.Last().Value);
				bankStatementDataModel.PeriodMonthsNum = ((to.Year - from.Year) * 12) + to.Month - from.Month;
				bankStatementDataModel.Period = string.Format("{0} - {1}", from.ToString("MMM yy", CultureInfo.InvariantCulture), to.ToString("MMM yy", CultureInfo.InvariantCulture));
				bankStatementDataModel.Revenues = yodlee.YodleeCashFlowReportModelDict.ContainsKey("1aRevenues") ? yodlee.YodleeCashFlowReportModelDict["1aRevenues"][TotalColumn] : 0;
				bankStatementDataModel.Opex = yodlee.YodleeCashFlowReportModelDict.ContainsKey("2aOpex") ? yodlee.YodleeCashFlowReportModelDict["2aOpex"][TotalColumn] : 0;
				bankStatementDataModel.Salaries = yodlee.YodleeCashFlowReportModelDict.ContainsKey("3aSalaries and Tax") ? yodlee.YodleeCashFlowReportModelDict["3aSalaries and Tax"][TotalColumn] : 0;
				bankStatementDataModel.Tax = yodlee.YodleeCashFlowReportModelDict.ContainsKey("4aCorporate tax") ? yodlee.YodleeCashFlowReportModelDict["4aCorporate tax"][TotalColumn] : 0;
				bankStatementDataModel.ActualLoansRepayment = yodlee.YodleeCashFlowReportModelDict.ContainsKey("5aLoan Repayments") ? yodlee.YodleeCashFlowReportModelDict["5aLoan Repayments"][TotalColumn] : 0;
			}

			return bankStatementDataModel;
		}

		public BankStatementDataModel GetAnualizedBankStatementDataModel(BankStatementDataModel model)
		{
			var bankStatementDataModel = new BankStatementDataModel() {
				DateFrom = model.DateFrom,
				DateTo = model.DateTo,
				Period = model.Period,
				PeriodMonthsNum = model.PeriodMonthsNum
			};

			if (bankStatementDataModel.DateFrom.HasValue && bankStatementDataModel.DateTo.HasValue) {
				double annualMult = 365.0/(bankStatementDataModel.DateTo.Value - bankStatementDataModel.DateFrom.Value).TotalDays;
				bankStatementDataModel.Revenues = model.Revenues*annualMult;
				bankStatementDataModel.Opex = model.Opex*annualMult;
				bankStatementDataModel.Salaries = model.Salaries*annualMult;
				bankStatementDataModel.Tax = model.Tax*annualMult;
				bankStatementDataModel.ActualLoansRepayment = model.ActualLoansRepayment*annualMult;
				bankStatementDataModel.TotalValueAdded = bankStatementDataModel.Revenues - Math.Abs(bankStatementDataModel.Opex);
				bankStatementDataModel.Ebida = bankStatementDataModel.TotalValueAdded - Math.Abs(bankStatementDataModel.Salaries) -
				                               Math.Abs(bankStatementDataModel.Tax);
				bankStatementDataModel.FreeCashFlow = bankStatementDataModel.Ebida -
				                                      Math.Abs(bankStatementDataModel.ActualLoansRepayment);
			}
			return bankStatementDataModel;
		}
	}
}
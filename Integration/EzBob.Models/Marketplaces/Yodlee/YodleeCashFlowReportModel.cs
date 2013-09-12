namespace EzBob.Models.Marketplaces.Yodlee
{
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
		private const int Total = 999999;
		private const string OtherIncomeCat = "Other Income";
		private const string OtherExpensesCat = "Other Expenses";
		private const char Credit = '0';
		private const char OtherCredit = '1';
		private const char Dedit = '2';
		private const char OtherDedit = '3';
		private readonly CurrencyConvertor _currencyConvertor;
		private readonly IConfigurationVariablesRepository _configVariables;
		public YodleeCashFlowReportModel(ISession session)
		{
			YodleeCashFlowReportModelDict = new SortedDictionary<string, SortedDictionary<int, double>>();
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
			var date = transaction.postDate.HasValue ? transaction.postDate.Value : transaction.transactionDate.Value;
			var cat = string.Format("{1}{0}", catName, baseType == "credit" ? Credit : Dedit);
			var yearmonth = date.Year * 100 + date.Month;


			Add(cat, amount, yearmonth);
			Add(cat, amount, Total);

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
		}

		private void CalculateOther(char baseType, string otherCat)
		{
			var otherList = new List<string>();
			//calculating other (less than 500 pound in totals (configurable)
			var maxYodleeOtherCategoryAmount = double.Parse(_configVariables.GetByName("MaxYodleeOtherCategoryAmount").Value);

			foreach (var cat in YodleeCashFlowReportModelDict)
			{
				var x = YodleeCashFlowReportModelDict[cat.Key];
				if (x[Total] < maxYodleeOtherCategoryAmount && cat.Key[0] == baseType)
				{
					otherList.Add(cat.Key);
				}
			}

			if (otherList.Count > 0)
			{
				Add(otherCat, 0, Total);
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
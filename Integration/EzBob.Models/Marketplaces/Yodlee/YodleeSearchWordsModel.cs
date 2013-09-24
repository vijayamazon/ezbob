namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using NHibernate;
	using Scorto.NHibernate.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

	public class YodleeSearchWordsModel
	{
		public SortedDictionary<string/*word*/, SortedDictionary<string/*Income/Expense*/, double/*amount/Count*/>> YodleeSearchWordModelDict { get; set; }
		public Dictionary<int /*id*/, string /*word*/> WordsDict;
		private readonly CurrencyConvertor _currencyConvertor;
		private readonly List<string> _yodleeSearchWords;

		public YodleeSearchWordsModel(ISession session)
		{
			YodleeSearchWordModelDict = new SortedDictionary<string, SortedDictionary<string, double>>();
			WordsDict = new Dictionary<int, string>();
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(session));
			var words = new YodleeSearchWordsRepository(session).GetAll();

			foreach (var word in words)
			{
				WordsDict.Add(word.Id, word.SearchWords);
			}

			_yodleeSearchWords = words.Select(x => x.SearchWords).ToList();
		}


		public void Add(MP_YodleeOrderItemBankTransaction transaction)
		{
			foreach (var word in _yodleeSearchWords)
			{
				if (transaction.description.ToLowerInvariant().Contains(word))
				{
					var amount = transaction.transactionAmount.HasValue
							 ? _currencyConvertor.ConvertToBaseCurrency(
								 transaction.transactionAmountCurrency,
								 transaction.transactionAmount.Value,
								 transaction.postDate ?? transaction.transactionDate).Value
							 : 0;

					if (transaction.transactionBaseType == "credit")
					{
						Add(word, "Income", amount);
						Add(word, "Income #", 1);
					}
					else
					{
						Add(word, "Expense", amount); 
						Add(word, "Expense #", 1);
					}
				}
			}
		}


		public void AddMissing()
		{
			//retrieving type list
			var typeList = (from word in YodleeSearchWordModelDict from type in YodleeSearchWordModelDict[word.Key] select type.Key).ToList();

			//adding amount 0 for missing word/type
			foreach (var word in YodleeSearchWordModelDict)
			{
				foreach (var type in typeList)
				{
					if (!YodleeSearchWordModelDict[word.Key].ContainsKey(type))
					{
						YodleeSearchWordModelDict[word.Key][type] = 0;
					}
				}
			}
		}

		private void Add(string word, string type, double amount)
		{
			if (!YodleeSearchWordModelDict.ContainsKey(word))
			{
				YodleeSearchWordModelDict[word] = new SortedDictionary<string, double>();
			}

			Sum(word, type, amount);
		}

		private void Sum(string cat, string type, double amount)
		{
			var x = YodleeSearchWordModelDict[cat];

			if (!x.ContainsKey(type))
			{
				x[type] = amount;
			}
			else
			{
				x[type] += amount;
			}
		}
	}
}

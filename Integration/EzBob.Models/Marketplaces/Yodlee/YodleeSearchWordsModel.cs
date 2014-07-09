namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

	[Serializable]
	public class YodleeSearchWordsModel
	{
		public SortedDictionary<string /*word*/, SortedDictionary<string /*Income/Expense*/, double /*amount/Count*/>> YodleeSearchWordModelDict { get; set; }
		public SortedDictionary<string /*id*/, string /*word*/> WordsDict;
	}

	public class YodleeSearchWordsModelBuilder
	{

		private readonly List<string> _yodleeSearchWords;
		private const string CustomerSurname = "0";
		private const string DirectorSurname = "1";
		private const string SearchWord = "2";
		private YodleeSearchWordsModel yodlee;

		public YodleeSearchWordsModelBuilder(ISession session, Customer customer, IEnumerable<string> directors)
		{
			yodlee = new YodleeSearchWordsModel
				{
					YodleeSearchWordModelDict = new SortedDictionary<string, SortedDictionary<string, double>>(),
					WordsDict = new SortedDictionary<string, string>()
				};
			var words = new YodleeSearchWordsRepository(session).GetAll();

			foreach (var word in words)
			{
				yodlee.WordsDict.Add(word.Id.ToString(), word.SearchWords);
			}

			_yodleeSearchWords = words.Select(x => string.Format("{0}{1}", SearchWord, x.SearchWords)).ToList();

			//Add customer surname
			_yodleeSearchWords.Add(string.Format("{0}{1}", CustomerSurname, customer.PersonalInfo.Surname));

			foreach (var director in directors)
			{
				_yodleeSearchWords.Add(string.Format("{0}{1}", DirectorSurname, director));
			}
		}


		public void Add(YodleeTransactionModel transaction)
		{
			foreach (var word in _yodleeSearchWords)
			{
				if (!string.IsNullOrEmpty(transaction.description) && transaction.description.ToLowerInvariant().Contains(word.Substring(1).ToLowerInvariant()))
				{
					var amount = transaction.transactionAmount.HasValue ? transaction.transactionAmount.Value : 0;

					if (transaction.transactionBaseType == "credit")
					{
						Add(word, "Trans Income", amount);
						Add(word, "Trans Income #", 1);
					}
					else
					{
						Add(word, "Trans Expense", amount);
						Add(word, "Trans Expense #", 1);
					}
				}
			}
		}


		public void AddMissing()
		{
			//retrieving type list
			var typeList = (from word in yodlee.YodleeSearchWordModelDict from type in yodlee.YodleeSearchWordModelDict[word.Key] select type.Key).ToList();

			//adding amount 0 for missing word/type
			foreach (var word in yodlee.YodleeSearchWordModelDict)
			{
				foreach (var type in typeList)
				{
					if (!yodlee.YodleeSearchWordModelDict[word.Key].ContainsKey(type))
					{
						yodlee.YodleeSearchWordModelDict[word.Key][type] = 0;
					}
				}
			}
		}

		private void Add(string word, string type, double amount)
		{
			if (!yodlee.YodleeSearchWordModelDict.ContainsKey(word))
			{
				yodlee.YodleeSearchWordModelDict[word] = new SortedDictionary<string, double>();
			}

			Sum(word, type, amount);
		}

		private void Sum(string cat, string type, double amount)
		{
			var x = yodlee.YodleeSearchWordModelDict[cat];

			if (!x.ContainsKey(type))
			{
				x[type] = amount;
			}
			else
			{
				x[type] += amount;
			}
		}

		public YodleeSearchWordsModel GetModel()
		{
			return yodlee;
		}
	}
}

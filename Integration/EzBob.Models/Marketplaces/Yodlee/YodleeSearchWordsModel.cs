namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using NHibernate;
	using Scorto.NHibernate.Repository;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;

	public class YodleeSearchWordsModel
	{
		public SortedDictionary<string/*word*/, SortedDictionary<string/*Income/Expense*/, double/*amount/Count*/>> YodleeSearchWordModelDict { get; set; }
		public Dictionary<int /*id*/, string /*word*/> WordsDict;
		private readonly List<string> _yodleeSearchWords;
		private const string CustomerSurname = "0";
		private const string DirectorSurname = "1";
		private const string SearchWord = "2";

		public YodleeSearchWordsModel(ISession session, Customer customer)
		{
			YodleeSearchWordModelDict = new SortedDictionary<string, SortedDictionary<string, double>>();
			WordsDict = new Dictionary<int, string>();
			var words = new YodleeSearchWordsRepository(session).GetAll();

			foreach (var word in words)
			{
				WordsDict.Add(word.Id, word.SearchWords);
			}

			_yodleeSearchWords = words.Select(x => string.Format("{0}{1}", SearchWord, x.SearchWords)).ToList();

			//Add customer surname
			_yodleeSearchWords.Add(string.Format("{0}{1}", CustomerSurname, customer.PersonalInfo.Surname));

			//Add directors surnames
			var directors = new List<string>();
			switch (customer.PersonalInfo.TypeOfBusiness.Reduce())
			{
				case TypeOfBusinessReduced.Limited:
					if (customer.LimitedInfo != null && customer.LimitedInfo.Directors.Any())
					{
						directors = customer.LimitedInfo.Directors.Select(d => d.Surname).ToList();
					}
					break;
				case TypeOfBusinessReduced.NonLimited:
					if (customer.NonLimitedInfo != null && customer.NonLimitedInfo.Directors.Any())
					{
						directors = customer.NonLimitedInfo.Directors.Select(d => d.Surname).ToList();
					}
					break;
				case TypeOfBusinessReduced.Personal:
				default:
					break;
			}

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

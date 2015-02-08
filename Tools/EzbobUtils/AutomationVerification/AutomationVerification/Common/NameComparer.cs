namespace AutomationCalculator.Common {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using Ezbob.Database;

	public class NameComparer {
		public NameComparer(Name customerName, List<Name> directorNames, AConnection db) {
			diffMap = new SortedDictionary<string, SortedDictionary<string, StringDifference>>();
			Load(customerName, directorNames, db);
		} // constructor

		public StringDifference this[string alice, string boob] {
			get {
				NormalStringPair tpl = Normalize(alice, boob);

				if (tpl.AreEqual)
					return StringDifference.Equal;

				if (diffMap.ContainsKey(tpl.Alice)) {
					SortedDictionary<string, StringDifference> nameMap = this.diffMap[tpl.Alice];

					if (nameMap.ContainsKey(tpl.Boob))
						return nameMap[tpl.Boob];
				} // if

				return StringDifference.NotEqual;
			} // get
		} // indexer

		private void Load(Name customerName, List<Name> directorNames, AConnection db) {
			var firstNames = new SortedSet<string>();
			var lastNames = new SortedSet<string>();

			foreach (Name name in directorNames) {
				string s = Normalize(name.FirstName);

				if (s != string.Empty)
					firstNames.Add(s);

				s = Normalize(name.LastName);

				if (s != string.Empty)
					lastNames.Add(s);
			} // for each

			diffMap.Clear();

			if ((firstNames.Count == 0) && (lastNames.Count == 0))
				return;

			db.ForEachRowSafe(
				sr => {
					NormalStringPair tpl = Normalize(sr["Alice"], sr["Boob"]);

					if (tpl.AreEqual)
						return;

					int mark = sr["Mark"];

					StringDifference diff = Enum.IsDefined(typeof(StringDifference), mark)
						? (StringDifference)mark
						: StringDifference.NotEqual;

					SortedDictionary<string, StringDifference> nameMap = this.diffMap.ContainsKey(tpl.Alice)
						? this.diffMap[tpl.Alice]
						: null;

					if (nameMap == null)
						this.diffMap[tpl.Alice] = new SortedDictionary<string, StringDifference> { { tpl.Boob, diff } };
					else
						nameMap[tpl.Boob] = diff;
				},
				"GetNameDifference",
				CommandSpecies.StoredProcedure,
				new QueryParameter("FirstName", Normalize(customerName.FirstName)),
				new QueryParameter("LastName", Normalize(customerName.LastName)),
				db.CreateTableParameter<string>("FirstNames", firstNames),
				db.CreateTableParameter<string>("LastNames", lastNames)
			);
		} // Load

		private static string Normalize(string s) {
			if (string.IsNullOrWhiteSpace(s))
				return string.Empty;

			return s.Trim().ToLowerInvariant();
		} // Normalize

		private static NormalStringPair Normalize(string alice, string boob) {
			alice = Normalize(alice);
			boob = Normalize(boob);

			if (string.Compare(alice, boob, StringComparison.InvariantCulture) > 0) {
				string t = alice;
				alice = boob;
				boob = t;
			} // if

			return new NormalStringPair(alice, boob);
		} // Normalize

		private class NormalStringPair {
			public NormalStringPair(string alice, string boob) {
				Alice = alice;
				Boob = boob;
			} // constructor

			public string Alice { get; private set; }
			public string Boob { get; private set; }

			public bool AreEqual {
				get { return Alice == Boob; }
			} // AreEqual
		} // NormalStringPair

		private readonly SortedDictionary<string, SortedDictionary<string, StringDifference>> diffMap;
	} // class NameComparer
} // namespace

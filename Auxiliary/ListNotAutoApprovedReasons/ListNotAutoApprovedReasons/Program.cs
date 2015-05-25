namespace ListNotAutoApprovedReasons {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	class Program {
		static void Main(string[] args) {
			string[] lines = File.ReadAllLines("not-auto-approved.csv");

			var trails = new SortedDictionary<long, Trail>();

			var relevant = lines
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.Where(s => !s.StartsWith("#"));

			foreach (string line in relevant) {
				string[] tokens = line.Split(',');

				if (tokens.Length != 3)
					continue;

				if (string.IsNullOrWhiteSpace(tokens[2]))
					continue;

				long id = 0;

				try {
					id = Convert.ToInt64(tokens[0]);
				} catch {
					// ignored
				} // try

				if (id <= 0)
					continue;

				string name = tokens[2].Substring(tokens[2].LastIndexOf(".") + 1);

				if (trails.ContainsKey(id))
					trails[id].Reasons.Add(name);
				else
					trails[id] = new Trail(id, name);
			} // for each input line

			var keys = new SortedDictionary<NonAffirmativeGroupKey, List<long>>();

			foreach (Trail trail in trails.Values) {
				if (keys.ContainsKey(trail.Key))
					keys[trail.Key].Add(trail.ID);
				else
					keys[trail.Key] = new List<long> { trail.ID, };
			} // for each trail

			foreach (var pair in keys)
				Console.WriteLine("{0};{1};{2}", pair.Value.Count, pair.Key.List, string.Join(", ", pair.Value));
		} // Main
	} // class Program
} // namespace

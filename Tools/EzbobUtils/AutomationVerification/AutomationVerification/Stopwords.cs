namespace AutomationCalculator {
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;

	internal class Stopwords {
		static Stopwords() {
			locker = new object();
			words = new SortedSet<string>();

			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AutomationCalculator.stopwords.txt");

			if (stream != null) {
				var reader = new StreamReader(stream);
				for ( ; ; ) {
					string line = reader.ReadLine();

					if (line == null)
						break;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					words.Add(line.Trim());
				} // for
			} // if assembly

		} // static constructor

		public bool Contains(string word) {
			if (string.IsNullOrWhiteSpace(word))
				return false;

			lock (locker)
				return words.Contains(word.Trim());
		} // Contains

		private static readonly SortedSet<string> words;
		private static readonly object locker;
	} // class Stopwords
} // namespace

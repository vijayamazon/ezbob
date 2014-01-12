namespace Ezbob.ExperianParser {
	using System.Collections.Generic;

	#region class ParsedDataItem

	public class ParsedDataItem {
		#region public

		#region constructor

		public ParsedDataItem() {
			Values = new SortedDictionary<string, string>();
			Children = new SortedDictionary<string, ParsedData>();
		} // constructor

		#endregion constructor

		public SortedDictionary<string, string> Values { get; private set; }
		public SortedDictionary<string, ParsedData> Children { get; private set; }

		public string this[string idx] {
			get { return Values[idx]; }
			set { Values[idx] = value; }
		} // indexer

		public bool Contains(string idx) {
			return Values.ContainsKey(idx);
		} // Contains

		public bool ContainsKey(string idx) {
			return Values.ContainsKey(idx);
		} // ContainsKey

		#endregion public
	} // class ParsedDataItem

	#endregion class ParsedDataItem
} // namespace Ezbob.ExperianParser

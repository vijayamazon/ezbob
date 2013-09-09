using System.Collections.Generic;

namespace Ezbob.ExperianParser {
	#region class ParsedData

	public class ParsedData {
		#region public

		#region constructor

		public ParsedData() {
			Data = new List<SortedDictionary<string, string>>();
			MetaData = new SortedDictionary<string, string>();
		} // constructor

		#endregion constructor

		#region property GroupName

		public string GroupName { get; set; }

		#endregion property GroupName

		#region property MetaData

		public SortedDictionary<string, string> MetaData { get; private set; }

		#endregion property MetaData

		#region property Data

		public List<SortedDictionary<string, string>> Data { get; private set; }

		#endregion property Data

		#endregion public
	} // class ParsedData

	#endregion class ParsedData
} // namespace Ezbob.ExperianParser

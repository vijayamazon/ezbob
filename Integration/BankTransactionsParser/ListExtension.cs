
using System.Collections.Generic;

namespace BankTransactionsParser {
	public static class ListExtension {

		public static string ToDelimitedString(this List<string> list, string separator = ",", bool insertSpaces = false) {
			var result = string.Empty;
			for (int i = 0; i < list.Count; i++) {
				var currentString = list[i];
				if (i < list.Count - 1) {
					currentString += separator;
					if (insertSpaces) {
						currentString += ' ';
					}
				}
				result += currentString;
			}
			return result;
		}
	}
}

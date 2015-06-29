namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public static class ValueReaderExt {
		public static IEnumerable<string> GetValuesForLog<T>(this T mr, params string[] names) where T: class {
			if ((mr == null) || (names.Length < 1))
				yield break;

			foreach (string name in names) {
				if (string.IsNullOrWhiteSpace(name))
					continue;

				PropertyInfo pi = typeof(T).GetProperty(name);

				if (pi == null)
					continue;

				yield return string.Format("'{0}' = '{1}'", name, pi.GetValue(mr));
			} // for each name
		} // GetValuesForLog

		public static IEnumerable<string> GetValuesByMaskForLog<T>(this T mr, string mask) where T: class {
			if (mr == null)
				yield break;

			if (string.IsNullOrWhiteSpace(mask))
				mask = null;

			IEnumerable<PropertyInfo> lst = typeof(T)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetGetMethod() != null);

			foreach (var pi in lst) {
				if ((mask == null) || (pi.Name.IndexOf(mask, StringComparison.InvariantCultureIgnoreCase) > -1))
					yield return string.Format("'{0}' = '{1}'", pi.Name, pi.GetValue(mr));
			} // for each name
		} // GetValuesForLog
	} // class ValueReaderExt
} // namespace

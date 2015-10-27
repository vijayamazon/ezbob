namespace EzBob.Web {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Web;
	using System.Web.Mvc;
	using Ezbob.Utils.Extensions;

	public static class EzbobHtmlExtensions {
		public static MvcHtmlString EnumToOptions<T>(this HtmlHelper helper, params T[] args) where T : struct, IComparable, IFormattable, IConvertible {
			 if (!typeof(T).IsEnum) 
				throw new ArgumentException("T must be an enumerated type");

			var os = new StringBuilder();

			var oOpts = new List<T>(args);

			if (oOpts.Count < 1)
				oOpts.AddRange((T[])Enum.GetValues(typeof(T)));

			foreach (T nVal in oOpts) {
				os.AppendFormat(
					"<option value=\"{0}\">{1}</option>",
					Convert.ToInt32(nVal),
					new HtmlString(nVal.DescriptionAttr())
				);
			} // for each

			return MvcHtmlString.Create(os.ToString());
		} // EnumToOptions

		public static List<Tuple<string, string>> EnumToOptionsTuple<T>(this HtmlHelper helper, params T[] args) where T : struct, IComparable, IFormattable, IConvertible {
			if (!typeof(T).IsEnum)
				throw new ArgumentException("T must be an enumerated type");

			var oOpts = new List<T>(args);

			if (oOpts.Count < 1)
				oOpts.AddRange((T[])Enum.GetValues(typeof(T)));

			var list = new List<Tuple<string, string>>();
			foreach (T nVal in oOpts) {
				list.Add(new Tuple<string, string>(Convert.ToInt32(nVal)
					.ToString(), nVal.DescriptionAttr()));
			} // for each

			return list;
		} // EnumToOptionsTuple
	} // class EzbobHtmlExtensions
} // namespace EzBob.Web

namespace EchoSignLib.Ext {
	using System.Collections.Generic;

	internal static class ListAddressExt {
		public static List<string> Append(this List<string> lst, string s) {
			if (lst == null)
				lst = new List<string>();

			if (!string.IsNullOrWhiteSpace(s))
				lst.Add(s.Trim());

			return lst;
		} // Append
	} // ListAddressExt
} // namespace

namespace EchoSignLib {
	using System.Collections.Generic;
	using Ext;

	internal class Address {
		public string Line1    { get; set; }
		public string Line2    { get; set; }
		public string Line3    { get; set; }
		public string Town     { get; set; }
		public string County   { get; set; }
		public string Postcode { get; set; }
		public string Country  { get; set; }

		public string FullAddress {
			get {
				return string.Join(" ", ((List<string>)null)
					.Append(Line1)
					.Append(Line2)
					.Append(Line3)
					.Append(Town)
					.Append(County)
					.Append(Postcode)
					.Append(Country)
				);
			} // get
		} // FullAddress
	} // class Address
} // namespace

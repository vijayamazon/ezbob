﻿using System.Collections.Generic;

namespace Html.Attributes {
	public class ID : AAttribute { protected override string Name { get { return "id"; } } }

	public class Class : AAttribute {
		public Class() {
			RawValues = new SortedSet<string>();
		} // constructor

		protected override void OnAppend(string sValue) {
			RawValues.Add("." + sValue);
		} // OnAppend

		protected override string Name { get { return "class"; } }

		public SortedSet<string> RawValues { get; private set; }
	} // Style

	public class Src : AAttribute { protected override string Name { get { return "src"; } } }

	public class Alt : AAttribute { protected override string Name { get { return "alt"; } } }

	public class Title : AAttribute { protected override string Name { get { return "title"; } } }

	public class Href : AAttribute { protected override string Name { get { return "href"; } } }

	public class Style : AAttribute { protected override string Name { get { return "style"; } } }
} // namespace Html.Attributes

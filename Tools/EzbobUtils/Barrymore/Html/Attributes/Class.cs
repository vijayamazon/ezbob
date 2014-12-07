namespace Ezbob.Utils.Html.Attributes {
	using System.Collections.Generic;

	public class Class : AAttribute {
		public Class() {
			RawValues = new SortedSet<string>();
		} // constructor

		public override AAttribute Append(string sValue) {
			sValue = (sValue ?? "").Trim();

			string[] ary = sValue.Split(' ');

			foreach (string sOneValue in ary) {
				if (sOneValue != string.Empty) {
					Values.Add(System.Web.HttpUtility.HtmlEncode(sOneValue));

					OnAppend(sOneValue);
				} // if
			} // foreach

			return this;
		} // Append

		protected override void OnAppend(string sValue) {
			RawValues.Add("." + sValue);
		} // OnAppend

		protected override string Name { get { return "class"; } }

		public SortedSet<string> RawValues { get; private set; }
	} // class Class
} // namespace

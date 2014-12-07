namespace Ezbob.Utils.Html.Tags {
	public class Style : Text {
		public Style(string sContent = null) : base(sContent) {}

		public override string Tag { get { return "style"; } }

		public override string ToString() {
			return "<" + Tag + ">" + base.ToString() + "</" + Tag + ">";
		} // ToString
	} // class Style
} // namespace

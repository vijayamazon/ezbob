namespace Ezbob.Utils.Html.Tags {
	using System;
	using System.Text;

	public class Text : ATag {
		public Text(string sContent = null) {
			Content = new StringBuilder();

			if (sContent != null)
				Append(sContent);
		} // constructor

		public override ATag Append(ATag oTag) {
			throw new NotImplementedException();
		} // Append
		
		public virtual ATag Append(string sContent) {
			Content.Append(sContent ?? "--null--");
			return this;
		} // Append

		public override string ToString() {
			return Content.ToString();
		} // ToString

		public override string Tag { get { return ""; } }

		private StringBuilder Content { get; set; }
	} // class Text
} // namespace

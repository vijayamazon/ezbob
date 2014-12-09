namespace Ezbob.Utils.Html {
	using System.Collections.Generic;

	public abstract class AAttribute {

		public virtual AAttribute Append(string sValue) {
			sValue = (sValue ?? "").Trim();

			if (sValue != string.Empty) {
				Values.Add(System.Web.HttpUtility.HtmlEncode(sValue));

				OnAppend(sValue);
			} // if

			return this;
		} // Append

		public override string ToString() {
			return Values.Count == 0 ? "" : " " + Name + "=\"" + string.Join(" ", Values.ToArray()) + "\"";
		} // ToString

		protected AAttribute() {
			Values = new List<string>();
		} // constructor

		protected virtual void OnAppend(string sValue) {
			// nothing here, for children classes
		} // OnAppend

		protected abstract string Name { get; }

		protected virtual List<string> Values { get; private set; }

	} // class AAttribute
} // namespace Html

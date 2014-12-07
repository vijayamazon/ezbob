namespace Ezbob.Utils.Html {
	using System.Collections.Generic;

	public abstract class AAttribute {
		#region public

		#region method Append

		public virtual AAttribute Append(string sValue) {
			sValue = (sValue ?? "").Trim();

			if (sValue != string.Empty) {
				Values.Add(System.Web.HttpUtility.HtmlEncode(sValue));

				OnAppend(sValue);
			} // if

			return this;
		} // Append

		#endregion method Append

		#region method ToString

		public override string ToString() {
			return Values.Count == 0 ? "" : " " + Name + "=\"" + string.Join(" ", Values.ToArray()) + "\"";
		} // ToString

		#endregion method ToString

		#endregion public

		#region protected

		#region constructor

		protected AAttribute() {
			Values = new List<string>();
		} // constructor

		#endregion constructor

		#region method OnAppend

		protected virtual void OnAppend(string sValue) {
			// nothing here, for children classes
		} // OnAppend

		#endregion method OnAppend

		#region property Name

		protected abstract string Name { get; }

		#endregion property Name

		#region property Values

		protected virtual List<string> Values { get; private set; }

		#endregion property Values

		#endregion protected
	} // class AAttribute
} // namespace Html

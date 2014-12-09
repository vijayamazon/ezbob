namespace Ezbob.Utils.Html {
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using Ezbob.Utils.Html.Attributes;

	public abstract class ATag {

		public abstract string Tag { get; }

		public virtual bool MustClose { get { return true; } }

		public virtual ATag Add<T>(string sValue) where T : AAttribute {
			PropertyInfo[] aryProperties = this.GetType().GetProperties();

			foreach (var prop in aryProperties) {
				if (prop.PropertyType == typeof (T)) {
					MethodInfo oMethod = typeof (T).GetMethod("Append");
					oMethod.Invoke(prop.GetGetMethod().Invoke(this, null), new object[] { sValue });
				} // if
			} // for each property

			return this;
		} // Add

		public virtual ATag ApplyToChildren<T>(string sValue) where T : AAttribute {
			foreach (var child in Children) {
				child.Add<T>(sValue);
				child.ApplyToChildren<T>(sValue);
			} // foreach

			return this;
		} // ApplyToChildren

		public virtual ATag Append(ATag oTag) {
			if (oTag != null)
				Children.Add(oTag);

			return this;
		} // Append

		public ATag MoveCssInline(Dictionary<string, string> oStyles) {
			foreach (string sClass in this.Class.RawValues)
				if (oStyles.ContainsKey(sClass))
					this.Style.Append(oStyles[sClass]);

			foreach (var child in Children)
				child.MoveCssInline(oStyles);

			return this;
		} // MoveCssInline

		public virtual ID ID { get; private set; }

		public virtual Class Class { get; private set; }

		public virtual Style Style { get; private set; }

		public override string ToString() {
			var sb = new StringBuilder();

			sb.Append("<" + Tag);

			PropertyInfo[] aryProperties = this.GetType().GetProperties();

			foreach (var prop in aryProperties) {
				if (typeof (AAttribute).IsAssignableFrom(prop.PropertyType)) {
					MethodInfo oToStr = prop.PropertyType.GetMethod("ToString");

					sb.Append(
						oToStr.Invoke(prop.GetGetMethod().Invoke(this, null), null)
					);
				} // if
			} // for each property

			sb.Append(">");

			foreach (var child in Children)
				sb.Append(child);

			if (MustClose)
				sb.Append("</" + Tag + ">");

			sb.Append("\n");

			return sb.ToString();
		} // ToString

		protected ATag() {
			Children = new List<ATag>();
			ID = new ID();
			Class = new Class();
			Style = new Style();
		} // constructor

		protected virtual List<ATag> Children { get; private set; }

	} // class ATag
} // namespace

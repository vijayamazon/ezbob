namespace Ezbob.Utils.Html {
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;
	using Ezbob.Utils.Html.Attributes;

	public abstract class ATag {
		#region public

		public abstract string Tag { get; }

		#region property MustClose

		public virtual bool MustClose { get { return true; } }

		#endregion property MustClose

		#region method Add

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

		#endregion method Add

		#region method ApplyToChildren

		public virtual ATag ApplyToChildren<T>(string sValue) where T : AAttribute {
			foreach (var child in Children) {
				child.Add<T>(sValue);
				child.ApplyToChildren<T>(sValue);
			} // foreach

			return this;
		} // ApplyToChildren

		#endregion method ApplyToChildren

		#region method Append

		public virtual ATag Append(ATag oTag) {
			if (oTag != null)
				Children.Add(oTag);

			return this;
		} // Append

		#endregion method Append

		#region method MoveCssInline

		public ATag MoveCssInline(Dictionary<string, string> oStyles) {
			foreach (string sClass in this.Class.RawValues)
				if (oStyles.ContainsKey(sClass))
					this.Style.Append(oStyles[sClass]);

			foreach (var child in Children)
				child.MoveCssInline(oStyles);

			return this;
		} // MoveCssInline

		#endregion method MoveCssInline

		#region property ID

		public virtual ID ID { get; private set; }

		#endregion property ID

		#region property Class

		public virtual Class Class { get; private set; }

		#endregion property Class

		#region property Style

		public virtual Style Style { get; private set; }

		#endregion property Style

		#region method ToString

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

		#endregion method ToString

		#endregion public

		#region protected

		#region constructor

		protected ATag() {
			Children = new List<ATag>();
			ID = new ID();
			Class = new Class();
			Style = new Style();
		} // constructor

		#endregion constructor

		#region property Children

		protected virtual List<ATag> Children { get; private set; }

		#endregion property Children

		#endregion protected
	} // class ATag
} // namespace

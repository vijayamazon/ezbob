using System;

namespace Integration.ChannelGrabberConfig {
	#region class ValidationMessage

	public class Stylesheet : ICloneable {
		#region constructor

		public Stylesheet() {
			Property = "";
			Value = "";
		} // constructor

		#endregion constructor

		#region properties

		public string Property { get; set; }
		public string Value { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			Property = (Property ?? "").Trim();
			Value = (Value ?? "").Trim();

			if (Property == string.Empty)
				throw new ConfigException("Stylesheet property name not specified.");

			if (Value == string.Empty)
				throw new ConfigException("Stylesheet property value not specified.");
		} // Validate

		#endregion method Validate

		#region method ToString

		public override string ToString() {
			return string.Format("{0}: {1};", Property, Value);
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			return new Stylesheet {
				Property = (string)this.Property.Clone(),
				Value = (string)this.Value.Clone(),
			};
		} // Clone

		#endregion method Clone

	} // class Stylesheet

	#endregion class Stylesheet
} // namespace Integration.ChannelGrabberConfig

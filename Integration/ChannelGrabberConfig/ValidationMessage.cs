using System;

namespace Integration.ChannelGrabberConfig {
	#region class ValidationMessage

	public class ValidationMessage : ICloneable {
		#region constructor

		public ValidationMessage() {
			PropertyName = "";
			Message = "";
		} // constructor

		#endregion constructor

		#region properties

		public string PropertyName { get; set; }
		public string Message { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			PropertyName = (PropertyName ?? "").Trim();
			Message = (Message ?? "").Trim();

			if (PropertyName == string.Empty)
				throw new ConfigException("Validation property name not specified.");

			if (Message == string.Empty)
				throw new ConfigException("Validation message not specified.");
		} // Validate

		#endregion method Validate

		#region method ToString

		public override string ToString() {
			return string.Format("{0}: {1}", PropertyName, Message);
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			return new ValidationMessage {
				PropertyName = (string)this.PropertyName.Clone(),
				Message = (string)(this.Message ?? "").Clone(),
			};
		} // Clone

		#endregion method Clone

	} // class ValidationMessage

	#endregion class ValidationMessage
} // namespace Integration.ChannelGrabberConfig

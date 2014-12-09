using System;

namespace Integration.ChannelGrabberConfig {

	public class ValidationMessage : ICloneable {

		public ValidationMessage() {
			PropertyName = "";
			Message = "";
		} // constructor

		public string PropertyName { get; set; }
		public string Message { get; set; }

		public void Validate() {
			PropertyName = (PropertyName ?? "").Trim();
			Message = (Message ?? "").Trim();

			if (PropertyName == string.Empty)
				throw new ConfigException("Validation property name not specified.");

			if (Message == string.Empty)
				throw new ConfigException("Validation message not specified.");
		} // Validate

		public override string ToString() {
			return string.Format("{0}: {1}", PropertyName, Message);
		} // ToString

		public object Clone() {
			return new ValidationMessage {
				PropertyName = (string)this.PropertyName.Clone(),
				Message = (string)(this.Message ?? "").Clone(),
			};
		} // Clone

	} // class ValidationMessage

} // namespace Integration.ChannelGrabberConfig

using System;

namespace Integration.ChannelGrabberConfig {

	public class ErrorMessage : ICloneable {

		public ErrorMessage() {
			ID = "";
			Text = "";
		} // constructor

		public string ID { get; set; }
		public string Text { get; set; }

		public void Validate() {
			ID = (ID ?? string.Empty).Trim();
			Text = (Text ?? string.Empty).Trim();

			if ((ID == string.Empty) || (Text == string.Empty))
				throw new ConfigException("Error message is not well configured.");
		} // Validate

		public override string ToString() {
			return string.Format("{0}: {1}", ID, Text);
		} // ToString

		public object Clone() {
			return new ErrorMessage {
				ID = (string)this.ID.Clone(),
				Text = (string)this.Text.Clone()
			};
		} // Clone

	} // class ErrorMessage

} // namespace Integration.ChannelGrabberConfig

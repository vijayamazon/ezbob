using System;

namespace Integration.ChannelGrabberConfig {
	#region class ErrorMessage

	public class ErrorMessage : ICloneable {
		#region public

		#region constructor

		public ErrorMessage() {
			ID = "";
			Text = "";
		} // constructor

		#endregion constructor

		#region properties

		public string ID { get; set; }
		public string Text { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			ID = (ID ?? string.Empty).Trim();
			Text = (Text ?? string.Empty).Trim();

			if ((ID == string.Empty) || (Text == string.Empty))
				throw new ConfigException("Error message is not well configured.");
		} // Validate

		#endregion method Validate

		#region method ToString

		public override string ToString() {
			return string.Format("{0}: {1}", ID, Text);
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			return new ErrorMessage {
				ID = (string)this.ID.Clone(),
				Text = (string)this.Text.Clone()
			};
		} // Clone

		#endregion method Clone

		#endregion public
	} // class ErrorMessage

	#endregion class ErrorMessage
} // namespace Integration.ChannelGrabberConfig

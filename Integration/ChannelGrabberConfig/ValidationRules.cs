using System;

namespace Integration.ChannelGrabberConfig {

	public class ValidationRules : ICloneable {

		public ValidationRules() {
			required = false;
			url = false;
			minlength = 0;
			maxlength = 0;
		} // constructor

		public bool required { get; set; }
		public bool url { get; set; }
		public int minlength { get; set; }
		public int maxlength { get; set; }

		public void Validate() {
			if (minlength < 0)
				throw new ConfigException("Invalid minlength value.", isWarn: true);

			if (maxlength < minlength)
				throw new ConfigException("maxlength is less than minlength.", isWarn: true);
		} // Validate

		public override string ToString() {
			return string.Format(
				"( required: {0}, url: {1}, {2} <= length <= {3} )",
				required ? "yes" : "no",
				url ? "yes" : "no",
				minlength, maxlength
			);
		} // ToString

		public object Clone() {
			return new ValidationRules {
				required = this.required,
				url = this.url,
				minlength = this.minlength,
				maxlength = this.maxlength
			};
		} // Clone

	} // class ValidationRules

} // namespace Integration.ChannelGrabberConfig

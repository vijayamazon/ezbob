using System;

namespace Integration.ChannelGrabberConfig {
	#region class ValidationRules

	public class ValidationRules : ICloneable {
		#region public

		#region constructor

		public ValidationRules() {
			required = false;
			url = false;
			minlength = 0;
			maxlength = 0;
		} // constructor

		#endregion constructor

		#region properties

		public bool required { get; set; }
		public bool url { get; set; }
		public int minlength { get; set; }
		public int maxlength { get; set; }

		#endregion properties

		#region method Validate

		public void Validate() {
			if (minlength < 0)
				throw new ConfigException("Invalid minlength value.");

			if (maxlength < minlength)
				throw new ConfigException("maxlength is less than minlength.");
		} // Validate

		#endregion method Validate

		#region method ToString

		public override string ToString() {
			return string.Format(
				"( required: {0}, url: {1}, {2} <= length <= {3} )",
				required ? "yes" : "no",
				url ? "yes" : "no",
				minlength, maxlength
			);
		} // ToString

		#endregion method ToString

		#region method Clone

		public object Clone() {
			return new ValidationRules {
				required = this.required,
				url = this.url,
				minlength = this.minlength,
				maxlength = this.maxlength
			};
		} // Clone

		#endregion method Clone

		#endregion public
	} // class ValidationRules

	#endregion class ValidationRules
} // namespace Integration.ChannelGrabberConfig

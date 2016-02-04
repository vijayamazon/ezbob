namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class Warning : ICanBeEmpty<Warning> {
		[DataMember]
		public string Value { get; set; }

		[DataMember]
		public string FeatureName { get; set; }

		[DataMember]
		public string MinValue { get; set; }

		[DataMember]
		public string MaxValue { get; set; }

		public bool IsEmpty {
			get { return string.IsNullOrWhiteSpace(FeatureName); }
		} // IsEmpty

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format("{0}: '{1}' (allowed: '{2}', '{3}')", FeatureName, Value, MinValue, MaxValue);
		} // ToString

		public Warning CloneTo() {
			return new Warning {
				Value = Value,
				FeatureName = FeatureName,
				MinValue = MinValue,
				MaxValue = MaxValue,
			};
		} // CloneFrom
	} // class Warning
} // namespace

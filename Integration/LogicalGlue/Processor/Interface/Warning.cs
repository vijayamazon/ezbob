namespace Ezbob.Integration.LogicalGlue.Processor.Interface {
	using System.Runtime.Serialization;

	[DataContract]
	public class Warning : ICanBeEmpty {
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
	} // class Warning

	public static class WarningExt {
		public static Warning CloneFrom(this Warning target, Warning source) {
			if (source == null)
				return new Warning();

			target = target ?? new Warning();

			target.Value = source.Value;
			target.FeatureName = source.FeatureName;
			target.MaxValue = source.MinValue;
			target.MaxValue = source.MaxValue;

			return target;
		} // CloneFrom
	} // class WarningExt
} // namespace

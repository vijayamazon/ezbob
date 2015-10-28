namespace Ezbob.Integration.LogicalGlue.Models {
	using Ezbob.Integration.LogicalGlue.Interface;

	public class Warning : IWarning {
		public string Value { get; set; }
		public string FeatureName { get; set; }
		public string MinValue { get; set; }
		public string MaxValue { get; set; }

		public bool IsEmpty {
			get { return string.IsNullOrWhiteSpace(FeatureName); }
		} // IsEmpty
	} // class Warning

	public static class WarningExt {
		public static Warning CloneFrom(this Warning target, IWarning source) {
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

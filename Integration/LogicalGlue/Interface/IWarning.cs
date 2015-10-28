namespace Ezbob.Integration.LogicalGlue.Interface {
	public interface IWarning : ICanBeEmpty {
		string Value { get; set; }
		string FeatureName { get; set; }
		string MinValue { get; set; }
		string MaxValue { get; set; }
	} // interface IWarning
} // namespace

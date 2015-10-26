namespace Ezbob.Integration.LogicalGlue.Interface {
	using System.ComponentModel;

	public enum RequestType {
		[Description("Fuzzy logic")]
		FuzzyLogic = 1,

		[Description("Neural network")]
		NeuralNetwork = 2,
	} // enum RequestType
} // namespace

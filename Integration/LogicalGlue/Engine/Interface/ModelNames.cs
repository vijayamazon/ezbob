namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract]
	public enum ModelNames : long {
		[EnumMember]
		[Description("Fuzzy logic")]
		FuzzyLogic = 1,

		[EnumMember]
		[Description("Neural network")]
		NeuralNetwork = 2,

		[EnumMember]
		[Description("Logistic regression")]
		LogisticRegression = 3,
	} // enum ModelNames
} // namespace

namespace Ezbob.Integration.LogicalGlue.Processor.Interface {
	using System.ComponentModel;
	using System.Runtime.Serialization;

	[DataContract]
	public enum RequestType : long {
		[EnumMember]
		[Description("Fuzzy logic")]
		FuzzyLogic = 1,

		[EnumMember]
		[Description("Neural network")]
		NeuralNetwork = 2,
	} // enum RequestType
} // namespace

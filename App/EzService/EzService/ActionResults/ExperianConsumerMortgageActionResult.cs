namespace EzService {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.Experian;

	[DataContract]
	public class ExperianConsumerMortgageActionResult : ActionResult
	{
		[DataMember]
		public ExperianConsumerMortgagesData Value { get; set; }
	} // class ExperianLtdActionResult
} // namespace

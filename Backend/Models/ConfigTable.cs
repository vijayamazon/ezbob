namespace EzBob.Backend.Models {
	using System.Runtime.Serialization;
	using Ezbob.Utils;

	[DataContract(IsReference = true)]
	public class ConfigTable {
		[DataMember]
		[NonTraversable]
		public int Id { get; set; }

		[DataMember]
		public int Start { get; set; }

		[DataMember]
		public int End { get; set; }

		[DataMember]
		public decimal Value { get; set; }
	}
}
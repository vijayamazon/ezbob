namespace Ezbob.Backend.Models {
	using System;
	using System.Runtime.Serialization;
	using Utils;

	[DataContract(IsReference = true)]
	public class VatReturnPeriod : ITraversable {
		[DataMember]
		public DateTime DateFrom { get; set; }

		[DataMember]
		public DateTime DateTo { get; set; }

		[DataMember]
		public long RegistrationNo { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public int SourceID { get; set; }
	} // class VatReturnPeriod
} // namespace Ezbob.Backend.Models

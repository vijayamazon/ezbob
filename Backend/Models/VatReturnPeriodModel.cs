namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;

	[DataContract(IsReference = true)]
	public class VatReturnPeriod {
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

		[DataMember]
		public Guid InternalID { get; set; }

		public override string ToString() {
			return string.Format(
				"{0} - {1} {2} {3} {4} {5}",
				DateFrom.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateTo.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				RegistrationNo,
				Name,
				SourceID,
				InternalID
			);
		} // ToString
	} // class VatReturnPeriod
} // namespace Ezbob.Backend.Models

namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class AccountsToUpdate {
		public AccountsToUpdate() {
			IsVatReturnUpToDate = false;
			HasYodlee = false;
			Ekms = new SortedDictionary<string, string>();
			LinkedHmrc = new SortedSet<string>();
		} // constructor

		[DataMember]
		public bool HasYodlee { get; set; }

		[DataMember]
		public SortedDictionary<string, string> Ekms { get; set; }

		[DataMember]
		public SortedSet<string> LinkedHmrc { get; set; }

		[DataMember]
		public bool IsVatReturnUpToDate { get; set; }

		[DataMember]
		public bool HasUploadedHmrc { get; set; }
	} // class AccountsToUpdate
} // namespace

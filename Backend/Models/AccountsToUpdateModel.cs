namespace Ezbob.Backend.Models {
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public class AccountsToUpdateModel {
		public AccountsToUpdateModel() {
			HasYodlee = false;
			Ekms = new Dictionary<string, string>();
			LinkedHmrc = new List<string>();
		} // constructor

		[DataMember]
		public bool HasYodlee { get; set; }

		[DataMember]
		public Dictionary<string, string> Ekms { get; set; }

		[DataMember]
		public List<string> LinkedHmrc { get; set; }
	} // class AccountsToUpdateModel
} // namespace

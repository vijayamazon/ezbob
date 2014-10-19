namespace MailApi.Model {
	public class EmailAddressModel {
		// ReSharper disable InconsistentNaming
		public string email { get; set; }
		public string name { get; set; }
		// ReSharper enable InconsistentNaming

		public override string ToString() {
			return string.Format("{0} <{1}>", name ?? "-- null --", email ?? "-- null --");
		} // ToString
	} // class EmailAddressModel
} // namespace

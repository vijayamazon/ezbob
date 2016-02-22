namespace EZBob.DatabaseLib.Model.Database.Loans {
	using System.Runtime.Serialization;

	[DataContract]
	public enum LoanSourceName {
		[EnumMember]
		Standard = 1,

		[EnumMember]
		EU = 2,

		[EnumMember]
		COSME = 3,
	} // enum LoanSourceName
} // namespace

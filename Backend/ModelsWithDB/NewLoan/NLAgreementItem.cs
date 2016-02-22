namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;

	[DataContract]
	public class NLAgreementItem : AStringable {
		[DataMember]
		public NL_LoanAgreements Agreement { get; set; }

		//[DataMember]
		//public TemplateModel TemplateModel { get; set; }

		[DataMember]
		public string Path1 { get; set; }

		[DataMember]
		public string Path2 { get; set; }
	} // class NLAgreementItem
} // namespace

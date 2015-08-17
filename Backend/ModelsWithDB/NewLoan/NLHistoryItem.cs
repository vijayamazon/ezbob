namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;

	[DataContract]
	public class NLHistoryItem : AStringable {
		[DataMember]
		public NL_LoanHistory HistoryItem { get; set; }


	} // class NLLoanHistory
} // namespace

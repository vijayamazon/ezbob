namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Runtime.Serialization;

	[DataContract]
	public class NLScheduleItem : AStringable {
		[DataMember]
		public NL_LoanSchedules ScheduleItem { get; set; }

		[DataMember]
		public NL_LoanSchedulePayments ScheduleItemPayment { get; set; }

	} // class NLScheduleItem
} // namespace

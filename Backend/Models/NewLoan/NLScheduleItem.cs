namespace Ezbob.Backend.Models.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NLScheduleItem {
		[DataMember]
		public NL_LoanSchedules ScheduleItem { get; set; }

		[DataMember]
		public NL_LoanSchedulePayments ScheduleItemPayment { get; set; }

	}
}

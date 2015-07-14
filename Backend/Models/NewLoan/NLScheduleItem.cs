namespace Ezbob.Backend.Models.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NLScheduleItem  {

        //public NLScheduleItem() {}

        [DataMember]
        public NL_LoanSchedules ScheduleItem { get; set; }

        //[DataMember]
        //public NL_LoanFees Fee { get; set; }

        [DataMember]
        public NL_LoanSchedulePayments ScheduleItemPayment { get; set; }

        /*	[DataMember]
            public int ScheduleID { get; set; }

            [DataMember]
            public int Position { get; set; }

            [DataMember]
            public decimal Principal { get; set; }

            [DataMember]
            public decimal InterestRate { get; set; }

            [DataMember]
            public DateTime PlannedDate { get; set; }

            [DataMember]
            public DateTime? CloseTime { get; set; }*/

        /*[DataMember]
        public DateTime? PayDate { get; set; }

        [DataMember]
        public decimal? PrincipalPaid { get; set; }

        [DataMember]
        public decimal? InterestPaid { get; set; }*/


	


	}
}
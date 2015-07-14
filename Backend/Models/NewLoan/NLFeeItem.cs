namespace Ezbob.Backend.Models.NewLoan {
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	[DataContract]
	public class NLScheduleItem {

        [DataMember]
        public NL_LoanFees Fee { get; set; }

        [DataMember]
		public decimal? PrincipalPaid { get; set; }
		

	}
}
namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanSchedules : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanScheduleID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("NL_LoanScheduleStatuses", "LoanScheduleStatusID")]
		[DataMember]
		[EnumName(typeof(NLScheduleStatuses))]
		public int LoanScheduleStatusID { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public DateTime PlannedDate { get; set; }

		[DataMember]
		public DateTime? ClosedTime { get; set; }

		[DataMember]
		public decimal Principal { get; set; }

		[DataMember]
		[DecimalFormat("F7")]
		public decimal InterestRate { get; set; }

		[DataMember]
		public bool TwoDaysDueMailSent { get; set; }

		[DataMember]
		public bool FiveDaysDueMailSent { get; set; }

		// additions

		/// <summary>
		/// open principal based on planned scheduled p'
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal Balance { get; set; }

		/// <summary>
		/// current open principal (planned p' - paid p')
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal OpenPrincipal { get; set; }

		[DataMember] // based on planned scheduled principal (balance)
		[NonTraversable]
		public decimal Interest {get; set; }

		[DataMember]
		[NonTraversable]
		public decimal Fees { get; set; }

		/// <summary>
		/// p' + i' + f' (i' calculation based on planned scheduled principal (balance))
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal AmountDue { get; set; }

		[DataMember] // p*r based on real open principal
		[NonTraversable]
		public decimal InterestOpenPrincipalBased { get; set; }

		/// <summary>
		/// p' + i' + f' (i' calculation based on real open principal)
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal AmountDueOpenPrincipalBased { get; set; }

		[DataMember]
		[NonTraversable]
		public bool LateFeesAttached { get; set; }
		
		public NL_LoanSchedules ShallowCopy() {
			NL_LoanSchedules cloned = (NL_LoanSchedules)this.MemberwiseClone();
			return cloned;
		}
	} // class NL_LoanSchedules
} // ns

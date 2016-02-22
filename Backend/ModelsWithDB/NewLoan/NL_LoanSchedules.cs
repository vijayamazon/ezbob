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
		[Ignore("Principal")]
		public decimal Principal { get; set; }

		[DataMember]
		[DecimalFormat("F7")]
		public decimal InterestRate { get; set; }

		[DataMember]
		public bool TwoDaysDueMailSent { get; set; }

		[DataMember]
		public bool FiveDaysDueMailSent { get; set; }

		// additions

		// --------------- used for real AmountDue calculations, based on payments ---------------
		/// <summary>
		/// open principal based on planned scheduled p'
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal Balance { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal Fees { get; set; }

		[DataMember] // based on planned scheduled principal (balance)
		[NonTraversable]
		public decimal Interest { get; set; }

		/// <summary>
		/// hold real open principal for item (based on payments)
		/// </summary>
		//[DataMember]
		//[NonTraversable]
		//public decimal BalancedPrincipal { get; set; }

		// --------------- ### used for real AmountDue calculations, based on payments ---------------

		/// <summary>
		/// p' + i' + f' (i' calculation based on planned scheduled principal (balance))
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal AmountDue { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal PrincipalPaid { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal InterestPaid { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal FeesAssigned { get; set; }

		[DataMember]
		[NonTraversable]
		public decimal FeesPaid { get; set; }

		/// <summary>
		///  p*r based on real open principal: used to calculate schedule's interest to pay
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal InterestOP { get; set; }

		/*/// <summary>
		/// p' + i' + f' (i' calculation based on real open principal)
		/// </summary>
		[DataMember]
		[NonTraversable]
		public decimal AmountDueOP { get; set; }*/

		//[DataMember]
		//[NonTraversable]
		//public bool LateFeesAttached { get; set; }
			
		public bool IsDeleted() {
			if (LoanScheduleStatusID.Equals((int)NLScheduleStatuses.ClosedOnReschedule) 
				|| LoanScheduleStatusID.Equals((int)NLScheduleStatuses.DeletedOnReschedule)
				|| LoanScheduleStatusID.Equals((int)NLScheduleStatuses.LateDeletedOnReschedule)) 
				return true;

			return false;
		}
		
		public void SetStatusOnRescheduling() {
			if ((Principal - PrincipalPaid + Interest - InterestPaid + FeesAssigned - FeesPaid) == 0) {
				LoanScheduleStatusID = (int)NLScheduleStatuses.DeletedOnReschedule;
			} else if ((Principal - PrincipalPaid) > 0 || (Interest - InterestPaid) > 0) {
				LoanScheduleStatusID = (int)NLScheduleStatuses.ClosedOnReschedule;
			} else if (LoanScheduleStatusID.Equals((int)NLScheduleStatuses.Late)) {
				LoanScheduleStatusID = (int)NLScheduleStatuses.LateDeletedOnReschedule;
			}
		}
		
		public NL_LoanSchedules ShallowCopy() {
			NL_LoanSchedules cloned = (NL_LoanSchedules)MemberwiseClone();
			return cloned;
		}
	} // class NL_LoanSchedules
} // ns

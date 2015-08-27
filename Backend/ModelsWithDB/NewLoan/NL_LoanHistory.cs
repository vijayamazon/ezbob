namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanHistory : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? UserID { get; set; }

		[FK("NL_LoanLegals", "LoanLegalID")]
		[DataMember]
		public long? LoanLegalID { get; set; }

		[DataMember]
		public decimal Amount { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public DateTime EventTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Description { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string AgreementModel { get; set; }

		[DataMember]
		public int InterestOnlyRepaymentCount { get; set; }

		// additions

		private List<NL_LoanSchedules> _schedule = new List<NL_LoanSchedules>();

		[DataMember]
		[NonTraversable]
		public List<NL_LoanSchedules> Schedule {
			get { return this._schedule; }
			set { this._schedule = value; }
		}


		// helpers
		public List<NL_LoanSchedules> ActiveSchedule () {
			return this._schedule.Where(s => (s.LoanScheduleStatusID != (int)NLScheduleStatuses.ClosedOnReschedule && s.LoanScheduleStatusID != (int)NLScheduleStatuses.DeletedOnReschedule)).ToList();
		}


		protected override bool DisplayFieldInToString(string fieldName) {
			return fieldName != "AgreementModel";
		} // DisplayFieldInToString
	} // class NL_LoanHistory
} // ns

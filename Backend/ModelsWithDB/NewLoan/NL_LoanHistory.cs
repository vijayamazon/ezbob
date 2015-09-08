﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
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
		[DecimalFormat("C2")]
		public decimal Amount { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		[EnumName(typeof(RepaymentIntervalTypes))]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		[DecimalFormat("P4")]
		public decimal InterestRate { get; set; }

		[DataMember]
		public DateTime EventTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Description { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		[ExcludeFromToString]
		public string AgreementModel { get; set; }

		[DataMember]
		public int InterestOnlyRepaymentCount { get; set; }

		// additions

		private List<NL_LoanSchedules> _schedule = new List<NL_LoanSchedules>();
		private List<NL_LoanAgreements> _agreements = new List<NL_LoanAgreements>();

		[DataMember]
		[NonTraversable]
		public List<NL_LoanSchedules> Schedule {
			get { return this._schedule; }
			set { this._schedule = value; }
		}

		[DataMember]
		[NonTraversable]
		public List<NL_LoanAgreements> Agreements {
			get { return this._agreements; }
			set { this._agreements = value; }
		}

		// helpers
		public List<NL_LoanSchedules> ActiveSchedule () {
			return this._schedule.Where(s => (s.LoanScheduleStatusID != (int)NLScheduleStatuses.ClosedOnReschedule && s.LoanScheduleStatusID != (int)NLScheduleStatuses.DeletedOnReschedule)).ToList();
		}


		public override string ToString() {

			StringBuilder sb = new StringBuilder().Append(base.ToString()).Append(Environment.NewLine);

			// schedule
			sb.Append(HeadersLine(typeof(NL_LoanSchedules), NL_LoanSchedules.ColumnTotalWidth));
			Schedule.ForEach(s => sb.Append(s.ToString()));

			// agreements
			sb.Append(Environment.NewLine);
			if (Agreements.Count > 0) {
				sb.Append("Agreements:");
				Agreements.ForEach(a => sb.Append(a.ToString()));
			} else 
				sb.Append("Agreements not loaded/found");

			return sb.ToString();
		}
	} // class NL_LoanHistory
} // ns

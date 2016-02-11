namespace Ezbob.Backend.ModelsWithDB.NewLoan {
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
		public decimal Amount { get; set; }

		[FK("NL_RepaymentIntervalTypes", "RepaymentIntervalTypeID")]
		[DataMember]
		[EnumName(typeof(RepaymentIntervalTypes))]
		public int RepaymentIntervalTypeID { get; set; }

		[DataMember]
		public int RepaymentCount { get; set; }

		[DataMember]
		[DecimalFormat("F6")]
		public decimal InterestRate { get; set; }

		[DataMember]
		public DateTime EventTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public DateTime RepaymentDate { get; set; }

		[DataMember]
		public decimal? PaymentPerInterval { get; set; }

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
		public List<NL_LoanSchedules> ActiveSchedule() {
			return this._schedule.Where(s => !s.IsDeleted()
				//(s.LoanScheduleStatusID != (int)NLScheduleStatuses.ClosedOnReschedule && s.LoanScheduleStatusID != (int)NLScheduleStatuses.DeletedOnReschedule)
				).ToList();
		}

		//set default to 3? \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 11-14, 143 -TODO should be from LoanSource
		public void SetDefaultRepaymentCount() {
			RepaymentCount = RepaymentCount == 0 ? 3 : RepaymentCount;
		}

		//	\ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 144
		public void SetDefaultInterestRate() {
			InterestRate = InterestRate == 0m ? 0.06M : InterestRate;
		}

		// startDate \ezbob\Integration\PaymentServices\Calculators\LoanScheduleCalculator.cs line 40-41 set default to DateTime.UtcNow;
		public void SetDefaultEventTime() {
			if (EventTime == DateTime.MinValue)
				EventTime = DateTime.UtcNow;
		}

		public void SetDefaultRepaymentIntervalType() {
			//Console.WriteLine("==={0}" , RepaymentIntervalTypeID);
			RepaymentIntervalTypeID = (RepaymentIntervalTypeID == 0) ? (int)RepaymentIntervalTypes.Month : RepaymentIntervalTypeID;
		}

		// +month|+7 days
		public void SetDefaultRepaymentDate() {
			if (RepaymentDate == DateTime.MinValue) {
				//NL_LoanHistory lastHistory = LastHistory() ?? new NL_LoanHistory();
				RepaymentDate = (RepaymentIntervalTypeID == (int)RepaymentIntervalTypes.Month) ? EventTime.Date.AddMonths(1) : EventTime.Date.AddDays(7);
			}
		}

		public void SetDefaults() {
			SetDefaultEventTime();
			SetDefaultRepaymentIntervalType();
			SetDefaultRepaymentDate();
			//SetDefaultInterestRate();
			//SetDefaultRepaymentCount();
		}

		/*public NL_LoanHistory ShallowCopy() {
			NL_LoanHistory cloned = (NL_LoanHistory)this.MemberwiseClone();
			return cloned;
		}*/

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public override string ToString() {
			// history
			StringBuilder sb = new StringBuilder().Append(PrintHeadersLine(typeof(NL_LoanHistory))).Append(ToStringAsTable());

			// schedule
			if (Schedule.Count > 0) {
				sb.Append("Schedules:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_LoanSchedules)));
				Schedule.ForEach(s => sb.Append(s.ToStringAsTable()));
			} else
				sb.Append("No Schedule.").Append(Environment.NewLine);

			// agreements
			if (Agreements.Count > 0) {
				sb.Append("Agreements:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_LoanAgreements)));
				Agreements.ForEach(a => sb.Append(a.ToStringAsTable()));
			} // else sb.Append("No Agreements.").Append(Environment.NewLine);

			return sb.ToString();
		}
	} // class NL_LoanHistory
} // ns

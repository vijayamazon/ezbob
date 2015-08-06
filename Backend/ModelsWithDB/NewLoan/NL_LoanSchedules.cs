namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils;
	using Ezbob.Utils.dbutils;

	[DataContract(IsReference = true)]
	public class NL_LoanSchedules {
		[PK(true)]
		[DataMember]
		public int LoanScheduleID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public int LoanHistoryID { get; set; }

		[FK("NL_LoanScheduleStatuses", "LoanScheduleStatusID")]
		[DataMember]
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
		public decimal InterestRate { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(GetType().Name + ": ");

			this.Traverse((ignored, pi) => {
				object obj = pi.GetValue(this);

				if (obj != null)
					sb.Append(pi.Name).Append(": ").Append(obj).Append(";\t");
			});

			return sb.ToString();
		} // ToString
	} // NL_LoanSchedules
} // ns

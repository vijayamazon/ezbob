using System;
using System.Data.Common;
using System.Globalization;
using Ezbob.Logger;

namespace LoanScheduleTransactionBackFill {

	class Schedule : SafeLog {

		public static CultureInfo Culture {
			get {
				if (ms_oCulture == null)
					ms_oCulture = new CultureInfo("en-GB", false);

				return ms_oCulture;
			} // get
		} // Culture

		private static CultureInfo ms_oCulture;

		public Schedule(ASafeLog log = null) : base(log) {
		} // constructor

		public Schedule(Schedule o) : this((ASafeLog)o) {
			ID = o.ID;
			Date = o.Date;
			Principal = o.Principal;
			IsAlreadyProcessed = o.IsAlreadyProcessed;
		} // constructor

		public Schedule(DbDataReader row, ASafeLog log = null) : base(log) {
			ID = Convert.ToInt32(row["ItemID"]);
			Date = Convert.ToDateTime(row["ItemDate"]).Date;
			Principal = Convert.ToDecimal(row["Principal"]);
			IsAlreadyProcessed = row["ProcessedID"] != System.DBNull.Value;
		} // constructor

		public Schedule(ScheduleModel sm, ASafeLog log = null) : base(log) {
			IsAlreadyProcessed = false;
			Date = sm.Date.Date;
			Principal = sm.LoanRepayment;
		} // constructor

		public int ID { get; set; }
		public DateTime Date { get; set; }
		public decimal Principal { get; set; }

		public bool IsAlreadyProcessed { get; set; }

		public override string ToString() {
			return string.Format(
				"{3} {0,7} on {1} p: {2,10}",
				ID, Date.ToString("MMM dd yyyy", Culture), Principal.ToString("C2", Culture),
				IsAlreadyProcessed ? "v" : " "
			);
		} // ToString

	} // class Schedule

} // namespace LoanScheduleTransactionBackFill

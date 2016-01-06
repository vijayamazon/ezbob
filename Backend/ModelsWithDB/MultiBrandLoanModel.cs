namespace EzBob.Backend.ModelsWithDB {
	using System;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	public class MultiBrandLoanModel : AResultRow {
		public int LoanID { get; set; }
		public DateTime IssueTime { get; set; }
		public decimal LoanAmount { get; set; }
		public int Term { get; set; }
		public string Origin { get; set; }
		public decimal RepaidAmount { get; set; }

		public decimal OpenBalance {
			get { return LoanAmount - RepaidAmount; }
		} // OpenBalance

		public string TimeLeft {
			get {
				DateTime now = DateTime.UtcNow.Date;
				DateTime plannedCloseTime = IssueTime.AddMonths(Term).Date;

				if (plannedCloseTime > now) {
					return string.Format(
						"{0} overdue",
						Grammar.Number(MiscUtils.DateDiffInMonths(now, plannedCloseTime), "month")
					);
				} // if

				return string.Format(
					"{0} left",
					Grammar.Number(MiscUtils.DateDiffInMonths(plannedCloseTime, now), "month")
				);
			} // get
		} // TimeLeft
	} // class MultiBrandLoanModel
} // namespace

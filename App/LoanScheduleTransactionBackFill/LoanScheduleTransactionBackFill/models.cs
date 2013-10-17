using System;
using System.Collections.Generic;

namespace LoanScheduleTransactionBackFill {
	#region class ScheduleModel

	class ScheduleModel {
		public DateTime Date { get; set; }
		public decimal LoanRepayment { get; set; }
	} // ScheduleModel

	#endregion class ScheduleModel

	#region class LoanModel

	class LoanModel {
		public List<ScheduleModel> Schedule { get; set; }
	} // LoanModel

	#endregion class LoanModel
} // namespace LoanScheduleTransactionBackFill

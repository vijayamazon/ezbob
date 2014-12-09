using System;
using System.Collections.Generic;

namespace LoanScheduleTransactionBackFill {

	class ScheduleModel {
		public DateTime Date { get; set; }
		public decimal LoanRepayment { get; set; }
	} // ScheduleModel

	class LoanModel {
		public List<ScheduleModel> Schedule { get; set; }
	} // LoanModel

} // namespace LoanScheduleTransactionBackFill

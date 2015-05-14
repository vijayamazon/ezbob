using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.ModelsWithDB.NewLoan {

	public class LoanBuilder {


		// all those are referenced to the latest history ID

		public List<LoanModel> GetAllSchedules(int loanID) {
			// call SP [dbo].[NL_AllSchedulesLoad]
			return null;
		}

		public List<LoanModel> GetNotPaidSchedules(int loanID) {
			// call SP [dbo].[NL_NotPaidSchedulesLoad]
			return null;
		}

		public List<LoanModel>  GetPaidSchedules (int loanID) {
			// call SP [dbo].[NL_PaidSchedulesLoad]
			return null;
		}


		public List<LoanModel> GetCancelledPaymentSchedules(int loanID) {
			// call SP [dbo].[NL_CancelledSchedulesLoad]
			return null;
		}

		

			
	}
}
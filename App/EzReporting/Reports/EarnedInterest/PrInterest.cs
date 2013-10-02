using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports {
	#region class PrInterest

	class PrInterest {
		public readonly DateTime Date;
		public decimal Principal;
		public decimal Interest;

		public PrInterest(DateTime oDate, decimal nPrincipal) {
			Date = oDate;
			Principal = nPrincipal;
			Interest = 0;
		} // constructor

		public override string ToString() {
			return string.Format("{0}: {1} * {2} = {3}", Date, Principal, Interest, Principal * Interest);
		} // ToString

		public bool Update(InterestData oDelta, InterestFreezePeriods ifp) {
			if (ifp == null)
				Interest = oDelta.Interest;
			else
				Interest = ifp.GetInterest(Date) ?? oDelta.Interest;

			return Date == oDelta.Date;
		} // Update

		public void Update(TransactionData oDelta) {
			if (Date > oDelta.Date)
				Principal -= oDelta.Repayment;
		} // Update
	} // class PrInterest

	#endregion class PrInterest
} // namespace Reports

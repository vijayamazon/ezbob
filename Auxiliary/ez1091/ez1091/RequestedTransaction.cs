using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Logger;

namespace ez1091 {

	class RequestedTransaction {

		static RequestedTransaction() {
			ms_oCulture = new CultureInfo("en-GB", false);
		} // static constructor

		public RequestedTransaction(string sRawData, ASafeLog oLog) {
			m_oLog = new SafeLog(oLog);

			string[] aryFields = sRawData.Split(',');

			if (aryFields.Length != 3)
				throw new Exception("Wrong number of fields in " + sRawData);

			Date = DateTime.ParseExact(aryFields[0], "dd/mm/yyyy", ms_oCulture, DateTimeStyles.None).Date;
			LoanID = Convert.ToInt32(aryFields[1]);
			PaidAmount = Convert.ToDecimal(aryFields[2]);

			m_oLog.Debug("{0} -> {1}", sRawData, this);
		} // constructor

		public DateTime Date { get; private set; }
		public int LoanID { get; private set; }
		public decimal PaidAmount { get; private set; }

		public override string ToString() {
			return string.Format(
				"[{0} for loan {1} on {2}]",
				PaidAmount.ToString("C2", ms_oCulture),
				LoanID.ToString("N0"),
				Date.ToString("MMMM d yyyy", ms_oCulture)
			);
		} // ToString

		private readonly ASafeLog m_oLog;
		private static readonly CultureInfo ms_oCulture;

	} // class RequestedTransaction

} // namespace ez1091

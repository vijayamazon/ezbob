namespace AutomationCalculator.AutoDecision.AutoRejection {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class AutoRejectionArguments {
		public AutoRejectionArguments(
			int customerID,
			int companyID,
			long? cashRequestID,
			long? nlCashRequestID,
			string tag,
			DateTime now,
			AConnection db,
			ASafeLog log
		) {
			CustomerID = customerID;
			CompanyID = companyID;

			CashRequestID = cashRequestID;
			NLCashRequestID = nlCashRequestID;
			Tag = tag;
			Now = now;

			DB = db;
			Log = log.Safe();

			TrailUniqueID = Guid.NewGuid();
		} // constructor

		public int CustomerID { get; private set; }
		public int CompanyID { get; private set; }

		public long? CashRequestID { get; private set; }
		public long? NLCashRequestID { get; private set; }
		public string Tag { get; private set; }
		public DateTime Now { get; private set; }

		public AConnection DB { get; private set; }
		public ASafeLog Log { get; private set; }

		public Guid TrailUniqueID { get; private set; }
	} // class AutoRejectionArguments
} // namespace

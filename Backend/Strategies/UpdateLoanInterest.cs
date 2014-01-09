namespace EzBob.Backend.Strategies {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using PaymentServices.Calculators;

	public class UpdateLoanInterest : AStrategy {
		public UpdateLoanInterest(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
		}

		public override string Name {
			get { return "Update interest"; }
		} // Name

		public override void Execute() {
			lock (updateInterestLock) {
				if (isExecuting)
				{
					Log.Warn("Update interest is already in progress.");
					return;
				} // if

				isExecuting = true;
			} // lock

			DoUpdateInterest();

			lock (updateInterestLock)
			{
				isExecuting = false;
			} // lock
		} // Execute

		private void DoUpdateInterest()
		{
			Log.Info("Starting updating loan interest");

			DataTable dt = DB.ExecuteReader("GetUnpaidLoans", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				int loanId = sr["Id"];

				try
				{
					UpdateLoan(loanId);
				}
				catch (Exception ex)
				{

					Log.Error("Failed to update loan #{0}. Exception: {1}", loanId, ex);
				}
			}
		}

		private void UpdateLoan(int loanId)
		{

			var date = DateTime.UtcNow;
			var calc = new LoanRepaymentScheduleCalculator(loanId, date); // Should be rewritten (then remove Scorto.Nhibernate)
			decimal updatedInterest = calc.GetInterest();

			DB.ExecuteNonQuery("UpdateLoanInterest", CommandSpecies.StoredProcedure, new QueryParameter("LoanId", loanId), new QueryParameter("InterestDue", updatedInterest));

			Log.Info("Loan #{0} updated.", loanId);
		}

		private static readonly object updateInterestLock = new object();
		private static bool isExecuting;
	} // UpdateLoanInterest
} // namespace

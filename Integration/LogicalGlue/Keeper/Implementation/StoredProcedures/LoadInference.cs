namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class LoadInference : ACustomerTimeStoredProc {
		public LoadInference(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return (ResponseID > 0) || base.HasValidParameters();
		} // HasValidParameters

		public virtual long ResponseID { get; set; }

		public virtual int HistoryLength { get; set; }

		public bool IncludeTryOutData { get; set; }

		public decimal MonthlyPayment {
			get { return this.monthlyPayment; }
			set { this.monthlyPayment = Math.Truncate(value); }
		} // MonthlyPayment

		private decimal monthlyPayment;
	} // class LoadInference
} // namespace

namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public abstract class AMainStrategyBase : AStrategy {
		public abstract int CustomerID { get; }

		protected virtual void LoadCompanyAndMonthlyPayment(DateTime now) {
			var sp = new GetCustomerCompanyID(DB, Log) {
				CustomerID = CustomerID,
				Now = now,
			};

			sp.ExecuteNonQuery();

			CompanyID = sp.CompanyID;

			decimal monthlyPayment = Math.Truncate(InjectorStub.GetEngine().GetMonthlyRepayment(CustomerID, now));

			if (monthlyPayment > Int32.MaxValue)
				MonthlyPayment = Int32.MaxValue;
			else if (monthlyPayment <= 0)
				MonthlyPayment = 0;
			else
				MonthlyPayment = (int)monthlyPayment;
		} // LoadCompanyAndMonthlyPayment

		protected virtual int CompanyID { get; private set; }
		protected virtual int MonthlyPayment { get; private set; }

		private class GetCustomerCompanyID : AStoredProcedure {
			public GetCustomerCompanyID(AConnection db, ASafeLog log) : base(db, log) { } // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (Now > longAgo);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public DateTime Now { get; set; }

			[UsedImplicitly]
			[Direction(ParameterDirection.Output)]
			public int CompanyID { get; set; }

			private static readonly DateTime longAgo = new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc);
		} // class GetCustomerCompanyID
	} // class AMainStrategyBase
} // namespace

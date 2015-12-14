namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Database;

    public class GetCustomerLoans : AStrategy, Inlstrategy {

		public override string Name { get { return "GetCustomerLoans"; } }

		public override void Execute() {

            if (!IsNewLoanRunStrategy)
                return;

			NL_AddLog(LogType.Info, "Strategy Start", Context.CustomerID, null, null, null);
			try {
				Loans = DB.Fill<NL_Loans>("NL_CustomerLoansGet",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", Context.CustomerID)
				).ToArray();

				NL_AddLog(LogType.Info, "Strategy End", Context.CustomerID, this.Loans, null, null);
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy Faild", Context.CustomerID, null, ex.ToString(), ex.StackTrace);
			}
		} // Execute

		public NL_Loans[] Loans { get; private set; }
		public string Error { get; set; }

	} // class GetLastOffer
} // ns

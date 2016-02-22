namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using NHibernate.Linq;

	public class GetCustomerLoans : AStrategy {

		public GetCustomerLoans(int customerID) {
			CustomerID = customerID;
		}

		public int CustomerID { get; private set; }

		public override string Name { get { return "GetCustomerLoans"; } }

		public override void Execute() {
			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			if (CustomerID == 0) {
				Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Failed", CustomerID, null, Error, null);
				return;
			}

			NL_AddLog(LogType.Info, "Strategy Start", CustomerID, null, Error, null);
			
			try {

				Loans = DB.Fill<NL_Loans>("NL_CustomerLoansGet", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", CustomerID)).ToArray();
	
				NL_AddLog(LogType.Info, "Strategy End", Context.CustomerID, Loans.Select(v => v.LoanID.ToString()), Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				Error = "nl loans for customer not found";
				NL_AddLog(LogType.Error, "Strategy Faild", CustomerID, Error, ex.ToString(), ex.StackTrace);
			}
		} // Execute

		public NL_Loans[] Loans { get; private set; }
		public string Error { get; set; }

	} // class GetLastOffer
} // ns

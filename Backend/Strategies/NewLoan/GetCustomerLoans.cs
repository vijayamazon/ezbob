namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using Ezbob.Utils;

    public class GetCustomerLoans : AStrategy
    {
        public override string Name { get { return "GetCustomerLoans"; } }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", Context.CustomerID, null, null, null);
            try
            {
                Loans = DB.Fill<NL_Loans>("NL_CustomerLoansGet",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("CustomerID", Context.CustomerID)
                ).ToArray();

                NL_AddLog(LogType.Info, "Strategy End", Context.CustomerID, this.Loans, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", Context.CustomerID, null, ex.ToString(), ex.StackTrace);
            }
        } // Execute

        public NL_Loans[] Loans { get; private set; }
        public string Error { get; set; }

    } // class GetLastOffer
} // ns

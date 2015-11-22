namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class GetCustomerLoans : AStrategy
    {
        public GetCustomerLoans(int customerID)
        {
            this.customerID = customerID;
        } // constructor

        public override string Name { get { return "GetCustomerLoans"; } }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", this.customerID, null, null, null);
            try
            {
                Loans = DB.Fill<NL_Loans>("NL_CustomerLoansGet",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("CustomerID", this.customerID)
                );
                NL_AddLog(LogType.Info, "Strategy End", this.customerID, this.Loans, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild", this.customerID, null, ex.ToString(), ex.StackTrace);
            }
        } // Execute

        public List<NL_Loans> Loans { get; private set; }
        private readonly int customerID;
    } // class GetLastOffer
} // ns

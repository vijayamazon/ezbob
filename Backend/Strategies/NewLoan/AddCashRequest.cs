namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using Newtonsoft.Json;

    public class AddCashRequest : NewLoanBaseStrategy
    {
        public AddCashRequest(NL_CashRequests cashRequest)
        {
            this.cashRequest = cashRequest;
        } // constructor

        public override string Name { get { return "AddCashRequest"; } }

        public override void NL_Execute() {
            NL_AddLog(LogType.Info, "Strategy Start", this.cashRequest,null, null, null);
            try
            {
                CashRequestID = DB.ExecuteScalar<long>(
                    "NL_CashRequestsSave",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<NL_CashRequests>("Tbl", this.cashRequest)
                    );
                NL_AddLog(LogType.Info, "Strategy End", this.cashRequest, CashRequestID, null, null);
                // ReSharper disable once CatchAllClause
            }
            catch (Exception ex)
            {
                Log.Alert("Failed to save NL_CashRequests, {0}, ex: {1}", this.cashRequest, ex);
                Error = string.Format("Failed to save NL_CashRequests, {0}, ex: {1}", this.cashRequest, ex.Message);
                NL_AddLog(LogType.Error, "Strategy Faild", this.cashRequest, null, ex.ToString(), ex.StackTrace);
            }

        } // Execute

        public long CashRequestID { get; set; }
        public string Error { get; set; }

        private readonly NL_CashRequests cashRequest;
    } // class AddCashRequest
} // ns

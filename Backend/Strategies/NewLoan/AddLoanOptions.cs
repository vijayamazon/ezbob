namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using Ezbob.Utils;

    public class AddLoanOptions : AStrategy
    {

        public List<string> PropertiesUpdateList { get; set; }
        public int? oldLoanId { get; set; }
        public string Error { get; set; }

        public long LoanOptionsID { get; set; }

        private readonly NL_LoanOptions nlLoanOptions;
        public AddLoanOptions(NL_LoanOptions loanOptions, int? OldLoanId, List<String> PropertiesUpdateList = null)
        {
            this.nlLoanOptions = loanOptions;
            this.oldLoanId = OldLoanId;
            this.PropertiesUpdateList = PropertiesUpdateList;
        } //constructor

        public override string Name { get { return "AddLoanOptions"; } }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", this.nlLoanOptions, null, null, null);
            try
            {
                long newLoanId = -1;

                if (oldLoanId != null)
                    newLoanId = DB.ExecuteScalar<long>("GetNewLoanIdByOldLoanId", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", oldLoanId));

                if (newLoanId > -1)
                {
                    this.nlLoanOptions.LoanID = newLoanId;

                    NL_LoanOptions existsOptions = DB.FillFirst<NL_LoanOptions>("NL_LoanOptionsGet", CommandSpecies.StoredProcedure, new QueryParameter("@LoanID", this.nlLoanOptions.LoanID));

                    PropertyInfo[] props = typeof(NL_LoanOptions).GetProperties();

                    existsOptions.Traverse((ignored, pi) =>
                    {
                        if (pi.GetValue(this.nlLoanOptions) != null)
                            pi.SetValue(existsOptions, pi.GetValue(this.nlLoanOptions));
                    });
                    
                    if (PropertiesUpdateList != null) {
                        foreach (var updateProperty in PropertiesUpdateList)
                        {
                            PropertyInfo pi = this.nlLoanOptions.GetType().GetProperty(updateProperty);
                            var fromClient = pi.GetValue(this.nlLoanOptions);
                            pi.SetValue(existsOptions, fromClient, null);
                        }   
                    }
                    
                    this.LoanOptionsID = DB.ExecuteScalar<long>("NL_SaveLoanOptions",
                        CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanOptions>("Tbl", existsOptions),
                        new QueryParameter("@LoanID", this.nlLoanOptions.LoanID));
                    NL_AddLog(LogType.Info, "Strategy End", this.nlLoanOptions, this.LoanOptionsID, null, null);
                }
                else
                {
                    NL_AddLog(LogType.DataExsistense, "Strategy Faild", this.nlLoanOptions,null, null, null);
                }

                // ReSharper disable once CatchAllClause
            }
            catch (Exception ex)
            {
                Log.Alert("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.nlLoanOptions.LoanID, ex);
                Error = string.Format("Failed to save NL_LoanOptions, oldLoanID: {0}, LoanID: {1}, ex: {2}", oldLoanId, this.nlLoanOptions.LoanID, ex.Message);
                NL_AddLog(LogType.Error, "Strategy Faild", this.nlLoanOptions, null, ex.ToString(), ex.StackTrace);
            }
        }//Execute


    }//class AddLoanOptions
}//ns

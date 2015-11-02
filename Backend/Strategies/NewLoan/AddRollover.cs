namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddRollover : AStrategy
    {
        public AddRollover(NL_LoanRollovers rollover)
        {
            this.rollover = rollover;
        }//constructor

        public override string Name { get { return "AddRollover"; } }

        public override void Execute()
        {
            NL_AddLog(LogType.Info, "Strategy Start", this.rollover, null, null, null);
            try
            {
                RolloverID = DB.ExecuteScalar<int>("NL_LoanRolloversSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanRollovers>("Tbl", this.rollover));
                NL_AddLog(LogType.Info, "Strategy End",this.rollover, RolloverID, null, null);
            }
            catch (Exception ex)
            {
                NL_AddLog(LogType.Error, "Strategy Faild",this.rollover, null, ex.ToString(), ex.StackTrace);
            }
        }//Execute


        public int RolloverID { get; set; }

        private readonly NL_LoanRollovers rollover;
    }//class AddRollover
}//ns

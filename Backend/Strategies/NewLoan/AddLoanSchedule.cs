namespace Ezbob.Backend.Strategies.NewLoan {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddLoanSchedule : AStrategy {
        public AddLoanSchedule(IEnumerable<NL_LoanSchedules> schedules) {
            this.schedules = schedules;
        }//constructor

        public override string Name { get { return "AddLoanSchedule"; } }

        public override void Execute() {
            DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<IEnumerable<NL_LoanSchedules>>("Tbl", this.schedules)); 
        }//Execute

        private readonly IEnumerable<NL_LoanSchedules> schedules;
    }//class AddLoanSchedule
}//ns

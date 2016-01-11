namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class InvestorParametersDAL : IInvestorParametersDAL {

        private Dictionary<int, double> investorsBalance;
        public Dictionary<int, double> InvestorsBalance {
            get {
                return this.investorsBalance ?? (this.investorsBalance = Library.Instance.DB.Fill<KeyValuePair<int, double>>
                    (string.Format("SELECT iiba.InvestorID, MAX(iisb.Timestamp) as Balance from I_InvestorSystemBalance iisb" +
                        " join I_InvestorBankAccount iiba on iiba.InvestorBankAccountID = iisb.InvestorBankAccountID " +
                        "WHERE iiba.InvestorAccountTypeID = 1 " +
                        "GROUP BY InvestorID "), CommandSpecies.Text)
                    .ToDictionary(x => x.Key, x => x.Value));
            }
        }

        public List<int> GetInvestorsIds() {
            return Library.Instance.DB.Fill<int>("select InvestorID from I_Investor i where i.IsActive = 1", CommandSpecies.Text);
        }


        public List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType) {
            return Library.Instance.DB.Fill<I_InvestorParams>(string.Format(" select * from I_InvestorParams where InvestorID ={0} and type = {1}", ruleType == RuleType.System ? (int?)null : investorId, (int)ruleType), CommandSpecies.Text);
        }

        public double GetGradeMonthlyInvestedAmount(int investorId, Grade grade) {
            return Library.Instance.DB.ExecuteScalar<double>(string.Format("select sum(l.LoanAmount) GradeSum from I_Portfolio ip  join Loan l on ip.LoanID = l.Id where ip.InvestorID = {0} and ip.GradeID = {1} and ip.Timestamp  <= DATEADD(month, -1, GETDATE())  group by ip.GradeID", investorId, grade), CommandSpecies.Text);
        }

        public decimal GetGradeMaxScore(int investorId, Grade grade) {
            var index = Library.Instance.DB.ExecuteScalar<I_Index>(string.Format("select * from I_Index where InvestorID ={0}", investorId), CommandSpecies.Text);
            switch (grade) {
                case Grade.A: return index.GradeAMaxScore;
                case Grade.B: return index.GradeBMaxScore;
                case Grade.C: return index.GradeCMaxScore;
                case Grade.D: return index.GradeDMaxScore;
                case Grade.E: return index.GradeEMaxScore;
                case Grade.F: return index.GradeFMaxScore;
            }
            return 0;
        }

        public double GetInvestorTotalMonthlyDeposits(int investorId) {
            return Library.Instance.DB.ExecuteScalar<double>(string.Format("SELECT SUM(iibat.TransactionAmount) AS TSum FROM I_InvestorBankAccountTransaction iibat JOIN I_InvestorBankAccount iiba ON iiba.InvestorBankAccountID = iibat.InvestorBankAccountID WHERE iibat.TransactionAmount > 0 AND iiba.InvestorID = {0} AND iibat.Timestamp >= DATEADD(MONTH, -1, GETDATE())", investorId), CommandSpecies.Text);
        }

        public double GetInvestorBalanceMonthAgo(int investorId) {
            return Library.Instance.DB.ExecuteScalar<double>(string.Format("SELECT top 1 NewBalance FROM I_InvestorSystemBalance iisb JOIN I_InvestorBankAccount iiba ON iiba.InvestorBankAccountID = iisb.InvestorBankAccountID WHERE iiba.InvestorID = {0} AND iisb.Timestamp <= DATEADD(MONTH, -1, GETDATE())ORDER BY iiba.Timestamp DESC", investorId), CommandSpecies.Text);
        }

        public double GetInvestorMonthlyFundingCapital(int investorId) {
            return Library.Instance.DB.ExecuteScalar<double>(string.Format("SELECT MonthlyFundingCapital FROM I_Investor WHERE InvestorID ={0}", investorId), CommandSpecies.Text);
        }

        public double GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod) {
            switch (invesmentPeriod) {
                case InvesmentPeriod.Day:
                    return Library.Instance.DB.ExecuteScalar<double>(string.Format("select sum(l.LoanAmount) as invesmentAmount from I_Portfolio ip  join Loan l on ip.LoanID = l.Id where ip.InvestorID = {0} and ip.Timestamp  <= DATEADD(day, -1, GETDATE())", investorId), CommandSpecies.Text);
                case InvesmentPeriod.Week:
                    return Library.Instance.DB.ExecuteScalar<double>(string.Format("select sum(l.LoanAmount) as invesmentAmount from I_Portfolio ip  join Loan l on ip.LoanID = l.Id where ip.InvestorID = {0} and ip.Timestamp  <= DATEADD(week, -1, GETDATE())", investorId), CommandSpecies.Text);
                case  InvesmentPeriod.Month:
                    return Library.Instance.DB.ExecuteScalar<double>(string.Format("select sum(l.LoanAmount) as invesmentAmount from I_Portfolio ip  join Loan l on ip.LoanID = l.Id where ip.InvestorID = {0} and ip.Timestamp  <= DATEADD(month, -1, GETDATE())", investorId), CommandSpecies.Text);
            }
            return 0;
        }
    }
}

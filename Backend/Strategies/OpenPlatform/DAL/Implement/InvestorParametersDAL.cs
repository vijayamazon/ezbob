namespace Ezbob.Backend.Strategies.OpenPlatform.DAL.Implement {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.ModelsWithDB.Wrappers;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Database;

    public class InvestorParametersDAL : IInvestorParametersDAL {

        private Dictionary<int, double> investorsBalance;
        public Dictionary<int, double> InvestorsBalance {
            get {
                return this.investorsBalance; // TODO  ?? (this.investorsBalance = Library.Instance.DB.Fill<I_InvestorBalance>("I_GetInvestorsBalance", CommandSpecies.StoredProcedure).ToDictionary(x => x.InvestorID, x => x.Balance));
            }
        }

        public List<int> GetInvestorsIds() {
            var temp = Library.Instance.DB.Fill<IntWrapper>("I_GetInvestorsIds", CommandSpecies.StoredProcedure);
                return temp.Select(x=> x.Value).ToList();
        }

        public List<I_InvestorParams> GetInvestorParametersDB(int investorId, RuleType ruleType) {
            var queryParameters = ruleType == RuleType.System ? new[] { new QueryParameter("TypeID", (int)ruleType) } : new[] { new QueryParameter("TypeID", (int)ruleType), new QueryParameter("InvestorId", investorId) };
            return Library.Instance.DB.Fill<I_InvestorParams>("I_GetInvestorParametersDB", CommandSpecies.StoredProcedure, queryParameters);
        }

        public double GetGradeMonthlyInvestedAmount(int investorId, Grade grade) {
            var myDate = DateTime.Now;
            var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            var result = Library.Instance.DB.ExecuteScalar<decimal>("I_GetGradeMonthlyInvestedAmount", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("GradeID", (int)grade), new QueryParameter("FirstOfMonth", TimeZoneInfo.ConvertTimeToUtc(firstOfMonth)));
            return (double)result;
        }

        public decimal GetGradeMaxScore(int investorId, Grade grade, int ruleType) {
            var index = Library.Instance.DB.ExecuteScalar<I_Index>("I_GetGradeMaxScore", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("TypeID", (int)ruleType));
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
            var myDate = DateTime.Now;
            var firstOfMonth = new DateTime(myDate.Year, myDate.Month, 1);
            return Library.Instance.DB.ExecuteScalar<double>("I_GetInvestorTotalMonthlyDeposits", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("FirstOfMonth", firstOfMonth));
        }

        public double GetInvestorMonthlyFundingCapital(int investorId) {
            return Library.Instance.DB.ExecuteScalar<double>("I_GetInvestorMonthlyFundingCapital", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId));
        }

        public double GetFundedAmountPeriod(int investorId, InvesmentPeriod invesmentPeriod) {
            DateTime periodAgo = new DateTime();
            switch (invesmentPeriod) {
                case InvesmentPeriod.Day:
                    periodAgo = DateTime.Today.AddDays(-1);
                    break;
                case InvesmentPeriod.Week:
                    periodAgo = DateTime.Today.AddDays(-7);
                    break;
                case InvesmentPeriod.Month:
                    periodAgo = DateTime.Today.AddDays(-30);
                    break;
            }

            var fundedAmount = Library.Instance.DB.ExecuteScalar<decimal>("I_GetFundedAmountPeriod", CommandSpecies.StoredProcedure, new QueryParameter("InvestorID", investorId), new QueryParameter("PeriodAgo", TimeZoneInfo.ConvertTimeToUtc(periodAgo)));
            return (double)fundedAmount;
        }

    }
}

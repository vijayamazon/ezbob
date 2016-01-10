namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Implement {
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.Models.Investor;
    using Ezbob.Backend.ModelsWithDB.OpenPlatform;
    using Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts;
    using Ezbob.Backend.Strategies.OpenPlatform.DAL.Contract;
    using Ezbob.Backend.Strategies.OpenPlatform.Models;
    using StructureMap.Attributes;

    public class InvestorParametersBLL : IInvestorParametersBLL {
        [SetterProperty]
        public IInvestorParametersDAL InvestorParametersDAL { get; set; }

        public Dictionary<int, InvestorParameters> GetInvestorsParameters() {
            return InvestorParametersDAL.GetInvestorsParameters();
        }

        public InvestorParameters GetInvestorParameters(int InvestorId, int ruleType) {
            return InvestorParametersDAL.GetInvestorParameters(InvestorId, ruleType);
        }

        public double GetGradeAvailableAmount(int investorId, InvestorLoanCashRequest investorLoanCashRequest, int ruleType) {

            //The amount that investor invest on grade this month.                                   
            double investorGradeInvestedAmount = InvestorParametersDAL.GetGradeMonthlyInvestedAmount(investorId, investorLoanCashRequest.Grade);

            //Sum of already funded this month + current
            var totalFunded = investorGradeInvestedAmount + investorLoanCashRequest.ManagerApprovedSum;

            //Calc max score for grade.
            var gradeMaxScore = (double)InvestorParametersDAL.GetGradeMaxScore(investorId, investorLoanCashRequest.Grade);

            //Calc total sum of positive transctions this month
            var totalMonthlyDeposits = InvestorParametersDAL.GetInvestorTotalMonthlyDeposits(investorId);

            var balanceOneMonthAgo = InvestorParametersDAL.GetInvestorBalanceMonthAgo(investorId);

            var investorMonthlyBalance = balanceOneMonthAgo + totalMonthlyDeposits;

            var monthlyFundingCapital = InvestorParametersDAL.GetInvestorMonthlyFundingCapital(investorId);

            var monthlyMax = Math.Max(investorMonthlyBalance, monthlyFundingCapital);

            return gradeMaxScore * monthlyMax - totalFunded;
        }
    }
}

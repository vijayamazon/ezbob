﻿using System;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class DecisionHistoryModel
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public string UnderwriterName { get; set; }

        public bool UseSetupFee { get; set; }
        public decimal InterestRate { get; set; }
        public int RepaymentPeriod { get; set; }
        public decimal ApprovedSum { get; set; }
        public string LoanType { get; set; }
		public int IsLoanTypeSelectionAllowed { get; set; }
        private static RepaymentCalculator _repaymentCalculator;

        public DecisionHistoryModel()
        {
            _repaymentCalculator = new RepaymentCalculator();
        }

        public static DecisionHistoryModel Create(DecisionHistory item)
        {
            var dm = new DecisionHistoryModel()
                                           {
                                               Id = item.Id,
                                               Action = item.Action.ToString(),
                                               Comment = item.Comment,
                                               Date = item.Date,
                                               UnderwriterName = item.Underwriter.FullName,
                                               LoanType = item.LoanType.Name,
                                               DiscountPlan = GetDiscountPlanName(item)
                                           };
            if (item.CashRequest != null)
            {
                dm.RepaymentPeriod = _repaymentCalculator.ReCalculateRepaymentPeriod(item.CashRequest);
                dm.InterestRate = item.CashRequest.InterestRate;
                dm.UseSetupFee = item.CashRequest.UseSetupFee;
                dm.ApprovedSum = item.CashRequest.ApprovedSum();
	            dm.IsLoanTypeSelectionAllowed = item.CashRequest.IsLoanTypeSelectionAllowed;
            }
            return dm;
        }

        public string DiscountPlan { get; set; }

        private static string GetDiscountPlanName(DecisionHistory item)
        {
            if (item.CashRequest == null || item.CashRequest.DiscountPlan == null) return "";
            return item.CashRequest.DiscountPlan.Name;
        }
    }
}
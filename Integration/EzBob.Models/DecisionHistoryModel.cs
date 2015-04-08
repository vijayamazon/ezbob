﻿namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Models;
	using Ezbob.Utils.Extensions;
	using System.Linq;
	using PaymentServices.Calculators;

    public class DecisionHistoryModel
	{
		public int Id { get; set; }
		public string Action { get; set; }
		public DateTime Date { get; set; }
		public string Comment { get; set; }
		public string UnderwriterName { get; set; }

		public decimal InterestRate { get; set; }
		public int RepaymentPeriod { get; set; }
		public decimal ApprovedSum { get; set; }
		public string LoanType { get; set; }
		public int IsLoanTypeSelectionAllowed { get; set; }
		public string Originator { get; set; }
		private static RepaymentCalculator _repaymentCalculator;

		public DecisionHistoryModel()
		{
			_repaymentCalculator = new RepaymentCalculator();
		}

		public static DecisionHistoryModel Create(DecisionHistory item)
		{
			var dm = new DecisionHistoryModel
										   {
											   Id = item.Id,
											   Action = item.Action.ToString(),
											   Comment = item.Comment,
											   Date = item.Date,
											   UnderwriterName = item.Underwriter.FullName,
											   LoanType = item.LoanType.Name,
											   DiscountPlan = GetDiscountPlanName(item),
											   LoanSourceName = GetLoanSourceName(item)
										   };
			if (item.CashRequest != null)
			{
				dm.RepaymentPeriod = _repaymentCalculator.ReCalculateRepaymentPeriod(item.CashRequest);
				dm.InterestRate = item.CashRequest.InterestRate;
				dm.ApprovedSum = item.CashRequest.ApprovedSum();
				dm.IsLoanTypeSelectionAllowed = item.CashRequest.IsLoanTypeSelectionAllowed;
				dm.Originator = item.CashRequest.Originator.HasValue ? item.CashRequest.Originator.DescriptionAttr() : "";

                var setupFeeCalculator = new SetupFeeCalculator(item.CashRequest.ManualSetupFeePercent, item.CashRequest.BrokerSetupFeePercent);
			    dm.TotalSetupFee = setupFeeCalculator.Calculate(dm.ApprovedSum);
                dm.BrokerSetupFee = setupFeeCalculator.CalculateBrokerFee(dm.ApprovedSum);
			}

			if (item.RejectReasons.Any())
			{
				dm.Comment = string.Format("{0} ({1})", 
					item.RejectReasons.Select(x => x.RejectReason.Reason).Aggregate((a, b) => a + ", " + b),
					item.Comment);
			}
			return dm;
		}

        public decimal BrokerSetupFee { get; set; }

        public decimal TotalSetupFee { get; set; }

        public string DiscountPlan { get; set; }

		private static string GetDiscountPlanName(DecisionHistory item)
		{
			if (item.CashRequest == null || item.CashRequest.DiscountPlan == null) return "";
			return item.CashRequest.DiscountPlan.Name;
		}

		public string LoanSourceName { get; set; }

		private static string GetLoanSourceName(DecisionHistory item)
		{
			if (item.CashRequest == null || item.CashRequest.LoanSource == null) return "";
			return item.CashRequest.LoanSource.Name;
		}
	}
}

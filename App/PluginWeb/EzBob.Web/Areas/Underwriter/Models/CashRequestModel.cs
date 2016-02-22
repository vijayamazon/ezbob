namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using PaymentServices.Calculators;

	public class CashRequestModel
	{
		public long Id { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal Amount { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public string Comments { get; set; }
		public int? RepaymentPeriod { get; set; }
		public string Action { get; set; }
		public string UnderwriterDecision { get; set; }
		public string LoanType { get; set; }
		public string DiscountPlan { get; set; }
		public string LoanSourceName { get; set; }
		public string Originator { get; set; }
		public string IsOpenPlatform { get; set; }

		public static CashRequestModel Create(DecisionHistoryDBModel item)
		{
			var setupFeeCalculator = new SetupFeeCalculator(item.ManualSetupFeePercent, item.BrokerSetupFeePercent);
			CashRequestOriginator originator;
			string originatorStr = item.Originator;
			if (Enum.TryParse(item.Originator, out originator)) {
				originatorStr = originator.DescriptionAttr();
			}

			return new CashRequestModel
				{
					Id = item.CashRequestID,
					Action = item.Action,
					Amount = item.ApprovedSum,
					StartDate = item.OfferStart,
					EndDate = item.OfferValidUntil,
					Comments = item.Comment,
					InterestRate = item.InterestRate,
					SetupFee = setupFeeCalculator.Calculate(item.ApprovedSum),
					RepaymentPeriod = item.ApprovedRepaymentPeriod,
					UnderwriterDecision = item.UnderwriterDecision,
					LoanType = item.LoanType,
					DiscountPlan = item.DiscountPlan,
					LoanSourceName = item.LoanSourceName,
					Originator = originatorStr,
					IsOpenPlatform = string.IsNullOrEmpty(item.FundingType) ? "No" : "Yes"
				};
		}

		
	}
}
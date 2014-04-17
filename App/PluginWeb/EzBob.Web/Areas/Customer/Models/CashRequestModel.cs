using System;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Customer.Models
{
	public class CashRequestModel
	{
		public virtual long Id { get; set; }
		public virtual DateTime? StartDate { get; set; }
		public virtual DateTime? EndDate { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual decimal InterestRate { get; set; }
		public virtual bool SetupFee { get; set; }
		public virtual string Comments { get; set; }
		public int RepaymentPeriod { get; set; }
		public string UnderwriterDecision { get; set; }
		public string LoanType { get; set; }
		public string DiscountPlan { get; set; }
		public string LoanSourceName { get; set; }
		public string Originator { get; set; }
		public static CashRequestModel Create(CashRequest c)
		{
			return new CashRequestModel
				{
					Amount =
						c.ManagerApprovedSum.HasValue
							? (decimal) c.ManagerApprovedSum.Value
							: 0,
					StartDate = c.OfferStart,
					EndDate = c.OfferValidUntil,
					Comments = c.UnderwriterComment,
					InterestRate = c.InterestRate,
					SetupFee = c.UseSetupFee,
					Id = c.Id,
					RepaymentPeriod = c.RepaymentPeriod,
					UnderwriterDecision = c.UnderwriterDecision == null ? null : c.UnderwriterDecision.Value.ToString(),
					LoanType = c.LoanType != null ? c.LoanType.Name : string.Empty,
					DiscountPlan = c.DiscountPlan == null ? "" : c.DiscountPlan.Name,
					LoanSourceName = c.LoanSource == null ? "" : c.LoanSource.Name,
					Originator = c.Originator.HasValue ? c.Originator.Value.DescriptionAttr() : ""
				};
		}
	}
}
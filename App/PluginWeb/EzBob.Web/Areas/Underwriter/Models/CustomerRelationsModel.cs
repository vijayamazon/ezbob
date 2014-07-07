namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Linq;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using System;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class CrmModel
	{
		public IOrderedEnumerable<CustomerRelationsModel> CustomerRelations { get; set; }
		public IOrderedEnumerable<FollowUpModel> FollowUps { get; set; }
		public CRMRanks CurrentRank { get; set; }
		public bool IsFollowed { get; set; }
		public string LastStatus { get; set; }
		public CustomerRelationFollowUp LastFollowUp { get; set; }
	}

	public class FollowUpModel
	{
		public int Id { get; set; }
		public DateTime Created { get; set; }
		public DateTime FollowUpDate { get; set; }
		public bool IsClosed { get; set; }
		public string Comment { get; set; }
		public DateTime? CloseDate { get; set; }
	}

	public class CustomerRelationsModel
	{
		public DateTime DateTime { get; set; }
		public string User { get; set; }
		public string Action { get; set; }
		public string Status { get; set; }
		public string Rank { get; set; }
		public string Comment { get; set; }
		public string Type { get; set; }

		public static CustomerRelationsModel Create(CustomerRelations customerRelations)
		{
			return new CustomerRelationsModel
			{
				User = customerRelations.UserName,
				Action = customerRelations.Action==null ? "-" : customerRelations.Action.Name,
				Status = customerRelations.Status== null ? "-" : customerRelations.Status.Name,
				Rank = customerRelations.Rank == null ? "-" : customerRelations.Rank.Name,
				DateTime = customerRelations.Timestamp,
				Comment = customerRelations.Comment,
				Type = customerRelations.Type
			};
		}

		public static CustomerRelationsModel CreateTookLoan(Loan loan)
		{
			return new CustomerRelationsModel
				{
					User = "System",
					Action = "Took a Loan",
					Comment = string.Format("Amount: {0}, Repayments: {1}, Type: {2}, Discount Plan: {3}, Source: {4}", loan.LoanAmount, loan.Repayments, loan.LoanType.Description, loan.CashRequest.DiscountPlan.ValuesStr, loan.LoanSource.Name),
					DateTime = loan.Date,
					Status = loan.Status.ToString()
				};
		}

		public static CustomerRelationsModel CreateRepaidLoan(Loan loan)
		{
			return new CustomerRelationsModel
			{
				User = "System",
				Action = "Repaid a Loan",
				Comment = string.Format("Amount: {0}, Repayments: {1}, Type: {2}, Discount Plan: {3}, Source: {4}", loan.LoanAmount, loan.Repayments, loan.LoanType.Description, loan.CashRequest.DiscountPlan.ValuesStr, loan.LoanSource.Name),
				DateTime = loan.DateClosed.Value,
				Status = loan.Status.ToString()
			};
		}
	}
}
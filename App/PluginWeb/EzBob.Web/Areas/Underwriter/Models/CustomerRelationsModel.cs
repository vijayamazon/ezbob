﻿namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using System;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class CrmStaticModel
	{
		public IEnumerable<CRMActions> CrmActions { get; set; }
		public IEnumerable<CRMStatuses> CrmStatuses { get; set; }
		public IEnumerable<CRMRanks> CrmRanks { get; set; }
	}

	public class CustomerRelationsModel
	{
		public DateTime DateTime { get; set; }
		public string User { get; set; }
		public string Action { get; set; }
		public string Status { get; set; }
		public string Rank { get; set; }
		public string Comment { get; set; }
		public string InOut { get; set; }

		public static CustomerRelationsModel Create(CustomerRelations customerRelations)
		{
			return new CustomerRelationsModel
			{
				User = customerRelations.UserName,
				Action = customerRelations.Action.Name,
				Status = customerRelations.Status.Name,
				Rank = customerRelations.Rank == null ? "-" : customerRelations.Rank.Name,
				DateTime = customerRelations.Timestamp,
				Comment = customerRelations.Comment,
				InOut = customerRelations.Incoming ? "Incoming" : "Outgoing"
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
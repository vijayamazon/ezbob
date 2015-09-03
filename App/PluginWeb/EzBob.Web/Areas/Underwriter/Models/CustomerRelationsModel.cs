namespace EzBob.Web.Areas.Underwriter.Models
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.CustomerRelations;
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class CrmModel
	{
		public string CustomerName { get; set; }
		public string Phone { get; set; }
		public bool IsPhoneVerified { get; set; }

		public IOrderedEnumerable<CustomerRelationsModel> CustomerRelations { get; set; }
		public IOrderedEnumerable<FollowUpModel> FollowUps { get; set; }
		public CRMRanks CurrentRank { get; set; }
		public bool IsFollowed { get; set; }
		public string LastStatus { get; set; }
		public CustomerRelationFollowUp LastFollowUp { get; set; }
		public List<CrmPhoneNumber> PhoneNumbers { get; set; }
		public string CreditResult { get; set; }
		public List<FrequentActionItem> ActionItems { get; set; }
		public ActionItem CostumeActionItem { get; set; }
	}

	public class ActionItem
	{
		public bool IsChecked { get; set; }
		public string Text { get; set; }
	}

	public class FrequentActionItem : ActionItem
	{
		public int Id { get; set; }
	}

	public class CrmPhoneNumber
	{
		public string Number { get; set; }
		public bool IsVerified { get; set; }
		public string Type { get; set; }
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
		public string PhoneNumber { get; set; }

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
				Type = customerRelations.Type,
				PhoneNumber = customerRelations.PhoneNumber
			};
		}

		public static CustomerRelationsModel CreateTookLoan(Loan loan)
		{
			return new CustomerRelationsModel
				{
					User = "System",
					Action = "Took a Loan",
					Comment = string.Format("Amount: {0}, SF: {5}, Repayments: {1}, Type: {2}, Discount Plan: {3}, Source: {4}", 
                        loan.LoanAmount, 
                        loan.Repayments,
						loan.LoanType == null ? "" : loan.LoanType.Description,
						loan.CashRequest == null || loan.CashRequest.DiscountPlan == null ? "" : loan.CashRequest.DiscountPlan.ValuesStr,
						loan.LoanSource == null ? "" : loan.LoanSource.Name, loan.SetupFee),
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
				Comment = string.Format("Amount: {0}, Repayments: {1}, Type: {2}, Discount Plan: {3}, Source: {4}",
                    loan.LoanAmount, 
                    loan.Repayments, 
                    loan.LoanType.Description,
					loan.CashRequest.DiscountPlan == null ? "" : loan.CashRequest.DiscountPlan.ValuesStr,
                    loan.LoanSource.Name),
				DateTime = loan.DateClosed ?? default(DateTime),
				Status = loan.Status.ToString()
			};
		}

	    public static CustomerRelationsModel CreateChangeStatus(CustomerStatusHistory customerStatusHistory) {
            return new CustomerRelationsModel
            {
                User = customerStatusHistory.Username == "se" ? "System" : customerStatusHistory.Username,
                Action = "Change Status",
                Comment = string.Format("description:{0}, previous status:{1}, current status:{2}{3}{4}{5}",
                    customerStatusHistory.Description, 
                    customerStatusHistory.PreviousStatus.Name,
                    customerStatusHistory.NewStatus.Name, 
                    string.IsNullOrEmpty(customerStatusHistory.Feedback) ? "" : ", feedback:" + customerStatusHistory.Feedback,
                    customerStatusHistory.Amount.HasValue ? ", amount:" + customerStatusHistory.Amount.Value : "",
                    customerStatusHistory.ApplyForJudgmentDate.HasValue ? ", apply for judgment date: " + customerStatusHistory.ApplyForJudgmentDate.Value.ToString("dd/MM/yyyy") : ""),
                DateTime = customerStatusHistory.Timestamp,
                Status = customerStatusHistory.NewStatus.Name
            };
	    }

	    public static CustomerRelationsModel CreateCollectionLog(CollectionLog collectionLog) {
            return new CustomerRelationsModel
            {
                User = "System",
                Action = collectionLog.Method,
                Comment = string.Format("{0} was sent to customer because of loan {1}", 
                    collectionLog.Method, 
                    collectionLog.LoanID),
                DateTime = collectionLog.TimeStamp,
                Type = collectionLog.Type,
            };
	    }
	}
}
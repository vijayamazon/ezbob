namespace EzBob.Web.Areas.Underwriter.Models.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class InvestorModel {
		public int InvestorID { get; set; }
		public int InvestorType { get; set; }
		public string InvestorTypeStr { get; set; }
		public string CompanyName { get; set; }
		public bool IsActive { get; set; }
		public DateTime Timestamp { get; set; }
		public int? FundsTransferDate { get; set; }
		public decimal? MonthlyFundingCapital { get; set; }
		public decimal? FundingLimitForNotification { get; set; }
		public IEnumerable<InvestorContactModel> Contacts { get; set; }
		public IEnumerable<InvestorBankAccountModel> Banks { get; set; } 
	}

	public class InvestorModelBuilder {
		public InvestorModel Build(Ezbob.Backend.Models.Investor.InvestorModel investor) {
			var model = new InvestorModel {
				InvestorID = investor.InvestorID,
				IsActive = investor.IsActive,
				CompanyName = investor.Name,
				InvestorType = investor.InvestorType.InvestorTypeID,
				InvestorTypeStr = investor.InvestorType.Name,
				Timestamp = investor.Timestamp,
				Contacts = investor.Contacts.Select(x => new InvestorContactModel{
					IsActive = x.IsActive,
					IsPrimary = x.IsPrimary,
                     IsGettingAlerts = x.IsGettingAlerts,
                     IsGettingReports = x.IsGettingReports,
					Role = x.Role,
					InvestorContactID = x.InvestorContactID,
					Comment = x.Comment,
					ContactPersonalName = x.PersonalName,
					ContactEmail = x.Email,
					ContactLastName = x.LastName,
					ContactMobile = x.Mobile,
					ContactOfficeNumber = x.OfficePhone,
                    TimeStamp = x.Timestamp
				}),
				Banks = investor.Banks.Select(x => new InvestorBankAccountModel{
					IsActive = x.IsActive,
					InvestorBankAccountID = x.InvestorBankAccountID,
					BankAccountNumber = x.BankAccountNumber,
					BankAccountName = x.BankAccountName,
					AccountType = x.AccountType.InvestorAccountTypeID,
					AccountTypeStr = x.AccountType.Name,
					BankSortCode = x.BankCode,
                     TimeStamp = x.Timestamp
				})
			};

			return model;

		}
	}
}
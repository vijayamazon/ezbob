namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using EzBob.Web.Areas.Underwriter.Models.Investor;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;

	using FrontInvestorModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorModel;
	using FrontInvestorContactModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorContactModel;
	using FrontInvestorBankAccountModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorBankAccountModel;

	public class InvestorController : Controller {
		private readonly IEzbobWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private readonly InvestorModelBuilder investorModelBuilder;

		public InvestorController(
			IEzbobWorkplaceContext context,
			ServiceClient serviceClient,
			InvestorModelBuilder investorModelBuilder) {
			this.context = context;
			this.serviceClient = serviceClient;
			this.investorModelBuilder = investorModelBuilder;
		}

		[Ajax]
		[HttpGet]
		public JsonResult Index() {
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult AddInvestor(FrontInvestorModel investor, FrontInvestorContactModel investorContact, List<FrontInvestorBankAccountModel> InvestorBank, bool SameBank) {

			if (SameBank) {
				var bank = InvestorBank.First();
				
				var repaymentsBank = new FrontInvestorBankAccountModel {
					IsActive = true,
					AccountType = (int)I_InvestorAccountTypeEnum.Repayments,
					BankAccountName = bank.BankAccountName,
					BankAccountNumber = bank.BankAccountNumber,
					BankSortCode = bank.BankSortCode
				};

				InvestorBank.Add(repaymentsBank);
			}

			var result = this.serviceClient.Instance.CreateInvestor(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorModel {
					InvestorType = new InvestorTypeModel{ InvestorTypeID = investor.InvestorType },
					Name = investor.CompanyName
				},
				new[] { 
					new Ezbob.Backend.Models.Investor.InvestorContactModel {
								Comment = investorContact.Comment,
								Email = investorContact.ContactEmail,
								IsPrimary = true,
								LastName = investorContact.ContactLastName,
								PersonalName = investorContact.ContactPersonalName,
								Mobile = investorContact.ContactMobile,
								OfficePhone = investorContact.ContactOfficeNumber,
								Role = investorContact.Role,
					}
				},
				InvestorBank.Select(x => new Ezbob.Backend.Models.Investor.InvestorBankAccountModel {
					BankAccountName = x.BankAccountName,
					BankAccountNumber = x.BankAccountNumber,
					BankCode = x.BankSortCode,
					AccountType = new InvestorAccountTypeModel { InvestorAccountTypeID = x.AccountType }
				}).ToArray());

			return Json(new {
				success = true,
				InvestorID = result.Value,
				investor,
				investorContact,
				InvestorBank,
				SameBank
			}, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Add / Update investor contact
		/// </summary>
		[Ajax]
		[HttpPost]
		public JsonResult ManageInvestorContact(int InvestorID, FrontInvestorContactModel contact) {
			
			var result = this.serviceClient.Instance.ManageInvestorContact(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorContactModel {
					InvestorContactID = contact.InvestorContactID,
					InvestorID = InvestorID,
					IsActive = contact.IsActive,
					Comment = contact.Comment,
					Email = contact.ContactEmail,
					IsPrimary = contact.IsPrimary,
					LastName = contact.ContactLastName,
					PersonalName = contact.ContactPersonalName,
					Mobile = contact.ContactMobile,
					OfficePhone = contact.ContactOfficeNumber,
					Role = contact.Role,
				});
			return Json(new { InvestorID, contact, success = result.Value }, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Add / Update investor bank account
		/// </summary>
		[Ajax]
		[HttpPost]
		public JsonResult ManageInvestorBankAccount(int InvestorID, FrontInvestorBankAccountModel bank) {
			
			var result = this.serviceClient.Instance.ManageInvestorBankAccount(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorBankAccountModel {
					InvestorBankAccountID = bank.InvestorBankAccountID,
					InvestorID = InvestorID,
					IsActive = bank.IsActive,
					BankAccountName = bank.BankAccountName,
					BankAccountNumber = bank.BankAccountNumber,
					BankCode = bank.BankSortCode,
					AccountType = new InvestorAccountTypeModel{ InvestorAccountTypeID = bank.AccountType }
				});
			return Json(new { InvestorID, bank, success = result.Value }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult LoadInvestor(int id) {
			var result = this.serviceClient.Instance.LoadInvestor(this.context.UserId, id);
			var investor = this.investorModelBuilder.Build(result.Investor);
			return Json(investor, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult FindInvestor(int customerId) {
			//TODO find investor chosen for pending offer
			return Json(true, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult SubmitInvestor(int customerID, int investorID) {
			//TODO submit investor for pending offer
			return Json(true, JsonRequestBehavior.AllowGet);
		}


	}
}
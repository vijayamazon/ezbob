namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Infrastructure;
	using Infrastructure.Attributes;
	using ServiceClientProxy;

	using FrontInvestorModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorModel;
	using FrontInvestorContactModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorContactModel;
	using FrontInvestorBankAccountModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorBankAccountModel;

	public class InvestorController : Controller {
		private readonly IEzbobWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		public InvestorController(
			IEzbobWorkplaceContext context,
			ServiceClient serviceClient) {
			this.context = context;
			this.serviceClient = serviceClient;
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
				var types = this.serviceClient.Instance.InvestorLoadTypes(this.context.UserId);
				bank.AccountType = int.Parse(types.InvestorBankAccountTypes.First(x => x.Value == "Repayments").Key);
				InvestorBank.Add(bank);
			}

			var result = this.serviceClient.Instance.CreateInvestor(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorModel {
					InvestorTypeID = investor.InvestorType,
					Name = investor.CompanyName
				},
				new[] { new Ezbob.Backend.Models.Investor.InvestorContactModel {
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
					InvestorAccountTypeID = x.AccountType
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

		[Ajax]
		[HttpPost]
		public JsonResult AddEditInvestorContact(int InvestorID, FrontInvestorContactModel contact) {
			//todo implement
			return Json(new { InvestorID, contact, success = true }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult AddEditInvestorBankAccount(int InvestorID, FrontInvestorBankAccountModel bank) {
			//todo implement
			return Json(new { InvestorID, bank, success = true }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetInvestor(int id) {
			var investor = new FrontInvestorModel {
				InvestorID = 1,
				InvestorType = 1,
				CompanyName = "investor1",
				IsActive = true,
				Contacts = new List<FrontInvestorContactModel> {
					new FrontInvestorContactModel {
						InvestorContactID = 1,
						ContactEmail = "a@b.c",
						ContactPersonalName = "John",
						ContactLastName = "Doe",
						IsActive = true,
						IsPrimary = true
					},
					new FrontInvestorContactModel {
						InvestorContactID = 2,
						ContactEmail = "aaa@b.c",
						ContactPersonalName = "John",
						ContactLastName = "Doe",
						IsActive = true,
						IsPrimary = true
					}
				},
				Banks = new List<FrontInvestorBankAccountModel> {
					new FrontInvestorBankAccountModel {
						InvestorBankAccountID = 1,
						BankAccountNumber = "12345678",
						BankAccountName = "Name",
						AccountType = 1,
						BankSortCode = "00000"
					},
					new FrontInvestorBankAccountModel {
						InvestorBankAccountID = 2,
						BankAccountNumber = "87654321",
						BankAccountName = "Name2",
						AccountType = 2,
						BankSortCode = "3156"
					}
				}
			};

			return Json(investor, JsonRequestBehavior.AllowGet);
		}

	}
}
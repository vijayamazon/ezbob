namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Infrastructure;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Infrastructure.Attributes;
	using ServiceClientProxy;

	using FrontInvestorModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorModel;
	using FrontInvestorContactModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorContactModel;
	using FrontInvestorBankAccountModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorBankAccountModel;

	public class InvestorController : Controller {
		private readonly IEzbobWorkplaceContext context;
		private readonly IUsersRepository users;
		private readonly ServiceClient serviceClient;
		public InvestorController(
			IEzbobWorkplaceContext context,
			IUsersRepository users,
			ServiceClient serviceClient) {
			this.context = context;
			this.users = users;
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
				bank.AccountType = types.InvestorBankAccountTypes.First(x => x.Value == "Repayments").Key;
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
					InvestorAccountTypeID = int.Parse(x.AccountType)
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
		[HttpGet]
		public JsonResult GetAllInvestors() {
			IEnumerable<FrontInvestorModel> investors = new List<FrontInvestorModel> {
				new FrontInvestorModel { InvestorID = 1, InvestorType = 1, CompanyName = "investor1" },
				new FrontInvestorModel { InvestorID = 2, InvestorType = 2, CompanyName = "investor2" }
			};

			return Json(investors, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetInvestor(int id) {
			var investor = new FrontInvestorModel {
				InvestorID = 1,
				InvestorType = 1,
				CompanyName = "investor1"
			};

			return Json(investor, JsonRequestBehavior.AllowGet);
		}

	}
}
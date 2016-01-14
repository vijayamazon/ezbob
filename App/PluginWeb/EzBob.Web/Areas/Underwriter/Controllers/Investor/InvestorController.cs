namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using EzBob.Web.Areas.Underwriter.Models.Investor;
	using Infrastructure;
	using Infrastructure.Attributes;
	using log4net;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using FrontInvestorModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorModel;
	using FrontInvestorContactModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorContactModel;
	using FrontInvestorBankAccountModel = EzBob.Web.Areas.Underwriter.Models.Investor.InvestorBankAccountModel;

	public class InvestorController : Controller {
		private readonly IEzbobWorkplaceContext context;
		private readonly ServiceClient serviceClient;
		private readonly InvestorModelBuilder investorModelBuilder;
		
		protected static readonly ILog Log = LogManager.GetLogger(typeof(InvestorController));

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
					InvestorType = new Ezbob.Backend.Models.Investor.InvestorTypeModel { 
						InvestorTypeID = investor.InvestorType 
					},
					Name = investor.CompanyName,
					FundingLimitForNotification = investor.FundingLimitForNotification,
					MonthlyFundingCapital = investor.MonthlyFundingCapital,
					FundsTransferDate = investor.FundsTransferDate
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
		[HttpGet]
		public JsonResult GetAccountingData() {

			AccountingDataResult result = this.serviceClient.Instance.LoadAccountingData(this.context.UserId);

			return Json(new { AccountingList = result.AccountingData }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetTransactionsData(int id, string bankAccountType) {
			Log.InfoFormat("GetTransactionsData for InvestorID={0}", id);
			I_InvestorAccountTypeEnum accountTypeEnum;
			if(!Enum.TryParse(bankAccountType, out accountTypeEnum)) {
				throw new Exception("Wrong account type");
			}

			TransactionsDataResult result = this.serviceClient.Instance.LoadTransactionsData(this.context.UserId, id, (int)accountTypeEnum);

			return Json(new { TransactionsList = result.TransactionsData }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult AddTransaction(int investorID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, string bankAccountType, string transactionComment) {

			I_InvestorAccountTypeEnum accountTypeEnum;
			if (!Enum.TryParse(bankAccountType, out accountTypeEnum)) {
				throw new Exception("Wrong account type");
			}

			var result = this.serviceClient.Instance.AddManualTransaction(this.context.UserId, investorAccountID, transactionAmount, transactionDate, (int)accountTypeEnum, transactionComment);

			return Json(new { investorID, bankAccountType, success = result.Value }, JsonRequestBehavior.AllowGet);
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
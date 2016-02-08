namespace EzBob.Web.Areas.Underwriter.Controllers.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Models.Investor;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using EzBob.Web.Areas.Underwriter.Models.Investor;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EZBob.DatabaseLib.Model.Database;
	using log4net;
	using Newtonsoft.Json;
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
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index() {
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
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
					InvestorType = new InvestorTypeModel {
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
		[ValidateJsonAntiForgeryToken]
		public JsonResult ManageInvestorContact(int InvestorID, FrontInvestorContactModel contact) {

			var result = this.serviceClient.Instance.ManageInvestorContact(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorContactModel {
					InvestorContactID = contact.InvestorContactID,
					InvestorID = InvestorID,
					IsGettingAlerts = contact.IsGettingAlerts,
					IsGettingReports = contact.IsGettingReports,
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

		[Ajax]
		[HttpPost]
		public JsonResult ManageInvestorDetails(int InvestorID, FrontInvestorModel investorDetails) {
			DateTime now = DateTime.UtcNow;
			Log.Debug("investor funding limit string " + investorDetails.FundingLimitForNotification);
			var result = this.serviceClient.Instance.ManageInvestorDetails(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorModel {

					InvestorID = InvestorID,
					IsActive = investorDetails.IsActive,
					Name = investorDetails.CompanyName,
					FundingLimitForNotification = investorDetails.FundingLimitForNotification,
					InvestorType = new InvestorTypeModel {
						InvestorTypeID = investorDetails.InvestorType
					},
					Timestamp = now,
					Contacts = null,
					Banks = null


				});
			return Json(new { InvestorID, investorDetails, success = result.Value }, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Edit all investor Contacts toghether 
		/// </summary>
		[Ajax]
		[HttpPost]
		public JsonResult SaveInvestorContactList(int InvestorID, string investor) {

			Log.Debug("investor string " + investor);
			var investorModel = JsonConvert.DeserializeObject<FrontInvestorModel>(investor);

			Log.DebugFormat("investor string {0} {1}", investorModel.Contacts.Count(), investorModel.Contacts.First().ContactEmail);
			//call the service
			var contacts = investorModel.Contacts.Select(x => new Ezbob.Backend.Models.Investor.InvestorContactModel {
				InvestorContactID = x.InvestorContactID,
				InvestorID = InvestorID,
				IsActive = x.IsActive,
				Comment = x.Comment,
				Email = x.ContactEmail,
				IsPrimary = x.IsPrimary,
				IsGettingAlerts = x.IsGettingAlerts,
				IsGettingReports = x.IsGettingReports,
				LastName = x.ContactLastName,
				PersonalName = x.ContactPersonalName,
				Mobile = x.ContactMobile,
				OfficePhone = x.ContactOfficeNumber,
				Role = x.Role
			}).ToArray();
			var result = this.serviceClient.Instance.SaveInvestorContactList(this.context.UserId, InvestorID, contacts);
			return Json(new { success = result.Value }, JsonRequestBehavior.AllowGet);
		}
		/// <summary>
		/// Edit all investor Banks toghether 
		/// </summary>
		[Ajax]
		[HttpPost]
		public JsonResult SaveInvestorBanksList(int InvestorID, string investor) {

			Log.Debug("investor string " + investor);
			var investorModel = JsonConvert.DeserializeObject<FrontInvestorModel>(investor);

			Log.DebugFormat("investor string {0} {1}", investorModel.Contacts.Count(), investorModel.Contacts.First().ContactEmail);
			//call the service
			var Banks = investorModel.Banks.Select(x => new Ezbob.Backend.Models.Investor.InvestorBankAccountModel {

				InvestorBankAccountID = x.InvestorBankAccountID,
				InvestorID = InvestorID,
				IsActive = x.IsActive,
				BankName = x.BankAccountName,
				BankCode = x.BankSortCode,
				BankAccountName = x.BankAccountName,
				BankAccountNumber = x.BankAccountNumber,
				AccountType = new InvestorAccountTypeModel {
					InvestorAccountTypeID = x.AccountType,
					Name = x.AccountTypeStr
				}

			}).ToArray();
			var result = this.serviceClient.Instance.SaveInvestorBanksList(this.context.UserId, InvestorID, Banks);
			return Json(new { success = result.Value }, JsonRequestBehavior.AllowGet);
		}
		/// <summary>
		/// Add / Update investor bank account
		/// </summary>
		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ManageInvestorBankAccount(int InvestorID, FrontInvestorBankAccountModel bank) {

			var result = this.serviceClient.Instance.ManageInvestorBankAccount(this.context.UserId,
				new Ezbob.Backend.Models.Investor.InvestorBankAccountModel {
					InvestorBankAccountID = bank.InvestorBankAccountID,
					InvestorID = InvestorID,
					IsActive = bank.IsActive,
					BankAccountName = bank.BankAccountName,
					BankAccountNumber = bank.BankAccountNumber,
					BankCode = bank.BankSortCode,
					AccountType = new InvestorAccountTypeModel { InvestorAccountTypeID = bank.AccountType }
				});
			return Json(new { InvestorID, bank, success = result.Value }, JsonRequestBehavior.AllowGet);
		}


		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult LoadInvestor(int id) {
			var result = this.serviceClient.Instance.LoadInvestor(this.context.UserId, id);
			var investor = this.investorModelBuilder.Build(result.Investor);
			return Json(investor, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetAccountingData() {

			AccountingDataResult result = this.serviceClient.Instance.LoadAccountingData(this.context.UserId);

			return Json(new { AccountingList = result.AccountingData }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult GetTransactionsData(int id, string bankAccountType) {
			Log.InfoFormat("GetTransactionsData for InvestorID={0}", id);
			I_InvestorAccountTypeEnum accountTypeEnum;
			if (!Enum.TryParse(bankAccountType, out accountTypeEnum)) {
				throw new Exception("Wrong account type");
			}

			TransactionsDataResult result = this.serviceClient.Instance.LoadTransactionsData(this.context.UserId, id, (int)accountTypeEnum);

			return Json(new { TransactionsList = result.TransactionsData }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult AddTransaction(int investorID, int investorAccountID, decimal transactionAmount, DateTime transactionDate, string bankAccountType, string transactionComment, string bankTransactionRef) {

			I_InvestorAccountTypeEnum accountTypeEnum;
			if (!Enum.TryParse(bankAccountType, out accountTypeEnum)) {
				throw new Exception("Wrong account type");
			}

			var result = this.serviceClient.Instance.AddManualTransaction(this.context.UserId, investorAccountID, transactionAmount, transactionDate, (int)accountTypeEnum, transactionComment, bankTransactionRef);

			return Json(new { investorID, bankAccountType, success = result.Value }, JsonRequestBehavior.AllowGet);

		}
		[Ajax]
		[HttpGet]

		public JsonResult GetInvestors() {

			ListInvestorsResult result = this.serviceClient.Instance.LoadInvestors(this.context.UserId);

			return Json(new { Investors = result.Investors }, JsonRequestBehavior.AllowGet);

		}

		[Ajax]
		[HttpGet]
		public JsonResult GetSchedulerData(int id) {
			Log.InfoFormat("Get Scheduler Data for InvestorID={0}", id);

			SchedulerDataResult result = this.serviceClient.Instance.LoadSchedulerData(this.context.UserId, id);


			return Json(new { InvestorID = id, SchedulerObject = result.SchedulerData }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult SubmitSchedulerData(int investorID, decimal monthlyFundingCapital, int fundsTransferDate, string fundsTransferSchedule, string repaymentsTransferSchedule) {

			var result = this.serviceClient.Instance.UpdateSchedulerData(this.context.UserId, investorID, monthlyFundingCapital, fundsTransferDate, fundsTransferSchedule, repaymentsTransferSchedule);

			return Json(new { investorID, success = result.Value }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult FindInvestor(int customerId, long cashRequestID) {
			var aiar = this.serviceClient.Instance.LoadApplicationInfo(
				this.context.UserId,
				customerId,
				cashRequestID,
				DateTime.UtcNow
			);

			var result = this.serviceClient.Instance.SetManualDecision(new DecisionModel {
				customerID = customerId,
				status = CreditResultStatus.PendingInvestor,
				underwriterID = this.context.UserId,
				attemptID = Guid.NewGuid().ToString("N"),
				cashRequestID = cashRequestID,
				cashRequestRowVersion = aiar.Model.CashRequestRowVersion
			});

			return Json(result.Map, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult SubmitInvestor(int customerID, int investorID, long cashRequestID) {
			var aiar = this.serviceClient.Instance.LoadApplicationInfo(
				this.context.UserId,
				customerID,
				cashRequestID,
				DateTime.UtcNow
			);

			var result = this.serviceClient.Instance.SetManualDecision(new DecisionModel {
				customerID = customerID,
				status = CreditResultStatus.Approved,
				underwriterID = this.context.UserId,
				attemptID = Guid.NewGuid().ToString("N"),
				cashRequestID = cashRequestID,
				cashRequestRowVersion = aiar.Model.CashRequestRowVersion,
				ForceInvestor = true,
				InvestorID = investorID

			});

			return Json(result.Map, JsonRequestBehavior.AllowGet);
		}


	}
}
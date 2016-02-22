// ReSharper disable InconsistentNaming
namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using Web.Areas.Customer.Models;

	public class CompanyInfoMap {
		public string BusinessPhone { get; set; }

		public double? CapitalExpenditure { get; set; }

		public string CompanyName { get; set; }

		public string CompanyNumber { get; set; }

		public virtual List<DirectorModel> Directors { get; set; }

		public string ExperianCompanyName { get; set; }

		public string ExperianRefNum { get; set; }

		public bool? PropertyOwnedByCompany { get; set; }

		public string RentMonthLeft { get; set; }

		public int? TimeAtAddress { get; set; }

		public string TimeInBusiness { get; set; }

		public TypeOfBusiness TypeOfBusiness { get; set; }

		public TypeOfBusinessReduced TypeOfBusinessReduced { get; set; }
		public bool IsRegulated { get { return TypeOfBusiness.IsRegulated(); } }

		public VatReporting? VatReporting { get; set; }
		public bool VatRegistered { get; set; }
		public string YearsInCompany { get; set; }
		public static CompanyInfoMap FromCompany(Company company) {
			if (company == null)
				return new CompanyInfoMap();

			return new CompanyInfoMap {
				CompanyNumber = company.CompanyNumber,
				CompanyName = company.ExperianCompanyName ?? company.CompanyName,
				TimeAtAddress = company.TimeAtAddress,
				BusinessPhone = company.BusinessPhone,
				TypeOfBusiness = company.TypeOfBusiness,
				TypeOfBusinessReduced = company.TypeOfBusiness.Reduce(),
				TimeInBusiness = company.TimeInBusiness,
				PropertyOwnedByCompany = company.PropertyOwnedByCompany,
				YearsInCompany = company.YearsInCompany,
				RentMonthLeft = company.RentMonthLeft,
				CapitalExpenditure = company.CapitalExpenditure,
				VatReporting = company.VatReporting,
				Directors = company.Directors.Select(d => DirectorModel.FromDirector(d, company.Directors.Where(x=> !x.IsDeleted).ToList())).ToList(),
				ExperianRefNum = company.ExperianRefNum,
				VatRegistered = company.VatRegistered
			};
		} // From Company
	} // class CompanyInfoMap

	[Serializable]
	public class CustomerModel {
		public AccountSettingsModel AccountSettings { get; set; }

		public string accountType { get; set; }

		public IEnumerable<PaymentRollover> ActiveRollovers { get; set; }

		public int ApplyCount { get; set; }

		public bool bankAccountAdded { get; set; }

		public string BankAccountNumber { get; set; }

		public string BusinessTypeReduced { get; set; }

		public bool CanHaveDirectors { get; set; }

		public CustomerAddress[] CompanyAddress { get; set; }

		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }

		public CompanyInfoMap CompanyInfo { get; set; }

		public string CreditResult { get; set; }

		public decimal? CreditSum { get; set; }

		public PersonalInfo CustomerPersonalInfo { get; set; }

		public string CustomerStatusName { get; set; }

		public bool DefaultCardSelectionAllowed { get; set; }

		public string Email { get; set; }

		public string EmailState { get; set; }

		public DateTime? GreetingMailSentDate { get; set; }

		public bool HasRollovers { get; set; }

		public bool? HasApprovalChance { get; set; }

		public int Id { get; set; }

		public string RefNumber { get; set; }

		public bool IsAlibaba { get; set; }

		public bool IsBrokerFill { get; set; }

		public bool IsCurrentCashRequestFromQuickOffer { get; set; }

		public bool IsDefaultCustomerStatus { get; set; }

		public bool IsDisabled { get; set; }

		public bool IsEarly { get; set; }

		public bool IsTest { get; set; }

		public bool IsLastApprovedLoanSourceEu { get; set; }
		public bool IsLastApprovedLoanSourceCOSME { get; set; }

		public int SignedLegalID { get; set; }
		public long LastCashRequestID { get; set; }

		public bool IsLoanDetailsFixed { get; set; }

		public int IsLoanTypeSelectionAllowed { get; set; }

		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		public bool? IsOffline { get; set; }

		//public long LoyaltyPoints { get; set; }
		public bool IsProfile { get; set; }

		public bool IsWhiteLabel { get; set; }

		public int LastApprovedLoanTypeID { get; set; }

		public int LastApprovedRepaymentPeriod { get; set; }

		public int LastRepaymentPeriod { get; set; }

		public decimal LastPaymentFees { get; set; }

		public decimal LastPaymentInterest { get; set; }

		public decimal LastPaymentPrincipal { get; set; }

		public decimal LastPaymentTotal { get; set; }

		public string LastSavedWizardStep { get; set; }

		public IEnumerable<LoanModel> Loans { get; set; }

		public bool loggedIn { get; set; }
		public string Medal { get; set; }

		public IEnumerable<SimpleMarketPlaceModel> mpAccounts { get; set; }

		public decimal NextPayment { get; set; }

		public DateTime? NextPaymentDate { get; set; }

		public DateTime? OfferStart { get; set; }

		public DateTime? OfferValidUntil { get; set; }

		public CustomerAddress[] OtherPropertiesAddresses { get; set; }

		public PayPointCardModel[] PayPointCards { get; set; }

		//public string Perks { get; set; }

		public CustomerAddress[] PersonalAddress { get; set; }

		public CustomerAddress[] PrevPersonAddresses { get; set; }

		public decimal PrincipalBalance { get; set; }

		public PropertyStatusModel PropertyStatus { get; set; }

		public QuickOfferModel QuickOffer { get; set; }

		public CustomerRequestedLoan RequestedLoan { get; set; }

		public string SortCode { get; set; }

		public string Status { get; set; }

		public decimal TotalBalance { get; set; }

		public decimal TotalEarlyPayment { get; set; }

		public decimal TotalLatePayment { get; set; }

		public decimal TotalPayEarlySavings { get; set; }

		public bool TrustPilotReviewEnabled { get; set; }

		public int TrustPilotStatusID { get; set; }

		public string userName { get; set; }

		public int WhiteLabelId { get; set; }

		public bool BlockTakingLoan { get; set; }

		public string LotteryPlayerID { get; set; }
		public string LotteryCode { get; set; }
		public string Origin { get; set; }
		public bool IsEverline { get; set; } //used to show popup in wizard
		
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }

		public int LastApprovedAmount { get; set; }

	} // class CustomerModel

	public class SimpleMarketPlaceModel {
		public string displayName { get; set; }
		public int MpId { get; set; }
		public string MpName { get; set; }
		//public string storeInfoStepModelShops { get; set; }
	} // class SimpleMarketPlaceModel
} // namespace
// ReSharper restore InconsistentNaming

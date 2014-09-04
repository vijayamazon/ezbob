// ReSharper disable InconsistentNaming
namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Loans;
	using Ezbob.Backend.Models;
	using Web.Areas.Customer.Models;

	#region class CustomerModel

	[Serializable]
	public class CustomerModel {
		public int Id { get; set; }
		public bool loggedIn { get; set; }
		public bool bankAccountAdded { get; set; }

		public string userName { get; set; }
		public string accountType { get; set; }

		public PropertyStatusModel PropertyStatus { get; set; }

		public PersonalInfo CustomerPersonalInfo { get; set; }

		public string BusinessTypeReduced { get; set; }

		public IEnumerable<SimpleMarketPlaceModel> mpAccounts { get; set; }

		public int LastApprovedLoanTypeID { get; set; }
		public int LastApprovedRepaymentPeriod { get; set; }

		public decimal? CreditSum { get; set; }
		public string CreditResult { get; set; }
		public DateTime? OfferStart { get; set; }
		public DateTime? OfferValidUntil { get; set; }
		public string Status { get; set; }

		public IEnumerable<LoanModel> Loans { get; set; }

		public AccountSettingsModel AccountSettings { get; set; }

		public decimal TotalBalance { get; set; }
		public decimal PrincipalBalance { get; set; }

		public decimal NextPayment { get; set; }
		public DateTime? NextPaymentDate { get; set; }

		public DateTime? GreetingMailSentDate { get; set; }

		public decimal LastPaymentTotal { get; set; }
		public decimal LastPaymentPrincipal { get; set; }
		public decimal LastPaymentInterest { get; set; }
		public decimal LastPaymentFees { get; set; }

		public string BankAccountNumber { get; set; }
		public string SortCode { get; set; }

		public PayPointCardModel[] PayPointCards { get; set; }

		public string Medal { get; set; }

		public CompanyInfoMap CompanyInfo { get; set; }

		public bool CanHaveDirectors { get; set; }

		public CustomerAddress[] PersonalAddress { get; set; }

		public CustomerAddress[] PrevPersonAddresses { get; set; }

		public CustomerAddress[] OtherPropertiesAddresses { get; set; }

		public CustomerAddress[] CompanyAddress { get; set; }

		public int ApplyCount { get; set; }

		public string CreditCardNo { get; set; }

		public string Email { get; set; }

		public decimal TotalEarlyPayment { get; set; }

		public decimal TotalPayEarlySavings { get; set; }

		public string EmailState { get; set; }

		public IEnumerable<PaymentRollover> ActiveRollovers { get; set; }

		public string CustomerStatusName { get; set; }

		public bool IsDefaultCustomerStatus { get; set; }

		public bool IsDisabled { get; set; }

		public bool HasRollovers { get; set; }

		public decimal TotalLatePayment { get; set; }

		public bool IsLoanDetailsFixed { get; set; }

		public int IsLoanTypeSelectionAllowed { get; set; }

		//public long LoyaltyPoints { get; set; }

		public bool? IsOffline { get; set; }

		public bool IsProfile { get; set; }

		public bool IsEarly { get; set; }

		public CompanyEmployeeCountInfo CompanyEmployeeCountInfo { get; set; }

		public string InviteFriendSource { get; set; }
		public IEnumerable<InvitedFriend> InvitedFriends { get; set; }
		public bool IsLastApprovedLoanSourceEu { get; set; }

		public string LastSavedWizardStep { get; set; }
		public string Perks { get; set; }

		public int TrustPilotStatusID { get; set; }
		public bool TrustPilotReviewEnabled { get; set; }

		public bool IsCurrentCashRequestFromQuickOffer { get; set; }
		public QuickOfferModel QuickOffer { get; set; }
		public decimal RequestedAmount { get; set; }
		public bool IsBrokerFill { get; set; }
		public bool DefaultCardSelectionAllowed { get; set; }

		public bool IsWhiteLabel { get; set; }
		public int WhiteLabelId { get; set; }
	} // class CustomerModel

	#endregion class CustomerModel

	#region class SimpleMarketPlaceModel

	public class SimpleMarketPlaceModel {
		public string MpName { get; set; }
		public int MpId { get; set; }
		public string displayName { get; set; }
		//public string storeInfoStepModelShops { get; set; }
	} // class SimpleMarketPlaceModel

	#endregion class SimpleMarketPlaceModel

	#region class InvitedFriend

	public class InvitedFriend {
		public string FriendName { get; set; }
		public string FriendTookALoan { get; set; }
	} // class InvitedFriend

	#endregion class InvitedFriend

	#region class CompanyInfoMap

	public class CompanyInfoMap {
		public TypeOfBusiness TypeOfBusiness { get; set; }
		public VatReporting? VatReporting { get; set; }
		public string CompanyNumber { get; set; }
		public string CompanyName { get; set; }
		public int? TimeAtAddress { get; set; }
		public string TimeInBusiness { get; set; }
		public string BusinessPhone { get; set; }
		public bool? PropertyOwnedByCompany { get; set; }
		public string YearsInCompany { get; set; }
		public string RentMonthLeft { get; set; }
		public double? CapitalExpenditure { get; set; }
		public string ExperianRefNum { get; set; }
		public string ExperianCompanyName { get; set; }

		public virtual List<DirectorModel> Directors { get; set; }

		#region method FromCompany

		public static CompanyInfoMap FromCompany(Company company) {
			if (company == null)
				return new CompanyInfoMap();

			return new CompanyInfoMap {
				CompanyNumber = company.CompanyNumber,
				CompanyName = company.ExperianCompanyName ?? company.CompanyName,
				TimeAtAddress = company.TimeAtAddress,
				BusinessPhone = company.BusinessPhone,
				TypeOfBusiness = company.TypeOfBusiness,
				TimeInBusiness = company.TimeInBusiness,
				PropertyOwnedByCompany = company.PropertyOwnedByCompany,
				YearsInCompany = company.YearsInCompany,
				RentMonthLeft = company.RentMonthLeft,
				CapitalExpenditure = company.CapitalExpenditure,
				VatReporting = company.VatReporting,
				Directors = company.Directors.Select(d => DirectorModel.FromDirector(d, new List<Director>(company.Directors))).ToList(),
				ExperianRefNum = company.ExperianRefNum,
			};
		} // From Company

		#endregion method FromCompany
	} // class CompanyInfoMap

	#endregion class CompanyInfoMap
} // namespace
// ReSharper restore InconsistentNaming
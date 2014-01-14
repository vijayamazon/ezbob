using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Email;
using EzBob.CommonLib;
using Ezbob.ExperianParser;
using Ezbob.Logger;
using Iesi.Collections.Generic;
using NHibernate.Type;
using Scorto.NHibernate.Types;
using StructureMap;
using log4net;

namespace EZBob.DatabaseLib.Model.Database
{
	using System.ComponentModel;

	#region relevant to Customer class

	#region enum PendingStatus

	public enum PendingStatus
	{
		AML,
		Bank,
		Bank_AML,
		Manual
	} // enum PendingStatus

	public class PendingStatusType : EnumStringType<PendingStatus> { }

	#endregion enum PendingStatus

	#region enum SystemDecision

	public enum SystemDecision
	{
		Approve,
		Reject,
		Manual
	} // enum SystemDecision

	public class SystemDecisionType : EnumStringType<SystemDecision> { }

	#endregion enum SystemDecision

	#region enum Status

	public enum Status
	{
		Registered,
		Approved,
		Rejected,
		Manual
	} // enum Status

	public class StatusType : EnumStringType<Status> { }

	#endregion enum Status

	#region enum CreditResultStatus

	public enum CreditResultStatus
	{
		WaitingForDecision,
		Escalated,
		Rejected,
		Approved,
		CustomerRefused,
		ApprovedPending,
		Active,
		Collection,
		Legal,
		PaidOff,
		WrittenOff,
		Late
	} // CreditResultStatus

	public class CreditResultStatusType : EnumStringType<CreditResultStatus> { }

	#endregion enum CreditResultStatus

	#region enum FraudStatus

	public enum FraudStatus
	{
		[Description("Ok")]
		Ok = 0,
		[Description("Fishy")]
		Fishy = 1,
		[Description("Fraud Suspect")]
		FraudSuspect = 2,
		[Description("Under Investigation")]
		UnderInvestigation = 3,
		[Description("Fraud Done")]
		FraudDone = 4,
		[Description("Identity/Details Theft")]
		IdentityOrDetailsTheft = 5
	} // enum FraudStatus

	public class GenderType : EnumStringType<Gender> { }

	#endregion enum FraudStatus

	public class MaritalStatusType : EnumStringType<MaritalStatus> { }

	
	public class MedalType : CaseInsensitiveEnumStringType<Medal> { }

	#region enum WizardStepType

	public enum WizardStepType
	{
		SignUp = 1,
		Marketplace = 2,
		PaymentAccounts = 3, // just because there are some customers in that state in DB
		AllStep = 4,
		PersonalDetails = 5,
		CompanyDetails = 6,
	} // enum WizardStepType

	#endregion enum WizardStepType

	#region enum BankAccountType

	public enum BankAccountType
	{
		Unknown,
		Personal,
		Business
	} // enum BankAccountType

	public class BankAccountTypeType : CaseInsensitiveEnumStringType<BankAccountType> { }

	#endregion enum BankAccountType

	#region enum PaymentdemeanorType

	public enum PaymentdemeanorType
	{
		Late,
		WasLate,
		Ok
	} // enum PaymentdemeanorType

	#endregion enum PaymentdemeanorType

	#region class BankAccount

	public class BankAccount
	{
		public string AccountNumber { get; set; }
		public string SortCode { get; set; }
		public BankAccountType Type { get; set; }
	} // class BankAccount

	#endregion class BankAccount

	#region class ParseExperianResult

	public class ParseExperianResult : Tuple<Dictionary<string, ParsedData>, ParsingResult, string, string, string>
	{
		public Dictionary<string, ParsedData> Dataset { get { return Item1; } } // Dataset
		public ParsingResult ParsingResult { get { return Item2; } } // ParsingResult
		public string ErrorMsg { get { return Item3; } } // ErrorMsg
		public string CompanyRefNum { get { return Item4; } } // CompanyRefNum
		public string CompanyName { get { return Item5; } } // CompanyName

		public ParseExperianResult(
			Dictionary<string, ParsedData> dataset,
			ParsingResult parsingResult,
			string errorMsg,
			string companyRefNum,
			string companyName
		)
			: base(dataset, parsingResult, errorMsg, companyRefNum, companyName)
		{ } // constructor
	} // class ParseExperianResult

	#endregion class ParseExperianResult

	//todo remove classes LimitedInfo/NonLimitedInfo/CompanyAdditionalInfo when new wizard
	#region class LimitedInfo

	public class LimitedInfo {
		public string LimitedRefNum { get; set; }
		public string LimitedCompanyNumber { get; set; }
		public string LimitedCompanyName { get; set; }
		public int? LimitedTimeAtAddress { get; set; }
		public bool? LimitedConsentToSearch { get; set; }
		public string LimitedBusinessPhone { get; set; }

		#region property Directors

		private Iesi.Collections.Generic.ISet<Director> _directors = new HashedSet<Director>();

		public virtual Iesi.Collections.Generic.ISet<Director> Directors {
			get { return _directors; }
			set { _directors = value; }
		} // Directors

		#endregion property Directors
	} // class LimitedInfo

	#endregion class LimitedInfo

	#region class NonLimitedInfo

	public class NonLimitedInfo {
		public string NonLimitedRefNum { get; set; }
		public string NonLimitedCompanyName { get; set; }
		public string NonLimitedTimeInBusiness { get; set; }
		public int? NonLimitedTimeAtAddress { get; set; }
		public bool? NonLimitedConsentToSearch { get; set; }
		public string NonLimitedBusinessPhone { get; set; }

		#region property Directors

		private Iesi.Collections.Generic.ISet<Director> _directors = new HashedSet<Director>();

		public virtual Iesi.Collections.Generic.ISet<Director> Directors {
			get { return _directors; }
			set { _directors = value; }
		} // Directors

		#endregion property Directors
	} // class NonLimitedInfo

	#endregion class NonLimitedInfo

	#region class CompanyAdditionalInfo

	public class CompanyAdditionalInfo {
		public bool? PropertyOwnedByCompany { get; set; }
		public string YearsInCompany { get; set; }
		public string RentMonthsLeft { get; set; }
		public double TotalMonthlySalary { get; set; }
		public double? CapitalExpenditure { get; set; }

		public string ExperianCompanyName { get; set; }
		public string ExperianCompanyAddrLine1 { get; set; }
		public string ExperianCompanyAddrLine2 { get; set; }
		public string ExperianCompanyAddrLine3 { get; set; }
		public string ExperianCompanyAddrLine4 { get; set; }
		public string ExperianCompanyPostcode { get; set; }
	} // class CompanyAdditionalInfo

	#endregion class CompanyAdditionalInfo

	#region class PersonalInfo

	public class PersonalInfo
	{
		public string FirstName { get; set; }
		public string MiddleInitial { get; set; }
		public string Surname { get; set; }
		public virtual string Fullname { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public int? TimeAtAddress { get; set; }
		public string ResidentialStatus { get; set; }
		public Gender Gender { get; set; }
		public MaritalStatus MaritalStatus { get; set; }
		public TypeOfBusiness TypeOfBusiness { get; set; }
		public string TypeOfBusinessName { get { return TypeOfBusiness.ToString(); } }
		public string DaytimePhone { get; set; }
		public string MobilePhone { get; set; }
		public decimal? OverallTurnOver { get; set; }
		public decimal? WebSiteTurnOver { get; set; }

		public string BirthDateYMD()
		{
			return DateOfBirth.HasValue ? DateOfBirth.Value.ToString("yyyy-M-d", CultureInfo.InvariantCulture) : "";
		} // BirthDateYMD
	} // class PersonalInfo

	#endregion class PersonalInfo

	#region class AddressInfo

	public class AddressInfo
	{
		#region property PersonalAddress

		private Iesi.Collections.Generic.ISet<CustomerAddress> _personalAddress = new HashedSet<CustomerAddress>();
		public virtual Iesi.Collections.Generic.ISet<CustomerAddress> PersonalAddress
		{
			get { return _personalAddress; }
			set { _personalAddress = value; }
		} // PersonalAddress

		#endregion property PersonalAddress

		#region property PrevPersonAddresses

		private Iesi.Collections.Generic.ISet<CustomerAddress> _prevPersonAddresses = new HashedSet<CustomerAddress>();
		public virtual Iesi.Collections.Generic.ISet<CustomerAddress> PrevPersonAddresses
		{
			get { return _prevPersonAddresses; }
			set { _prevPersonAddresses = value; }
		} // PrevPersonAddresses

		#endregion property PrevPersonAddresses

		#region property AllAddresses

		private Iesi.Collections.Generic.ISet<CustomerAddress> _allAddresses = new HashedSet<CustomerAddress>();
		public virtual Iesi.Collections.Generic.ISet<CustomerAddress> AllAddresses
		{
			get { return _allAddresses; }
			set { _allAddresses = value; }
		} // AllAddresses

		#endregion property AllAddresses

		#region property OtherPropertyAddress

		private Iesi.Collections.Generic.ISet<CustomerAddress> _otherPropertyAddress = new HashedSet<CustomerAddress>();
		public virtual Iesi.Collections.Generic.ISet<CustomerAddress> OtherPropertyAddress
		{
			get { return _otherPropertyAddress; }
			set { _otherPropertyAddress = value; }
		} // OtherPropertyAddress

		#endregion property OtherPropertyAddress
	} // class AddressInfo

	#endregion class AddressInfo

	#region class ScoringResult

	public class ScoringResult
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual string ACParameters { get; set; }
		public virtual string ACDescription { get; set; }
		public virtual string Weights { get; set; }
		public virtual string MAXPossiblePoints { get; set; }
		public virtual DateTime ScoreDate { get; set; }
		public virtual string Medal { get; set; }
		public virtual double ScorePoints { get; set; }
		public virtual double ScoreResult { get; set; }
	} // class ScoringResult

	#endregion class ScoringResult

	

	#region class CollectionStatus

	[Serializable]
	public class CollectionStatus
	{
		[Newtonsoft.Json.JsonIgnore]
		public virtual CustomerStatuses CurrentStatus { get; set; }
		public virtual string CollectionDescription { get; set; }
	} // class CollectionStatus

	#endregion class CollectionStatus

	#endregion relevant to Customer class

	#region class Customer

	public class Customer : IEqualityComparer<Customer>
	{
		public Customer()
		{
			CustomerMarketPlaces = new HashedSet<MP_CustomerMarketPlace>();
		} // constructor

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_CustomerMarketPlace> CustomerMarketPlaces { get; set; }

		private Iesi.Collections.Generic.ISet<CashRequest> _cashRequests = new HashedSet<CashRequest>();
		public virtual Iesi.Collections.Generic.ISet<CashRequest> CashRequests
		{
			get { return _cashRequests; }
			set { _cashRequests = value; }
		} // CashRequests
		
		public virtual bool IsWasLate { get; set; }

		public virtual PaymentdemeanorType PaymentDemenaor
		{
			get
			{
				return CreditResult == CreditResultStatus.Late
					? PaymentdemeanorType.Late
					: (IsWasLate ? PaymentdemeanorType.WasLate : PaymentdemeanorType.Ok);
			} // get
		} // PaymentDemenaor

		public virtual Application LastStartedMainStrategy { get; set; }
		public virtual DateTime? LastStartedMainStrategyEndTime { get; set; }

		/// <summary>
		/// The amount of offer, that is still valid for user. It is reduced on taking each new loan.
		/// </summary>
		public virtual decimal? CreditSum { get; set; }

		public virtual int IsLoanTypeSelectionAllowed { get; set; }

		public virtual Status? Status { get; set; }
		public virtual CreditResultStatus? CreditResult { get; set; }
		public virtual SystemDecision? SystemDecision { get; set; }

		public virtual bool Fraud { get; set; }
		public virtual FraudStatus FraudStatus { get; set; }
		public virtual bool Eliminated { get; set; }
		public virtual int FinancialAccounts { get; set; }

		//public virtual LimitedInfo LimitedInfo { get; set; }
		//public virtual NonLimitedInfo NonLimitedInfo { get; set; }
		//public virtual CompanyAdditionalInfo CompanyAdditionalInfo { get; set; }
		public virtual PersonalInfo PersonalInfo { get; set; }

		public virtual AddressInfo AddressInfo { get; set; }

		private BankAccount _bankAccount = new BankAccount();

		public virtual bool HasBankAccount
		{
			get
			{
				return
					BankAccount != null &&
					!string.IsNullOrEmpty(BankAccount.AccountNumber) &&
					!string.IsNullOrEmpty(BankAccount.SortCode);
			} // get
		} // HasBankAccount

		public virtual BankAccount BankAccount
		{
			get { return _bankAccount ?? (_bankAccount = new BankAccount()); }
			set { _bankAccount = value; }
		} // BankAccount

		private Iesi.Collections.Generic.ISet<Loans.Loan> _loans = new HashedSet<Loans.Loan>();
		public virtual Iesi.Collections.Generic.ISet<Loans.Loan> Loans
		{
			get { return _loans; }
			set { _loans = value; }
		} // Loans

		public virtual Customer AddLoan(Loans.Loan loan)
		{
			loan.Position = Loans.Count;
			Loans.Add(loan);
			loan.Customer = this;
			return this;
		} // AddLoan

		public virtual Loans.Loan GetLoan(int loanId)
		{
			return Loans.Single(l => l.Id == loanId);
		} // GetLoan

		public virtual string PayPointTransactionId { get; set; }

		private Iesi.Collections.Generic.ISet<PayPointCard> _payPointCards = new HashedSet<PayPointCard>();
		public virtual Iesi.Collections.Generic.ISet<PayPointCard> PayPointCards
		{
			get { return _payPointCards; }
			set { _payPointCards = value; }
		} // PayPointCards

		private Iesi.Collections.Generic.ISet<CardInfo> _bankAccounts = new HashedSet<CardInfo>();
		public virtual Iesi.Collections.Generic.ISet<CardInfo> BankAccounts
		{
			get { return _bankAccounts; }
			set { _bankAccounts = value; }
		} // BankAccounts

		public virtual Medal? Medal { get; set; }

		public virtual DateTime? GreetingMailSentDate { get; set; }

		private Iesi.Collections.Generic.ISet<DecisionHistory> _decisionHistory = new HashedSet<DecisionHistory>();
		public virtual Iesi.Collections.Generic.ISet<DecisionHistory> DecisionHistory
		{
			get { return _decisionHistory; }
			set { _decisionHistory = value; }
		} // DecisionHistory

		private Iesi.Collections.Generic.ISet<CampaignClients> _activeCampaigns = new HashedSet<CampaignClients>();
		public virtual Iesi.Collections.Generic.ISet<CampaignClients> ActiveCampaigns
		{
			get { return _activeCampaigns; }
			set { _activeCampaigns = value; }
		} // ActiveCampaigns

		/// <summary>
		/// Number of attempts to receive cash. Actually number of clicks on Request Cash button.
		/// </summary>
		public virtual int ApplyCount { get; set; }

		public virtual DateTime? DateEscalated { get; set; }
		public virtual DateTime? DateApproved { get; set; }
		public virtual DateTime? DateRejected { get; set; }
		public virtual string UnderwriterName { get; set; }
		public virtual string ManagerName { get; set; }
		public virtual string EscalationReason { get; set; }
		public virtual string RejectedReason { get; set; }
		public virtual string ApprovedReason { get; set; }
		public virtual string BWAResult { get; set; }
		public virtual string AMLResult { get; set; }
		public virtual string Comment { get; set; }
		public virtual string Details { get; set; }
		public virtual PendingStatus PendingStatus { get; set; }

		/// <summary>
		/// Offer start date
		/// </summary>
		public virtual DateTime? OfferStart { get; set; }

		/// <summary>
		/// Date untill offer is considered as valid
		/// </summary>
		public virtual DateTime? OfferValidUntil { get; set; }

		/// <summary>
		/// Last 4 digits for credit card number
		/// </summary>
		public virtual string CreditCardNo { get; set; }

		public virtual IEnumerable<Loans.Loan> ActiveLoans
		{
			get
			{
				return
					from loan in Loans
					where loan.Status == LoanStatus.Live || loan.Status == LoanStatus.Late
					select loan;
			} // get
		} // ActiveLoans

		public virtual decimal TotalBalance { get { return ActiveLoans.Sum(l => l.Balance); } }

		private Iesi.Collections.Generic.ISet<ScoringResult> _scoringResults = new HashedSet<ScoringResult>();
		public virtual Iesi.Collections.Generic.ISet<ScoringResult> ScoringResults
		{
			get { return _scoringResults; }
			set { _scoringResults = value; }
		} // ScoringResults

		public virtual CashRequest LastCashRequest { get { return CashRequests.LastOrDefault(); } }

		/// <summary>
		/// Reference number, that is used in loan reference number generation
		/// </summary>
		public virtual string RefNumber { get; set; }

		public virtual int PayPointErrorsCount { get; set; }

		/// <summary>
		/// 0 if setup fee was not payed
		/// amount if it was payed
		/// </summary>
		public virtual decimal SetupFee { get; set; }

		public virtual string ReferenceSource { get; set; }
		public virtual EmailConfirmationRequestState EmailState { get; set; }
		public virtual bool IsTest { get; set; }
		public virtual bool IsAvoid { get; set; }
		public virtual CardInfo CurrentCard { get; set; }
		public virtual string ABTesting { get; set; }
		public virtual bool IsOffline { get; set; }
		public virtual IList<CustomerSession> Session { get; set; }

		public virtual WizardStep WizardStep { get; set; }

		private CollectionStatus _collectionStatus;
		public virtual CollectionStatus CollectionStatus
		{
			get { return _collectionStatus ?? (_collectionStatus = new CollectionStatus() { CurrentStatus = new CustomerStatuses() }); }
			set { _collectionStatus = value; }
		} // CollectionStatus

		/// <summary>
		/// Количество неудачных проверок банковского счета
		/// </summary>
		public virtual int BankAccountValidationInvalidAttempts { get; set; }

		public virtual bool HasLateLoans
		{
			get { return Loans.Any(l => l.Status == LoanStatus.Late); }
		} // HasLateLoans

		public virtual void ValidateOfferDate(DateTime? offerDate = null)
		{
			var offer = offerDate ?? DateTime.UtcNow;

			if (OfferStart == null || OfferValidUntil == null)
				throw new OfferExpiredException();

			if (OfferStart >= OfferValidUntil)
				throw new OfferExpiredException();

			if (OfferStart >= offer)
				throw new OfferExpiredException();

			if (OfferValidUntil <= offer)
				throw new OfferExpiredException();
		} // ValidateOfferDate

		/// <summary>
		/// Add new paypoint card from paypoint callback.
		/// </summary>
		/// <param name="transId">Paypoint transaction id. null for test card</param>
		/// <param name="cardNo">Last digits of credit card. null for test card</param>
		/// <param name="expiry">Format: MMYY</param>
		/// <returns></returns>
		public virtual PayPointCard TryAddPayPointCard(string transId, string cardNo, string expiry, string cardHolder)
		{
			var card = new PayPointCard
			{
				Customer = this,
				DateAdded = DateTime.UtcNow,
				CardNo = cardNo,
				TransactionId = transId,
				ExpireDateString = expiry,
				CardHolder = cardHolder
			};

			if (!string.IsNullOrEmpty(expiry) && expiry.Length == 4)
			{
				DateTime dt;

				if (DateTime.TryParseExact(expiry, "MMyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dt))
					card.ExpireDate = dt;
			} // if

			//if (!string.IsNullOrEmpty(card.CardNo) && card.ExpireDate.HasValue) {
			//	var existing = PayPointCards
			//		.Where(c => c.ExpireDate != null)
			//		.Where(c => c.ExpireDate.Value.Year == card.ExpireDate.Value.Year)
			//		.Where(c => c.ExpireDate.Value.Month == card.ExpireDate.Value.Month)
			//		.FirstOrDefault(c => c.CardNo == card.CardNo);

			//	if (existing != null) {
			//		existing.TransactionId = card.TransactionId;
			//		existing.DateAdded = card.DateAdded;
			//		return existing;
			//	} // if
			//} // if

			PayPointCards.Add(card);

			return card;
		} // TryAddPayPointCard

		public virtual void SetDefaultCard(CardInfo card)
		{
			BWAResult = card.BWAResult;
			BankAccount.AccountNumber = card.BankAccount;
			BankAccount.SortCode = card.SortCode;
			BankAccount.Type = card.Type;
			CurrentCard = card;
		} // SetDefaultCard

		public virtual void UpdateCreditResultStatus()
		{
			if (Loans.Any(l => l.Status == LoanStatus.Late))
				CreditResult = CreditResultStatus.Late;

			if (CreditResult == CreditResultStatus.Late && Loans.All(l => l.Status != LoanStatus.Late))
				CreditResult = CreditResultStatus.Approved;
		} // UpdateCreditResultStatus

		public virtual int NumRejects { get; set; }
		public virtual int NumApproves { get; set; }
		public virtual decimal SystemCalculatedSum { get; set; }
		public virtual decimal ManagerApprovedSum { get; set; }

		private string _lastStatus = "N/A";
		public virtual string LastStatus
		{
			get { return _lastStatus; }
			set { _lastStatus = value; }
		} // LastStatus

		public virtual decimal TotalPrincipalRepaid { get; set; }

		public virtual DateTime? FirstLoanDate { get; set; }
		public virtual DateTime? LastLoanDate { get; set; }
		public virtual decimal AmountTaken { get; set; }
		public virtual decimal LastLoanAmount { get; set; }

		public virtual string RegisteredMpStatuses
		{
			get
			{
				var mpStatuses = CustomerMarketPlaces.Select(mp => mp.Marketplace.Name + ":" + mp.GetUpdatingStatus()).ToList();
				return mpStatuses.Any() ? mpStatuses.Aggregate((i, j) => i + ", " + j) : " ";
			} // get
		} // RegisteredMpStatuses

		public virtual string MpList { get; set; }
		public virtual decimal OutstandingBalance { get; set; }
		public virtual int Delinquency { get; set; }
		public virtual DateTime? NextRepaymentDate { get; set; }
		public virtual DateTime? DateOfLate { get; set; }
		public virtual DateTime? OfferDate { get; set; }
		public virtual string LatestCRMstatus { get; set; }
		public virtual string LatestCRMComment { get; set; }
		public virtual decimal LateAmount { get; set; }
		public virtual string CustomerStatus { get; set; }
		public virtual decimal AmountOfInteractions { get; set; }

		public virtual bool LoanForCurrentOfferIsTaken
		{
			get { return Loans.Select(l => l.CashRequest.Id).Contains(LastCashRequest.Id); }
		} // LoanForCurrentOfferIsTaken

		public virtual long LoyaltyPoints()
		{
			var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
			CustomerLoyaltyProgramPoints p = oDBHelper == null ? null : oDBHelper.CustomerLoyaltyPoints.Get(Id);
			return p == null ? 0 : p.EarnedPoints;
		} // LoyaltyPoints

		public virtual DateTime? LastLoyaltyProgramActionDate()
		{
			var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
			CustomerLoyaltyProgramPoints p = oDBHelper == null ? null : oDBHelper.CustomerLoyaltyPoints.Get(Id);
			return p == null ? (DateTime?)null : p.LastActionDate;
		} // LastLoyaltyProgramActionDate

		public virtual string PromoCode { get; set; }
		public virtual bool? MonthlyStatusEnabled { get; set; }

		public virtual IList<CustomerRequestedLoan> CustomerRequestedLoan { get; set; }
		public virtual IList<CustomerInviteFriend> CustomerInviteFriend { get; set; }
		public virtual IList<Company> Companies { get; set; }

		public static ParseExperianResult ParseExperian(string sCompanyRefNum, string sCompanyName, string sParserConfiguration)
		{
			var oLog = LogManager.GetLogger(typeof(Customer));

			if (string.IsNullOrWhiteSpace(sCompanyRefNum))
			{
				string sErrMsg = string.Format("Company ref num not specified.");
				oLog.Info(sErrMsg);
				return new ParseExperianResult(null, ParsingResult.NotFound, sErrMsg, null, null);
			} // if

			var repo =
				ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

			MP_ExperianDataCache oCachedValue = repo.GetAll().FirstOrDefault(
				x => x.CompanyRefNumber == sCompanyRefNum
			);

			if (oCachedValue == null)
			{
				string sErrMsg = string.Format("No data found for Company with ref num = {0}", sCompanyRefNum);
				oLog.Info(sErrMsg);
				return new ParseExperianResult(null, ParsingResult.NotFound, sErrMsg, null, null);
			} // if

			var parser = new Ezbob.ExperianParser.Parser(sParserConfiguration, new SafeILog(oLog));

			var doc = new XmlDocument();

			try
			{
				doc.LoadXml(oCachedValue.JsonPacket);
			}
			catch (Exception e)
			{
				string sErrMsg = string.Format("Failed to parse Experian data as XML for Company Score tab with company ref num = {0}", sCompanyRefNum);
				oLog.Error(sErrMsg, e);
				return new ParseExperianResult(null, ParsingResult.Fail, sErrMsg, null, null);
			} // try

			try
			{
				Dictionary<string, ParsedData> oParsed = parser.NamedParse(doc);
				return new ParseExperianResult(oParsed, ParsingResult.Ok, null, sCompanyRefNum, sCompanyName);
			}
			catch (Exception e)
			{
				string sErrMsg = string.Format("Failed to extract Company Score tab data from Experian data with company ref num = {0}", sCompanyRefNum);
				oLog.Error(sErrMsg, e);
				return new ParseExperianResult(null, ParsingResult.Fail, sErrMsg, null, null);
			} // try
		} // ParseExperian

		public virtual ParseExperianResult ParseExperian(string sParserConfiguration)
		{
			var company = Companies.FirstOrDefault();
			if (company != null && company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
			{
				return ParseExperian(company.ExperianRefNum, company.ExperianCompanyName, sParserConfiguration);
			}

			return new ParseExperianResult(null, ParsingResult.Fail, "No limited company", null, null);
		} // ParseExperian

		public virtual bool Equals(Customer x, Customer y)
		{
			return x.Id == y.Id;
		} // Equals

		public virtual int GetHashCode(Customer obj)
		{
			return obj.Id.GetHashCode();
		} // GetHashCode

		public virtual string SegmentType()
		{
			return IsOffline ? "Offline" : "Online";
		} // SegmentType

		public virtual string IsWasLateName()
		{
			return PaymentDemenaor == PaymentdemeanorType.Ok ? "" : "iswaslate";
		} // IsWasLateName 

		public virtual bool CciMark { get; set; }

		public virtual string GoogleCookie { get; set; }

		public virtual string ManualAddressWarning()
		{
			var oResult = new List<CustomerAddressType>();

			foreach (var ai in AddressInfo.AllAddresses)
				if (ai.Id != null && ai.Id.StartsWith("MANUAL"))
					oResult.Add(ai.AddressType);

			if (oResult.Count < 1)
				return string.Empty;

			return string.Format(
				"Customer has entered manually the following address type{0}: {1}.",
				oResult.Count == 1 ? "" : "s",
				string.Join(", ", oResult)
			);
		} // ManualAddressWarning
	} // class Customer

	#endregion class Customer
} // namespace EZBob.DatabaseLib.Model.Database

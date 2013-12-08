using EzBob.Web.Infrastructure;

namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;
	using NHibernate.Linq;
	using Scorto.Web;
	using StructureMap;
	using ApplicationCreator;
	using Models;
	using Code;
	using Infrastructure.csrf;

	public class CustomersController : Controller {
		#region public

		#region constructor

		public CustomersController(
			ISession session,
			CustomerStatusesRepository customerStatusesRepository,
			CustomerRepository customers,
			IAppCreator appCreator,
			IEzBobConfiguration config,
			IDecisionHistoryRepository historyRepository,
			IWorkplaceContext context,
			LoanLimit limit,
			MarketPlaceRepository mpType,
			UnderwriterRecentCustomersRepository underwriterRecentCustomersRepository)
		{
			_context = context;
			_session = session;
			_customers = customers;
			_appCreator = appCreator;
			_historyRepository = historyRepository;
			_limit = limit;
			_mpType = mpType;
			_config = config;

			_customerStatusesRepository = customerStatusesRepository;

			if (statusIndex2Name == null) {
				statusIndex2Name = new Dictionary<int, string>();

				foreach (CustomerStatuses status in _customerStatusesRepository.GetAll().ToList()) {
					statusIndex2Name.Add(status.Id, status.Name);

					if (status.Name == "Default")
						defaultIndex = status.Id;
					else if (status.Name == "Legal")
						legalIndex = status.Id;
				} // foreach
			} // if

			this.underwriterRecentCustomersRepository = underwriterRecentCustomersRepository;
		} // constructor

		#endregion constructor

		#region method Index

		public ViewResult Index() {
			var grids = new LoansGrids {
				IsEscalated = _context.User.Roles.Any(r => r.Name == "manager"),
				MpTypes = _mpType.GetAll().ToList(),
				CollectionStatuses = _customerStatusesRepository.GetAll().ToList(),
				Config = _config,
				MaxLoan = _limit.GetMaxLimit()
			};

			return View(grids);
		} // Index

		#endregion method Index

		#region underwriter grids

		#region method GridWaiting

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridWaiting(bool includeTestCustomers) {
			return GridWaitingEscalated(includeTestCustomers, CreditResultStatus.WaitingForDecision);
		} // GridWaiting

		#endregion method GridWaiting

		#region method GridEscalated

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridEscalated(bool includeTestCustomers) {
			return GridWaitingEscalated(includeTestCustomers, CreditResultStatus.Escalated);
		} // GridEscalated

		#endregion method GridEscalated

		#region method GridPending

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridPending(bool includeTestCustomers) {
			return GridWaitingEscalated(includeTestCustomers, CreditResultStatus.ApprovedPending);
		} // GridPending

		#endregion method GridPending

		#region method GridWaitingEscalated

		private JsonNetResult GridWaitingEscalated(bool bIncludeTestCustomers, CreditResultStatus nStatus) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(bIncludeTestCustomers, c => (c.CreditResult == nStatus));

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer);

				oRow["CurrentStatus"] = oCustomer.LastStatus;
				oRow["OSBalance"] = oCustomer.OutstandingBalance;

				if (nStatus == CreditResultStatus.Escalated) {
					oRow["EscalationDate"] = oCustomer.DateEscalated;
					oRow["Underwriter"] = oCustomer.UnderwriterName;
					oRow["Reason"] = oCustomer.EscalationReason;
				}
				else if (nStatus == CreditResultStatus.ApprovedPending)
					oRow["Pending"] = oCustomer.PendingStatus.ToString();

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridWaitingEscalated

		#endregion method GridWaitingEscalated

		#region method GridApproved

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridApproved(bool includeTestCustomers) {
			return GridApprovedLate(includeTestCustomers, CreditResultStatus.Approved);
		} // GridApproved

		#endregion method GridApproved

		#region method GridLate

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridLate(bool includeTestCustomers) {
			return GridApprovedLate(includeTestCustomers, CreditResultStatus.Late);
		} // GridLate

		#endregion method GridLate

		#region method GridApprovedLate

		private JsonNetResult GridApprovedLate(bool bIncludeTestCustomers, CreditResultStatus nStatus) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(bIncludeTestCustomers, c => (c.CreditResult == nStatus));

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer);

				oRow["ApproveDate"] = oCustomer.DateApproved;
				oRow["ApprovedSum"] = oCustomer.ManagerApprovedSum;
				oRow["AmountTaken"] = oCustomer.AmountTaken;
				oRow["ApprovesNum"] = oCustomer.NumApproves;
				oRow["RejectsNum"] = oCustomer.NumRejects;

				if (nStatus == CreditResultStatus.Approved)
					oRow["OfferExpireDate"] = oCustomer.OfferValidUntil;
				else if (nStatus == CreditResultStatus.Late) {
					oRow["OSBalance"] = oCustomer.OutstandingBalance;
					oRow["LatePaymentDate"] = oCustomer.DateOfLate;
					oRow["LatePaymentAmount"] = oCustomer.LateAmount;
					oRow["Delinquency"] = oCustomer.Delinquency;
					oRow["CRMstatus"] = oCustomer.LatestCRMstatus ?? string.Empty;
					oRow["CRMcomment"] = oCustomer.LatestCRMComment ?? string.Empty;
				} // if

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridApprovedLate

		#endregion method GridApprovedLate

		#region method GridLoans

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridLoans(bool includeTestCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(includeTestCustomers, c => (c.Loans.Count > 0));

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer, bCalcAmount: false);

				oRow["FirstLoanDate"] = oCustomer.FirstLoanDate;
				oRow["LastLoanDate"] = oCustomer.LastLoanDate;
				oRow["LastLoanAmount"] = oCustomer.LastLoanAmount;
				oRow["AmountTaken"] = oCustomer.AmountTaken;
				oRow["TotalPrincipalRepaid"] = oCustomer.TotalPrincipalRepaid;
				oRow["OSBalance"] = oCustomer.OutstandingBalance;
				oRow["NextRepaymentDate"] = oCustomer.NextRepaymentDate;
				oRow["CustomerStatus"] = oCustomer.CustomerStatus;

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridLoans

		#endregion method GridLoans

		#region method GridSales

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridSales(bool includeTestCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(
				includeTestCustomers,
				c => ((c.ManagerApprovedSum > c.AmountTaken) && (c.LatestCRMstatus != "NoSale" || c.LatestCRMstatus == null))
			);

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer, bCart: false, bCalcAmount: false);

				SetSalesCollectionFields(oRow, oCustomer);

				oRow["ApprovedSum"] = oCustomer.ManagerApprovedSum;
				oRow["OfferDate"] = oCustomer.OfferDate;
				oRow["Interactions"] = oCustomer.AmountOfInteractions;

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridSales

		#endregion method GridSales

		#region method GridCollection

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridCollection(bool includeTestCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(
				includeTestCustomers,
				c => ((c.CollectionStatus.CurrentStatus.Id == legalIndex) || (c.CollectionStatus.CurrentStatus.Id == defaultIndex))
			);

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer, bCart: false, bCalcAmount: false);

				SetSalesCollectionFields(oRow, oCustomer);

				oRow["CollectionStatus"] = oCustomer.CollectionStatus.CurrentStatus.Name;

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridCollection

		#endregion method GridCollection

		#region method GridRejected

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridRejected(bool includeTestCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(
				includeTestCustomers,
				c => (c.CreditResult == CreditResultStatus.Rejected)
			);

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer, bCalcAmount: false);

				oRow["Reason"] = oCustomer.RejectedReason;
				oRow["ApprovesNum"] = oCustomer.NumApproves;
				oRow["RejectsNum"] = oCustomer.NumRejects;
				oRow["OSBalance"] = oCustomer.OutstandingBalance;

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridRejected

		#endregion method GridRejected

		#region method GridAll

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridAll(bool includeTestCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(
				includeTestCustomers,
				c => (c.CreditResult != null)
			);

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				SetCommonGridFields(oRow, oCustomer);

				oRow["ApprovedSum"] = oCustomer.ManagerApprovedSum;
				oRow["OSBalance"] = oCustomer.OutstandingBalance;

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridAll

		#endregion method GridAll

		#region method GridRegistered

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult GridRegistered(bool includeTestCustomers, bool includeAllCustomers) {
			var aryOutput = new List<object>();

			IEnumerable<Customer> oRelevant = RelevantCustomers(
				includeTestCustomers,
				c => ((c.CreditResult == null) && (includeAllCustomers || c.GreetingMailSentDate >= DateTime.Today.AddDays(-7)))
			);

			foreach (var oCustomer in oRelevant) {
				var oRow = new Dictionary<string, object>();

				oRow["UserId"] = oCustomer.Id;
				oRow["Email"] = oCustomer.Name;

				oRow["UserStatus"] = oCustomer.WizardStep.TheLastOne ? "credit calculation" : "registered";

				oRow["RegDate"] = oCustomer.GreetingMailSentDate;
				oRow["MP_Statuses"] = oCustomer.RegisteredMpStatuses;

				oRow["WizardStep"] = oCustomer.WizardStep.Name;

				oRow["SegmentType"] = oCustomer.SegmentType();
				oRow["IsWasLate"] = oCustomer.IsWasLateName();

				aryOutput.Add(oRow);
			} // foreach

			return this.JsonNet(new { aaData = aryOutput });
		} // GridRegistered

		#endregion method GridRegistered

		#region private grid fields setters

		#region method SetSalesCollectionFields

		private void SetSalesCollectionFields(Dictionary<string, object> oRow, Customer oCustomer) {
			oRow["MobilePhone"] = oCustomer.PersonalInfo == null ? string.Empty : (oCustomer.PersonalInfo.MobilePhone ?? string.Empty);
			oRow["DaytimePhone"] = oCustomer.PersonalInfo == null ? string.Empty : (oCustomer.PersonalInfo.DaytimePhone ?? string.Empty);
			oRow["AmountTaken"] = oCustomer.AmountTaken;
			oRow["OSBalance"] = oCustomer.OutstandingBalance;
			oRow["CRMstatus"] = oCustomer.LatestCRMstatus ?? string.Empty;
			oRow["CRMcomment"] = oCustomer.LatestCRMComment ?? string.Empty;
		} // SetSalesCollectionFields

		#endregion method SetSalesCollectionFields

		#region method SetCommonGridFields

		private void SetCommonGridFields(Dictionary<string, object> oRow, Customer oCustomer, bool bCalcAmount = true, bool bCart = true) {
			oRow["Id"] = oCustomer.Id;

			if (bCart) {
				oRow["Cart"] = oCustomer.Medal.HasValue ? oCustomer.Medal.Value.ToString() : "";
				oRow["MP_List"] = oCustomer.MpList;
				oRow["ApplyDate"] = oCustomer.OfferStart;
				oRow["RegDate"] = oCustomer.GreetingMailSentDate;
			} // if

			oRow["CustomerStatus"] = oCustomer.CustomerStatus;
			oRow["Name"] = oCustomer.PersonalInfo == null ? "" : oCustomer.PersonalInfo.Fullname;
			oRow["Email"] = oCustomer.Name;

			if (bCalcAmount)
				oRow["CalcAmount"] = oCustomer.SystemCalculatedSum;

			oRow["SegmentType"] = oCustomer.SegmentType();

			oRow["IsWasLate"] = oCustomer.IsWasLateName();
		} // SetCommonGridFields

		#endregion method SetCommonGridFields

		#region method RelevantCustomers

		private IEnumerable<Customer> RelevantCustomers(bool bIncludeTestCustomers, Func<Customer, bool> fCustomFilter) {
			var oResult = new List<Customer>();

// ReSharper disable LoopCanBeConvertedToQuery
			// The following loop cannot be converted to linq expression because nhibernate cannot convert
			// custom filter to SQL thus throwing a run time exception.
			foreach (var oCustomer in _customers.GetAll().Where(c => bIncludeTestCustomers || !c.IsTest))
				if (fCustomFilter(oCustomer))
					oResult.Add(oCustomer);
// ReSharper restore LoopCanBeConvertedToQuery

			return oResult;
		} // RelevantCustomers

		#endregion method RelevantCustomers

		#endregion private grid fields setters

		#endregion underwriter grids

		#region method ChangeStatus

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public void ChangeStatus(int id, CreditResultStatus status, string reason) {
			var workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
			var user = workplaceContext.User;
			var customer = _customers.GetChecked(id);

			customer.CreditResult = status;
			customer.UnderwriterName = user.Name;

			var request = customer.LastCashRequest ?? new CashRequest();
			request.IdUnderwriter = user.Id;
			request.UnderwriterDecisionDate = DateTime.UtcNow;
			request.UnderwriterDecision = status;
			request.UnderwriterComment = reason;

			switch (status) {
			case CreditResultStatus.Approved:
				customer.DateApproved = DateTime.UtcNow;
				customer.Status = Status.Approved;
				customer.ApprovedReason = reason;

				var sum = request.ApprovedSum();
				if (sum <= 0)
					throw new Exception("Credit sum cannot be zero or less");

				_limit.Check(sum);

				customer.CreditSum = sum;
				customer.IsLoanTypeSelectionAllowed = request.IsLoanTypeSelectionAllowed;
				request.ManagerApprovedSum = (double?)sum;

				_historyRepository.LogAction(DecisionActions.Approve, reason, user, customer);

				if (!request.EmailSendingBanned)
					_appCreator.ApprovedUser(user, customer, sum);

				break;

			case CreditResultStatus.Rejected:
				customer.DateRejected = DateTime.UtcNow;
				customer.RejectedReason = reason;
				customer.Status = Status.Rejected;

				_historyRepository.LogAction(DecisionActions.Reject, reason, user, customer);

				request.ManagerApprovedSum = null;

				if (!request.EmailSendingBanned)
					_appCreator.RejectUser(user, customer.Name, customer.Id, customer.PersonalInfo.FirstName);

				break;

			case CreditResultStatus.Escalated:
				customer.CreditResult = CreditResultStatus.Escalated;
				customer.DateEscalated = DateTime.UtcNow;
				customer.EscalationReason = reason;
				_historyRepository.LogAction(DecisionActions.Escalate, reason, user, customer);
				_appCreator.Escalated(customer);

				break;

			case CreditResultStatus.ApprovedPending:
				customer.CreditResult = CreditResultStatus.ApprovedPending;
				customer.PendingStatus = PendingStatus.Manual;
				_historyRepository.LogAction(DecisionActions.Pending, "", user, customer);

				break;

			case CreditResultStatus.WaitingForDecision:
				customer.CreditResult = CreditResultStatus.WaitingForDecision;
				_historyRepository.LogAction(DecisionActions.Waiting, "", user, customer);

				break;
			} // switch
		} // ChangeStatus

		#endregion method ChangeStatus

		#region method CheckCustomer

		[HttpGet]
		[Ajax]
		public JsonNetResult CheckCustomer(int customerId) {
			var customer = _customers.TryGet(customerId);

			var nState = (customer == null)
				? CustomerState.NotFound
				: (
					customer.WizardStep.TheLastOne
					? CustomerState.Ok
					: CustomerState.NotSuccesfullyRegistred
				);

			return this.JsonNet(new { State = nState.ToString() });
		} // CheckCustomer

		#endregion method CheckCustomer

		#region method SetRecentCustomer

		[HttpPost]
		[Ajax]
		public JsonNetResult SetRecentCustomer(int id) {
			underwriterRecentCustomersRepository.Add(id, User.Identity.Name);
			return GetRecentCustomers();
		} // SetRecentCustomer

		#endregion method SetRecentCustomer

		#region method GetRecentCustomers

		[HttpGet]
		[Ajax]
		public JsonNetResult GetRecentCustomers() {
			string underwriter = User.Identity.Name;
			var recentCustomersMap = new List<System.Tuple<int, string>>();

			var recentCustomers = underwriterRecentCustomersRepository.GetAll()
				.Where(e => e.UserName == underwriter).OrderByDescending(e => e.Id);

			foreach (var recentCustomer in recentCustomers) {
				var customer = _customers.TryGet(recentCustomer.CustomerId);

				if (customer != null)
					recentCustomersMap.Add(new System.Tuple<int, string>(recentCustomer.CustomerId, string.Format("{0}, {1}, {2}", recentCustomer.CustomerId, customer.PersonalInfo.Fullname, customer.Name)));
			} // for each

			return this.JsonNet(new { RecentCustomers = recentCustomersMap });
		} // GetRecentCustomers

		#endregion method GetRecentCustomers

		#region method GetCounters

		[HttpGet]
		[Ajax]
		public JsonNetResult GetCounters(bool isTest) {
			int nWaiting = 0;
			int nPending = 0;
			int nRegistered = 0;
			int nEscalated = 0;

			var oRelevantCustomers = _customers.GetAll().Where(c =>
				(isTest || !c.IsTest)
				&&
				(
					((c.CreditResult == null) && c.WizardStep.TheLastOne) ||
					(c.CreditResult == CreditResultStatus.Escalated) ||
					(c.CreditResult == CreditResultStatus.WaitingForDecision) ||
					(c.CreditResult == CreditResultStatus.ApprovedPending)
				)
			);

			foreach (var oCustomer in oRelevantCustomers) {
				switch (oCustomer.CreditResult) {
				case null:
					nRegistered++;
					break;

				case CreditResultStatus.Escalated:
					nEscalated++;
					break;
				
				case CreditResultStatus.ApprovedPending:
					nPending++;
					break;
				
				case CreditResultStatus.WaitingForDecision:
					nWaiting++;
					break;
				} // switch
			} // for each

			return this.JsonNet(new List<CustomersCountersModel> {
				new CustomersCountersModel { Count = nWaiting,    Name = "waiting" },
				new CustomersCountersModel { Count = nPending,    Name = "pending" },
				new CustomersCountersModel { Count = nRegistered, Name = "RegisteredCustomers" },
				new CustomersCountersModel { Count = nEscalated,  Name = "escalated" },
			});
		} // GetCounters

		#endregion method GetCounters

		#region method FindCustomer

		[HttpGet]
		[Ajax]
		public JsonNetResult FindCustomer(string term) {
			term = term.Trim();
			int id;
			int.TryParse(term, out id);

			var findResult = _session.Query<Customer>()
				.Where(x => x.WizardStep.TheLastOne)
				.Where(c =>
					c.Id == id || c.Name.Contains(term) ||
					c.PersonalInfo.Fullname.Contains(term)
				)
				.Select(x => string.Format("{0}, {1}, {2}", x.Id, x.PersonalInfo.Fullname, x.Name))
				.Take(20);

			var retVal = new HashSet<string>(findResult);

			return this.JsonNet(retVal.Take(15));
		} // FindCustomer

		#endregion method FindCustomer

		#endregion public

		#region private

		#region enum CustomerState

		private enum CustomerState {
			NotSuccesfullyRegistred,
			NotFound,
			Ok
		} // enum CustomerState

		#endregion enum CustomerState

		#region properties

		private readonly ISession _session;
		private readonly CustomerRepository _customers;
		private readonly IAppCreator _appCreator;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly LoanLimit _limit;
		private readonly IWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpType;
		private readonly IEzBobConfiguration _config;

		private readonly Dictionary<int, string> statusIndex2Name;
		private readonly int defaultIndex = -1;
		private readonly int legalIndex = -1;
		private readonly CustomerStatusesRepository _customerStatusesRepository;
		private readonly IUnderwriterRecentCustomersRepository underwriterRecentCustomersRepository;

		#endregion properties

		#endregion private
	} // class CustomersController
} // namespace

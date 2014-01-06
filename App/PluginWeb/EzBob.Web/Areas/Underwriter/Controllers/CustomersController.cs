using EzBob.Web.Infrastructure;

namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NHibernate;
	using NHibernate.Linq;
	using Scorto.Web;
	using StructureMap;
	using Models;
	using Code;
	using Infrastructure.csrf;
	using log4net;
	using ActionResult = Ezbob.Database.ActionResult;

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
			UnderwriterRecentCustomersRepository underwriterRecentCustomersRepository
		) {
			m_oLog = new SafeILog(LogManager.GetLogger(typeof(CustomersController)));
			m_oDB = DbConnectionGenerator.Get();

			_context = context;
			_session = session;
			_customers = customers;
			_appCreator = appCreator;
			_historyRepository = historyRepository;
			_limit = limit;
			_mpType = mpType;
			_config = config;

			_customerStatusesRepository = customerStatusesRepository;

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
		public JsonResult GridWaiting(bool includeTestCustomers) {
			return LoadGrid("UwGridWaiting", includeTestCustomers, () => new GridWaitingRow());
		} // GridWaiting

		#endregion method GridWaiting

		#region method GridEscalated

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridEscalated(bool includeTestCustomers) {
			return LoadGrid("UwGridEscalated", includeTestCustomers, () => new GridEscalatedRow());
		} // GridEscalated

		#endregion method GridEscalated

		#region method GridPending

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridPending(bool includeTestCustomers) {
			return LoadGrid("UwGridPending", includeTestCustomers, () => new GridPendingRow());
		} // GridPending

		#endregion method GridPending

		#region method GridApproved

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridApproved(bool includeTestCustomers) {
			return LoadGrid("UwGridApproved", includeTestCustomers, () => new GridApprovedRow());
		} // GridApproved

		#endregion method GridApproved

		#region method GridLate

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridLate(bool includeTestCustomers) {
			return LoadGrid("UwGridLate", includeTestCustomers, () => new GridLateRow());
		} // GridLate

		#endregion method GridLate

		#region method GridLoans

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridLoans(bool includeTestCustomers) {
			return LoadGrid("UwGridLoans", includeTestCustomers, () => new GridLoansRow());
		} // GridLoans

		#endregion method GridLoans

		#region method GridSales

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridSales(bool includeTestCustomers) {
			return LoadGrid("UwGridSales", includeTestCustomers, () => new GridSalesRow());
		} // GridSales

		#endregion method GridSales

		#region method GridCollection

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridCollection(bool includeTestCustomers) {
			return LoadGrid("UwGridCollection", includeTestCustomers, () => new GridCollectionRow());
		} // GridCollection

		#endregion method GridCollection

		#region method GridRejected

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridRejected(bool includeTestCustomers) {
			return LoadGrid("UwGridRejected", includeTestCustomers, () => new GridRejectedRow());
		} // GridRejected

		#endregion method GridRejected

		#region method GridOffline

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridOffline(bool includeTestCustomers) {
			return LoadGrid("UwGridOffline", includeTestCustomers, () => new GridOfflineRow());
		} // GridOffline

		#endregion method GridOffline

		#region method GridAll

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridAll(bool includeTestCustomers) {
			return LoadGrid("UwGridAll", includeTestCustomers, () => new GridAllRow());
		} // GridAll

		#endregion method GridAll

		#region method GridRegistered

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridRegistered(bool includeTestCustomers, bool includeAllCustomers) {
			return LoadGrid("UwGridRegistered", includeTestCustomers, includeAllCustomers, () => new GridRegisteredRow());
		} // GridRegistered

		#endregion method GridRegistered

		#region method LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, Func<AGridRowBase> oFactory) {
			return LoadGrid(sSpName, bIncludeTestCustomers, oFactory, null);
		} // LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, bool bIncludeAllCustomers, Func<AGridRowBase> oFactory) {
			return LoadGrid(sSpName, bIncludeTestCustomers, oFactory, bIncludeAllCustomers);
		} // LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, Func<AGridRowBase> oFactory, bool? bIncludeAllCustomers) {
			var oRes = new SortedDictionary<int, AGridRowBase>();

			var args = new List<QueryParameter> {
				new QueryParameter("@WithTest", bIncludeTestCustomers)
			};

			if (bIncludeAllCustomers.HasValue)
				args.Add(new QueryParameter("@WithAll", bIncludeAllCustomers));

			m_oDB.ForEachRowSafe(
				(sr, bRowSetStarts) => {
					int nCustomerID = sr["CustomerID"];

					if (oRes.ContainsKey(nCustomerID))
						oRes[nCustomerID].Add(sr);
					else {
						AGridRowBase r = oFactory();
						r.Init(nCustomerID, sr);

						if (r.IsValid())
							oRes[nCustomerID] = r;
					} // if

					return ActionResult.Continue;
				},
				sSpName,
				CommandSpecies.StoredProcedure,
				args.ToArray()
			); // foreach

			m_oLog.Debug("{0}: traversing done.", sSpName);

			var j = Json(new { aaData = oRes.Values }, JsonRequestBehavior.AllowGet);

			m_oLog.Debug("{0}: converted to json.", sSpName);

			return j;
		} // LoadGrid

		#endregion method LoadGrid

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

		private readonly CustomerStatusesRepository _customerStatusesRepository;
		private readonly IUnderwriterRecentCustomersRepository underwriterRecentCustomersRepository;

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#endregion properties

		#endregion private
	} // class CustomersController
} // namespace

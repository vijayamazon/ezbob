namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Infrastructure;
	using Infrastructure.Attributes;
	using NHibernate;
	using NHibernate.Linq;
	using Newtonsoft.Json;
	using ServiceClientProxy;
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
			IDecisionHistoryRepository historyRepository,
			IWorkplaceContext context,
			LoanLimit limit,
			MarketPlaceRepository mpType,
			UnderwriterRecentCustomersRepository underwriterRecentCustomersRepository, 
			RejectReasonRepository rejectReasonRepository) {
			m_oLog = new SafeILog(LogManager.GetLogger(typeof(CustomersController)));
			m_oDB = DbConnectionGenerator.Get();

			_context = context;
			_session = session;
			_customers = customers;
			m_oServiceClient = new ServiceClient();
			_historyRepository = historyRepository;
			_limit = limit;
			_mpType = mpType;

			_customerStatusesRepository = customerStatusesRepository;

			this.underwriterRecentCustomersRepository = underwriterRecentCustomersRepository;
			_rejectReasonRepository = rejectReasonRepository;
			} // constructor

		#endregion constructor

		#region method Index

		public ViewResult Index() {
			var grids = new LoansGrids {
				IsEscalated = _context.User.Roles.Any(r => r.Name == "manager"),
				MpTypes = _mpType.GetAll().ToList(),
				CollectionStatuses = _customerStatusesRepository.GetAll().ToList(),
				MaxLoan = _limit.GetMaxLimit()
			};

			return View(grids);
		} // Index

		#endregion method Index

		#region method AddLogbookEntry

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		public JsonResult AddLogbookEntry(int type, string content) {
			bool bSuccess;
			string sMsg = string.Empty;

			try {
				if (string.IsNullOrWhiteSpace(content))
					throw new Exception("Content is empty.");

				var context = ObjectFactory.GetInstance<IWorkplaceContext>();

				m_oDB.ExecuteNonQuery(
					"LogbookAdd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LogbookEntryTypeID", type),
					new QueryParameter("@UserID", context.User.Id),
					new QueryParameter("@EntryContent", content)
				);

				bSuccess = true;
			}
			catch (Exception e) {
				bSuccess = false;
				sMsg = e.Message;
			} // try

			return Json(new { success = bSuccess, msg = sMsg });
		} // AddLogbookEntry

		#endregion method AddLogbookEntry

		#region method LoadLogbookEntryTypeList

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult LoadLogbookEntryTypeList() {
			var oRes = new List<object>();

			const string sSpName = "LogbookEntryTypeList";

			m_oDB.ForEachRowSafe(
				(sr, bRowSetStarts) => {
					oRes.Add(new {
						ID = (int)sr["LogbookEntryTypeID"],
						Name = (string)sr["LogbookEntryType"],
						Description = (string)sr["LogbookEntryTypeDescription"],
					});

					return ActionResult.Continue;
				},
				sSpName,
				CommandSpecies.StoredProcedure
			); // foreach

			m_oLog.Debug("{0}: traversing done.", sSpName);

			var j = Json(oRes, JsonRequestBehavior.AllowGet);

			m_oLog.Debug("{0}: converted to json.", sSpName);

			return j;
		} // LoadLogbookEntryTypeList

		#endregion method LoadLogbookEntryTypeList

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
			return LoadGrid("UwGridLate", includeTestCustomers, () => new GridLateRow(),null, DateTime.UtcNow);
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
			return LoadGrid("UwGridSales", includeTestCustomers, () => new GridSalesRow(), null, DateTime.UtcNow);
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

		#region method GridLogbook

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridLogbook(bool includeTestCustomers) {
			return LoadGrid("UwGridLogbook", includeTestCustomers, () => new GridLogbookRow());
		} // GridLogbook

		#endregion method GridLogbook

		#region method GridBrokers

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult GridBrokers(bool includeTestCustomers) {
			return LoadGrid("UwGridBrokers", includeTestCustomers, () => new GridBroker());
		} // GridBrokers

		#endregion method GridBrokers

		#region method LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, Func<AGridRow> oFactory) {
			return LoadGrid(sSpName, bIncludeTestCustomers, oFactory, null);
		} // LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, bool bIncludeAllCustomers, Func<AGridRow> oFactory) {
			return LoadGrid(sSpName, bIncludeTestCustomers, oFactory, bIncludeAllCustomers);
		} // LoadGrid

		private JsonResult LoadGrid(string sSpName, bool bIncludeTestCustomers, Func<AGridRow> oFactory, bool? bIncludeAllCustomers, DateTime? now = null) {
			var oRes = new SortedDictionary<long, AGridRow>();

			var args = new List<QueryParameter> {
				new QueryParameter("@WithTest", bIncludeTestCustomers)
			};

			if (bIncludeAllCustomers.HasValue)
				args.Add(new QueryParameter("@WithAll", bIncludeAllCustomers));

			if (now.HasValue)
				args.Add(new QueryParameter("@Now", now.Value));

			m_oDB.ForEachRowSafe(
				(sr, bRowSetStarts) => {
					AGridRow r = oFactory();

					long nRowID = sr[r.RowIDFieldName()];

					if (oRes.ContainsKey(nRowID))
						oRes[nRowID].Add(sr);
					else {
						r.Init(nRowID, sr);

						if (r.IsValid())
							oRes[nRowID] = r;
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
		public JsonResult ChangeStatus(DecisionModel model) {
			var workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
			var user = workplaceContext.User;
			var customer = _customers.GetChecked(model.id);

			customer.CreditResult = model.status;
			customer.UnderwriterName = user.Name;

			var request = customer.LastCashRequest ?? new CashRequest();
			request.IdUnderwriter = user.Id;
			request.UnderwriterDecisionDate = DateTime.UtcNow;
			request.UnderwriterDecision = model.status;
			request.UnderwriterComment = model.reason;

			string sWarning = string.Empty;
			int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

			switch (model.status) {
				case CreditResultStatus.Approved:
				if (!customer.WizardStep.TheLastOne) {
					try {
						var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);
						oArgs.CustomerID = customer.Id;
						m_oServiceClient.Instance.FinishWizard(oArgs, user.Id);
					}
					catch (Exception e) {
						m_oLog.Alert(e, "Something went horribly not so cool while finishing customer's wizard.");
					} // try
				} // if

				customer.DateApproved = DateTime.UtcNow;
				customer.Status = Status.Approved;
				customer.ApprovedReason = model.reason;

				var sum = request.ApprovedSum();
				if (sum <= 0)
					throw new Exception("Credit sum cannot be zero or less");

				_limit.Check(sum);

				customer.CreditSum = sum;
				customer.IsLoanTypeSelectionAllowed = request.IsLoanTypeSelectionAllowed;
				request.ManagerApprovedSum = (double?)sum;

				_historyRepository.LogAction(DecisionActions.Approve, model.reason, user, customer);

				bool bSendBrokerForceResetCustomerPassword = false;
				

				if (customer.FilledByBroker) {
					

					if (numOfPreviousApprovals == 0)
						bSendBrokerForceResetCustomerPassword = true;
				} // if

				bool bSendApprovedUser = !request.EmailSendingBanned;
				_session.Flush();

				int validForHours = (int)(request.OfferValidUntil - request.OfferStart).Value.TotalHours;
				if (bSendBrokerForceResetCustomerPassword && bSendApprovedUser) {
					try {
						m_oServiceClient.Instance.BrokerApproveAndResetCustomerPassword(user.Id, customer.Id, sum, validForHours, numOfPreviousApprovals == 0);
					}
					catch (Exception e) {
						sWarning = "Failed to force reset customer password and send 'approved user' email: " + e.Message;
						m_oLog.Alert(e, "Failed to force reset customer password and send 'approved user' email.");
					} // try
				}
				else if (bSendApprovedUser) {
					try {
						
						m_oServiceClient.Instance.ApprovedUser(user.Id, customer.Id, sum, validForHours, numOfPreviousApprovals == 0);
					}
					catch (Exception e) {
						sWarning = "Failed to send 'approved user' email: " + e.Message;
						m_oLog.Warn(e, "Failed to send 'approved user' email.");
					} // try
				}
				else if (bSendBrokerForceResetCustomerPassword) {
					try {
						m_oServiceClient.Instance.BrokerForceResetCustomerPassword(user.Id, customer.Id);
					}
					catch (Exception e) {
						m_oLog.Alert(e, "Something went horribly not so cool while resetting customer password.");
					} // try
				} // if

				break;

			case CreditResultStatus.Rejected:
				customer.DateRejected = DateTime.UtcNow;
				customer.RejectedReason = model.reason;
				customer.Status = Status.Rejected;
				//var rejections = model.rejectionReasons.Select(int.Parse);
				_historyRepository.LogAction(DecisionActions.Reject, model.reason, user, customer, model.rejectionReasons);

				request.ManagerApprovedSum = null;

				bool bSendToCustomer = true;

				if (customer.FilledByBroker) {
					if (numOfPreviousApprovals == 0)
						bSendToCustomer = false;
				} // if

				if (!request.EmailSendingBanned) {
					try {
						m_oServiceClient.Instance.RejectUser(user.Id, customer.Id, bSendToCustomer);
					}
					catch (Exception e) {
						sWarning = "Failed to send 'reject user' email: " + e.Message;
						m_oLog.Warn(e, "Failed to send 'reject user' email.");
					} // try
				} // if

				break;

			case CreditResultStatus.Escalated:
				customer.CreditResult = CreditResultStatus.Escalated;
				customer.DateEscalated = DateTime.UtcNow;
				customer.EscalationReason = model.reason;
				_historyRepository.LogAction(DecisionActions.Escalate, model.reason, user, customer);

				try {
					m_oServiceClient.Instance.Escalated(customer.Id, _context.UserId);
				}
				catch (Exception e) {
					sWarning = "Failed to send 'escalated' email: " + e.Message;
					m_oLog.Warn(e, "Failed to send 'escalated' email.");
				} // try

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

			return Json(new { warning = sWarning });
		} // ChangeStatus

		#endregion method ChangeStatus

		#region method CheckCustomer

		[HttpGet]
		[Ajax]
		public JsonResult CheckCustomer(int customerId) {
			var customer = _customers.TryGet(customerId);

			var nState = (customer == null)
				? CustomerState.NotFound
				: (
					customer.WizardStep.TheLastOne
					? CustomerState.Ok
					: CustomerState.NotSuccesfullyRegistred
				);

			return Json(new { State = nState.ToString() }, JsonRequestBehavior.AllowGet);
		} // CheckCustomer

		#endregion method CheckCustomer

		#region method SetRecentCustomer

		[HttpPost]
		[Ajax]
		public JsonResult SetRecentCustomer(int id) {
			underwriterRecentCustomersRepository.Add(id, User.Identity.Name);
			return GetRecentCustomers();
		} // SetRecentCustomer

		#endregion method SetRecentCustomer

		#region method GetRecentCustomers

		[HttpGet]
		[Ajax]
		public JsonResult GetRecentCustomers() {
			string underwriter = User.Identity.Name;
			var recentCustomersMap = new List<System.Tuple<int, string>>();

			var recentCustomers = underwriterRecentCustomersRepository.GetAll()
				.Where(e => e.UserName == underwriter).OrderByDescending(e => e.Id);

			foreach (var recentCustomer in recentCustomers) {
				var customer = _customers.ReallyTryGet(recentCustomer.CustomerId);

				if (customer != null)
					recentCustomersMap.Add(new System.Tuple<int, string>(recentCustomer.CustomerId, string.Format("{0}, {1}, {2}", recentCustomer.CustomerId, customer.PersonalInfo == null ? null : customer.PersonalInfo.Fullname, customer.Name)));
			} // for each

			return Json(new { RecentCustomers = recentCustomersMap }, JsonRequestBehavior.AllowGet);
		} // GetRecentCustomers

		#endregion method GetRecentCustomers

		#region method GetCounters

		[HttpGet]
		[Ajax]
		public JsonResult GetCounters(bool isTest) {
			int nWaiting = 0;
			int nPending = 0;
			int nRegistered = 0;
			int nEscalated = 0;

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string sCustomerType = sr["CustomerType"];

					if (sCustomerType == "Registered")
						nRegistered = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.Escalated.ToString())
						nEscalated = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.ApprovedPending.ToString())
						nPending = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.WaitingForDecision.ToString())
						nWaiting = sr["CustomerCount"];

					return ActionResult.Continue;
				},
				"UwGetCounters",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@isTest", isTest)
			);
			
			return Json(new List<CustomersCountersModel> {
				new CustomersCountersModel { Count = nWaiting,    Name = "waiting" },
				new CustomersCountersModel { Count = nPending,    Name = "pending" },
				new CustomersCountersModel { Count = nRegistered, Name = "RegisteredCustomers" },
				new CustomersCountersModel { Count = nEscalated,  Name = "escalated" },
			}, JsonRequestBehavior.AllowGet);
		} // GetCounters

		#endregion method GetCounters

		#region method FindCustomer

		[HttpGet]
		[Ajax]
		public JsonResult FindCustomer(string term) {
			term = term.Trim();
			int id;
			int.TryParse(term, out id);

			var findResult = _session.Query<Customer>()
				.Where(c =>
					c.Id == id || c.Name.Contains(term) ||
					c.PersonalInfo.Fullname.Contains(term)
				)
				.Select(x => string.Format("{0}, {1}, {2}", x.Id, x.PersonalInfo.Fullname, x.Name))
				.Take(20);

			var retVal = new HashSet<string>(findResult);

			return Json(retVal.Take(15), JsonRequestBehavior.AllowGet);
		} // FindCustomer

		#endregion method FindCustomer

		#region RejectReason
		[Ajax]
		[HttpGet]
		public JsonResult RejectReasons()
		{
			return Json(new {reasons = _rejectReasonRepository.GetAll().ToList()}, JsonRequestBehavior.AllowGet);
		}
		#endregion

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
		private readonly ServiceClient m_oServiceClient;
		private readonly IDecisionHistoryRepository _historyRepository;
		private readonly LoanLimit _limit;
		private readonly IWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpType;

		private readonly CustomerStatusesRepository _customerStatusesRepository;
		private readonly IUnderwriterRecentCustomersRepository underwriterRecentCustomersRepository;
		private readonly RejectReasonRepository _rejectReasonRepository;
		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;

		#endregion properties

		#endregion private
	} // class CustomersController
} // namespace

namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using ConfigManager;
	using DbConstants;
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
	using Ezbob.Utils;
	using Ezbob.Utils.Extensions;
	using EzBob.CommonLib;
	using Infrastructure.csrf;
	using log4net;
	using SalesForceLib.Models;
	using ActionResult = Ezbob.Database.ActionResult;

	public class CustomersController : Controller {

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

		public ViewResult Index() {
			var grids = new LoansGrids {
				IsEscalated = _context.User.Roles.Any(r => r.Name == "manager"),
				MpTypes = _mpType.GetAll().ToList(),
				CollectionStatuses = _customerStatusesRepository.GetAll().ToList(),
				MaxLoan = _limit.GetMaxLimit()
			};

			return View(grids);
		} // Index

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

		private enum GridActions {
			UwGridWaiting,
			UwGridPending,
			UwGridRegistered,
			UwGridRejected,
			UwGridSignature,
			UwGridAll,
			UwGridApproved,
			UwGridCollection,
			UwGridEscalated,
			UwGridLate,
			UwGridLoans,
			UwGridLogbook,
			UwGridSales,
			UwGridBrokers,
		} // enum GridActions

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public ContentResult GetGrid(string grid, bool includeTestCustomers, bool includeAllCustomers) {
			m_oLog.Debug("Started: GetGrid('{0}', {1}, {2}...)", grid, includeTestCustomers, includeAllCustomers);

			GridActions nAction;

			if (!Enum.TryParse(grid, true, out nAction)) {
				string sMsg = string.Format("Cannot load underwriter grid because '{0}' is not known grid name.", grid);
				throw new Exception(sMsg);
			} // if

			switch (nAction) {
			case GridActions.UwGridWaiting:
				return LoadGrid(nAction, includeTestCustomers, () => new GridWaitingRow());

			case GridActions.UwGridPending:
				return LoadGrid(nAction, includeTestCustomers, () => new GridPendingRow());

			case GridActions.UwGridRegistered:
				return LoadGrid(
					nAction,
					includeTestCustomers,
					() => new GridRegisteredRow(),
					includeAllCustomers,
					oMoreSpArgs: new [] { new QueryParameter("@Now", DateTime.UtcNow), }
				);

			case GridActions.UwGridRejected:
				return LoadGrid(
					nAction,
					includeTestCustomers,
					() => new GridRejectedRow(),
					oMoreSpArgs: new [] { new QueryParameter("@Now", DateTime.UtcNow), }
				);

			case GridActions.UwGridSignature:
				return LoadGrid(nAction, includeTestCustomers, () => new GridPendingRow());

			case GridActions.UwGridAll:
				return LoadGrid(nAction, includeTestCustomers, () => new GridAllRow());

			case GridActions.UwGridApproved:
				return LoadGrid(nAction, includeTestCustomers, () => new GridApprovedRow());

			case GridActions.UwGridCollection:
				return LoadGrid(nAction, includeTestCustomers, () => new GridCollectionRow());

			case GridActions.UwGridEscalated:
				return LoadGrid(nAction, includeTestCustomers, () => new GridEscalatedRow());

			case GridActions.UwGridLate:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLateRow(), null, DateTime.UtcNow);

			case GridActions.UwGridLoans:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLoansRow());

			case GridActions.UwGridLogbook:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLogbookRow());

			case GridActions.UwGridSales:
				return LoadGrid(nAction, includeTestCustomers, () => new GridSalesRow(), null, DateTime.UtcNow);

			case GridActions.UwGridBrokers:
				return LoadGrid(nAction, includeTestCustomers, () => new GridBroker());

			default:
				string sMsg = string.Format("Cannot load underwriter grid because '{0}' is not implemented.", nAction);
				throw new ArgumentOutOfRangeException(sMsg);
			} // switch
		} // GetGrid

		private ContentResult LoadGrid(GridActions nSpName, bool bIncludeTestCustomers, Func<AGridRow> oFactory, IEnumerable<QueryParameter> oMoreSpArgs = null) {
			return LoadGrid(nSpName, bIncludeTestCustomers, oFactory, bIncludeAllCustomers: null, oMoreSpArgs: oMoreSpArgs);
		} // LoadGrid

		private ContentResult LoadGrid(
			GridActions nSpName,
			bool bIncludeTestCustomers,
			Func<AGridRow> oFactory,
			bool? bIncludeAllCustomers,
			DateTime? now = null,
			IEnumerable<QueryParameter> oMoreSpArgs = null
		) {

			TimeCounter tc = new TimeCounter("LoadGrid building time for grid " + nSpName);
			var oRes = new SortedDictionary<long, AGridRow>();

			var args = new List<QueryParameter> {
				new QueryParameter("@WithTest", bIncludeTestCustomers)
			};

			if (bIncludeAllCustomers.HasValue)
				args.Add(new QueryParameter("@WithAll", bIncludeAllCustomers));

			if (now.HasValue)
				args.Add(new QueryParameter("@Now", now.Value));

			if (oMoreSpArgs != null)
				args.AddRange(oMoreSpArgs);

			using (tc.AddStep("retrieving from db and processing")) {
				m_oDB.ForEachRowSafe(
					(sr, bRowSetStarts) => {
						AGridRow r = oFactory();

						long nRowID = sr[r.RowIDFieldName()];

						r.Init(nRowID, sr);

						if (r.IsValid())
							oRes[nRowID] = r;
						
						return ActionResult.Continue;
					},
					nSpName.ToString(),
					CommandSpecies.StoredProcedure,
					args.ToArray()
					); // foreach
			}
			m_oLog.Debug("{0}: traversing done.", nSpName);

			var sb = new StringBuilder();
			sb.AppendLine(tc.Title);
			foreach (var time in tc.Steps)
				sb.AppendFormat("\t{0}: {1}ms\n", time.Name, time.Length);

			m_oLog.Info("{0}", sb);

			var serializer = new JavaScriptSerializer {
				MaxJsonLength = Int32.MaxValue
			};

			return new ContentResult {
				Content = serializer.Serialize(new { aaData = oRes.Values }),
				ContentType = "application/json",
			};
		} // LoadGrid

		[Transactional]
		[HttpPost]
		[Ajax]
		[ValidateJsonAntiForgeryToken]
		public JsonResult ChangeStatus(DecisionModel model) {
			var workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
			var user = workplaceContext.User;
			var customer = _customers.GetChecked(model.id);
			DateTime now = DateTime.UtcNow;
			customer.CreditResult = model.status;
			customer.UnderwriterName = user.Name;

			var request = customer.LastCashRequest ?? new CashRequest();
			request.IdUnderwriter = user.Id;
			request.UnderwriterDecisionDate = DateTime.UtcNow;
			request.UnderwriterDecision = model.status;
			request.UnderwriterComment = model.reason;

			string sWarning = string.Empty;
			int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

			if (model.status != CreditResultStatus.ApprovedPending)
				customer.IsWaitingForSignature = false;
			string stage;
			switch (model.status) {
			case CreditResultStatus.Approved:
				if (!customer.WizardStep.TheLastOne) {
					try {
						customer.AddAlibabaDefaultBankAccount();

						var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(CurrentValues.Instance.FinishWizardForApproved);
						oArgs.CustomerID = customer.Id;

						m_oServiceClient.Instance.FinishWizard(oArgs, user.Id);
					}
					catch (Exception e) {
						m_oLog.Alert(e, "Something went horribly not so cool while finishing customer's wizard.");
					} // try
				} // if

				customer.DateApproved = now;
				customer.Status = Status.Approved;
				customer.ApprovedReason = model.reason;

				var sum = request.ApprovedSum();
				if (sum <= 0)
					throw new Exception("Credit sum cannot be zero or less");

				_limit.Check(sum);

				customer.CreditSum = sum;
				customer.ManagerApprovedSum = sum;
				customer.NumApproves++;
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
				stage = OpportunityStage.s90.DescriptionAttr();
				m_oServiceClient.Instance.SalesForceUpdateOpportunity(_context.UserId, customer.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel {
					Email = customer.Name,
					ApprovedAmount = (int)sum,
					Stage = stage,
					ExpectedEndDate = request.OfferValidUntil
				}); 
				break;

			case CreditResultStatus.Rejected:
				customer.DateRejected = now;
				customer.RejectedReason = model.reason;
				customer.Status = Status.Rejected;
				customer.NumRejects++;
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

				m_oServiceClient.Instance.SalesForceUpdateOpportunity(_context.UserId, customer.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel 
				{
					Email = customer.Name,
					CloseDate = now,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = customer.RejectedReason
				}); 
				break;

			case CreditResultStatus.Escalated:
				customer.CreditResult = CreditResultStatus.Escalated;
				customer.DateEscalated = DateTime.UtcNow;
				customer.EscalationReason = model.reason;
				_historyRepository.LogAction(DecisionActions.Escalate, model.reason, user, customer);
				stage = OpportunityStage.s20.DescriptionAttr();
				try {
					m_oServiceClient.Instance.Escalated(customer.Id, _context.UserId);
				}
				catch (Exception e) {
					sWarning = "Failed to send 'escalated' email: " + e.Message;
					m_oLog.Warn(e, "Failed to send 'escalated' email.");
				} // try
				m_oServiceClient.Instance.SalesForceUpdateOpportunity(_context.UserId, customer.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }); 
				break;

			case CreditResultStatus.ApprovedPending:
				customer.IsWaitingForSignature = model.signature == 1;
				customer.CreditResult = CreditResultStatus.ApprovedPending;
				customer.PendingStatus = PendingStatus.Manual;
				customer.ManagerApprovedSum = request.ApprovedSum();
				_historyRepository.LogAction(DecisionActions.Pending, "", user, customer);

				stage = model.signature == 1 ? OpportunityStage.s75.DescriptionAttr() : OpportunityStage.s50.DescriptionAttr();
				m_oServiceClient.Instance.SalesForceUpdateOpportunity(_context.UserId, customer.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel {Email = customer.Name,Stage = stage}); 
				break;

			case CreditResultStatus.WaitingForDecision:
				customer.CreditResult = CreditResultStatus.WaitingForDecision;
				_historyRepository.LogAction(DecisionActions.Waiting, "", user, customer);
				stage = OpportunityStage.s40.DescriptionAttr();
				m_oServiceClient.Instance.SalesForceUpdateOpportunity(_context.UserId, customer.Id, new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage }); 

				break;
			} // switch

			return Json(new { warning = sWarning });
		} // ChangeStatus

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

		[HttpPost]
		[Ajax]
		public JsonResult SetRecentCustomer(int id) {
			underwriterRecentCustomersRepository.Add(id, User.Identity.Name);
			return GetRecentCustomers();
		} // SetRecentCustomer

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

		[HttpGet]
		[Ajax]
		public JsonResult GetCounters(bool isTest) {
			int nWaiting = 0;
			int nPending = 0;
			int nRegistered = 0;
			int nEscalated = 0;
			int nSignature = 0;

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					string sCustomerType = sr["CustomerType"];

					if (sCustomerType == "Signature")
						nSignature = sr["CustomerCount"];
					else if (sCustomerType == "Registered")
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
				new CustomersCountersModel { Count = nSignature,  Name = "signature" },
			}, JsonRequestBehavior.AllowGet);
		} // GetCounters

		[HttpGet]
		[Ajax]
		public JsonResult FindCustomer(string term) {
			term = term.Trim();
			int id;
			if (!int.TryParse(term, out id)) {
				term = term.Replace(" ", "%");
			}

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

		[Ajax]
		[HttpGet]
		public JsonResult RejectReasons()
		{
			return Json(new {reasons = _rejectReasonRepository.GetAll().ToList()}, JsonRequestBehavior.AllowGet);
		}

		private enum CustomerState {
			NotSuccesfullyRegistred,
			NotFound,
			Ok
		} // enum CustomerState

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

	} // class CustomersController
} // namespace

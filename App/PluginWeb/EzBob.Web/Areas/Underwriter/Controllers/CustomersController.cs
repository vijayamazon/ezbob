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
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Utils;
    using Ezbob.Utils.Extensions;
    using EZBob.DatabaseLib.Model.Database.UserManagement;
    using Infrastructure.csrf;
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
            RejectReasonRepository rejectReasonRepository
        ) {
            this.m_oDB = DbConnectionGenerator.Get();

            this._context = context;
            this._session = session;
            this._customers = customers;
            this.m_oServiceClient = new ServiceClient();
            this._historyRepository = historyRepository;
            this._limit = limit;
            this._mpType = mpType;

            this._customerStatusesRepository = customerStatusesRepository;

            this.underwriterRecentCustomersRepository = underwriterRecentCustomersRepository;
            this._rejectReasonRepository = rejectReasonRepository;
        } // constructor

        public ViewResult Index() {
            var grids = new LoansGrids {
                IsEscalated = this._context.User.Roles.Any(r => r.Name == "manager"),
                MpTypes = this._mpType.GetAll().ToList(),
                CollectionStatuses = this._customerStatusesRepository.GetAll().ToList(),
                MaxLoan = this._limit.GetMaxLimit(),
                ManagerMaxLoan = CurrentValues.Instance.ManagerMaxLoan
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

                this.m_oDB.ExecuteNonQuery(
                    "LogbookAdd",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("@LogbookEntryTypeID", type),
                    new QueryParameter("@UserID", context.User.Id),
                    new QueryParameter("@EntryContent", content)
                );

                bSuccess = true;
            } catch (Exception e) {
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

            this.m_oDB.ForEachRowSafe(
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

            log.Debug("{0}: traversing done.", sSpName);

            var j = Json(oRes, JsonRequestBehavior.AllowGet);

            log.Debug("{0}: converted to json.", sSpName);

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
            log.Debug("Started: GetGrid('{0}', {1}, {2}...)", grid, includeTestCustomers, includeAllCustomers);

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
                        oMoreSpArgs: new[] { new QueryParameter("@Now", DateTime.UtcNow), }
                    );

                case GridActions.UwGridRejected:
                    return LoadGrid(
                        nAction,
                        includeTestCustomers,
                        () => new GridRejectedRow(),
                        oMoreSpArgs: new[] { new QueryParameter("@Now", DateTime.UtcNow), }
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

        private ContentResult LoadGrid(
            GridActions nSpName,
            bool bIncludeTestCustomers,
            Func<AGridRow> oFactory,
            IEnumerable<QueryParameter> oMoreSpArgs = null
        ) {
            return LoadGrid(
                nSpName,
                bIncludeTestCustomers,
                oFactory,
                bIncludeAllCustomers: null,
                oMoreSpArgs: oMoreSpArgs
            );
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
                this.m_oDB.ForEachRowSafe(
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
            log.Debug("{0}: traversing done.", nSpName);

            var sb = new StringBuilder();
            sb.AppendLine(tc.Title);
            foreach (var time in tc.Steps)
                sb.AppendFormat("\t{0}: {1}ms\n", time.Name, time.Length);

            log.Info("{0}", sb);

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
            var user = this._context.User;
            var customer = this._customers.GetChecked(model.id);
            DateTime now = DateTime.UtcNow;
            customer.CreditResult = model.status;
            customer.UnderwriterName = user.Name;

            var request = customer.LastCashRequest ?? new CashRequest();
            request.IdUnderwriter = user.Id;
            request.UnderwriterDecisionDate = DateTime.UtcNow;
            request.UnderwriterDecision = model.status;
            request.UnderwriterComment = model.reason;

            var newDecision = new NL_Decisions {
                UserID = user.Id,
                DecisionTime = now,
                Notes = model.reason
            };


            string sWarning = string.Empty;
            int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);

            if (model.status != CreditResultStatus.ApprovedPending)
                customer.IsWaitingForSignature = false;

            bool runSilentAutomation = false;

            switch (model.status) {
                case CreditResultStatus.Approved:
                    if (customer.WizardStep.TheLastOne) {
                        runSilentAutomation = true;
                    }

                    ApproveCustomer(model, customer, user, now, request, newDecision, numOfPreviousApprovals, ref sWarning);
                    break;

                case CreditResultStatus.Rejected:
                    runSilentAutomation = true;
                    RejectCustomer(model, customer, now, user, request, newDecision, numOfPreviousApprovals, ref sWarning);
                    break;

                case CreditResultStatus.Escalated:
                    EscalateCustomer(model, customer, user, request, newDecision, ref sWarning);
                    break;

                case CreditResultStatus.ApprovedPending:
                    runSilentAutomation = true;
                    PendCustomer(model, customer, request, user, newDecision);
                    break;

                case CreditResultStatus.WaitingForDecision:
                    ReturnCustomerToWaitingForDecision(customer, user, request, newDecision);

                break;
            } // switch

            log.Debug("update decision for customer {0} with decision {1} signature {2}", customer.Id, model.status, model.signature);

            // send final decision data (0002) to Alibaba parther (if exists)
            if (customer.IsAlibaba && (model.status == CreditResultStatus.Rejected || model.status == CreditResultStatus.Approved)) {
                this.m_oServiceClient.Instance.DataSharing(customer.Id, ServiceClientProxy.EzServiceReference.AlibabaBusinessType.APPLICATION_REVIEW, this._context.UserId);
            }

            if (runSilentAutomation)
                this.m_oServiceClient.Instance.SilentAutomation(customer.Id, user.Id);

            return Json(new { warning = sWarning });
        }//ChangeStatus

        private void ReturnCustomerToWaitingForDecision(Customer customer, User user, CashRequest oldCashRequest, NL_Decisions newDecision) {
            customer.CreditResult = CreditResultStatus.WaitingForDecision;
            this._historyRepository.LogAction(DecisionActions.Waiting, "", user, customer);
            var stage = OpportunityStage.s40.DescriptionAttr();

            newDecision.DecisionNameID = (int)DecisionActions.Waiting;
            this.m_oServiceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);
            this.m_oServiceClient.Instance.SalesForceUpdateOpportunity(this._context.UserId, customer.Id,
                new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage });
        }//ReturnCustomerToWaitingForDecision

        private void PendCustomer(DecisionModel model, Customer customer, CashRequest oldCashRequest, User user, NL_Decisions newDecision) {
            customer.IsWaitingForSignature = model.signature == 1;
            customer.CreditResult = CreditResultStatus.ApprovedPending;
            customer.PendingStatus = PendingStatus.Manual;
            customer.ManagerApprovedSum = oldCashRequest.ApprovedSum();
            this._historyRepository.LogAction(DecisionActions.Pending, "", user, customer);

            newDecision.DecisionNameID = (int)DecisionActions.Pending;
            this.m_oServiceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

            var stage = model.signature == 1
                ? OpportunityStage.s75.DescriptionAttr()
                : OpportunityStage.s50.DescriptionAttr();

            this.m_oServiceClient.Instance.SalesForceUpdateOpportunity(this._context.UserId, customer.Id,
                new ServiceClientProxy.EzServiceReference.OpportunityModel {Email = customer.Name, Stage = stage});
        }//PendCustomer

        private void EscalateCustomer(DecisionModel model, Customer customer, User user, CashRequest oldCashRequest, NL_Decisions newDecision, ref string sWarning) {
            customer.CreditResult = CreditResultStatus.Escalated;
            customer.DateEscalated = DateTime.UtcNow;
            customer.EscalationReason = model.reason;
            this._historyRepository.LogAction(DecisionActions.Escalate, model.reason, user, customer);
            var stage = OpportunityStage.s20.DescriptionAttr();

            try {
                this.m_oServiceClient.Instance.Escalated(customer.Id, this._context.UserId);
            } catch (Exception e) {
                sWarning = "Failed to send 'escalated' email: " + e.Message;
                log.Warn(e, "Failed to send 'escalated' email.");
            } // try

            newDecision.DecisionNameID = (int)DecisionActions.Escalate;
            this.m_oServiceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

            this.m_oServiceClient.Instance.SalesForceUpdateOpportunity(this._context.UserId, customer.Id,
                new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage });
        }//EscalateCustomer

        private void RejectCustomer(DecisionModel model, Customer customer, DateTime now, User user, CashRequest oldCashRequest, NL_Decisions newDecision, int numOfPreviousApprovals, ref string sWarning) {
            customer.DateRejected = now;
            customer.RejectedReason = model.reason;
            customer.Status = Status.Rejected;
            customer.NumRejects++;
            this._historyRepository.LogAction(DecisionActions.Reject, model.reason, user, customer, model.rejectionReasons);

            oldCashRequest.ManagerApprovedSum = null;

            bool bSendToCustomer = true;

            if (customer.FilledByBroker) {
                if (numOfPreviousApprovals == 0)
                    bSendToCustomer = false;
            } // if

            if (!oldCashRequest.EmailSendingBanned) {
                try {
                    this.m_oServiceClient.Instance.RejectUser(user.Id, customer.Id, bSendToCustomer);
                } catch (Exception e) {
                    sWarning = "Failed to send 'reject user' email: " + e.Message;
                    log.Warn(e, "Failed to send 'reject user' email.");
                } // try
            } // if

            newDecision.DecisionNameID = (int)DecisionActions.Reject;

            var rejectReasons = model.rejectionReasons.Select(x => new NL_DecisionRejectReasons {
                RejectReasonID = x
            }).ToArray();

            this.m_oServiceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, rejectReasons);


            this.m_oServiceClient.Instance.SalesForceUpdateOpportunity(this._context.UserId, customer.Id,
                new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, CloseDate = now,
                    DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
                    DealLostReason = customer.RejectedReason
                });
        }//RejectCustomer

        private void ApproveCustomer(DecisionModel model, Customer customer, User user, DateTime now, CashRequest oldCashRequest, NL_Decisions newDecision, int numOfPreviousApprovals, ref string sWarning) {
            if (!customer.WizardStep.TheLastOne) {
                try {
                    customer.AddAlibabaDefaultBankAccount();

                    var oArgs = JsonConvert.DeserializeObject<FinishWizardArgs>(
                        CurrentValues.Instance.FinishWizardForApproved
                        );
                    oArgs.CustomerID = customer.Id;

                    this.m_oServiceClient.Instance.FinishWizard(oArgs, user.Id);
                } catch (Exception e) {
                    log.Alert(e, "Something went horribly not so cool while finishing customer's wizard.");
                } // try
            } // if

            customer.DateApproved = now;
            customer.Status = Status.Approved;
            customer.ApprovedReason = model.reason;

            var sum = oldCashRequest.ApprovedSum();
            if (sum <= 0)
                throw new Exception("Credit sum cannot be zero or less");

            this._limit.Check(sum);

            customer.CreditSum = sum;
            customer.ManagerApprovedSum = sum;
            customer.NumApproves++;
            customer.IsLoanTypeSelectionAllowed = oldCashRequest.IsLoanTypeSelectionAllowed;
            oldCashRequest.ManagerApprovedSum = (double?)sum;

            this._historyRepository.LogAction(DecisionActions.Approve, model.reason, user, customer);

            bool bSendBrokerForceResetCustomerPassword = false;

            if (customer.FilledByBroker) {
                if (numOfPreviousApprovals == 0)
                    bSendBrokerForceResetCustomerPassword = true;
            } // if

            bool bSendApprovedUser = !oldCashRequest.EmailSendingBanned;
            this._session.Flush();

            int validForHours = (int)(oldCashRequest.OfferValidUntil - oldCashRequest.OfferStart).Value.TotalHours;

            if (bSendBrokerForceResetCustomerPassword && bSendApprovedUser) {
                try {
                    this.m_oServiceClient.Instance.BrokerApproveAndResetCustomerPassword(
                        user.Id,
                        customer.Id,
                        sum,
                        validForHours,
                        numOfPreviousApprovals == 0
                        );
                } catch (Exception e) {
                    sWarning = "Failed to force reset customer password and send 'approved user' email: " + e.Message;
                    log.Alert(e, "Failed to force reset customer password and send 'approved user' email.");
                } // try
            } else if (bSendApprovedUser) {
                try {
                    this.m_oServiceClient.Instance.ApprovedUser(
                        user.Id,
                        customer.Id,
                        sum,
                        validForHours,
                        numOfPreviousApprovals == 0
                        );
                } catch (Exception e) {
                    sWarning = "Failed to send 'approved user' email: " + e.Message;
                    log.Warn(e, "Failed to send 'approved user' email.");
                } // try
            } else if (bSendBrokerForceResetCustomerPassword) {
                try {
                    this.m_oServiceClient.Instance.BrokerForceResetCustomerPassword(user.Id, customer.Id);
                } catch (Exception e) {
                    log.Alert(e, "Something went horribly not so cool while resetting customer password.");
                } // try
            } // if


            newDecision.DecisionNameID = (int)DecisionActions.Approve;
            NL_Offers lastOffer = this.m_oServiceClient.Instance.GetLastOffer(user.Id, customer.Id);
            var decisionID = this.m_oServiceClient.Instance.AddDecision(user.Id, customer.Id, newDecision, oldCashRequest.Id, null);

            lastOffer.DecisionID = decisionID.Value;
            lastOffer.CreatedTime = now;
            this.m_oServiceClient.Instance.AddOffer(user.Id, customer.Id, lastOffer);

            var stage = OpportunityStage.s90.DescriptionAttr();

            this.m_oServiceClient.Instance.SalesForceUpdateOpportunity(this._context.UserId, customer.Id,
                new ServiceClientProxy.EzServiceReference.OpportunityModel { Email = customer.Name, Stage = stage,
                    ApprovedAmount = (int)sum, ExpectedEndDate = oldCashRequest.OfferValidUntil});
        }//ApproveCustomer
        
        [HttpGet]
        [Ajax]
        public JsonResult CheckCustomer(int customerId) {
            var customer = this._customers.ReallyTryGet(customerId);

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
            this.underwriterRecentCustomersRepository.Add(id, User.Identity.Name);
            return GetRecentCustomers();
        } // SetRecentCustomer

        [HttpGet]
        [Ajax]
        public JsonResult GetRecentCustomers() {
            string underwriter = User.Identity.Name;
            var recentCustomersMap = new List<System.Tuple<int, string>>();

            var recentCustomers = this.underwriterRecentCustomersRepository.GetAll()
                .Where(e => e.UserName == underwriter).OrderByDescending(e => e.Id);

            foreach (var recentCustomer in recentCustomers) {
                var customer = this._customers.ReallyTryGet(recentCustomer.CustomerId);

                if (customer != null) {
                    recentCustomersMap.Add(
                        new System.Tuple<int, string>(
                            recentCustomer.CustomerId,
                            string.Format(
                                "{0}, {1}, {2}",
                                recentCustomer.CustomerId,
                                customer.PersonalInfo == null ? null : customer.PersonalInfo.Fullname,
                                customer.Name
                            )
                        )
                    );
                } // if
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

            this.m_oDB.ForEachRowSafe(
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
            if (!int.TryParse(term, out id))
                term = term.Replace(" ", "%");

            var findResult = this._session.Query<Customer>()
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
        public JsonResult RejectReasons() {
            return Json(new { reasons = this._rejectReasonRepository.GetAll().ToList() }, JsonRequestBehavior.AllowGet);
        } // RejectReasons

        private enum CustomerState {
            NotSuccesfullyRegistred,
            NotFound,
            Ok,
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
        private readonly AConnection m_oDB;

        private static readonly ASafeLog log = new SafeILog(typeof(CustomersController));
    } // class CustomersController
} // namespace

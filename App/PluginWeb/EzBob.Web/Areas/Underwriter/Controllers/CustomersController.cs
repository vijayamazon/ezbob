using System;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using PluginWebApp.Code.jqGrid;
using Scorto.PluginWeb.Core.jqGrid;
using Scorto.Web;
using StructureMap;
using ZohoCRM;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ISession _session;
        private readonly CustomerRepository _customers;
        private readonly IAppCreator _appCreator;
        private readonly IEzBobConfiguration _config;
        private readonly IDecisionHistoryRepository _historyRepository;
        private readonly IZohoFacade _crm;
        private readonly IWorkplaceContext _context;
        private readonly LoanLimit _limit;

        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridWaiting;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridEscalated;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridApproved;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridLate;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridRejected;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridAll;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridRegisteredCustomers;

        public ViewResult Index()
        {
            var grids = new LoansGrids
                            {

                                WaitingForDecision = new CustomerGridModel
                                {
                                    Action = "Waiting",
                                    ColModel = _gridWaiting.RenderColModel(),
                                    ColNames = _gridWaiting.RenderColNames()
                                },
                                Approved = new CustomerGridModel
                                {
                                    Action = "Approved",
                                    ColModel = _gridApproved.RenderColModel(),
                                    ColNames = _gridApproved.RenderColNames()
                                },

                                Late = new CustomerGridModel
                                {
                                    Action = "Late",
                                    ColModel = _gridLate.RenderColModel(),
                                    ColNames = _gridLate.RenderColNames()
                                },
                     
                                Rejected = new CustomerGridModel
                                {
                                    Action = "Rejected",
                                    ColModel = _gridRejected.RenderColModel(),
                                    ColNames = _gridRejected.RenderColNames()
                                },
                                All = new CustomerGridModel
                                {
                                    Action = "All",
                                    ColModel = _gridAll.RenderColModel(),
                                    ColNames = _gridAll.RenderColNames()
                                },
                                RegisteredCustomers = new CustomerGridModel
                                {
                                    Action = "RegisteredCustomers",
                                    ColModel = _gridRegisteredCustomers.RenderColModel(),
                                    ColNames = _gridRegisteredCustomers.RenderColNames()
                                },
                                Config = _config,
                                MaxLoan = _limit.GetMaxLimit()
                            };

            if (_context.User.Roles.Any(r => r.Name == "manager"))
            {
                grids.Escalated = new CustomerGridModel
                                      {
                                          Action = "Escalated",
                                          ColModel = _gridEscalated.RenderColModel(),
                                          ColNames = _gridEscalated.RenderColNames()
                                      };
            }

            return View(grids);
         }

        public CustomersController(
                                    ISession session, 
                                    CustomerRepository customers, 
                                    IAppCreator appCreator, 
                                    IEzBobConfiguration config,
                                    IDecisionHistoryRepository historyRepository,
                                    IZohoFacade crm,
                                    IWorkplaceContext context, LoanLimit limit)
        {
            _session = session;
            _customers = customers;
            _appCreator = appCreator;
            _config = config;
            _historyRepository = historyRepository;
            _crm = crm;
            _context = context;
            _limit = limit;

            _gridWaiting = CreateColumnsWaitingForDesicion();

            _gridEscalated = CreateColumnsEscalated();

            _gridApproved = CreateColumnsApproved();

            _gridApproved.GetColumnByIndex("RejectedReason").Hidden = true;

            _gridRejected = CreateColumnsApproved();

            _gridAll = CreateColumnsApproved();

            _gridLate = CreateColumnsLate();
            _gridLate.GetColumnByIndex("RejectedReason").Hidden = true;

            _gridRegisteredCustomers = CreateColumnsRegisteredCustomers();
            _gridRegisteredCustomers.GetColumnByIndex("Id").Formatter = "profileWithTypeLink";
        }

        //---------------------------------------------------------------------------------
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Waiting(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridWaiting, settings)
                             {
                                 CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.WaitingForDecision))
                             };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Escalated(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridEscalated, settings)
                             {
                                 CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.Escalated))
                             };
            

            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Approved(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridApproved, settings)
                             {
                                 CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.Approved))
                             };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Rejected(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridRejected, settings)
                             {
                                 CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.Rejected))
                             };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult All(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridRejected, settings)
            {
                CustomizeFilter = crit => crit.Add( Restrictions.IsNotNull("CreditResult") )
            };
            //var result = new GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer>(_session, null, _gridAll, settings);
            return result;
        }
        
        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer> RegisteredCustomers(GridSettings settings)
        {
            var result = new GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer>(_session, null, _gridRegisteredCustomers, settings)
            {
                CustomizeFilter = crit => crit.Add(Restrictions.IsNull("CreditResult"))
            };
            //var result = new GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer>(_session, null, _gridAll, settings);
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Late(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridLate, settings)
            {
                CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.Late))
            };
            return result;
        }

        //---------------------------------------------------------------------------------
        static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsRegisteredCustomers()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateRefNumWithoutLinkColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateUserStatusColumn(gridModel);
            GridHelpers.CreateMPStatusColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateEbayStatus(gridModel);
            GridHelpers.CreateAmazonStatus(gridModel);
            GridHelpers.CreatePayPalStatus(gridModel);
            GridHelpers.CreateWizardStep(gridModel);
            
            return gridModel;
        }

        //---------------------------------------------------------------------------------
        static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsWaitingForDesicion()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();

            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateRefNumColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);            
            GridHelpers.CreateLoanAmountColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateStatusColumn(gridModel);

            return gridModel;
        }

        //---------------------------------------------------------------------------------
        static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsEscalated()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();

            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateRefNumColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateLoanAmountColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateDateEscalatedColumn(gridModel);
            GridHelpers.CreateUnderwriterNameColumn(gridModel);
            GridHelpers.CreateEscalationReasonColumn(gridModel);
            GridHelpers.CreateStatusColumn(gridModel);

            return gridModel;
        }

        //-----------------------------------------------------------------------------
        static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsApproved()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();

            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateRefNumColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateLoanAmountColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateDateApprovedColumn(gridModel);
            GridHelpers.CreateUnderwriterNameColumn(gridModel);
            GridHelpers.CreateManagerNameColumn(gridModel);
            GridHelpers.CreateStatusColumn(gridModel);
            GridHelpers.CreateRejectedReasonColumn(gridModel);

            return gridModel;
        }

        static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsLate()
        {
            var gridModel = CreateColumnsApproved();
            GridHelpers.CreateDelinquency(gridModel);

            return gridModel;
        }
        //-----------------------------------------------------------
        [Transactional]
        [HttpPost]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        public void ChangeStatus(int id, CreditResultStatus status, string reason)
        {
            var workplaceContext = ObjectFactory.GetInstance<IWorkplaceContext>();
            var user = workplaceContext.User;
            var customer = _customers.Get(id);

            customer.CreditResult = status;

            customer.UnderwriterName = user.Name;

            var request = customer.LastCashRequest;
            if (request != null)
            {
                request.IdUnderwriter = user.Id;
                request.UnderwriterDecisionDate = DateTime.UtcNow;
                request.UnderwriterDecision = status;
                request.UnderwriterComment = reason;
            }

            switch (status)
            {
                case CreditResultStatus.Approved:
                    customer.DateApproved = DateTime.UtcNow;
                    customer.Status = Status.Approved;
                    customer.ApprovedReason = reason;
                    var sum = request.ApprovedSum();
                    customer.CreditSum = sum;
                    if (sum <= 0)throw new Exception("Credit sum cannot be zero or less");
                    request.ManagerApprovedSum = (double?) sum;
                    customer.OfferStart = request.OfferStart;
                    customer.OfferValidUntil = request.OfferValidUntil;
                    _historyRepository.LogAction(DecisionActions.Approve, reason, user, customer);
                    _crm.ApproveOffer(request);
                    if (!request.EmailSendingBanned)
                    {
                        _appCreator.ApprovedUser(user, customer, sum);
                    }
                    break;

                case CreditResultStatus.Rejected:
                    customer.RejectedReason = reason;
                    customer.Status = Status.Rejected;
                    _historyRepository.LogAction(DecisionActions.Reject, reason, user, customer);
                    customer.OfferStart = request.OfferStart;
                    customer.OfferValidUntil = request.OfferValidUntil;
                    request.ManagerApprovedSum = null;
                    _crm.RejectOffer(request);
                    if (!request.EmailSendingBanned)
                    {
                        _appCreator.RejectUser(user, customer.Name, customer.Id, customer.PersonalInfo.FirstName);
                    }
                    break;

                case CreditResultStatus.Escalated:
                    customer.CreditResult = CreditResultStatus.Escalated;
                    customer.DateEscalated = DateTime.UtcNow;
                    customer.EscalationReason = reason;
                    _historyRepository.LogAction(DecisionActions.Escalate, reason, user, customer);
                    _appCreator.Escalated(customer);
                    break;
            }
        }
    }

}
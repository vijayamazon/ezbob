using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Aspose.Cells;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.AmazonServiceLib;
using EzBob.CommonLib;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Code;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.csrf;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
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
        private readonly MarketPlaceRepository _mpType;

        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridWaiting;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridEscalated;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridApproved;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridLate;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridRejected;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridAll;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridRegisteredCustomers;
        private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridPending;
		private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridLoans;
		private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridSales;
		private readonly GridModel<EZBob.DatabaseLib.Model.Database.Customer> _gridCollection;

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
                    Pending = new CustomerGridModel
                        {
                            Action = "Pending",
                            ColModel = _gridPending.RenderColModel(),
                            ColNames = _gridPending.RenderColNames()
                        },
                    Loans = new CustomerGridModel
                        {
                            Action = "Loans",
                            ColModel = _gridLoans.RenderColModel(),
                            ColNames = _gridLoans.RenderColNames()
						},
					Sales = new CustomerGridModel
					{
						Action = "Sales",
						ColModel = _gridSales.RenderColModel(),
						ColNames = _gridSales.RenderColNames()
					},
					Collection = new CustomerGridModel
					{
						Action = "Collection",
						ColModel = _gridCollection.RenderColModel(),
						ColNames = _gridCollection.RenderColNames()
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

            grids.MpTypes = _mpType.GetAll().ToList();
            return View(grids);
        }

        public CustomersController(
            ISession session,
            CustomerRepository customers,
            IAppCreator appCreator,
            IEzBobConfiguration config,
            IDecisionHistoryRepository historyRepository,
            IZohoFacade crm,
            IWorkplaceContext context, LoanLimit limit, GridModel<EZBob.DatabaseLib.Model.Database.Customer> pending,
            GridModel<EZBob.DatabaseLib.Model.Database.Customer> loans, MarketPlaceRepository mpType)
        {
            _session = session;
            _customers = customers;
            _appCreator = appCreator;
            _config = config;
            _historyRepository = historyRepository;
            _crm = crm;
            _context = context;
            _limit = limit;
            _gridPending = pending;
            _gridLoans = loans;
            _mpType = mpType;

            _gridWaiting = CreateColumnsWaitingForDesicion();
            _gridEscalated = CreateColumnsEscalated();
            _gridApproved = CreateColumnsApproved();
            _gridRejected = CreateColumnsRejected();
            _gridAll = CreateColumnsAll();
            _gridLate = CreateColumnsLate();
			_gridLoans = CreateColumnsLoans();
			_gridSales = CreateColumnsSales();
			_gridCollection = CreateColumnsCollection();
            _gridPending = CreateColumnsPending();
            _gridRegisteredCustomers = CreateColumnsRegisteredCustomers();
            _gridRegisteredCustomers.GetColumnByIndex("Id").Formatter = "profileWithTypeLink";
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Waiting(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridWaiting, settings)
                {
                    CustomizeFilter =
                        crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.WaitingForDecision))
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
            var result = new UnderwriterGridResult(_session, null, _gridAll, settings)
                {
                    CustomizeFilter = crit => crit.Add(Restrictions.IsNotNull("CreditResult"))
                };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer> RegisteredCustomers(GridSettings settings)
        {
            var result = new GridCriteriaResult<EZBob.DatabaseLib.Model.Database.Customer>(_session, null,
                                                                                           _gridRegisteredCustomers,
                                                                                           settings)
                {
                    CustomizeFilter = crit => crit.Add(Restrictions.IsNull("CreditResult"))
                };
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

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Pending(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridPending, settings)
            {
                CustomizeFilter = crit => crit.Add(Restrictions.Eq("CreditResult", CreditResultStatus.ApprovedPending))
            };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Loans(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridLoans, settings)
            {
                CustomizeFilter = crit => crit.Add(Restrictions.IsNotEmpty("Loans"))
            };
            return result;
        }

        [ValidateJsonAntiForgeryToken]
        [Ajax]
        [HttpGet]
        [Transactional]
        public UnderwriterGridResult Sales(GridSettings settings)
        {
            var result = new UnderwriterGridResult(_session, null, _gridSales, settings)
            {
                CustomizeFilter = crit => crit.Add(Restrictions.Where<EZBob.DatabaseLib.Model.Database.Customer>(c => c.ManagerApprovedSum > c.AmountTaken))
                    .Add(Restrictions.Where<EZBob.DatabaseLib.Model.Database.Customer>(c => c.LatestCRMstatus != "NoSale" || c.LatestCRMstatus == null))
            };
            return result;
        }

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		[Transactional]
		public UnderwriterGridResult Collection(GridSettings settings)
		{
			var result = new UnderwriterGridResult(_session, null, _gridCollection, settings)
			{
				CustomizeFilter = crit => crit.Add(Restrictions.Where<EZBob.DatabaseLib.Model.Database.Customer>(c => c.CollectionStatus.CurrentStatus == CollectionStatusType.Default || c.CollectionStatus.CurrentStatus == CollectionStatusType.Legal))
			};
			return result;
		}

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsAll()
        {
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel, true);
			GridHelpers.CreateMpListColumn(gridModel);
			GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateStatusColumn(gridModel);
            GridHelpers.CreateSystemCalculatedSum(gridModel);
            GridHelpers.CreateManualyApprovedSum(gridModel);
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsRegisteredCustomers()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateUserStatusColumn(gridModel);
            GridHelpers.CreateMpStatusColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateEbayStatusColumn(gridModel);
            GridHelpers.CreateAmazonStatusColumn(gridModel);
			GridHelpers.CreateEkmStatusColumn(gridModel);
			GridHelpers.CreateVolusionStatusColumn(gridModel);
			GridHelpers.CreatePlayStatusColumn(gridModel);
			GridHelpers.CreateShopifyStatusColumn(gridModel);
			GridHelpers.CreatePayPalStatusColumn(gridModel);
			GridHelpers.CreatePayPointStatusColumn(gridModel);
			GridHelpers.CreateYodleeStatusColumn(gridModel);
			GridHelpers.CreateFreeAgentStatusColumn(gridModel);
			GridHelpers.CreateXeroStatusColumn(gridModel);
			GridHelpers.CreateSageStatusColumn(gridModel);
            GridHelpers.CreateWizardStepColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsWaitingForDesicion()
        {
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel, true);
            GridHelpers.CreateMpListColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateLastStatusColumn(gridModel);
            GridHelpers.CreateSystemCalculatedSum(gridModel);
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsEscalated()
        {
            var gridModel = CreateColumnsWaitingForDesicion();
            GridHelpers.CreateDateEscalatedColumn(gridModel);
            GridHelpers.CreateUnderwriterNameColumn(gridModel);
            GridHelpers.CreateEscalationReasonColumn(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsApproved()
        {
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel, true);
            GridHelpers.CreateMpListColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateDateApprovedColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateSystemCalculatedSum(gridModel);
            GridHelpers.CreateManualyApprovedSum(gridModel);
            GridHelpers.CreateAmountTaken(gridModel);
            GridHelpers.CreateOfferExpiryDate(gridModel);
            GridHelpers.CreateNumApprovals(gridModel);
            GridHelpers.CreateNumRejections(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsRejected()
        {
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel, true);
            GridHelpers.CreateMpListColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateDateRejectedColumn(gridModel);
            GridHelpers.CreateRejectedReasonColumn(gridModel);
            GridHelpers.CreateNumRejections(gridModel);
            GridHelpers.CreateNumApprovals(gridModel);
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsLate()
        {
            var gridModel = CreateColumnsApproved();
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.DateOfLatePayment(gridModel);
            GridHelpers.CreateLateAmount(gridModel);
            gridModel.Columns.First(x => x.Index == "OfferValidUntil").Hidden = true;
            GridHelpers.CreateDelinquencyColumn(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsPending()
        {
            var gridModel = CreateColumnsWaitingForDesicion();
            GridHelpers.CreatePendingStatusColumn(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsLoans()
        {
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateCartColumn(gridModel, true);
            GridHelpers.CreateMpListColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateRegisteredDateColumn(gridModel);
            GridHelpers.CreateDateApplyedColumn(gridModel);
            GridHelpers.CreateFirstLoanColumn(gridModel);
            GridHelpers.CreateLastLoanDateColumn(gridModel);
            GridHelpers.CreateLastLoanAmountColumn(gridModel);
            GridHelpers.CreateAmountTaken(gridModel);
            GridHelpers.CreateTotalPrincipalRepaidColumn(gridModel);
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.CreateNextRepaymentDateColumn(gridModel);
            GridHelpers.CreateStatusColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

        private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsSales()
        {
            var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
            GridHelpers.CreateIdColumn(gridModel);
            GridHelpers.CreateEmailColumn(gridModel);
            GridHelpers.CreateNameColumn(gridModel);
            GridHelpers.CreatePhoneColumn(gridModel);
            GridHelpers.CreateManualyApprovedSum(gridModel);
            GridHelpers.CreateAmountTaken(gridModel);
            GridHelpers.CreateManualyOfferDate(gridModel);
            GridHelpers.CreateOutstandingBalanceColumn(gridModel);
            GridHelpers.CreateLatestCRMstatus(gridModel);
            GridHelpers.CreateAmountOfInteractions(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
            return gridModel;
        }

		private static GridModel<EZBob.DatabaseLib.Model.Database.Customer> CreateColumnsCollection()
		{
			var gridModel = new GridModel<EZBob.DatabaseLib.Model.Database.Customer>();
			GridHelpers.CreateIdColumn(gridModel);
			GridHelpers.CreateEmailColumn(gridModel);
			GridHelpers.CreateNameColumn(gridModel);
			GridHelpers.CreatePhoneColumn(gridModel);
			GridHelpers.CreateAmountTaken(gridModel);
			GridHelpers.CreateOutstandingBalanceColumn(gridModel);
			GridHelpers.CreateLatestCRMstatus(gridModel);
			GridHelpers.CreateCollectionStatusColumn(gridModel);
            GridHelpers.CreatePaymentDemeanor(gridModel);
			return gridModel;
		}

        [Transactional]
        [HttpPost]
        [Ajax]
        [ValidateJsonAntiForgeryToken]
        public void ChangeStatus(int id, CreditResultStatus status, string reason)
        {
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

            switch (status)
            {
                case CreditResultStatus.Approved:
                    customer.DateApproved = DateTime.UtcNow;
                    customer.Status = Status.Approved;
                    customer.ApprovedReason = reason;

                    var sum = request.ApprovedSum();
                    if (sum <= 0) throw new Exception("Credit sum cannot be zero or less");
                    _limit.Check(sum);

                    customer.CreditSum = sum;
                    customer.IsLoanTypeSelectionAllowed = request.IsLoanTypeSelectionAllowed;
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
                    customer.DateRejected = DateTime.UtcNow;
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
                case CreditResultStatus.ApprovedPending:
                    customer.CreditResult = CreditResultStatus.ApprovedPending;
                    customer.PendingStatus = PendingStatus.Manual;
                    _historyRepository.LogAction(DecisionActions.Pending, "", user, customer);
                    break;
                case CreditResultStatus.WaitingForDecision:
                    customer.CreditResult = CreditResultStatus.WaitingForDecision;
                    _historyRepository.LogAction(DecisionActions.Waiting, "", user, customer);
                    break;
            }
        }

        private enum CustomerState
        {
            NotSuccesfullyRegistred,
            NotFound,
            Ok
        }

        [HttpGet]
        [Ajax]
        public JsonNetResult CheckCustomer(int customerId)
        {
            var customer = _customers.TryGet(customerId);
            if (customer == null) return this.JsonNet(new { State = CustomerState.NotFound.ToString() });
            if (customer.WizardStep != WizardStepType.AllStep) return this.JsonNet(new { State = CustomerState.NotSuccesfullyRegistred.ToString() });
            return this.JsonNet(new { State = CustomerState.Ok.ToString() });
        }

        [HttpGet]
        [Ajax]
        public JsonNetResult GetCounters(bool isTest)
        {   
            var model = new List<CustomersCountersModel>
                {
                    new CustomersCountersModel
                        {
                            Count =
                                _customers.GetAll().Count(x => x.CreditResult == CreditResultStatus.WaitingForDecision && (isTest || x.IsTest == false)),
                            Name = "waiting"
                        },
                    new CustomersCountersModel
                        {
                            Count =
                                _customers.GetAll().Count(x => x.CreditResult == CreditResultStatus.ApprovedPending && (isTest || x.IsTest == false)),
                            Name = "pending"
                        },
                    new CustomersCountersModel
                        {
                            Count =
                                _customers.GetAll().Count(x => x.CreditResult == null && x.WizardStep == WizardStepType.AllStep && (isTest || x.IsTest == false)),
                            Name = "RegisteredCustomers"
                        },
                    new CustomersCountersModel
                        {
                            Count =
                                _customers.GetAll().Count(x => x.CreditResult == CreditResultStatus.Escalated && (isTest || x.IsTest == false)),
                            Name = "escalated"
                        },
                };
            return this.JsonNet(model);
        }
        
        [HttpGet]
        [Ajax]
        public JsonNetResult FindCustomer(string term)
        {
            term = term.Trim();
            int id;
            int.TryParse(term, out id);

            var findResult =
                _session.Query<EZBob.DatabaseLib.Model.Database.Customer>()
						.Where(x => x.WizardStep == WizardStepType.AllStep)
                        .Where(
                            c =>
                            c.Id == id || c.Name.Contains(term) ||
                            c.PersonalInfo.Fullname.Contains(term))
                        .Select(x => string.Format("{0}, {1}, {2}", x.Id, x.PersonalInfo.Fullname, x.Name))
                        .Take(20);

            var retVal = new HashSet<string>(findResult);
            return this.JsonNet(retVal.Take(15));
        }

        public string AllMarketplaceId()
        {
            var mps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
            var amazon = mps.GetAll().Where(x => x.Marketplace.Name == "Amazon");
            var retVal = (from a in amazon
                                   let mid =
                                       string.Join(",",
                                                   SerializeDataHelper.DeserializeType<AmazonSecurityInfo>(
                                                       a.SecurityData).MarketplaceId)
                                   let mrid =
                                       SerializeDataHelper.DeserializeType<AmazonSecurityInfo>(a.SecurityData)
                                                          .MerchantId
                                   select
                                       string.Format("Id = {0}, MarketplaceId = {1}, MerchantId = {2}, DisplayName = {3}, Customer.Id = {4}", a.Id, mid, mrid, a.DisplayName, a.Customer.Id))
                .ToList();

            return string.Join("<br/>", retVal);
        }

        public FileResult AllMarketplaceIdExcel()
        {
            var mps = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();
            var amazon = mps.GetAll().Where(x => x.Marketplace.Name == "Amazon").ToList();
            var e = new Workbook();
            var w = e.Worksheets[e.Worksheets.ActiveSheetIndex];

            w.Cells[0,0].PutValue("MPCustomerMarketplaceId");
            w.Cells[0, 1].PutValue("MarketplaceId");
            w.Cells[0, 2].PutValue("MerchantId");
            w.Cells[0, 3].PutValue("DisplayName");
            w.Cells[0, 4].PutValue("Customer.Id");

            for (var i = 0; i < amazon.Count(); i++ )
            {
                var a = amazon.ElementAt(i);
                var mid = string.Join(",", SerializeDataHelper.DeserializeType<AmazonSecurityInfo>(a.SecurityData).MarketplaceId);
                var mrid = SerializeDataHelper.DeserializeType<AmazonSecurityInfo>(a.SecurityData).MerchantId;


                w.Cells[i + 1, 0].PutValue(a.Id);
                w.Cells[i + 1, 1].PutValue(mid);
                w.Cells[i + 1, 2].PutValue(mrid);
                w.Cells[i + 1, 3].PutValue(a.DisplayName);
                w.Cells[i + 1, 4].PutValue(a.Customer.Id);
            }

            using (var streamForDoc = new MemoryStream())
            {
                e.Save(streamForDoc, FileFormatType.Excel2007Xlsx);
                var fs = new FileContentResult(streamForDoc.ToArray(), "application/vnd.ms-excel")
                    {
                        FileDownloadName = "AllMarketplaceIdExcel_" + DateTime.UtcNow.ToShortTimeString() + ".xlsx"
                    };
                return fs;

            }
        }
    }
}
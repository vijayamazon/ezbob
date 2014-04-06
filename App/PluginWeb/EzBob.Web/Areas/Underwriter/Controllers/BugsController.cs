using System;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Infrastructure;
using System.Linq;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using Infrastructure.Attributes;

	public class BugsController : Controller
    {
        private readonly IBugRepository _bugs;
        private readonly IEzbobWorkplaceContext _context;
        private readonly ICustomerRepository _customers;
        private readonly IUsersRepository _users;

        public BugsController(IBugRepository bugs, IEzbobWorkplaceContext context, ICustomerRepository customers, IUsersRepository users)
        {
            _bugs = bugs;
            _context = context;
            _customers = customers;
            _users = users;
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult CreateBug(BugModel bug)
        {
            var b = bug.FromModel(_customers, _users);
            b.UnderwriterOpened = _context.User;
            _bugs.Save(b);
            return Json(new {});
        }
        
        [Ajax]
        [Transactional]
        public JsonResult UpdateBug(BugModel bug)
        {
            var b = bug.FromModel(_customers, _users);
            b.UnderwriterOpened = _context.User;
            _bugs.Update(b);
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonResult TryGet(int customerid, string bugtype, int? mp, int? director)
        {
            var bug = _bugs.Search(customerid, bugtype, mp, director);
            return Json(BugModel.ToModel(bug), JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Close(BugModel bugm)
        {
            var bug = _bugs.Get(bugm.Id);
            bug.State = BugState.Closed;
            bug.DateClosed = DateTime.UtcNow;
            bug.UnderwriterClosed = _context.User;
            bug.TextClosed = bugm.TextClosed;
            return Json(BugModel.ToModel(bug));
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonResult Reopen(BugModel bugModel)
        {
            var bug = _bugs.Get(bugModel.Id);
            bug.State = BugState.Reopened;
            bug.DateOpened = DateTime.UtcNow;
            bug.UnderwriterOpened = _context.User;
            return Json(BugModel.ToModel(bug));
        }

        [Ajax]
        [HttpGet]
        public JsonResult GetAllForCustomer(int customerId)
        {
            var bugs = _bugs.GetAll()
                .Where(x => x.Customer.Id == customerId)
                .Select(x=> BugModel.ToModel(x));
            return Json(bugs, JsonRequestBehavior.AllowGet);
        }
    }

    public class BugModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string Type { get; set; }
        public int? MarketPlaceId { get; set; }
        public int? DirectorId { get; set; }
        public DateTime DateOpened { get; set; }
        public DateTime? DateClosed { get; set; }
        public string TextOpened { get; set; }
        public string TextClosed { get; set; }
        public string UnderwriterOpenedName { get; set; }
        public string UnderwriterClosedName { get; set; }
        public int UnderwriterOpenedId { get; set; }
        public int? UnderwriterClosedId { get; set; }
        public string State { get; set; }

        public Bug FromModel(ICustomerRepository customers, IUsersRepository users)
        {
            return new Bug()
                       {
                           Id = Id,
                           Customer = CustomerId == null ? null : customers.Load(CustomerId.Value),
                           DateOpened = DateOpened,
                           DateClosed = DateClosed,
                           MarketPlaceId = MarketPlaceId,
                           CreditBureauDirectorId = DirectorId,
                           TextOpened = TextOpened,
                           TextClosed = TextClosed,
                           Type = Type,
                           UnderwriterClosed = UnderwriterClosedId == null ? null : users.Load(UnderwriterClosedId.Value),
                           UnderwriterOpened = users.Load(UnderwriterOpenedId)
                       };
        }

        public static BugModel ToModel(Bug bug)
        {
            if (bug == null) return null;
            var bugModel = new BugModel()
                               {
                                   Id = bug.Id, 
                                   CustomerId = bug.Customer.Id, 
                                   DateOpened = bug.DateOpened, 
                                   DateClosed = bug.DateClosed, 
                                   MarketPlaceId = bug.MarketPlaceId, 
                                   TextOpened = bug.TextOpened, 
                                   TextClosed = bug.TextClosed, 
                                   Type = bug.Type,
                                   State = bug.State.ToString(),
                                   DirectorId = bug.CreditBureauDirectorId
                               };

            if (bug.UnderwriterOpened != null)
            {
                bugModel.UnderwriterOpenedId = bug.UnderwriterOpened.Id;
                bugModel.UnderwriterOpenedName = bug.UnderwriterOpened.FullName;
            }
            if (bug.UnderwriterClosed != null)
            {
                bugModel.UnderwriterClosedId = bug.UnderwriterClosed.Id;
                bugModel.UnderwriterClosedName = bug.UnderwriterClosed.FullName;
            }

            return bugModel;
        }
    }
}
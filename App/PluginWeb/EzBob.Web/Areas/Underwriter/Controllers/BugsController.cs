namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Infrastructure.Attributes;
	using Models;

	public class BugsController : Controller
	{
		private readonly IBugRepository _bugs;
		private readonly IEzbobWorkplaceContext _context;
		private readonly ICustomerRepository _customers;
		private readonly IUsersRepository _users;

		public BugsController(IBugRepository bugs,
			IEzbobWorkplaceContext context,
			ICustomerRepository customers, 
			IUsersRepository users)
		{
			_bugs = bugs;
			_context = context;
			_customers = customers;
			_users = users;
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "OpenBug")]
		public JsonResult CreateBug(BugModel bug)
		{
			var b = bug.FromModel(_customers, _users);
			b.UnderwriterOpened = _context.User;
			_bugs.Save(b);
			return Json(new { });
		}

		[Ajax]
		[Transactional]
		[Permission(Name = "CloseEditBug")]
		public JsonResult UpdateBug(BugModel bugModel)
		{
			var bug = _bugs.Get(bugModel.Id);

			bug.UnderwriterOpened = _context.User;
			bug.TextOpened = bugModel.TextOpened;
			bug.TextClosed = bugModel.TextClosed;
			_bugs.Update(bug);
			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpGet]
		public JsonResult TryGet(int customerid, string bugtype, int? mp, int? director)
		{
			var bug = _bugs.Search(customerid, bugtype, mp, director);
			return Json(BugModel.ToModel(bug), JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[Transactional]
		[Permission(Name = "CloseEditBug")]
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
		[Permission(Name = "OpenBug")]
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
				.Select(x => BugModel.ToModel(x));
			return Json(bugs, JsonRequestBehavior.AllowGet);
		}
	}
}
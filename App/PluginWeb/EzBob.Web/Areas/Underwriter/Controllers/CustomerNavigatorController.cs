namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;
	using NHibernate.Linq;

	public class CustomerNavigatorController : Controller {
		public CustomerNavigatorController(
			ISession session,
			CustomerRepository customersRepo,
			UnderwriterRecentCustomersRepository uwRecentCustomersRepo
		) {
			this.session = session;
			this.customersRepo = customersRepo;
			this.uwRecentCustomersRepo = uwRecentCustomersRepo;
		} // constructor

		[HttpGet]
		[Ajax]
		public JsonResult CheckCustomer(int customerId) {
			var customer = this.customersRepo.ReallyTryGet(customerId);

			var nState = (customer == null)
				? CustomerState.NotFound
				: (customer.WizardStep.TheLastOne ? CustomerState.Ok : CustomerState.NotSuccesfullyRegistred);

			return Json(new { State = nState.ToString() }, JsonRequestBehavior.AllowGet);
		} // CheckCustomer

		[HttpPost]
		[Ajax]
		public JsonResult SetRecentCustomer(int id) {
			this.uwRecentCustomersRepo.Add(id, User.Identity.Name);
			return GetRecentCustomers();
		} // SetRecentCustomer

		[HttpGet]
		[Ajax]
		public JsonResult GetRecentCustomers() {
			string underwriter = User.Identity.Name;
			var recentCustomersMap = new List<System.Tuple<int, string>>();

			var recentCustomers = this.uwRecentCustomersRepo
				.GetAll()
				.Where(e => e.UserName == underwriter)
				.OrderByDescending(e => e.Id);

			foreach (var recentCustomer in recentCustomers) {
				var customer = this.customersRepo.ReallyTryGet(recentCustomer.CustomerId);

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
		public JsonResult FindCustomer(string term) {
			term = term.Trim();
			int id;
			if (!int.TryParse(term, out id))
				term = term.Replace(" ", "%");

			var findResult = this.session.Query<Customer>()
				.Where(c =>
					c.Id == id || c.Name.Contains(term) ||
					c.PersonalInfo.Fullname.Contains(term)
				)
				.Select(x => string.Format("{0}, {1}, {2}", x.Id, x.PersonalInfo.Fullname, x.Name))
				.Take(20);

			var retVal = new HashSet<string>(findResult);

			return Json(retVal.Take(15), JsonRequestBehavior.AllowGet);
		} // FindCustomer

		private enum CustomerState {
			NotSuccesfullyRegistred,
			NotFound,
			Ok,
		} // enum CustomerState

		private readonly ISession session;
		private readonly UnderwriterRecentCustomersRepository uwRecentCustomersRepo;
		private readonly CustomerRepository customersRepo;
	} // class CustomerNavigatorController
} // namespace

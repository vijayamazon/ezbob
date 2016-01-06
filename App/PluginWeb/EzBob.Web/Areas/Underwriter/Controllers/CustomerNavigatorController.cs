namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Ezbob.Database;
	using EzBob.Web.Infrastructure.Attributes;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class CustomerNavigatorController : Controller {
		public CustomerNavigatorController(
			CustomerRepository customersRepo,
			UnderwriterRecentCustomersRepository uwRecentCustomersRepo
		) {
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

			foreach (UnderwriterRecentCustomers recentCustomer in recentCustomers) {
				var customer = this.customersRepo.ReallyTryGet(recentCustomer.CustomerId);

				if (customer != null) {
					recentCustomersMap.Add(
						new System.Tuple<int, string>(
							recentCustomer.CustomerId,
							string.Format(
								"{0}: {1}, {2} ({3})",
								recentCustomer.CustomerId,
								customer.PersonalInfo == null ? null : customer.PersonalInfo.Fullname,
								customer.Name,
								customer.CustomerOrigin.Name
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
			bool queryDB = false;

			term = (term ?? string.Empty).Trim();

			int id;

			if (int.TryParse(term, out id))
				queryDB = true;
			else {
				id = 0;

				if (term.Length > 2) {
					queryDB = true;
					term = term.Replace(" ", "%");
				} // if
			} // if

			var retVal = new HashSet<string>();

			if (!queryDB)
				return Json(retVal, JsonRequestBehavior.AllowGet);

			DbConnectionGenerator.Get().ForEachRowSafe(
				sr => {
					retVal.Add(string.Format(
						"{0}: {1}, {2} ({3})",
						(int)sr["CustomerID"],
						(string)sr["CustomerName"],
						(string)sr["Email"],
						(string)sr["Origin"]
					));
				},
				"FindCustomer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Ntoken", id),
				new QueryParameter("Stoken", term.ToLowerInvariant())
			);

			return Json(retVal, JsonRequestBehavior.AllowGet);
		} // FindCustomer

		private enum CustomerState {
			NotSuccesfullyRegistred,
			NotFound,
			Ok,
		} // enum CustomerState

		private readonly UnderwriterRecentCustomersRepository uwRecentCustomersRepo;
		private readonly CustomerRepository customersRepo;
	} // class CustomerNavigatorController
} // namespace

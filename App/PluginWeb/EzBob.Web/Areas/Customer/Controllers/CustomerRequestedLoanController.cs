namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Infrastructure;
	using Infrastructure.csrf;
	using log4net;

	public class CustomerRequestedLoanController : Controller {
		public CustomerRequestedLoanController(
			IEzbobWorkplaceContext oContext,
			CustomerRepository customerRepository
		) {
			this.context = oContext;
			this.customerRepository = customerRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RequestedLoan() {
			var customer = this.context.Customer;
			var requestedLoan = customer.CustomerRequestedLoan.OrderByDescending(x => x.Created).FirstOrDefault() ?? new CustomerRequestedLoan();
			Log.Info("Load requested loan for customer " + customer.Id + " " + requestedLoan.Amount + " " + requestedLoan.Term);
			return Json(requestedLoan, JsonRequestBehavior.AllowGet);
		} // RequestedLoan Get

		[Transactional]
		[Ajax]
		[HttpPut]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RequestedLoan(CustomerRequestedLoan requested) {
			
			var customer = this.context.Customer;
			Log.Info("Save requested loan for customer " + customer.Id);
			var requestedLoan = new CustomerRequestedLoan {
				Created = DateTime.UtcNow,
				Amount = requested.Amount,
				Term = requested.Term,
				CustomerId = customer.Id
			};
			customer.CustomerRequestedLoan.Add(requestedLoan);
			this.customerRepository.Save(customer);
			return Json(requestedLoan, JsonRequestBehavior.AllowGet);
		} // RequestedLoan Save


		private readonly IEzbobWorkplaceContext context;
		private readonly CustomerRepository customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRequestedLoanController));
	} // class CustomerRequestedLoanController
} // namespace EzBob.Web.Areas.Customer.Controllers

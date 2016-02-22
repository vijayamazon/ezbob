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
	using ServiceClientProxy;
	using FrontRequestedLoanModel = EzBob.Web.Areas.Customer.Models.RequestedLoanModel;

	public class CustomerRequestedLoanController : Controller {
		private readonly ServiceClient serviceClient;

		public CustomerRequestedLoanController(
			ServiceClient serviceClient,
			IEzbobWorkplaceContext oContext,
			CustomerRepository customerRepository
		) {
			this.serviceClient = serviceClient;
			this.context = oContext;
			this.customerRepository = customerRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RequestedLoan() {
			var customer = this.context.Customer;
			var slidersResult = this.serviceClient.Instance.GetSlidersData(customer.Id);

			var requestedLoanResult = new EzBob.Web.Areas.Customer.Models.RequestedLoanModel {
				CustomerId = customer.Id,
				Amount = (double?)slidersResult.SlidersData.Amount,
				Term = slidersResult.SlidersData.Term,
				MinLoanAmount = slidersResult.SlidersData.MinLoanAmount,
				MaxLoanAmount = slidersResult.SlidersData.MaxLoanAmount,
				MinTerm = slidersResult.SlidersData.MinTerm,
				MaxTerm = slidersResult.SlidersData.MaxTerm
			};

			Log.Info("Load requested loan for customer " + customer.Id + " of amount " + slidersResult.SlidersData.Amount + " for term of " + slidersResult.SlidersData.Term);

			return Json(requestedLoanResult, JsonRequestBehavior.AllowGet);
		} // RequestedLoan Get

		[Transactional]
		[Ajax]
		[HttpPut]
		[ValidateJsonAntiForgeryToken]
		public JsonResult RequestedLoan(FrontRequestedLoanModel requested) {
			
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

			var requestedLoanResult = new FrontRequestedLoanModel {
				Created = DateTime.UtcNow,
				Amount = requested.Amount,
				Term = requested.Term,
				CustomerId = customer.Id
			};
			return Json(requestedLoanResult, JsonRequestBehavior.AllowGet);
		} // RequestedLoan Save


		private readonly IEzbobWorkplaceContext context;
		private readonly CustomerRepository customerRepository;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerRequestedLoanController));
	} // class CustomerRequestedLoanController
} // namespace EzBob.Web.Areas.Customer.Controllers

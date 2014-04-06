﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Infrastructure.Attributes;
	using ActionResult = System.Web.Mvc.ActionResult;

	public class ProfileController : Controller
    {
        private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
        private CustomerRepository CustomerRepository { get; set; }

        public ProfileController(CustomerRepository customerRepository, ProfileSummaryModelBuilder summaryModelBuilder) {
            _summaryModelBuilder = summaryModelBuilder;
            CustomerRepository = customerRepository;
        }

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index(int id)
        {
            var customer = CustomerRepository.Get(id);
            var model = _summaryModelBuilder.CreateProfile(customer);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Ajax]
        [HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonResult SaveComment(string comment, int id)
        {
            var customer = CustomerRepository.Get(id);
            customer.Comment = comment;
            return Json(new {Saved = "true"});
        }
    }
}
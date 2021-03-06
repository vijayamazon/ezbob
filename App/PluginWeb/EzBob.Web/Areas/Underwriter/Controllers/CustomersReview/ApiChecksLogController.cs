﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Models;
	using Infrastructure.csrf;

	public class ApiChecksLogController : Controller
	{
		private readonly ICustomerRepository _customerRepository;
		private readonly ApiCheckLogBuilder _builder;

		public ApiChecksLogController(ICustomerRepository customerRepository, ApiCheckLogBuilder builder)
		{
			_customerRepository = customerRepository;
			_builder = builder;
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id)
		{
			var customer = _customerRepository.Get(id);

			var models = _builder.Create(customer);

			return Json(models, JsonRequestBehavior.AllowGet);
		}

	}
}

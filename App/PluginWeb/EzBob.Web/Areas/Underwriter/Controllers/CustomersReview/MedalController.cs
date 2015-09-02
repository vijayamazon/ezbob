﻿namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using Code;
	using System.Web.Mvc;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using ServiceClientProxy;

	public class MedalController : Controller {
		public MedalController(CustomerRepository customersRepository, IWorkplaceContext context) {
			this.customerRepository = customersRepository;
			this.serviceClient = new ServiceClient();
			this.context = context;
		} // constructor

		[Ajax]
		[HttpGet]
		public ActionResult Index(int id) {
			var customer = this.customerRepository.Get(id);
			var medalCalculator = new MedalCalculators(customer);
			return Json(medalCalculator, JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpGet]
		public ActionResult ExportToExel(int id) {
			var customer = this.customerRepository.Get(id);
			return new MedalExcelReportResult(customer);
		} // exportToExcel

		[Ajax]
		[HttpPost]
		public void RecalculateMedal(int customerId) {
			this.serviceClient.Instance.CalculateMedal(this.context.UserId, customerId, null);
		} // RecalculateMedal

		private readonly CustomerRepository customerRepository;
		private readonly ServiceClient serviceClient;
		private readonly IWorkplaceContext context;
	} // class MedalController
} // namespace

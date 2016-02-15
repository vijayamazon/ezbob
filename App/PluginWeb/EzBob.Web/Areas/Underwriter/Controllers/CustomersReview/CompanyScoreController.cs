namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EzBob.Web.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Iesi.Collections.Generic;
	
	public class CompanyScoreController : Controller {
		public CompanyScoreController(ICustomerRepository customerRepository,
			CustomerCompanyHistoryRepository customerCompanyHistoryRepository) {
			this.m_oCustomerRepository = customerRepository;
			this.m_oCustomerCompanyHistoryRepository = customerCompanyHistoryRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id) {
			var customer = this.m_oCustomerRepository.Get(id);
			var builder = new CompanyScoreModelBuilder();
			return Json(builder.Create(customer), JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		[Permission(Name="ChangeCompany")]
		public JsonResult ChangeCompany(CompanyDetails details) {
			var customer = this.m_oCustomerRepository.Get(details.CustomerId);
			if (customer == null) {
				return Json(new {
					success = false,
					message = "customer is null"
				});
			} // if

			var typeOfBusiness = (TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), details.TypeOfBusiness);
			if (!details.CompanyAddress.Any() || details.CompanyAddress.Count() > 1) {
				return Json(new {
					success = false,
					message = "no address"
				});
			} // if

			if (customer.PersonalInfo == null)
				customer.PersonalInfo = new PersonalInfo();

			customer.PersonalInfo.TypeOfBusiness = typeOfBusiness;
			var company = new Company {
				TypeOfBusiness = typeOfBusiness,
				CompanyName = details.CompanyName,
				CompanyNumber = details.CompanyRefNum,
			};

			var address = details.CompanyAddress.First();
			address.Company = company;
			address.Customer = customer;
			address.AddressId = 0;

			switch (typeOfBusiness.Reduce()) {
			case TypeOfBusinessReduced.Limited:
				address.AddressType = CustomerAddressType.LimitedCompanyAddress;
				break;
			default:
				address.AddressType = CustomerAddressType.NonLimitedCompanyAddress;
				break;
			} // switch

			company.CompanyAddress = new HashedSet<CustomerAddress> {
				address,
			};

			if (customer.Company != null) {
				company.RentMonthLeft = customer.Company.RentMonthLeft;
				company.TimeAtAddress = customer.Company.TimeAtAddress;
				company.TimeInBusiness = customer.Company.TimeInBusiness;
				company.VatReporting = customer.Company.VatReporting;
				company.YearsInCompany = customer.Company.YearsInCompany;
			} // if

			this.m_oCustomerRepository.BeginTransaction();
			this.m_oCustomerRepository.EnsureTransaction(() => {
				customer.Company = company;
				this.m_oCustomerRepository.SaveOrUpdate(customer);
			});
			this.m_oCustomerRepository.CommitTransaction();

			this.m_oCustomerCompanyHistoryRepository.SaveOrUpdate(new CustomerCompanyHistory {
				CustomerId = customer.Id,
				CompanyId = customer.Company.Id,
				InsertDate = DateTime.UtcNow,
			});

			return Json(new {
				success = true,
			});
		} // ChangeCompany

		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly CustomerCompanyHistoryRepository m_oCustomerCompanyHistoryRepository;
	} // CompanyScoreController
} // namespace

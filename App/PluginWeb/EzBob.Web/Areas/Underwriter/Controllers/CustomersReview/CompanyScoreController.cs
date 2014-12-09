namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Iesi.Collections.Generic;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Web.Models;
	using System.Collections.Generic;

	public class CompanyScoreController : Controller
	{
		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly CustomerCompanyHistoryRepository m_oCustomerCompanyHistoryRepository;
		public CompanyScoreController(ICustomerRepository customerRepository, 
			CustomerCompanyHistoryRepository customerCompanyHistoryRepository) {
			m_oCustomerRepository = customerRepository;
			m_oCustomerCompanyHistoryRepository = customerCompanyHistoryRepository;
		}

		// constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id)
		{
			var customer = m_oCustomerRepository.Get(id);
			var builder = new CompanyScoreModelBuilder();
			return Json(builder.Create(customer), JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonResult ChangeCompany(CompanyDetails details) {
			var customer = m_oCustomerRepository.Get(details.CustomerId);
			if (customer == null) {
				return Json(new { success = false, message = "customer is null"});
			}
			var typeOfBusiness = (TypeOfBusiness) Enum.Parse(typeof (TypeOfBusiness), details.TypeOfBusiness);
			if (!details.CompanyAddress.Any() || details.CompanyAddress.Count() > 1) {
				return Json(new { success = false, message = "no address" });
			}

			if (customer.PersonalInfo == null) {
				customer.PersonalInfo = new PersonalInfo();
			}

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
				case TypeOfBusinessReduced.NonLimited:
					address.AddressType = CustomerAddressType.NonLimitedCompanyAddress;
					break;
				case TypeOfBusinessReduced.Personal:
					//TODO not sure about it
					address.AddressType = CustomerAddressType.NonLimitedCompanyAddress;
					break;
			}

			company.CompanyAddress = new HashedSet<CustomerAddress> { address };

			if (customer.Company != null) {
				company.RentMonthLeft = customer.Company.RentMonthLeft;
				company.TimeAtAddress = customer.Company.TimeAtAddress;
				company.TimeInBusiness = customer.Company.TimeInBusiness;
				company.VatReporting = customer.Company.VatReporting;
				company.YearsInCompany = customer.Company.YearsInCompany;
			}

			m_oCustomerRepository.BeginTransaction();
			m_oCustomerRepository.EnsureTransaction(() => {
				customer.Company = company;
				m_oCustomerRepository.SaveOrUpdate(customer);
			});
			m_oCustomerRepository.CommitTransaction();

			m_oCustomerCompanyHistoryRepository.SaveOrUpdate(new CustomerCompanyHistory {
				CustomerId = customer.Id,
				CompanyId = customer.Company.Id,
				InsertDate = DateTime.UtcNow
			});

			return Json(new {success=true});
		}
	} // CompanyScoreController
} // namespace

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using EzBob.Web.Infrastructure;
    using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
    using log4net;
    using Models;
    using MoreLinq;
    using ServiceClientProxy;

    public class CustomerInfoController : Controller
	{
		public CustomerInfoController(CustomerRepository customerRepository, CustomerPhoneRepository customerPhoneRepository, IEzbobWorkplaceContext context) {
		    this.customerRepository = customerRepository;
		    this.customerPhoneRepository = customerPhoneRepository;
		    this.context = context;
		}

	    [Ajax]
		[HttpGet]
		public JsonResult Index(int id)
		{
			var customer = this.customerRepository.Get(id);
			var model = new PersonalInfoModel();
			model.InitFromCustomer(customer);
			return Json(model, JsonRequestBehavior.AllowGet);
		}//Index

        [Ajax]
        [HttpPost]
		[Permission(Name = "ChangePhone")]
        public JsonResult ChangePhone(int customerID, string phoneType, string newPhone) {
            Log.InfoFormat("Change phone for customer {0} type {1} new phone {2}", customerID, phoneType, newPhone);
            DateTime now = DateTime.UtcNow;
            var customer = this.customerRepository.Get(customerID);
            var customerPhones = this.customerPhoneRepository.GetByCustomer(customerID).ToList();
            switch (phoneType) {
                case "Mobile":
                    customer.PersonalInfo.MobilePhone = newPhone;
                    break;
                case "Daytime":
                    customer.PersonalInfo.DaytimePhone = newPhone;
                    break;
            }

            customerPhones
                .Where(x => x.PhoneType == phoneType)
                .ForEach(x => { x.IsCurrent = false; });

            customerPhones.Add(new CustomerPhone {
                CustomerId = customerID,
                PhoneType = phoneType,
                Phone = newPhone,
                IsCurrent = true,
                IsVerified = true,
                VerificationDate = now,
                VerifiedBy = User.Identity.Name
            });

            new Transactional(() => {
                this.customerRepository.Update(customer);
                foreach (var phone in customerPhones) {
                    this.customerPhoneRepository.SaveOrUpdate(phone);
                }
            }).Execute();
            
            ServiceClient client = new ServiceClient();
            client.Instance.SalesForceAddUpdateLeadAccount(this.context.UserId, customer.Name, customerID, false, false);

            return Json(new {success = true});
        }//ChangePhone

        private readonly CustomerRepository customerRepository;
        private readonly CustomerPhoneRepository customerPhoneRepository;
        private readonly IEzbobWorkplaceContext context;
        private static readonly ILog Log = LogManager.GetLogger(typeof(CustomerInfoController));
    }//class CustomerInfoController
}//ns

using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure;
using Scorto.Web;
using ZohoCRM;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class CustomerInfoController : Controller
    {
        private readonly IZohoFacade _crm;
        private readonly ICustomerRepository _customers;

        public CustomerInfoController(IZohoFacade crm, ICustomerRepository customers)
        {
            _crm = crm;
            _customers = customers;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id); 
            var model = new PersonalInfoModel();
            model.InitFromCustomer(customer);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        [Permission(Name = "CRM")]
        public JsonNetResult UpdateCrm(int id)
        {
            var customer = _customers.Get(id);
            _crm.UpdateOrCreate(customer);
            return this.JsonNet(new {});
        }
    }
}

using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure;
using NHibernate;
using Scorto.Web;
using ZohoCRM;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class CustomerInfoController : Controller
    {
        private readonly IZohoFacade _crm;
        private readonly ICustomerRepository _customers;
        private readonly ISession _session;

        public CustomerInfoController(IZohoFacade crm, ICustomerRepository customers, ISession session)
        {
            _crm = crm;
            _customers = customers;
            _session = session;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index(int id)
        {
            var customer = _customers.Get(id); 
            var model = new PersonalInfoModel();
            model.InitFromCustomer(customer, _session);
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

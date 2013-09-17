using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
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
        [Transactional]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            var customer = _customerRepository.Get(id);

            var models = _builder.Create(customer);

            return this.JsonNet(models);
        }

    }
}

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using Infrastructure.csrf;
	using Scorto.Web;

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
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        [ValidateJsonAntiForgeryToken]
        public JsonNetResult Index(int id)
        {
            var customer = _customerRepository.Get(id);

            var models = _builder.Create(customer);

            return this.JsonNet(models);
        }

    }
}

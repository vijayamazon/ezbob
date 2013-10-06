namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using NHibernate;
	using Scorto.Web;

	public class CustomerInfoController : Controller
	{
		private readonly ICustomerRepository _customers;
		private readonly ISession _session;

		public CustomerInfoController(ICustomerRepository customers, ISession session)
		{
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
	}
}

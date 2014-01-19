namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Models;
	using NHibernate;
	using Scorto.Web;
	using StructureMap;

	public class CustomerInfoController : Controller
	{
		[Ajax]
		[HttpGet]
		[Transactional]
		public JsonNetResult Index(int id)
		{
			var newSession = ObjectFactory.GetInstance<ISession>();
			newSession.CacheMode = CacheMode.Ignore;

			var newCustomers = new CustomerRepository(newSession);
			var customer = newCustomers.Get(id);
			var model = new PersonalInfoModel();
			model.InitFromCustomer(customer, newSession);
			return this.JsonNet(model);
		}
	}
}

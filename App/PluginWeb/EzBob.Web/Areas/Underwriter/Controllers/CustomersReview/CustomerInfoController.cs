namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Data;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Models;
	using NHibernate;
	using StructureMap;

	public class CustomerInfoController : Controller
	{
		[Ajax]
		[HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Index(int id)
		{
			var newSession = ObjectFactory.GetInstance<ISession>();
			newSession.CacheMode = CacheMode.Ignore;

			var newCustomers = new CustomerRepository(newSession);
			var customer = newCustomers.Get(id);
			var model = new PersonalInfoModel();
			model.InitFromCustomer(customer, newSession);
			return Json(model, JsonRequestBehavior.AllowGet);
		}
	}
}

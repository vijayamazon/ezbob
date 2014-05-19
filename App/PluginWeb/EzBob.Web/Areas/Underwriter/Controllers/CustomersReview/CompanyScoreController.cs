namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Web.Models;

	public class CompanyScoreController : Controller
	{
		private readonly ICustomerRepository m_oCustomerRepository;
		public CompanyScoreController(ICustomerRepository customerRepository)
		{
			m_oCustomerRepository = customerRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index(int id)
		{
			var customer = m_oCustomerRepository.Get(id);
			var builder = new CompanyScoreModelBuilder();
			return Json(builder.Create(customer), JsonRequestBehavior.AllowGet);
		} // Index
	} // CompanyScoreController
} // namespace

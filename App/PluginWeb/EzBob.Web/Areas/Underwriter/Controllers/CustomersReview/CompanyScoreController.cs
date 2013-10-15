using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Infrastructure.csrf;
using EzBob.Web.Models;
using Scorto.Web;
using StructureMap;
using log4net;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	public class CompanyScoreController : Controller {
		private readonly ICustomerRepository m_oCustomerRepository;
		private readonly ServiceLogRepository m_oServiceLogRepository;

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof (CompanyScoreController));

		public CompanyScoreController(ICustomerRepository customerRepository) {
			m_oCustomerRepository = customerRepository;
			m_oServiceLogRepository = ObjectFactory.GetInstance<ServiceLogRepository>();
		} // constructor

		[Ajax]
		[HttpGet]
		[Transactional]
		[ValidateJsonAntiForgeryToken]
		public JsonNetResult Index(int id) {
			var customer = m_oCustomerRepository.Get(id);
			var builder = new CompanyScoreModelBuilder();
			return this.JsonNet(builder.Create(customer));
		} // Index
	} // CompanyScoreController
} // namespace

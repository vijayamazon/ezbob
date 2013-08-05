using Scorto.Web;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using CommonLib;
	using Models.Fraud;

	public class FraudStatusController : Controller
	{
		private readonly ICustomerRepository _customerRepository;

		public FraudStatusController(ICustomerRepository customerRepository)
		{
			_customerRepository = customerRepository;
		}


		public JsonNetResult Index(int id)
		{
			var customer = _customerRepository.Get(id);

			var data = new FraudStatusModel
				{
					CurrentStatusId = (int)customer.FraudStatus,
					CurrentStatus = customer.FraudStatus.Description()
				};
			return this.JsonNet(data);
		}

		[HttpPost]
		[Transactional]
		[Ajax]
		public JsonNetResult Save(int customerId, int currentStatus)
		{
			var customer = _customerRepository.Get(customerId);
			customer.FraudStatus = (FraudStatus)currentStatus;
			return this.JsonNet(new { });
		}
	}
}

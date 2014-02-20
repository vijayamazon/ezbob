namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Data;
	using CommonLib;
	using Models.Fraud;
	using Scorto.Web;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;

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
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		public JsonNetResult Save(int customerId, int currentStatus)
		{
			var customer = _customerRepository.Get(customerId);
			customer.FraudStatus = (FraudStatus)currentStatus;
			return this.JsonNet(new { });
		}
	}
}

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Linq;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Web.Areas.Underwriter.Models;
	using Scorto.Web;
	using System;
	using ApplicationMng.Repository;
	using FraudChecker;

	public class FraudDetectionLogController : Controller {
		private readonly FraudDetectionRepository _fraudDetectionLog;
		private readonly ServiceClient m_oServiceClient;
		private readonly IUsersRepository _usersRepository;

		public FraudDetectionLogController(FraudDetectionRepository fraudDetectionLog, IUsersRepository usersRepository)
		{
			_fraudDetectionLog = fraudDetectionLog;
			m_oServiceClient = new ServiceClient();
			_usersRepository = usersRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonNetResult Index(int id)
		{
			DateTime? lastCheckDate = null;
			var rows = _fraudDetectionLog.GetLastDetections(id, out lastCheckDate).Select(x => new FraudDetectionLogRowModel(x)).OrderByDescending(x => x.Id).ToList();
			var model = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = rows,
					LastCheckDate = lastCheckDate
				};
			return this.JsonNet(model);
		}

		[Ajax]
		[HttpPost]
		public JsonNetResult Recheck(int customerId)
		{
			var user = _usersRepository.Get(customerId);
			m_oServiceClient.Instance.FraudChecker(user.Id, FraudMode.FullCheck);
			return this.JsonNet(new { success = true });
		}
	}
}
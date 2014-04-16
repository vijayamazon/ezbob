namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Linq;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EZBob.DatabaseLib.Repository;
	using EzBob.Web.Areas.Underwriter.Models;
	using System;
	using ApplicationMng.Repository;
	using Ezbob.Backend.Models;
	using FraudChecker;
	using Infrastructure.Attributes;

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
		public JsonResult Index(int id)
		{
			DateTime? lastCheckDate = null;
			var rows = _fraudDetectionLog.GetLastDetections(id, out lastCheckDate).Select(x => new FraudDetectionLogRowModel(x)).OrderByDescending(x => x.Id).ToList();
			var model = new FraudDetectionLogModel
				{
					FraudDetectionLogRows = rows,
					LastCheckDate = lastCheckDate
				};
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult Recheck(int customerId)
		{
			var user = _usersRepository.Get(customerId);
			m_oServiceClient.Instance.FraudChecker(user.Id, FraudMode.FullCheck);
			return Json(new { success = true });
		}
	}
}
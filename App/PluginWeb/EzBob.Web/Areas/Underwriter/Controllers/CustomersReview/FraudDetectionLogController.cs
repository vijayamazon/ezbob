using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EZBob.DatabaseLib.Repository;
using EzBob.Web.Areas.Underwriter.Models;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;

	public class FraudDetectionLogController : Controller
	{
		private readonly FraudDetectionRepository _fraudDetectionLog;
		private readonly IAppCreator _appCreator;
		private readonly IUsersRepository _usersRepository;

		public FraudDetectionLogController(FraudDetectionRepository fraudDetectionLog, IAppCreator appCreator, IUsersRepository usersRepository)
		{
			_fraudDetectionLog = fraudDetectionLog;
			_appCreator = appCreator;
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
			_appCreator.FraudChecker(user);
			return this.JsonNet(new { success = true });
		}
	}
}
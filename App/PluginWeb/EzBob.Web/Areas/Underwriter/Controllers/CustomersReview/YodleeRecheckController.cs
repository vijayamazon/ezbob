namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System.Web.Mvc;
	using ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Scorto.Web;
	using log4net;

	public class YodleeRecheckController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeRecheckController));
		private readonly CustomerMarketPlaceRepository _customerMarketplaces;
		private readonly IAppCreator _appCreator;

		public YodleeRecheckController(IAppCreator appCreator, CustomerMarketPlaceRepository customerMarketplaces)
		{
			_appCreator = appCreator;
			_customerMarketplaces = customerMarketplaces;
		}
		
		[Transactional]
		public ViewResult YodleeCallback(int id, string oauth_token = "", string oauth_error_problem = "", string oauth_error_code = "")
		{
			if (id == -1)
			{
				return View(new { error = "Error occured (umi not found)" });
			}

			if (!string.IsNullOrEmpty(oauth_error_problem) || !string.IsNullOrEmpty(oauth_error_code))
			{
				return View(new { error = "Error occured (oauth) " + oauth_error_code + " " + oauth_error_problem });
			}

			var mp = _customerMarketplaces.Get(id);
			_appCreator.CustomerMarketPlaceAdded(mp.Customer, mp.Id);
			
			return View(new { error = "test" });
		}

	}
}

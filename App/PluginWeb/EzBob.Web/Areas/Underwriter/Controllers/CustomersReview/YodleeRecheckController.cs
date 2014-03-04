namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview {
	using System.Data;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzServiceReference;
	using Scorto.Web;

	public class YodleeRecheckController : Controller {
		private readonly CustomerMarketPlaceRepository _customerMarketplaces;
		private readonly EzServiceClient m_oServiceClient;

		public YodleeRecheckController(CustomerMarketPlaceRepository customerMarketplaces) {
			m_oServiceClient = ServiceClient.Instance;
			_customerMarketplaces = customerMarketplaces;
		} // constructor

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public ViewResult YodleeCallback(int id, string oauth_token = "", string oauth_error_problem = "", string oauth_error_code = "") {
			if (id == -1)
				return View(new { error = "Error occured (umi not found)" });

			if (!string.IsNullOrEmpty(oauth_error_problem) || !string.IsNullOrEmpty(oauth_error_code))
				return View(new { error = "Error occured (oauth) " + oauth_error_code + " " + oauth_error_problem });

			var mp = _customerMarketplaces.Get(id);
			m_oServiceClient.UpdateMarketplace(mp.Customer.Id, mp.Id, true);
			
			return View(new { error = "test" });
		} // YodleeCallback
	} // class YodleeRecheckController
} // namespace

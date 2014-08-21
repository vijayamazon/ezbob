namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using EKM;
	using Code.MpUniq;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using log4net;
	using EZBob.DatabaseLib;

	public class EkmMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
		private readonly EkmConnector _validator = new EkmConnector();
		private readonly DatabaseDataHelper _helper;

		public EkmMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			IMPUniqChecker mpChecker
		) {
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			m_oServiceClient = new ServiceClient();
			_helper = helper;
		}

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts()
		{
			var oEsi = new EkmServiceInfo();

			var ekms = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(mp => new EkmAccountModel{ id = mp.Id, login = mp.DisplayName})
				.ToList();
			return Json(ekms, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts(EkmAccountModel model)
		{
			string errorMsg;
			if (!_validator.Validate(model.login, model.password, out errorMsg))
			{
				var errorObject = new { error = errorMsg };
				return Json(errorObject);
			}
			try
			{
				var customer = _context.Customer;

				var mpId = SaveAccountTrn(customer, model.login, model.password);
				if (mpId > 0)
					m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mpId, true, customer.Id);

				return Json(new EkmAccountModel { id = mpId, login = model.login }, JsonRequestBehavior.AllowGet);
			}
			catch (MarketPlaceAddedByThisCustomerException e) {
				Log.Debug(e);
				return Json(new { error = DbStrings.StoreAddedByYou }, JsonRequestBehavior.AllowGet);
			}
			catch (MarketPlaceIsAlreadyAddedException e) {
				Log.Debug(e);
				return Json(new { error = DbStrings.StoreAlreadyExistsInDb }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				Log.Error(e);
				return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		private int SaveAccountTrn(Customer customer, string login, string password) {
			int nResult = 0;

			new Transactional(() => {
				var username = login;
				var ekm = new EkmDatabaseMarketPlace();
				_mpChecker.Check(ekm.InternalId, customer, username);
				var oEsi = new EkmServiceInfo();
				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				var ekmSecurityInfo = new EkmSecurityInfo {MarketplaceId = marketPlaceId, Name = username, Password = password};

				var mp = _helper.SaveOrUpdateCustomerMarketplace(username, ekm, ekmSecurityInfo.Password, customer);

				nResult = mp.Id;
			}).Execute();

			return nResult;
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Update(string name, string password) {
			string errorMsg;

			if (!_validator.Validate(name, password, out errorMsg)) {
				var errorObject = new { error = errorMsg };
				return Json(errorObject);
			} // if

			try {
				Transactional.Execute(() => {
					var customer = _context.Customer;
					var ekm = new EkmDatabaseMarketPlace();
					_helper.SaveOrUpdateCustomerMarketplace(name, ekm, password, customer);
				});

				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e) {
				Log.Error(e);
				return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
			} // try
		} // Update
	} // EkmMarketplacesController
} // namespace

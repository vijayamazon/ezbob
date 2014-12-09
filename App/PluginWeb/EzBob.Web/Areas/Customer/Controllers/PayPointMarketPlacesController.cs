namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using Models;
	using PayPoint;
	using Code.MpUniq;
	using ServiceClientProxy;
	using Web.Models.Strings;
	using log4net;
	using EZBob.DatabaseLib;

	public class PayPointMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPointMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
		private readonly DatabaseDataHelper _helper;
		private readonly int _payPointMarketTypeId;

		public PayPointMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			IMPUniqChecker mpChecker)
		{
			_context = context;
			_helper = helper;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			m_oServiceClient = new ServiceClient();

			var payPointServiceInfo = new PayPointServiceInfo();
			_payPointMarketTypeId = _mpTypes.GetAll().First(a => a.InternalId == payPointServiceInfo.InternalId).Id;
		}

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult Accounts()
		{
			var payPoints = _customer.CustomerMarketPlaces.Where(mp => mp.Marketplace.Id == _payPointMarketTypeId).Select(PayPointAccountModel.ToModel).ToList();
			return Json(payPoints, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Accounts(PayPointAccountModel model)
		{
			string errorMsg;
			if (!PayPointConnector.Validate(model.mid, model.vpnPassword, model.remotePassword, out errorMsg))
			{
				var errorObject = new { error = errorMsg };
				Log.ErrorFormat("PayPoint validation failed: {0}", errorObject);
				return Json(errorObject);
			}
			try
			{
				var mpId = SavePaypoint(model);
				m_oServiceClient.Instance.UpdateMarketplace(_context.Customer.Id, mpId, true, _context.Customer.Id);
				return Json(model);
			}
			catch (MarketPlaceAddedByThisCustomerException e)
			{
				Log.Error(e);
				return Json(new { error = DbStrings.AccountAddedByYou });
			}
			catch (MarketPlaceIsAlreadyAddedException e)
			{
				Log.Error(e);
				return Json(new { error = DbStrings.StoreAlreadyExistsInDb });
			}
			catch (Exception e)
			{
				Log.Error(e);
				return Json(new { error = e.Message });
			}
		}

		private int SavePaypoint(PayPointAccountModel model)
		{
			var customer = _context.Customer;
			var username = model.mid;
			var payPointDatabaseMarketPlace = new PayPointDatabaseMarketPlace();

			_mpChecker.Check(payPointDatabaseMarketPlace.InternalId, customer, username);

			var payPointSecurityInfo = new PayPointSecurityInfo(model.id, model.remotePassword, model.vpnPassword, model.mid);

			var payPoint = _helper.SaveOrUpdateCustomerMarketplace(username, payPointDatabaseMarketPlace, payPointSecurityInfo, customer);
			return payPoint.Id;
		}
	}
}

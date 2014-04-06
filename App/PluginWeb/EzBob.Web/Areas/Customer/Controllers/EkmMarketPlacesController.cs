namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using Code;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Infrastructure;
	using EKM;
	using Code.MpUniq;
	using Infrastructure.Attributes;
	using Web.Models.Strings;
	using log4net;
	using NHibernate;
	using System.Data;
	using CommonLib.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class EkmMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly MarketPlaceRepository _mpTypes;
		private readonly Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ServiceClient m_oServiceClient;
		private readonly EkmConnector _validator = new EkmConnector();
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public EkmMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			MarketPlaceRepository mpTypes,
			IMPUniqChecker mpChecker,
			ISession session
		) {
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			m_oServiceClient = new ServiceClient();
			_session = session;
			_helper = helper;
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonResult Accounts()
		{
			var oEsi = new EkmServiceInfo();

			var ekms = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(EkmAccountModel.ToModel)
				.ToList();
			return Json(ekms, JsonRequestBehavior.AllowGet);
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
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
				var username = model.login;
				var ekm = new EkmDatabaseMarketPlace();
				_mpChecker.Check(ekm.InternalId, customer, username);
				var oEsi = new EkmServiceInfo();
				int marketPlaceId = _mpTypes
					.GetAll()
					.First(a => a.InternalId == oEsi.InternalId)
					.Id;

				var ekmSecurityInfo = new EkmSecurityInfo { MarketplaceId = marketPlaceId, Name = username, Password = model.password };

				var mp = _helper.SaveOrUpdateCustomerMarketplace(username, ekm, ekmSecurityInfo.Password, customer);

				_session.Flush();

				m_oServiceClient.Instance.UpdateMarketplace(customer.Id, mp.Id, true);

				return Json(EkmAccountModel.ToModel(mp), JsonRequestBehavior.AllowGet);
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

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		[Ajax]
		[HttpPost]
		public JsonResult Update(string name, string password)
		{
			string errorMsg;
			if (!_validator.Validate(name, password, out errorMsg))
			{
				var errorObject = new { error = errorMsg };
				return Json(errorObject);
			}
			try
			{
				var customer = _context.Customer;
				var ekm = new EkmDatabaseMarketPlace();
				_helper.SaveOrUpdateCustomerMarketplace(name, ekm, password, customer);
				_session.Flush();
				return Json(new { success = true }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception e)
			{
				Log.Error(e);
				return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
			}
		}
	}

	public class EkmAccountModel
	{
		public int id { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public string displayName { get { return login; } }

		public static EkmAccountModel ToModel(IDatabaseCustomerMarketPlace account)
		{
			return new EkmAccountModel
					   {
						   id = account.Id,
						   login = account.DisplayName,
						   password = Encryptor.Decrypt(account.SecurityData),
					   };
		}

		public static EkmAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new EkmAccountModel
			{
				id = account.Id,
				login = account.DisplayName,
				password = Encryptor.Decrypt(account.SecurityData),
			};
		} // ToModel
	}
}
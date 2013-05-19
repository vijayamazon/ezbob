namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using YodleeLib;
	using YodleeLib.connector;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
	using log4net;
	using CommonLib.Security;
	using ApplicationCreator;
	using NHibernate;

	public class YodleeMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(YodleeMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly EZBob.DatabaseLib.Model.Database.Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly IAppCreator _appCreator;
		private readonly YodleeConnector _validator = new YodleeConnector();
		private readonly ISession _session;

		public YodleeMarketPlacesController(
			IEzbobWorkplaceContext context,
			IRepository<MP_MarketplaceType> mpTypes,
			IMPUniqChecker mpChecker,
			IAppCreator appCreator,
			ISession session)
		{
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_appCreator = appCreator;
			_session = session;
		}

		[Transactional]
		public JsonNetResult Accounts()
		{
			var oEsi = new YodleeServiceInfo();

			var Yodlees = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(YodleeAccountModel.ToModel)
				.ToList();
			return this.JsonNet(Yodlees);
		}

		[Transactional]
		[Ajax]
		[HttpPost]
		public JsonNetResult Accounts(YodleeAccountModel model)
		{
			//string errorMsg;
			//if (!_validator.Validate(model.login, model.password, out errorMsg))
			//{
			//	var errorObject = new { error = errorMsg };
			//	return this.JsonNet(errorObject);
			//}
			//try
			//{
			//	var customer = _context.Customer;
			//	var username = model.login;
			//	var Yodlee = new YodleeDatabaseMarketPlace();
			//	_mpChecker.Check(Yodlee.InternalId, customer, username);
			//	var oEsi = new YodleeServiceInfo();
			//	int marketPlaceId = _mpTypes
			//		.GetAll()
			//		.First(a => a.InternalId == oEsi.InternalId)
			//		.Id;

			//	var mp = new MP_CustomerMarketPlace
			//				 {
			//					 Marketplace = _mpTypes.Get(marketPlaceId),
			//					 DisplayName = model.login,
			//					 SecurityData = Encryptor.EncryptBytes(model.password),
			//					 Customer = _customer,
			//					 Created = DateTime.UtcNow,
			//					 UpdatingStart = DateTime.UtcNow,
			//					 Updated = DateTime.UtcNow,
			//					 UpdatingEnd = DateTime.UtcNow
			//				 };

			//	_customer.CustomerMarketPlaces.Add(mp); 

			//	if (_customer.WizardStep != WizardStepType.PaymentAccounts || _customer.WizardStep != WizardStepType.AllStep)
			//		_customer.WizardStep = WizardStepType.Marketplace;

			//	_session.Flush();
			//	_appCreator.EbayAdded(customer, mp.Id);

			//	return this.JsonNet(YodleeAccountModel.ToModel(mp));
			//}
			//catch (MarketPlaceAddedByThisCustomerException e)
			//{
			//	Log.Debug(e);
			//	return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
			//}
			//catch (MarketPlaceIsAlreadyAddedException e)
			//{
			//	Log.Debug(e);
			//	return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
			//}
			//catch (Exception e)
			//{
			//	Log.Error(e);
			//	return this.JsonNet(new { error = e.Message });
			//}
			return this.JsonNet(new { error = "not implemented" });
		}

		public void Success()
		{
			int a = 1;
		}

		public RedirectResult AttachYodlee(int csId, string bankName)
		{
			// Create yodlee account for this customer, or get existing one from DB

			var callback = Url.Action("Success", "YodleeMarketPlaces", new { Area = "Customer" }, "https");
			//	string url = "https://www.google.com";

			YodleeLib.YodleeMain ym = new YodleeMain();
			string finalUrl = ym.GetFinalUrl(csId, callback);

			return Redirect(finalUrl);
		}
	}

	public class YodleeAccountModel
	{
		public int bankId { get; set; }
		public string bankName { get; set; }
		public string displayName { get { return bankName; } }

		public static YodleeAccountModel ToModel(MP_CustomerMarketPlace account)
		{
			return new YodleeAccountModel
				{
					bankId = 1, // TODO: get real id
					bankName = account.DisplayName
				};
		}
	}
}
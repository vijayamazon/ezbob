namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using ApplicationMng.Repository;
	using CompanyFiles;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Scorto.Web;
	using Code.MpUniq;
	using log4net;
	using NHibernate;
	using System.Data;
	using EZBob.DatabaseLib;

	public class CompanyFilesMarketPlacesController : Controller
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(EkmMarketPlacesController));
		private readonly IEzbobWorkplaceContext _context;
		private readonly IRepository<MP_MarketplaceType> _mpTypes;
		private readonly Customer _customer;
		private readonly IMPUniqChecker _mpChecker;
		private readonly ISession _session;
		private readonly DatabaseDataHelper _helper;

		public CompanyFilesMarketPlacesController(
			IEzbobWorkplaceContext context,
			DatabaseDataHelper helper,
			IRepository<MP_MarketplaceType> mpTypes,
			IMPUniqChecker mpChecker,
			ISession session)
		{
			_context = context;
			_mpTypes = mpTypes;
			_customer = context.Customer;
			_mpChecker = mpChecker;
			_session = session;
			_helper = helper;
		}

		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		public JsonNetResult Accounts()
		{
			var oEsi = new CompanyFilesServiceInfo();

			var companyFiles = _customer
				.CustomerMarketPlaces
				.Where(mp => mp.Marketplace.InternalId == oEsi.InternalId)
				.Select(x => x.DisplayName)
				.ToList();
			return this.JsonNet(companyFiles);
		}

		//[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
		//[Ajax]
		//[HttpPost]
		//public JsonNetResult Accounts(EkmAccountModel model)
		//{
		//	string errorMsg;
		//	if (!_validator.Validate(model.login, model.password, out errorMsg))
		//	{
		//		var errorObject = new { error = errorMsg };
		//		return this.JsonNet(errorObject);
		//	}
		//	try
		//	{
		//		var customer = _context.Customer;
		//		var username = model.login;
		//		var ekm = new EkmDatabaseMarketPlace();
		//		_mpChecker.Check(ekm.InternalId, customer, username);
		//		var oEsi = new EkmServiceInfo();
		//		int marketPlaceId = _mpTypes
		//			.GetAll()
		//			.First(a => a.InternalId == oEsi.InternalId)
		//			.Id;

		//		var ekmSecurityInfo = new EkmSecurityInfo { MarketplaceId = marketPlaceId, Name = username, Password = model.password };

		//		var mp = _helper.SaveOrUpdateCustomerMarketplace(username, ekm, ekmSecurityInfo.Password, customer);

		//		_session.Flush();

		//		_appCreator.CustomerMarketPlaceAdded(customer, mp.Id);

		//		return this.JsonNet(EkmAccountModel.ToModel(mp));
		//	}
		//	catch (MarketPlaceAddedByThisCustomerException e)
		//	{
		//		Log.Debug(e);
		//		return this.JsonNet(new { error = DbStrings.StoreAddedByYou });
		//	}
		//	catch (MarketPlaceIsAlreadyAddedException e)
		//	{
		//		Log.Debug(e);
		//		return this.JsonNet(new { error = DbStrings.StoreAlreadyExistsInDb });
		//	}
		//	catch (Exception e)
		//	{
		//		Log.Error(e);
		//		return this.JsonNet(new { error = e.Message });
		//	}
		//}
	}
}
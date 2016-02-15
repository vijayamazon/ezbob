namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System.Linq;
	using System.Web.Mvc;
	using EzBob.Web.Infrastructure;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Model.Fraud;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure.Attributes;
	using Models.Fraud;

	public class FraudController : Controller
	{
		private readonly FraudUserRepository _fraudUserRepository;
		private readonly MarketPlaceRepository _mpType;

		public FraudController(FraudUserRepository fraudUserRepository, MarketPlaceRepository mpType)
		{
			_fraudUserRepository = fraudUserRepository;
			_mpType = mpType;
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetAll()
		{
			var model = FraudModel.FromFraudModel(_fraudUserRepository.GetAll().OrderByDescending(x => x.Id).ToList());
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Ajax]
		[Transactional]
		[Permission(Name = "AddFraudUser")]
		public void AddNewUser(FraudModel user)
		{
			var fraudUser = new FraudUser
				{
					FirstName = user.FirstName,
					LastName = user.LastName
				};
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			user.Addresses.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			user.Phones.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			user.Emails.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			user.Companies.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			user.BankAccounts.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			user.EmailDomains.Select(x => { x.FraudUser = fraudUser; return x; }).ToList();
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed

			fraudUser.Addresses.AddAll(user.Addresses);
			fraudUser.Phones.AddAll(user.Phones);
			fraudUser.Emails.AddAll(user.Emails);
			fraudUser.Companies.AddAll(user.Companies);
			fraudUser.BankAccounts.AddAll(user.BankAccounts);
			fraudUser.EmailDomains.AddAll(user.EmailDomains);
			fraudUser.Shops.AddAll(
				user.Shops.Select(x => new FraudShop
					{
						Name = x.Name,
						Type = _mpType.GetAll().First(y => y.Name == x.Type),
						FraudUser = fraudUser
					}).ToList()
				);

			_fraudUserRepository.Save(fraudUser);
		}
	}
}
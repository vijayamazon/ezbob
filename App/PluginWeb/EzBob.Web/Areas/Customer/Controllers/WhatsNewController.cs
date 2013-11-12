namespace EzBob.Web.Areas.Customer.Controllers
{
	using System;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Infrastructure.csrf;
	using Scorto.Web;
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class WhatsNewController : Controller
	{
		private readonly IEzbobWorkplaceContext _context;
		private readonly IWhatsNewRepository _whatsNewRepository;
		private readonly IWhatsNewCustomerMapRepository _whatsNewCustomerMapRepository;

		//-------------------------------------------------------------------
		public WhatsNewController(
			IEzbobWorkplaceContext context,
			IWhatsNewCustomerMapRepository whatsNewCustomerMapRepository,
			IWhatsNewRepository whatsNewRepository)
		{
			_context = context;
			_whatsNewCustomerMapRepository = whatsNewCustomerMapRepository;
			_whatsNewRepository = whatsNewRepository;
		}

		//-------------------------------------------------------------------

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonNetResult Index()
		{
			var sawWhatsNew =
				_whatsNewCustomerMapRepository
					.GetAll()
					.Where(x => x.Customer == _context.Customer && x.Understood)
					.ToList();
			if (sawWhatsNew.Any())
			{
				var sawWhatsNewIdList = sawWhatsNew.Select(w => w.WhatsNew.Id).ToList();
				var whatsNew =
					_whatsNewRepository
						.GetAll()
						.FirstOrDefault(
							x =>
							!sawWhatsNewIdList.Contains(x.Id) &&
							x.Active &&
							x.ValidUntil.Date >= DateTime.Today);

				if (whatsNew != null)
				{
					return this.JsonNet(new { whatsNew = whatsNew.WhatsNewHtml, whatsNewId = whatsNew.Id });
				}

			}
			else
			{
				var whatsNew =
					_whatsNewRepository
						.GetAll()
						.FirstOrDefault(x => x.Active &&
											 x.ValidUntil.Date >= DateTime.Today);
				if (whatsNew != null)
				{
					return this.JsonNet(new { whatsNew = whatsNew.WhatsNewHtml, whatsNewId = whatsNew.Id });
				}
			}

			return this.JsonNet(new { noWhatsNew = true });
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public void GotIt(int whatsNewId)
		{
			var whatsNew = _whatsNewRepository.Get(whatsNewId);
			if (whatsNew == null)
			{
				return;
			}

			_whatsNewCustomerMapRepository.Save(new WhatsNewCustomerMap
				{
					Customer = _context.Customer,
					Date = DateTime.UtcNow,
					Understood = true,
					WhatsNew = whatsNew
				});
		}

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public void ShowLater(int whatsNewId)
		{
			var whatsNew = _whatsNewRepository.Get(whatsNewId);
			if (whatsNew == null)
			{
				return;
			}

			_whatsNewCustomerMapRepository.Save(new WhatsNewCustomerMap
			{
				Customer = _context.Customer,
				Date = DateTime.UtcNow,
				Understood = false,
				WhatsNew = whatsNew
			});
		}
	}
}

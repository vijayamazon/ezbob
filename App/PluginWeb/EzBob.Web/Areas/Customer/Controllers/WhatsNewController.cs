namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Infrastructure.csrf;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using StructureMap;

	public class WhatsNewController : Controller {
		public WhatsNewController(
			IEzbobWorkplaceContext context,
			IWhatsNewCustomerMapRepository whatsNewCustomerMapRepository,
			IWhatsNewRepository whatsNewRepository
		) {
			this.context = context;
			this.whatsNewCustomerMapRepository = whatsNewCustomerMapRepository;
			this.whatsNewRepository = whatsNewRepository;
		} // constructor

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		public JsonResult Index() {
			List<WhatsNewCustomerMap> sawWhatsNew = this.whatsNewCustomerMapRepository
				.GetAll()
				.Where(x => x.Customer == this.context.Customer && x.Understood)
				.ToList();

			List<int> excludedWhatsNews = ObjectFactory.GetInstance<IWhatsNewExcludeCustomerOriginRepository>()
				.GetAll()
				.Where(x => x.CustomerOriginID == this.context.Customer.CustomerOrigin.CustomerOriginID)
				.Select(x => x.WhatsNewId)
				.ToList();

			if (sawWhatsNew.Any()) {
				List<int> sawWhatsNewIdList = sawWhatsNew.Select(w => w.WhatsNew.Id).ToList();

				WhatsNew whatsNew = this.whatsNewRepository
					.GetAll()
					.FirstOrDefault(x =>
						!excludedWhatsNews.Contains(x.Id) &&
						!sawWhatsNewIdList.Contains(x.Id) &&
						x.Active &&
						x.ValidUntil.Date >= DateTime.Today
					);

				if (whatsNew != null) {
					return Json(new {
						success = true,
						whatsNew = whatsNew.WhatsNewHtml,
						whatsNewId = whatsNew.Id,
					}, JsonRequestBehavior.AllowGet);
				} // if
			} else {
				var whatsNew = this.whatsNewRepository
					.GetAll()
					.FirstOrDefault(x =>
						!excludedWhatsNews.Contains(x.Id) &&
						x.Active &&
						x.ValidUntil.Date >= DateTime.Today
					);

				if (whatsNew != null) {
					return Json(new {
						success = true,
						whatsNew = whatsNew.WhatsNewHtml,
						whatsNewId = whatsNew.Id,
					}, JsonRequestBehavior.AllowGet);
				} // if
			} // if

			return Json(new { success = true, noWhatsNew = true }, JsonRequestBehavior.AllowGet);
		} // Index

		[Ajax]
		[HttpPost]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public void CustomerSaw(int whatsNewId, bool gotIt) {
			WhatsNew whatsNew = this.whatsNewRepository.Get(whatsNewId);

			if (whatsNew == null)
				return;

			this.whatsNewCustomerMapRepository.Save(new WhatsNewCustomerMap {
				Customer = this.context.Customer,
				Date = DateTime.UtcNow,
				Understood = gotIt,
				WhatsNew = whatsNew
			});
		} // CustomerSaw

		private readonly IEzbobWorkplaceContext context;
		private readonly IWhatsNewRepository whatsNewRepository;
		private readonly IWhatsNewCustomerMapRepository whatsNewCustomerMapRepository;
	} // class WhatsNewController
} // namespace

﻿using System.Linq;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.Filters;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using StructureMap;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Web.Areas.Customer.Controllers
{
	using EZBob.DatabaseLib.Model.Database.Repository;

	public class WizardController : Controller
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly ISecurityQuestionRepository _questions;
        private readonly CustomerModelBuilder _customerModelBuilder;
        private readonly IEzBobConfiguration _config;
		private readonly ISession _session;
		private readonly ICustomerReasonRepository _reasons;
		private readonly ICustomerSourceOfRepaymentRepository _sourcesOfRepayment;

        //-------------------------------------------------------------------
        public WizardController(
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			IEzBobConfiguration config,
			ISession session, 
			ICustomerReasonRepository customerReasonRepository, 
			ICustomerSourceOfRepaymentRepository customerSourceOfRepaymentRepository)
        {
            _context = context;
            _questions = questions;
            _customerModelBuilder = customerModelBuilder;
            _config = config;
			_session = session;
	        _reasons = customerReasonRepository;
	        _sourcesOfRepayment = customerSourceOfRepaymentRepository;
        }

        //-------------------------------------------------------------------
        [Transactional]
        [IsSuccessfullyRegisteredFilter]
        public ActionResult Index()
        {
            ViewData["Questions"] = _questions.GetAll().ToList();
			ViewData["Reasons"] = _reasons.GetAll().OrderBy(x => x.Id).ToList();
			ViewData["Sources"] = _sourcesOfRepayment.GetAll().OrderBy(x => x.Id).ToList();
            ViewData["CaptchaMode"] = _config.CaptchaMode;
            ViewData["WizardTopNaviagtionEnabled"] = _config.WizardTopNaviagtionEnabled;
            ViewData["TargetsEnabled"] = _config.TargetsEnabled;
            ViewData["Config"] = _config;

			ViewData["MarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.ToArray();

            var wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer);

            return View(wizardModel);
        }       

		[Ajax]
		[HttpGet]
		[ValidateJsonAntiForgeryToken]
		[Transactional]
		public JsonNetResult EarnedPointsStr() {
			var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
			EZBob.DatabaseLib.Model.Database.Customer oCustomer = oDBHelper == null ? null : oDBHelper.FindCustomerByEmail(User.Identity.Name.Trim());
			string sPoints = "";

			if (oCustomer != null)
				sPoints = string.Format("{0:N0}", oCustomer.LoyaltyPoints());

			return this.JsonNet(new { EarnedPointsStr = sPoints });
		} // EarnedPointsStr

    }
}

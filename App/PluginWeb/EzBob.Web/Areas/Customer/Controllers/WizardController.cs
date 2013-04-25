using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Infrastructure;
using EzBob.Web.Infrastructure.Filters;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using log4net;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Web.Areas.Customer.Controllers
{
    public class WizardController : Controller
    {
        private readonly IAppCreator _creator;
        private readonly IEzbobWorkplaceContext _context;
        private readonly ISecurityQuestionRepository _questions;
        private readonly CustomerModelBuilder _customerModelBuilder;
        private readonly IEzBobConfiguration _config;
        private readonly ILoanTypeRepository _loanTypes;
        private static readonly ILog _log = LogManager.GetLogger(typeof(WizardController));
        private readonly MembershipProvider _membershipProvider;
		private readonly ISession _session;

        //-------------------------------------------------------------------
        public WizardController(
			IAppCreator creator,
			IEzbobWorkplaceContext context,
			ISecurityQuestionRepository questions,
			CustomerModelBuilder customerModelBuilder,
			IEzBobConfiguration config,
			ILoanTypeRepository loanTypes,
			MembershipProvider membershipProvider,
			ISession session
		)
        {
            _context = context;
            _creator = creator;
            _questions = questions;
            _customerModelBuilder = customerModelBuilder;
            _config = config;
            _loanTypes = loanTypes;
            _membershipProvider = membershipProvider;
			_session = session;
        }

        //-------------------------------------------------------------------
        [Transactional]
        [IsSuccessfullyRegisteredFilter]
        public ActionResult Index()
        {
            ViewData["Questions"] = _questions.GetAll().ToList();
            ViewData["CaptchaMode"] = _config.CaptchaMode;
            ViewData["WizardTopNaviagtionEnabled"] = _config.WizardTopNaviagtionEnabled;
            ViewData["TargetsEnabled"] = _config.TargetsEnabled;
            ViewData["Config"] = _config;

			ViewData["ActiveMarketPlaces"] = _session
				.Query<MP_MarketplaceType>()
				.Where(x => x.Active)
				.Select(x => x.Name)
				.ToArray();

            var wizardModel = _customerModelBuilder.BuildWizardModel(_context.Customer);

            return View(wizardModel);
        }       
    }
}

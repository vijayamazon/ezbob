namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
	using System;
	using System.Data;
	using System.Web.Mvc;
	using ApplicationMng.Model;
	using ApplicationMng.Repository;
	using Code.ApplicationCreator;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob.Models;
	using Scorto.Web;
	using EzServiceReference;
	using ActionResult = System.Web.Mvc.ActionResult;
	using NHibernate;

	public class ProfileController : Controller
    {
        private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
        private CustomerRepository CustomerRepository { get; set; }
        private readonly IUsersRepository _users;
		private readonly IAppCreator _creator;
		private readonly ISession session;

        public ProfileController(CustomerRepository customerRepository, ProfileSummaryModelBuilder summaryModelBuilder,
								 IUsersRepository users, IAppCreator creator, ISession session)
        {
            _summaryModelBuilder = summaryModelBuilder;
            _users = users;
            _creator = creator;
            CustomerRepository = customerRepository;
	        this.session = session;
        }

        [Ajax]
        [HttpGet]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public ActionResult Index(int id)
        {
            var customer = CustomerRepository.Get(id);
            var model = _summaryModelBuilder.CreateProfile(customer);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
		[Transactional(IsolationLevel = IsolationLevel.ReadUncommitted)]
        public JsonNetResult SaveComment(string comment, int id)
        {
            var customer = CustomerRepository.Get(id);
            customer.Comment = comment;
            return this.JsonNet(new {Saved = "true"});
        }

        private static string MainStrategyUpdatingStatus(Application app)
        {
            if (app != null &&
                (app.State == ApplicationStrategyState.SecurityViolation ||
                 app.State == ApplicationStrategyState.StrategyFinishedWithoutErrors ||
                 app.State == ApplicationStrategyState.StrategyFinishedWithErrors ||
                 app.State == ApplicationStrategyState.Error
                ) || app == null)
            {
                return "Finished";
            }
            return "Running";
        }

        [Ajax]
        [HttpPost]
        public void StartMainStrategy(int customerId)
        {
            var customer = CustomerRepository.Get(customerId);

            if (customer.PersonalInfo == null)
            {
                throw new Exception("Customer did not finished wizard");
            }
            var mainStrat = customer.LastStartedMainStrategy;

            if (MainStrategyUpdatingStatus(mainStrat) == "Finished")
            {
				var underwriter = _users.GetUserByLogin(User.Identity.Name);
                _creator.Evaluate(underwriter.Id, _users.Get(customerId), NewCreditLineOption.UpdateEverythingAndApplyAutoRules, Convert.ToInt32(customer.IsAvoid), false, false);
            }
            else
            {
                throw new Exception("Main strategy already running");
            }
        }
    }
}
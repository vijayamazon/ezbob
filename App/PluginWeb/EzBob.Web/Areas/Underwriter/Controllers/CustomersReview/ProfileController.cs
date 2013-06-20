using System;
using System.Web.Mvc;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using System.Linq;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.CustomersReview
{
    public class ProfileController : Controller
    {
        private readonly ProfileSummaryModelBuilder _summaryModelBuilder;
        private CustomerRepository CustomerRepository { get; set; }
        private readonly IUsersRepository _users;
        private readonly IAppCreator _creator;

        public ProfileController(CustomerRepository customerRepository, ProfileSummaryModelBuilder summaryModelBuilder,
                                 IUsersRepository users, IAppCreator creator)
        {
            _summaryModelBuilder = summaryModelBuilder;
            _users = users;
            _creator = creator;
            CustomerRepository = customerRepository;
        }

        [Ajax]
        [HttpGet]
        [Transactional]
        public ActionResult Index(int id)
        {
            var customer = CustomerRepository.Get(id);
            var model = _summaryModelBuilder.CreateProfile(customer);
            return this.JsonNet(model);
        }

        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult SaveComment(string comment, int id)
        {
            var customer = CustomerRepository.Get(id);
            customer.Comment = comment;
            return this.JsonNet(new {Saved = "true"});
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult GetRegisteredCustomerInfo(int customerId)
        {
            var customer = CustomerRepository.Get(customerId);
            var mpModel = customer.CustomerMarketPlaces.Select(x => new
                                                                        {
                                                                            id = x.Id,
                                                                            Name = x.DisplayName,
                                                                            Type = x.Marketplace.Name,
                                                                            Status = MpUpdatingStatus(x),
                                                                            StartTime = x.UpdatingStart,
                                                                            EndTime = x.UpdatingEnd
                                                                        }).ToList();
            var strategyModel = new
                {
                    StrategyStatus = MainStrategyUpdatingStatus(customer.LastStartedMainStrategy),
                    StartTime = customer.LastStartedMainStrategy == null ? null  : (DateTime?)customer.LastStartedMainStrategy.CreationDate,
                    EndTime = customer.LastStartedMainStrategyEndTime
                };

            var isWizardFinished = customer.PersonalInfo != null;
            return this.JsonNet(new { mps = mpModel, sm = strategyModel, isWizardFinished, cName = customer.PersonalInfo!=null ? customer.PersonalInfo.Fullname : customer.Name});
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

        private static string MpUpdatingStatus(MP_CustomerMarketPlace mp)
        {
            if (mp.UpdatingStart == null) return "Not started";

            if (mp.UpdatingStart != null && mp.UpdatingEnd == null) return "Updating";

            if (!String.IsNullOrEmpty(mp.UpdateError)) return "Error";

            return "Completed";
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
                _creator.Evaluate(_users.Get(customerId), NewCreditLineOption.UpdateEverythingAndApplyAutoRules);
            }
            else
            {
                throw new Exception("Main strategy already running");
            }
        }
    }
}
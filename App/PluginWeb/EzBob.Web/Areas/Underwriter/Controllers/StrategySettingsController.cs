using System.Web.Mvc;
using EZBob.DatabaseLib.Model;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{
    public class StrategySettingsModel
    {
        public string EnableAutomaticRejection { get; set; }
        public string EnableAutomaticApproval { get; set; }
        public string LowCreditScore { get; set; }
        public string TotalAnnualTurnover { get; set; }
        public string TotalThreeMonthTurnover { get; set; }

        public string EnableAutomaticRejectionDesc { get; set; }
        public string EnableAutomaticApprovalDesc { get; set; }
        public string LowCreditScoreDesc { get; set; }
        public string TotalAnnualTurnoverDesc { get; set; }
        public string TotalThreeMonthTurnoverDesc { get; set; }
    }

    public class StrategySettingsController : Controller
    {

        private readonly IConfigurationVariablesRepository _configurationVariablesRepository;

        public StrategySettingsController(IConfigurationVariablesRepository configurationVariablesRepository)
        {
            _configurationVariablesRepository = configurationVariablesRepository;
        }

        private StrategySettingsModel GetSettings()
        {
            var enableAutomaticRejection = _configurationVariablesRepository.GetByName("EnableAutomaticRejection");
            var enableAutomaticApproval = _configurationVariablesRepository.GetByName("EnableAutomaticApproval");
            var lowCreditScore = _configurationVariablesRepository.GetByName("LowCreditScore");
            var totalAnnualTurnover = _configurationVariablesRepository.GetByName("TotalAnnualTurnover");
            var totalThreeMonthTurnover = _configurationVariablesRepository.GetByName("TotalThreeMonthTurnover");
            
            var currentSettings = new StrategySettingsModel
            {
                EnableAutomaticRejection = enableAutomaticRejection.Value,
                EnableAutomaticRejectionDesc = enableAutomaticRejection.Description,
                EnableAutomaticApproval = enableAutomaticApproval.Value,
                EnableAutomaticApprovalDesc = enableAutomaticApproval.Description,
                LowCreditScore = lowCreditScore.Value,
                LowCreditScoreDesc = lowCreditScore.Description,
                TotalAnnualTurnover = totalAnnualTurnover.Value,
                TotalAnnualTurnoverDesc = totalAnnualTurnover.Description,
                TotalThreeMonthTurnover = totalThreeMonthTurnover.Value,
                TotalThreeMonthTurnoverDesc = totalThreeMonthTurnover.Description
            };
            
            return currentSettings;
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index()
        {
            return this.JsonNet(GetSettings());
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult Index(StrategySettingsModel newSettings)
        {
            var oldSettings = GetSettings();           

            if (oldSettings.EnableAutomaticApproval != newSettings.EnableAutomaticApproval) _configurationVariablesRepository.SetByName("EnableAutomaticRejection", newSettings.EnableAutomaticRejection);
            if (oldSettings.EnableAutomaticApproval != newSettings.EnableAutomaticApproval) _configurationVariablesRepository.SetByName("EnableAutomaticApproval", newSettings.EnableAutomaticApproval);
            if (oldSettings.LowCreditScore != newSettings.LowCreditScore) _configurationVariablesRepository.SetByName("LowCreditScore", newSettings.LowCreditScore);
            if (oldSettings.TotalAnnualTurnover != newSettings.TotalAnnualTurnover) _configurationVariablesRepository.SetByName("TotalAnnualTurnover", newSettings.TotalAnnualTurnover);
            if (oldSettings.TotalThreeMonthTurnover!= newSettings.TotalThreeMonthTurnover) _configurationVariablesRepository.SetByName("TotalThreeMonthTurnover", newSettings.TotalThreeMonthTurnover);
            return this.JsonNet(newSettings);
        }
    }
}
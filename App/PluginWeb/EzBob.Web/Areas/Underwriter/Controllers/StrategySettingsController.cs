using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Repository;
using ExperianLib.CaisFile;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Underwriter.Models;
using EzBob.Web.Areas.Underwriter.Models.CAIS;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;
using StructureMap;

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
        //
        // GET: /Underwriter/StrategySettings/
        
        private readonly ConfigurationVariablesRepository _configurationVariablesRepository;
        private StrategySettingsModel _currentSettings;

        public StrategySettingsController()
        {
            _configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
            _currentSettings = new StrategySettingsModel();
        }

        private StrategySettingsModel GetSettings()
        {
            var enableAutomaticRejection = _configurationVariablesRepository.GetByName("EnableAutomaticRejection");
            var enableAutomaticApproval = _configurationVariablesRepository.GetByName("EnableAutomaticApproval");
            var lowCreditScore = _configurationVariablesRepository.GetByName("LowCreditScore");
            var totalAnnualTurnover = _configurationVariablesRepository.GetByName("TotalAnnualTurnover");
            var totalThreeMonthTurnover = _configurationVariablesRepository.GetByName("TotalThreeMonthTurnover");
            _currentSettings = new StrategySettingsModel
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
            return _currentSettings;
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
        public JsonNetResult Index(StrategySettingsModel st)
        {
            if (_currentSettings.EnableAutomaticApproval != st.EnableAutomaticApproval) _configurationVariablesRepository.SetByName("EnableAutomaticRejection", st.EnableAutomaticRejection);
            if (_currentSettings.EnableAutomaticApproval != st.EnableAutomaticApproval) _configurationVariablesRepository.SetByName("EnableAutomaticApproval", st.EnableAutomaticApproval);
            if (_currentSettings.LowCreditScore != st.LowCreditScore) _configurationVariablesRepository.SetByName("LowCreditScore", st.LowCreditScore);
            if (_currentSettings.TotalAnnualTurnover != st.TotalAnnualTurnover) _configurationVariablesRepository.SetByName("TotalAnnualTurnover", st.TotalAnnualTurnover);
            if (_currentSettings.TotalThreeMonthTurnover!= st.TotalThreeMonthTurnover) _configurationVariablesRepository.SetByName("TotalThreeMonthTurnover", st.TotalThreeMonthTurnover);
            return this.JsonNet(GetSettings());
        }


    }
}




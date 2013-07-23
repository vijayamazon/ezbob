using System.Collections.Generic;
using System.Web.Mvc;
using EZBob.DatabaseLib.Model;
using EzBob.Web.Infrastructure.csrf;
using Scorto.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers
{

    public class StrategySettingsController : Controller
    {

        private readonly IConfigurationVariablesRepository _configurationVariablesRepository;

        public StrategySettingsController(IConfigurationVariablesRepository configurationVariablesRepository)
        {
            _configurationVariablesRepository = configurationVariablesRepository;
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult Index()
        {
            return this.JsonNet(string.Empty);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult SettingsGeneral()
        {
            var bwaBusinessCheck = _configurationVariablesRepository.GetByName("BWABusinessCheck");
            var displayEarnedPoints = _configurationVariablesRepository.GetByName("DisplayEarnedPoints");

            var st = new
                {
                    BWABusinessCheck = bwaBusinessCheck.Value,
                    BWABusinessCheckDesc = bwaBusinessCheck.Description,
                    DisplayEarnedPoints = displayEarnedPoints.Value,
                    DisplayEarnedPointsDesc = displayEarnedPoints.Description
                };
            return this.JsonNet(st);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult SettingsGeneral(string BWABusinessCheck, string DisplayEarnedPoints)
        {
            _configurationVariablesRepository.SetByName("BWABusinessCheck", BWABusinessCheck);
            _configurationVariablesRepository.SetByName("DisplayEarnedPoints", DisplayEarnedPoints);
            return SettingsGeneral();
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult SettingsCharges()
        {
            var latePaymentCharge = _configurationVariablesRepository.GetByName("LatePaymentCharge");
            var rolloverCharge = _configurationVariablesRepository.GetByName("RolloverCharge");
            var partialPaymentCharge = _configurationVariablesRepository.GetByName("PartialPaymentCharge");
            var administrationCharge = _configurationVariablesRepository.GetByName("AdministrationCharge");
            var otherCharge = _configurationVariablesRepository.GetByName("OtherCharge");

            var sc = new
                {
                    LatePaymentCharge = latePaymentCharge.Value,
                    LatePaymentChargeDesc = latePaymentCharge.Description,
                    RolloverCharge = rolloverCharge.Value,
                    RolloverChargeDesc = rolloverCharge.Description,
                    PartialPaymentCharge = partialPaymentCharge.Value,
                    PartialPaymentChargeDesc = partialPaymentCharge.Description,
                    AdministrationCharge = administrationCharge.Value,
                    AdministrationChargeDesc = administrationCharge.Description,
                    OtherCharge = otherCharge.Value,
                    OtherChargeDesc = otherCharge.Description
                };
            return this.JsonNet(sc);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult SettingsCharges(   string AdministrationCharge,
                                                string LatePaymentCharge,
                                                string OtherCharge,
                                                string PartialPaymentCharge,
                                                string RolloverCharge
            )
        {
            _configurationVariablesRepository.SetByName("AdministrationCharge", AdministrationCharge);
            _configurationVariablesRepository.SetByName("LatePaymentCharge", LatePaymentCharge);
            _configurationVariablesRepository.SetByName("OtherCharge", OtherCharge);
            _configurationVariablesRepository.SetByName("PartialPaymentCharge", PartialPaymentCharge);
            _configurationVariablesRepository.SetByName("RolloverCharge", RolloverCharge);
            return SettingsCharges();
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult AutomationGeneral()
        {
            return this.JsonNet(string.Empty);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult AutomationGeneral(string[] newSettings)
        {
            return this.JsonNet(string.Empty);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult AutomationApproval()
        {
            var enableAutomaticApproval = _configurationVariablesRepository.GetByName("EnableAutomaticApproval");
            var maxCapHomeOwner = _configurationVariablesRepository.GetByName("MaxCapHomeOwner");
            var maxCapNotHomeOwner = _configurationVariablesRepository.GetByName("MaxCapNotHomeOwner");

            var sa = new
                {
                    EnableAutomaticApproval = enableAutomaticApproval.Value,
                    EnableAutomaticApprovalDesc = enableAutomaticApproval.Description,
                    MaxCapHomeOwner = maxCapHomeOwner.Value,
                    MaxCapHomeOwnerDesc = maxCapHomeOwner.Description,
                    MaxCapNotHomeOwner = maxCapNotHomeOwner.Value,
                    MaxCapNotHomeOwnerDesc = maxCapNotHomeOwner.Description
                };
            return this.JsonNet(sa);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult AutomationApproval(
                                                string EnableAutomaticApproval,
                                                string MaxCapHomeOwner,
                                                string MaxCapNotHomeOwner
            )
        {
            _configurationVariablesRepository.SetByName("EnableAutomaticApproval", EnableAutomaticApproval);
            _configurationVariablesRepository.SetByName("MaxCapHomeOwner", MaxCapHomeOwner);
            _configurationVariablesRepository.SetByName("MaxCapNotHomeOwner", MaxCapNotHomeOwner);
            return AutomationApproval();
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpGet]
        [Transactional]
        public JsonNetResult AutomationRejection()
        {
            var enableAutomaticRejection = _configurationVariablesRepository.GetByName("EnableAutomaticRejection");
            var lowCreditScore = _configurationVariablesRepository.GetByName("LowCreditScore");
            var totalAnnualTurnover = _configurationVariablesRepository.GetByName("TotalAnnualTurnover");
            var totalThreeMonthTurnover = _configurationVariablesRepository.GetByName("TotalThreeMonthTurnover");

            var reject_Defaults_CreditScore = _configurationVariablesRepository.GetByName("Reject_Defaults_CreditScore");
            var reject_Defaults_AccountsNum = _configurationVariablesRepository.GetByName("Reject_Defaults_AccountsNum");
            var reject_Defaults_Amount = _configurationVariablesRepository.GetByName("Reject_Defaults_Amount");
            var reject_Defaults_MonthsNum = _configurationVariablesRepository.GetByName("Reject_Defaults_MonthsNum");

            var enableAutomaticReRejection = _configurationVariablesRepository.GetByName("EnableAutomaticReRejection");
            var autoRejectionExceptionCreditScore = _configurationVariablesRepository.GetByName("AutoRejectionException_CreditScore");
            var autoRejectionExceptionAnualTurnover = _configurationVariablesRepository.GetByName("AutoRejectionException_AnualTurnover");


            var sr = new
                {
                    EnableAutomaticRejection = enableAutomaticRejection.Value,
                    EnableAutomaticRejectionDesc = enableAutomaticRejection.Description,
                    LowCreditScore = lowCreditScore.Value,
                    LowCreditScoreDesc = lowCreditScore.Description,
                    TotalAnnualTurnover = totalAnnualTurnover.Value,
                    TotalAnnualTurnoverDesc = totalAnnualTurnover.Description,
                    TotalThreeMonthTurnover = totalThreeMonthTurnover.Value,
                    TotalThreeMonthTurnoverDesc = totalThreeMonthTurnover.Description,
                    Reject_Defaults_CreditScore = reject_Defaults_CreditScore.Value,
                    Reject_Defaults_CreditScoreDesc = reject_Defaults_CreditScore.Description,
                    Reject_Defaults_AccountsNum = reject_Defaults_AccountsNum.Value,
                    Reject_Defaults_AccountsNumDesc = reject_Defaults_AccountsNum.Description,
                    Reject_Defaults_Amount = reject_Defaults_Amount.Value,
                    Reject_Defaults_AmountDesc = reject_Defaults_Amount.Description,
                    Reject_Defaults_MonthsNum = reject_Defaults_MonthsNum.Value,
                    Reject_Defaults_MonthsNumDesc = reject_Defaults_MonthsNum.Description,
                    EnableAutomaticReRejection = enableAutomaticReRejection.Value,
                    EnableAutomaticReRejectionDesc = enableAutomaticReRejection.Description,
                    AutoRejectionException_CreditScore = autoRejectionExceptionCreditScore.Value,
                    AutoRejectionException_CreditScoreDesc = autoRejectionExceptionCreditScore.Description,
                    AutoRejectionException_AnualTurnover = autoRejectionExceptionAnualTurnover.Value,
                    AutoRejectionException_AnualTurnoverDesc = autoRejectionExceptionAnualTurnover.Description
                    };
            return this.JsonNet(sr);
        }

        [Ajax]
        [ValidateJsonAntiForgeryToken]
        [HttpPost]
        [Transactional]
        public JsonNetResult AutomationRejection(string EnableAutomaticRejection, 
                                                 string LowCreditScore,
                                                 string Reject_Defaults_AccountsNum,
                                                 string Reject_Defaults_Amount,
                                                 string Reject_Defaults_CreditScore,
                                                 string Reject_Defaults_MonthsNum,
                                                 string TotalAnnualTurnover,
                                                 string TotalThreeMonthTurnover,
                                                 string EnableAutomaticReRejection,
                                                 string AutoRejectionException_CreditScore,
                                                 string AutoRejectionException_AnualTurnover)
        {
            _configurationVariablesRepository.SetByName("EnableAutomaticRejection", EnableAutomaticRejection);
            _configurationVariablesRepository.SetByName("LowCreditScore", LowCreditScore);
            _configurationVariablesRepository.SetByName("Reject_Defaults_AccountsNum", Reject_Defaults_AccountsNum);
            _configurationVariablesRepository.SetByName("Reject_Defaults_Amount", Reject_Defaults_Amount);
            _configurationVariablesRepository.SetByName("Reject_Defaults_CreditScore", Reject_Defaults_CreditScore);
            _configurationVariablesRepository.SetByName("Reject_Defaults_MonthsNum", Reject_Defaults_MonthsNum);
            _configurationVariablesRepository.SetByName("TotalAnnualTurnover", TotalAnnualTurnover);
            _configurationVariablesRepository.SetByName("TotalThreeMonthTurnover", TotalThreeMonthTurnover);
            _configurationVariablesRepository.SetByName("EnableAutomaticReRejection", EnableAutomaticReRejection);
            _configurationVariablesRepository.SetByName("AutoRejectionException_CreditScore", AutoRejectionException_CreditScore);
            _configurationVariablesRepository.SetByName("AutoRejectionException_AnualTurnover", AutoRejectionException_AnualTurnover);

            return AutomationRejection();
        }

        [Ajax]
        [HttpGet]
        public JsonNetResult SettingsExperian()
        {
            var mainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_MainApplicant").Value;
            var aliasOfMainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AliasOfMainApplicant").Value;
            var associationOfMainApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AssociationOfMainApplicant").Value;
            var jointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_JointApplicant").Value;
            var aliasOfJointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AliasOfJointApplicant").Value;
            var associationOfJointApplicant = _configurationVariablesRepository.GetByName("FinancialAccounts_AssociationOfJointApplicant").Value;
            var noMatch = _configurationVariablesRepository.GetByName("FinancialAccounts_No_Match").Value;

            var model = new
            {
                FinancialAccounts_MainApplicant = mainApplicant,
                FinancialAccounts_AliasOfMainApplicant = aliasOfMainApplicant,
                FinancialAccounts_AssociationOfMainApplicant = associationOfMainApplicant,
                FinancialAccounts_JointApplicant = jointApplicant,
                FinancialAccounts_AliasOfJointApplicant = aliasOfJointApplicant,
                FinancialAccounts_AssociationOfJointApplicant = associationOfJointApplicant,
                FinancialAccounts_No_Match = noMatch
            };
            return this.JsonNet(model);
        }

        // ReSharper disable  InconsistentNaming
        [Ajax]
        [HttpPost]
        [Transactional]
        public JsonNetResult SettingsExperian(

            string FinancialAccounts_MainApplicant,
            string FinancialAccounts_AliasOfMainApplicant,
            string FinancialAccounts_AssociationOfMainApplicant,
            string FinancialAccounts_JointApplicant,
            string FinancialAccounts_AliasOfJointApplicant,
            string FinancialAccounts_AssociationOfJointApplicant,
            string FinancialAccounts_No_Match)
        {
            _configurationVariablesRepository.SetByName("FinancialAccounts_MainApplicant", FinancialAccounts_MainApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfMainApplicant", FinancialAccounts_AliasOfMainApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfMainApplicant", FinancialAccounts_AssociationOfMainApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_JointApplicant", FinancialAccounts_JointApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_AliasOfJointApplicant", FinancialAccounts_AliasOfJointApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_AssociationOfJointApplicant", FinancialAccounts_AssociationOfJointApplicant);
            _configurationVariablesRepository.SetByName("FinancialAccounts_No_Match", FinancialAccounts_No_Match);
            return SettingsGeneral();
        }
    }
}
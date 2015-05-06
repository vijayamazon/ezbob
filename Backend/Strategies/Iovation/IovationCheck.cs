namespace Ezbob.Backend.Strategies.Iovation {
    using System;
    using System.Linq;
    using ConfigManager;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Database;
    using EZBob.DatabaseLib.Model.Fraud;
    using IovationLib;
    using IovationLib.IovationCheckTransactionDetailsNS;
    using Newtonsoft.Json;
    using StructureMap;

    public class IovationCheck : AStrategy {
        public IovationCheck(IovationCheckModel model) {
            this.model = model;
        }

        public override string Name { get { return "Iovation Check"; } }

        public override void Execute() {
            if (!CurrentValues.Instance.IovationEnabled) {
                Log.Info("Iovation is disabled, not invoking the service");
                return;
            }

            IovationLastCheckModel iovationLastCheck = DB.FillFirst<IovationLastCheckModel>("IovationGetCheckStatus", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.model.CustomerID));

            if (iovationLastCheck.LastCheckDate.HasValue && iovationLastCheck.LastCheckDate.Value > DateTime.UtcNow.AddDays(-ConfigManager.CurrentValues.Instance.IovationCheckPeriod)) {
                Log.Info("Not performing iovation check, last one was done on {0}", iovationLastCheck.LastCheckDate.Value);
                return;
            }

            if (!iovationLastCheck.FinishedWizard && iovationLastCheck.FilledByBroker) {
                Log.Info("Not performing iovation check, filled by broker");
                return;
            }

            IovationLib.IovationAppClient client = new IovationAppClient(
                CurrentValues.Instance.IovationSubscriberId, 
                CurrentValues.Instance.IovationSubscriberAccount,
                CurrentValues.Instance.IovationSubscriberPasscode, 
                CurrentValues.Instance.IovationAdminPassword, 
                CurrentValues.Instance.IovationAdminAccountName,
                CurrentValues.Instance.IovationEnvironment);

            CheckTransactionDetailsResponse response = client.CheckTransactionDetails(this.model);

            if (response == null) {
                Log.Warn("Iovation CheckTransactionDetails Response is null for customer {0}", this.model.CustomerID);
                return;
            }

            Log.Info("Iovation Response : \n{0}", JsonConvert.SerializeObject(response, Formatting.Indented));

            CheckTransactionDetailsResponseDetail score = response.details == null ? null : response.details.FirstOrDefault(x => x.name == "ruleset.score");
            
            IovationResult result;
            if(!Enum.TryParse(response.result, out result)){
                result = IovationResult.U;
            }

            FraudIovationRepository repo = ObjectFactory.GetInstance<FraudIovationRepository>();
            repo.Save(new FraudIovation{
                Created = DateTime.UtcNow,
                CustomerID = this.model.CustomerID,
                Origin = this.model.Origin,
                Reason = response.reason,
                Result = result,
                Score =  score == null ? null : score.value,
                TrackingNumber = response.trackingnumber,
                Details = JsonConvert.SerializeObject(response.details, Formatting.Indented)
            });
        }

        private readonly IovationCheckModel model;
    }
}

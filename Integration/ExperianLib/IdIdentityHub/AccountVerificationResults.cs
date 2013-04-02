using System;
using System.Text;
using ExperianLib.Web_References.IDHubService;

namespace ExperianLib.IdIdentityHub
{
    public class AccountVerificationResults : BaseIdHubResult
    {
        public string AuthenticationText { get; set; }
        public string AccountStatus { get; set; }
        public decimal NameScore { get; set; }
        public decimal AddressScore { get; set; }

        //-----------------------------------------------------------------------------------
        public override void Parse(ProcessConfigResponseType response)
        {
            base.Parse(response);

            var errors = new StringBuilder();

            Utils.TryRead(() => AuthenticationText = response.DecisionHeader.AuthenticationText, "AuthenticationText", errors);
            Utils.TryRead(() => AccountStatus = response.ProcessConfigResultsBlock.BWAResultBlock.AccountStatus, "AccountStatus", errors);
            Utils.TryRead(() => NameScore = Convert.ToDecimal(response.ProcessConfigResultsBlock.BWAResultBlock.NameScore), "NameScore", errors);
            Utils.TryRead(() => AddressScore = Convert.ToDecimal(response.ProcessConfigResultsBlock.BWAResultBlock.AddressScore), "AddressScore", errors);

            if (string.IsNullOrEmpty(Error) && (errors.Length > 0))
            {
                Error = errors.ToString();
            }
        }
    }
}
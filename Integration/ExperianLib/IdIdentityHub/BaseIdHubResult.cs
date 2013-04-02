using System;
using System.Text;
using ExperianLib.Web_References.IDHubService;

namespace ExperianLib.IdIdentityHub
{
    public class BaseIdHubResult
    {
        public bool HasError
        {
            get { return !string.IsNullOrEmpty(Error); }
        }
        public string Error { get; set; }
        public string AuthenticationDecision { get; set; }

        public virtual void Parse(ProcessConfigResponseType response)
        {
            AuthenticationDecision = response.DecisionHeader.AuthenticationDecision.ToString();
            if(response.DecisionHeader.AuthenticationDecision != AuthenticationDecisionType.Authenticated)
            {
                Error = String.Format("AuthenticationDecision is '{0}', AuthenticationText is '{1}'",response.DecisionHeader.AuthenticationDecision,response.DecisionHeader.AuthenticationText);
            }
        }
    }
}
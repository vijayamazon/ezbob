using System;
using System.Text;
using ExperianLib.Web_References.IDHubService;

namespace ExperianLib.IdIdentityHub
{
    public class AuthenticationResults:BaseIdHubResult
    {
        public decimal AuthenticationIndexType { get; set; }
        public string AuthIndexText { get; set; }
        public decimal NumPrimDataItems { get; set; }
        public decimal NumPrimDataSources { get; set; }
        public decimal NumSecDataItems { get; set; }
        public string StartDateOldestPrim { get; set; }
        public string StartDateOldestSec { get; set; }
        public decimal ReturnedHRPCount { get; set; }
        public ReturnedHRPType[] ReturnedHRP { get; set; }


        public override void Parse(ProcessConfigResponseType response)
        {
            base.Parse(response);

            var errors = new StringBuilder();

            Utils.TryRead(() => AuthenticationIndexType = Convert.ToDecimal(response.ProcessConfigResultsBlock.EIAResultBlock.AuthenticationIndex), "AuthenticationIndexType", errors);
            Utils.TryRead(() => AuthIndexText = response.ProcessConfigResultsBlock.EIAResultBlock.AuthIndexText, "AuthIndexText", errors);
            Utils.TryRead(() => NumPrimDataItems = Convert.ToDecimal(response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.IDandLocDataAtCL.NumPrimDataItems), "NumPrimDataItems", errors);
            Utils.TryRead(() => NumPrimDataSources = Convert.ToDecimal(response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.IDandLocDataAtCL.NumPrimDataSources), "NumPrimDataSources", errors);
            Utils.TryRead(() => NumSecDataItems = Convert.ToDecimal(response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.IDandLocDataAtCL.NumSecDataItems), "NumSecDataItems", errors);
            Utils.TryRead(() => StartDateOldestPrim = response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.IDandLocDataAtCL.StartDateOldestPrim, "StartDateOldestPrim", errors);
            Utils.TryRead(() => StartDateOldestSec = response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.IDandLocDataAtCL.StartDateOldestSec, "StartDateOldestSec", errors);
            Utils.TryRead(() => ReturnedHRPCount = Convert.ToDecimal(response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.ReturnedHRPCount), "ReturnedHRPCount", errors);
            Utils.TryRead(() => ReturnedHRP = response.ProcessConfigResultsBlock.EIAResultBlock.EIAResults.ReturnedHRP, "ReturnedHRP", errors);
            if (ReturnedHRP == null) ReturnedHRP = new ReturnedHRPType[0];

            if (string.IsNullOrEmpty(Error) && (errors.Length > 0))
            {
                Error = errors.ToString();
            }
        }
    }
}
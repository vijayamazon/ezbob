namespace EKM
{
    using EKM.API;
    using System.Text;

    /// <summary>
    /// Here we use EKM API classes
    /// The API service reference was generated from http://partnerapi.ekmpowershop1.com/v1.1/partnerapi.asmx
    /// </summary>
    public class EkmConnector
    {
        private static string PartnerKey = "4kNLfm+jv37k0sWb8ojpxGSQ7yx169xz/nS3mmKGiCwUn7fJIl5UxAZthlm44iiEJynebcGHOG/9fJV2/cM4BQ==";
        private static string LineBreak = "<br/>";

        public bool Validate(string userName, string password, out string errMsg)
        {
            // Instantiate Soap Client to access shop data
            var shopClient = new PartnerAPISoapClient();

            // Form request to retrieve shop data (Shop details)
            var getKeyRequest = new GetKeyRequest();

            // Your unique PartnerKey must be passed with each request
            getKeyRequest.PartnerKey = PartnerKey;

            // The customers ekmPowershop username
            getKeyRequest.UserName = userName;

            // The customers ekmPowershop password
            getKeyRequest.Password = password;

            // Retrieve shop data (Shop details)
            var getKeyResponse = shopClient.GetKey(getKeyRequest);

            // Check if the request failed
            if (getKeyResponse.Status == StatusCodes.Failure)
            {
                // Combine the errors explaining why the request failed
                StringBuilder sb = new StringBuilder();
                int counter = 1;
                foreach (var error in getKeyResponse.Errors)
                {
                    if (counter != 1)
                    {
                        sb.Append(LineBreak);
                    }
                    sb.Append(error);
                    counter++;
                }

                if (counter != 1)
                {
                    sb.Append(LineBreak);
                }
                sb.Append("Login to Shop Failed. UserName:").Append(userName);
                errMsg = sb.ToString();
                return false;
            }

            errMsg = string.Empty;
            return true;
        }
    }
}
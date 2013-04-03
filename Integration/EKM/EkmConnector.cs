namespace EKM
{
    // The API service reference was generated from http://partnerapi.ekmpowershop1.com/v1.1/partnerapi.asmx
    using EKM.EkmApiNS;

    public class EkmConnector
    {
        public static string PartnerKey = "4kNLfm+jv37k0sWb8ojpxGSQ7yx169xz/nS3mmKGiCwUn7fJIl5UxAZthlm44iiEJynebcGHOG/9fJV2/cM4BQ==";

        public bool Validate(string id, string password, out string errMsg)
        {
            errMsg = "fake reason";
            return false;
        }

        public bool test(string id, string password, out string errMsg)
        {
            // should i encrypt decrypt?
            //var decrypted = Encryptor.Decrypt(password);




            errMsg = string.Empty;

            // Instantiate Soap Client to access shop data
            var shopClient = new PartnerAPISoapClient();

            // Form request to retrieve shop data (Shop details)
            var getKeyRequest = new GetKeyRequest();

            // Your unique PartnerKey must be passed with each request
            getKeyRequest.PartnerKey = PartnerKey;

            // The customers ekmPowershop username
            getKeyRequest.UserName = id;

            // The customers ekmPowershop password
            getKeyRequest.Password = password;

            // Retrieve shop data (Shop details)
            var getKeyResponse = shopClient.GetKey(getKeyRequest);

            // Check if the request failed
            if (getKeyResponse.Status == StatusCodes.Failure)
            {
                // Output the errors explaining why the request failed
                int counter = 1;
                foreach (var error in getKeyResponse.Errors)
                {
                    errMsg += string.Format("Error #{0}:{1}", counter, error);
                    counter++;
                }

                errMsg += string.Format("Login to Shop Failed {0}", id);
                return false;
            }

            return true;
        }
    }
}
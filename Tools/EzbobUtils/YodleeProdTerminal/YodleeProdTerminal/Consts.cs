namespace EKMConnector
{
    public class Consts
    {
        public static string CryptoKey = "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
        public static string CryptoSalt = "EzBB0b";
        public static string PartnerKey = "4kNLfm+jv37k0sWb8ojpxGSQ7yx169xz/nS3mmKGiCwUn7fJIl5UxAZthlm44iiEJynebcGHOG/9fJV2/cM4BQ==";
        public static string PartnerEndpointName = "PartnerAPISoap";

        public static string GetNewShopsSpName = "EKMGetNewShops";
        public static string GetNewShopsSpDisplayNameColumn = "DisplayName";
        public static string GetNewShopsSpSecurityDataColumn = "SecurityData";
        public static string GetNewShopsSpCustomerIdColumn = "CustomerId";
        public static string GetNewShopsSpShopIdColumn = "Id";
        
        public static string UpdateLastHandledIdSpName = "EkmUpdateLastHandledId";
        public static string UpdateLastHandledIdSpParam1Name = "@LastHandledId";

        public static string GetEkmConnectorConfigsSpName = "GetEkmConnectorConfigs";
        public static string NoErrorIndication = "No data returned";
    }
}
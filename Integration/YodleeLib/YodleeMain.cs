using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
    using StructureMap;
    using config;

    public class YodleeMain : ApplicationSuper
    {
        public UserContext userContext = null;
        ContentServiceTraversalService contentServieTravelService = new ContentServiceTraversalService();
        ServerVersionManagementService serverVersionManagementService = new ServerVersionManagementService();
        private static IYodleeMarketPlaceConfig _config;

        public YodleeMain()
        {
            _config = ObjectFactory.GetInstance<IYodleeMarketPlaceConfig>();
            contentServieTravelService.Url = _config.soapServer + "/" + contentServieTravelService.GetType().FullName;
            serverVersionManagementService.Url = _config.soapServer + "/" + serverVersionManagementService.GetType().FullName;
        }

        public string loginUser(string userName, string password)
        {
            //userName = "Stas";
            //password = "Ab12cD";
            LoginUser loginUser = new LoginUser();

            try
            {
                userContext = loginUser.loginUser(userName, password);
                return "User Logged in Successfully..";
            }
            catch (SoapException se)
            {
                return "User login failed -> " + se.Message;
            }
        }
    }
}
using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
    using config;

    public class YodleeMain : ApplicationSuper
    {
        public UserContext userContext = null;
        ContentServiceTraversalService contentServieTravelService = null;
        ServerVersionManagementService serverVersionManagementService = null;
       // internal readonly IYodleeMarketPlaceConfig config;// = new YodleeEnvConnectionConfig();
        public YodleeMain()
        {
            contentServieTravelService = new ContentServiceTraversalService();
            contentServieTravelService.Url = YodleeConfig._Config.soapServer + "/" + contentServieTravelService.GetType().FullName;
            serverVersionManagementService = new ServerVersionManagementService();
            serverVersionManagementService.Url = YodleeConfig._Config.soapServer + "/" + serverVersionManagementService.GetType().FullName;
        }

        public string loginUser(string userName, string password)
        {
            //System.Console.Write("Login [" + userName + "]: ");
            userName = "Stas";// System.Console.ReadLine();

            password = "Ab12cD";
            //System.Console.Write("Password [" + password + "]: ");
            //password = System.Console.ReadLine();

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
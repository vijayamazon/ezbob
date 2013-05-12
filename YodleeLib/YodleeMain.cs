using System;
using System.Web.Services.Protocols;

namespace YodleeLib
{
    public class YodleeMain : ApplicationSuper
    {
        public UserContext userContext = null;
        public String userName = null;
        ContentServiceTraversalService contentServieTravelService = null;
        ServerVersionManagementService serverVersionManagementService = null;
        public YodleeMain()
        {
            contentServieTravelService = new ContentServiceTraversalService();
            contentServieTravelService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + contentServieTravelService.GetType().FullName;
            serverVersionManagementService = new ServerVersionManagementService();
            serverVersionManagementService.Url = System.Configuration.ConfigurationManager.AppSettings.Get("soapServer") + "/" + serverVersionManagementService.GetType().FullName;
        }

        public string loginUser()
        {
            //System.Console.Write("Login [" + userName + "]: ");
            userName = "Stas";// System.Console.ReadLine();

            String password = "Ab12cD";
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
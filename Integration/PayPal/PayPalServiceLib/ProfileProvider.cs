using System;
using System.IO;
using System.Web;
using EzBob.PayPalServiceLib.Common;
using PayPal.Platform.SDK;
using StructureMap;

namespace EzBob.PayPalServiceLib
{
    public static class ProfileProvider
    {
        public static BaseAPIProfile GetCachedProfile(IPayPalConfig payPalConfig)
        {
            var profile = HttpContext.Current.Session["PROFILE"] as BaseAPIProfile;
            if(profile == null)
            {
                HttpContext.Current.Session["PROFILE"] = profile = CreateProfile(payPalConfig);
            }
            return profile;
        }

        public static BaseAPIProfile CreateProfile(IPayPalConfig config)
        {
            BaseAPIProfile profile = null;
            byte[] bCert = null;
            string filePath = string.Empty;
            FileStream fs = null;

			var factory = ObjectFactory.GetInstance<IServiceEndPointFactory>();

			var connectionInfo = factory.Create( PayPalServiceType.Permissions, config.ServiceType );
            try
            {

                if (config.ApiAuthenticationMode == PayPalProfileType.ThreeToken)
                {
                    ////Three token 
                    profile = new BaseAPIProfile
                                  {
                                      APIProfileType = ConvertToServerEnum(config.ApiAuthenticationMode),
                                      ApplicationID = config.PPApplicationId,
                                      APIUsername = config.ApiUsername,
                                      APIPassword = config.ApiPassword,
                                      APISignature = config.ApiSignature,
									  Environment = connectionInfo.ServiceEndPoint,
                                      RequestDataformat = config.ApiRequestformat.ToString(),
                                      ResponseDataformat = config.ApiResponseformat.ToString(),
                                      IsTrustAllCertificates = config.TrustAll
                                  };
                }
                else
                {
//                    ////Certificate
//                    profile = new BaseAPIProfile();
//                    profile.APIProfileType = ProfileType.Certificate;
//                    profile.APIUsername = config.API_USERNAME;
//                    profile.APIPassword = config.API_PASSWORD;
//                    profile.ApplicationID = config.APPLICATION-ID;
//                    profile.RequestDataformat = config.API_REQUESTFORMAT;
//                    profile.ResponseDataformat = config.API_RESPONSEFORMAT;
//
//                    profile.IsTrustAllCertificates = Convert.ToBoolean(config.TrustAll);
//                    ///loading the certificate file into profile.
//                    filePath = HttpContext.Current.Server.MapPath(config.CERTIFICATE.ToString());
//                    fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
//                    bCert = new byte[fs.Length];
//                    fs.Read(bCert, 0, int.Parse(fs.Length.ToString()));
//                    fs.Close();
//
//                    profile.Certificate = bCert;
//                    profile.PrivateKeyPassword = config.PRIVATE_KEY_PASSWORD;
//                    profile.APISignature = "";
//                    profile.Environment = config.ENDPOINT;
                    throw new CerteficateConfigIsNotSupported();
                }

            }
            catch (FATALException FATALEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return profile;
        }

    	private static ProfileType ConvertToServerEnum(PayPalProfileType payPalProfileType)
    	{
    		switch (payPalProfileType)
    		{
    			case PayPalProfileType.ThreeToken:
					return ProfileType.ThreeToken;

				case PayPalProfileType.Certificate:
					return ProfileType.Certificate;

				default:
					throw new NotImplementedException();
    		}
    	}
    }
}
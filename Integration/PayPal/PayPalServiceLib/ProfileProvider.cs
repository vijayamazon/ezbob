namespace EzBob.PayPalServiceLib
{
	using System;
	using System.Web;
	using Common;
	using PayPal.Platform.SDK;
	using StructureMap;
	using ConfigManager;

	public static class ProfileProvider
    {
		private enum PayPalProfileType
		{
			ThreeToken,
			Certificate
		}

        public static BaseAPIProfile GetCachedProfile()
        {
            var profile = HttpContext.Current.Session["PROFILE"] as BaseAPIProfile;
            if(profile == null)
            {
                HttpContext.Current.Session["PROFILE"] = profile = CreateProfile();
            }
            return profile;
        }

        public static BaseAPIProfile CreateProfile()
        {
            BaseAPIProfile profile = null;

			var factory = ObjectFactory.GetInstance<IServiceEndPointFactory>();

			var connectionInfo = factory.Create(PayPalServiceType.Permissions);
            try
            {
				if (CurrentValues.Instance.PayPalApiAuthenticationMode == PayPalProfileType.ThreeToken.ToString())
                {
	                profile = new BaseAPIProfile
		                {
			                APIProfileType = ConvertToServerEnum(PayPalProfileType.ThreeToken),
			                ApplicationID = CurrentValues.Instance.PayPalPpApplicationId,
			                APIUsername = CurrentValues.Instance.PayPalApiUsername,
			                APIPassword = CurrentValues.Instance.PayPalApiPassword,
			                APISignature = CurrentValues.Instance.PayPalApiSignature,
			                Environment = connectionInfo.ServiceEndPoint,
			                RequestDataformat = CurrentValues.Instance.PayPalApiRequestFormat.Value,
							ResponseDataformat = CurrentValues.Instance.PayPalApiResponseFormat.Value,
			                IsTrustAllCertificates = CurrentValues.Instance.PayPalTrustAll
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
            catch (FATALException )
            {
                throw;
            }
            catch (Exception )
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
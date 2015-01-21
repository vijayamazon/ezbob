using EzBob.AmazonServiceLib.Common;

namespace EzBob.AmazonServiceLib.Config
{
	public class AmazonMarketPlaceTypeConnection : AmazonMarketPlaceTypeConnectionBase
	{
		public AmazonMarketPlaceTypeConnection()
		{
			KeyId = "AKIAJXUDX6A3XIMZLWFA";
			SecretKeyId = "4yQzxltFZjlytmkKmlHhkAAcZTTZUbHpJekTOFj2";
			MarketCountry = AmazonServiceCountry.UK;
			ServiceType = AmazonServiceType.Live;
            AskvilleAmazonLogin = "it@ezbob.com";
		    AskvilleAmazonPass = "1qazxsw2";
		}
	}
}
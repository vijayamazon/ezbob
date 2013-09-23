namespace YodleeLib
{
	using EzBob.Configuration;
	using config;

	public class ContentServiceHelper : ApplicationSuper
	{
		private static YodleeEnvConnectionConfig config;
		protected ContentServiceTraversalService Cst;
		protected UserContext UserContext;

		public ContentServiceHelper(UserContext userContext)
		{
			config = YodleeConfig._Config;
			UserContext = userContext;
			Cst = new ContentServiceTraversalService
				{
					Url = config.soapServer + "/" + "ContentServiceTraversalService"
				};
		}

		/**
		 * @param content_service_id
		 * @return true if content service is MFA
		 */
		public bool IsMfAType(long content_service_id)
		{
			ContentServiceInfo csi = Cst.getContentServiceInfo(GetCobrandContext(), content_service_id, true);
			return csi.mfaType == null;
		}
	}
}

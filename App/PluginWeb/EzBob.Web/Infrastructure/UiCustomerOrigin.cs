namespace EzBob.Web.Infrastructure {
	using System;
	using System.Web;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public static class UiCustomerOrigin {
		public static CustomerOrigin Get(Uri uri = null) {
			CustomerOriginRepository customerOriginRepository = ObjectFactory.GetInstance<CustomerOriginRepository>();

			string host = (uri ?? HttpContext.Current.Request.Url).Host;

			foreach (var co in customerOriginRepository.GetAllOrdered())
				if (host.Contains(co.UrlNeedle))
					return co;

			return customerOriginRepository.GetDefault();
		} // Get

		public static void Set(dynamic viewBag, string phoneNumber = null, CustomerOrigin customerOrigin = null) {
			customerOrigin = customerOrigin ?? Get();
			viewBag.CustomerOrigin = customerOrigin;
			viewBag.PhoneNumber = phoneNumber ?? customerOrigin.PhoneNumber;
		} // Set

		public static void SetDefault(dynamic viewBag) {
			CustomerOriginRepository customerOriginRepository = ObjectFactory.GetInstance<CustomerOriginRepository>();
			Set(viewBag, null, customerOriginRepository.GetDefault());
		} // SetDefault

		public static string GetTrusteSeal(dynamic viewBag) {
			return string.Format(TrusteSealFormat, GetOriginFromViewBag(viewBag).TrusteSealUniqueID.ToString("D"));
		} // GetTrusteSeal

		public static string GetVeriSignSeal(dynamic viewBag) {
			string customerSite = GetOriginFromViewBag(viewBag).CustomerSite;
			if (customerSite != null) {
				customerSite = customerSite.Replace("https://", "").Replace(":44300", "");
			}
			return string.Format(VeriSignSealFormat, customerSite);
		} // GetVeriSignSeal

		public static string GetSecuritySeals(dynamic viewBag) {
			return string.Format("{0}{1}", GetVeriSignSeal(viewBag), GetTrusteSeal(viewBag));
		} // GetSecuritySeals

		private static CustomerOrigin GetOriginFromViewBag(dynamic viewBag) {
			CustomerOrigin origin = null;

			try {
				if (viewBag != null)
					origin = viewBag.CustomerOrigin;
			} catch {
				// Silently ignore.
			} // try

			return origin ?? ObjectFactory.GetInstance<CustomerOriginRepository>().GetDefault();
		} // GetOriginFromViewBag

		private const string TrusteSealFormat = @"<div>
	<a
		href=""//privacy.truste.com/privacy-seal/validation?rid={0}""
		title=""TRUSTe Privacy Certification""
		target=""_blank""
	><img
		style=""border:none;""
		src=""//privacy-policy.truste.com/privacy-seal/seal?rid={0}""
		alt=""TRUSTe Privacy Certification""
		onerror=""this.src='/Content/img/logo-truste.png';""
		class=""truste-privacy-certification""
	/></a>
</div>";

		private const string VeriSignSealFormat = @"<a href=""javascript:vrsn_splash()"">
	<img
		alt=""Click to Verify - This site has chosen an SSL Certificate to improve Web site security""
		width=""100""
		height=""72""
		src=""https://seal.verisign.com/getseal?at=0&sealid=2&dn={0}&lang=en&tpt=transparent""
		name=""seal""
	>
</a>";
	} // class UiCustomerOrigin
} // namespace

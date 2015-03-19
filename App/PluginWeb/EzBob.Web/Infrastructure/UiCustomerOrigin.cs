namespace EzBob.Web.Infrastructure {
	using System.Web;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public static class UiCustomerOrigin {
		public static CustomerOrigin Get() {
			CustomerOriginRepository customerOriginRepository = ObjectFactory.GetInstance<CustomerOriginRepository>();

			foreach (var co in customerOriginRepository.GetAllOrdered())
				if (HttpContext.Current.Request.Url.Host.Contains(co.UrlNeedle))
					return co;

			return customerOriginRepository.GetDefault();
		} // Set

		public static void Set(dynamic viewBag, string phoneNumber = null, CustomerOrigin customerOrigin = null) {
			customerOrigin = customerOrigin ?? Get();
			viewBag.CustomerOrigin = customerOrigin;
			viewBag.PhoneNumber = phoneNumber ?? customerOrigin.PhoneNumber;
		} // Set

		public static void SetDefault(dynamic viewBag) {
			CustomerOriginRepository customerOriginRepository = ObjectFactory.GetInstance<CustomerOriginRepository>();
			Set(viewBag, null, customerOriginRepository.GetDefault());
		} // SetDefault
	} // class UiCustomerOrigin
} // namespace

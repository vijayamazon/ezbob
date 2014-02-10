namespace EzBob.Web.Areas.Broker {
	using System.Web.Mvc;

	public class BrokerAreaRegistration : AreaRegistration {
		public override string AreaName {
			get {
				return "Broker";
			} // get
		} // AreaName

		public override void RegisterArea(AreaRegistrationContext context) {
			context.MapRoute(
				"Broker_default",
				"Broker/{controller}/{action}/{id}",
				new { action = "Index", controller = "BrokerHome", id = UrlParameter.Optional }
			);
		} // RegisterArea
	} // class BrokerAreaRegistration
} // namespace

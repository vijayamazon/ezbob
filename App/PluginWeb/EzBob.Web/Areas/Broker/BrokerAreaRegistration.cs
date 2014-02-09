using System.Web.Mvc;

namespace EzBob.Web.Areas.Broker {
	public class BrokerAreaRegistration : AreaRegistration {
		public override string AreaName {
			get {
				return "Broker";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context) {
			context.MapRoute(
				"Broker_default",
				"Broker/{controller}/{action}/{id}",
				new { action = "Index", controller = "BrokerHome", id = UrlParameter.Optional }
			);
		}
	}
}

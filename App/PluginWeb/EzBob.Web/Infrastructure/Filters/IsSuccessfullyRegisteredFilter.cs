namespace EzBob.Web.Infrastructure.Filters {
	using System.Linq;
	using System.Web.Mvc;
	using System.Web.Routing;
	using EzBob.Web.Code;
	using StructureMap;

	public class IsSuccessfullyRegisteredFilter : ActionFilterAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			var workplaceContext = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();

			if (workplaceContext.User != null) {
				var oBrokerHelper = new BrokerHelper();

				if (oBrokerHelper.IsBroker(workplaceContext.User.EMail)) {
					filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
						{"action", "Index"},
						{"controller", "BrokerHome"},
						{"Area", "Broker"}
					});

					return;
				} // if
			} // if

			var isUnderwriter = workplaceContext.UserRoles.Any(
				x => x.ToLower() == "crm" || x.ToLower() == "manager" || x.ToLower() == "underwriter"
			);

			if (isUnderwriter) {
				filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
					{"action", "Index"},
					{"controller", "Customers"},
					{"Area", "Underwriter"}
				});

				return;
			} // if

			var customer = workplaceContext.Customer;

			if (customer == null)
				return;

			var routeDictionary = new RouteValueDictionary {
				{ "action", "Index" },
				{ "Area", "Customer" }
			};

			if (customer.WizardStep.TheLastOne) {
				if (filterContext.RouteData.Values["controller"].ToString() == "Profile")
					return;

				routeDictionary.Add("controller", "Profile");

				filterContext.Result = new RedirectToRouteResult(routeDictionary);
			}
			else {
				if (filterContext.RouteData.Values["controller"].ToString() == "Wizard")
					return;

				routeDictionary.Add("controller", "Wizard");

				filterContext.Result = new RedirectToRouteResult(routeDictionary);
			} // if
		} // OnActionExecuting
	} // class IsSuccessfullyRegisteredFilter
} // namespace

using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace EzBob.Web.Infrastructure.Filters
{
	using EZBob.DatabaseLib.Model.Database;

	public class IsSuccessfullyRegisteredFilter : ActionFilterAttribute
    {
        //-------------------------------------------------------------------------
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var workplaceContext = ObjectFactory.GetInstance<IEzbobWorkplaceContext>();

            var isUnderwriter = workplaceContext.User != null && workplaceContext.User.Roles.Any(
                x =>
                x.Name.ToLower() == "crm" || x.Name.ToLower() == "manager" || x.Name.ToLower() == "Underwriter");
            var redirectRouteDict = new RouteValueDictionary
                {
                    {"action", "Index"},
                    {"controller", "Customers"},
                    {"Area", "Underwriter"}
                };
            if (isUnderwriter)
            {
                filterContext.Result = new RedirectToRouteResult(redirectRouteDict);
            }
            else
            {
                var customer = workplaceContext.Customer;
                if (workplaceContext.Customer == null) return;

                if (customer == null) return;

                var routeDictionary = new RouteValueDictionary {{"action", "Index"}, {"Area", "Customer"}};

                if (customer.WizardStep.TheLastOne) {
                    if (filterContext.RouteData.Values["controller"].ToString() == "Profile") return;
                    routeDictionary.Add("controller", "Profile");
                    filterContext.Result = new RedirectToRouteResult(routeDictionary);
                }
                else {
                    if (filterContext.RouteData.Values["controller"].ToString() == "Wizard") return;
                    routeDictionary.Add("controller", "Wizard");
                    filterContext.Result = new RedirectToRouteResult(routeDictionary);
                }
            }
        }
    }
}
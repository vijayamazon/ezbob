namespace Demo.Filters {
	using System.Web.Http.Filters;
	using Controllers.Versions;

	internal class DemoActionFilterAttribute : ActionFilterAttribute {
		protected int GetApiVersion(System.Web.Http.Controllers.HttpActionContext actionContext) {
			var oController = actionContext.ControllerContext.Controller as DemoApiControllerBase;
			return oController == null ? 0 : oController.ApiVersion;
		} // GetApiVersoin
	} // class DemoActionFilterAttribute
} // namespace

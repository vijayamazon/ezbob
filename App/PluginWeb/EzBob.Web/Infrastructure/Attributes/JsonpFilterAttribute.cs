namespace EzBob.Web.Infrastructure.Attributes {
	using System.Web.Mvc;

	public class JsonpFilterAttribute : ActionFilterAttribute {
		public override void OnActionExecuted(ActionExecutedContext filterContext) {
			if (filterContext == null)
				throw new System.ArgumentNullException("filterContext");

			string text = filterContext.HttpContext.Request.QueryString["callback"];

			if (!string.IsNullOrEmpty(text)) {
				var jsonResult = filterContext.Result as JsonResult;

				if (jsonResult == null)
					throw new System.InvalidOperationException("JsonpFilterAttribute must be applied only on controllers and actions that return a JsonResult object.");

				filterContext.Result = new JsonpResult {
					ContentEncoding = jsonResult.ContentEncoding,
					ContentType = jsonResult.ContentType,
					Data = jsonResult.Data,
					Callback = text
				};
			} // if
		} // OnActionExecuted
	} // class JsonpFilterAttribute
} // namespace

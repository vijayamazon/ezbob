namespace EzBob.Web.Infrastructure.Attributes {
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;

	public class JsonpResult : JsonResult {
		public string Callback { get; set; }

		public override void ExecuteResult(ControllerContext context) {
			if (context == null)
				throw new System.ArgumentNullException("context");

			HttpResponseBase response = context.HttpContext.Response;

			if (!string.IsNullOrEmpty(ContentType))
				response.ContentType = ContentType;
			else
				response.ContentType = "application/javascript";

			if (ContentEncoding != null)
				response.ContentEncoding = ContentEncoding;

			if (string.IsNullOrEmpty(Callback))
				Callback = context.HttpContext.Request.QueryString["callback"];

			if (Data != null) {
				var javaScriptSerializer = new JavaScriptSerializer();
				string str = javaScriptSerializer.Serialize(Data);
				response.Write(Callback + "(" + str + ");");
			} // if
		} // ExecuteRequest
	} // class JsonpResult
} // namespace

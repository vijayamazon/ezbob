namespace Demo.Controllers.Versions {
	using System.Web.Http;
	using Filters;

	public class DemoApiControllerBase : ApiController {
		public int ApiVersion {
			get {
				if (m_nApiVersion > 0)
					return m_nApiVersion;

				object[] oAttrList = this.GetType().GetCustomAttributes(typeof(HandleActionExecutedAttribute), false);

				if (oAttrList.Length > 0)
					m_nApiVersion = ((HandleActionExecutedAttribute)oAttrList[0]).ApiVersion;

				return m_nApiVersion;
			} // get
		} // ApiVersion

		private int m_nApiVersion;
	} // class DemoApiControllerBase
} // namespace
